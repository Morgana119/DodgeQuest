using UnityEngine;

/// <summary>
/// Controlador principal del nivel. 
/// Administra las fases del juego: inicio, fase 1, jefe (boss) y fin.
/// Se encarga de habilitar/deshabilitar spawners, limpiar enemigos y detectar condiciones de game over.
/// </summary>
public class LevelController : MonoBehaviour
{
    /// <summary>Fases posibles del nivel.</summary>
    public enum Phase { None, Phase1, Boss, End }

    /// <summary>Bandera para evitar múltiples triggers de game over.</summary>
    private bool gameOverTriggered = false;

    [Header("Spawners de Fase 1 (AreaEnemy)")]
    /// <summary>Spawners que generan enemigos durante la fase 1.</summary>
    public AreaEnemy[] spawners;

    [Header("Temporización exacta (segundos)")]
    /// <summary>Segundo exacto en que termina la fase 1 y comienza la transición al boss.</summary>
    public float phase1EndAt = 30f;
    /// <summary>Tiempo de espera (segundos) entre el final de fase 1 y la aparición del boss.</summary>
    public float bossDelay   = 1.0f;
    /// <summary>Tiempo exacto (segundos) en el que el juego termina automáticamente.</summary>
    public float gameEndAt   = 60f;

    [Header("Boss (Fase 2)")]
    /// <summary>Objeto raíz del jefe. Debe estar inactivo al inicio.</summary>
    public GameObject bossRoot;

    [Header("Debug")]
    /// <summary>Si es true, muestra mensajes de transición en la consola.</summary>
    public bool logTransitions = true;

    /// <summary>Fase actual del nivel.</summary>
    public Phase CurrentPhase { get; private set; } = Phase.None;

    // --- Estados internos ---
    private bool phase1Started, bossTransitionDone, bossShown, gameEnded;
    private float bossEnableAt = -1f;

    /// <summary>
    /// Inicializa el nivel: arranca el <see cref="TimeManager"/>, 
    /// comienza en fase 1 y suscribe eventos de muerte de jugador y jefe.
    /// </summary>
    void Start()
    {
        if (!TimeManager.Instance) gameObject.AddComponent<TimeManager>();
        TimeManager.Instance.ResetAndStart();

        CurrentPhase = Phase.Phase1;
        phase1Started = true;
        bossShown = false;
        bossTransitionDone = false;
        gameEnded = false;

        SetAllAreaSpawnersEnabled(true);
        if (bossRoot) bossRoot.SetActive(false);

        Player.OnPlayerDied += HandlePlayerDied;
        BulletSpawnerBoss.OnBossDefeated += HandleBossDefeated;
    }

    /// <summary>
    /// Limpia las suscripciones a eventos al destruirse el objeto.
    /// </summary>
    void OnDestroy()
    {
        Player.OnPlayerDied -= HandlePlayerDied;
        BulletSpawnerBoss.OnBossDefeated -= HandleBossDefeated;
    }

    /// <summary>
    /// Se ejecuta cada frame. Administra el flujo temporal de fases:
    /// - Activa Fase 1 al inicio
    /// - Cambia al Boss al llegar a <see cref="phase1EndAt"/>
    /// - Enciende al Boss tras el <see cref="bossDelay"/>
    /// </summary>
    void Update()
    {
        float t = TimeManager.Instance ? TimeManager.Instance.elapsed : 0f;

        // Fase 1 
        if (!phase1Started && t >= 0f)
        {
            phase1Started = true;
            CurrentPhase = Phase.Phase1;
            SetAllAreaSpawnersEnabled(true);
        }

        // Transición a Boss
        if (!bossTransitionDone && t >= phase1EndAt)
        {
            bossTransitionDone = true;
            CurrentPhase = Phase.Boss;

            SetAllAreaSpawnersEnabled(false);
            HardClearAllEnemiesAndEnemyBullets();

            bossEnableAt = t + Mathf.Max(0f, bossDelay);
        }

        // Enciende boss cuando toque
        if (!bossShown && bossEnableAt > 0f && t >= bossEnableAt)
        {
            bossShown = true;
            if (bossRoot) bossRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Activa o desactiva todos los spawners del tipo <see cref="AreaEnemy"/>.
    /// Incluye tanto los asignados en el inspector como los encontrados en escena.
    /// </summary>
    void SetAllAreaSpawnersEnabled(bool enabled)
    {
        if (spawners != null)
            foreach (var s in spawners)
                if (s) s.gameObject.SetActive(enabled);

        var all = FindObjectsByType<AreaEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var s in all)
            if (s) s.gameObject.SetActive(enabled);
    }

    /// <summary>
    /// Destruye todos los enemigos normales y balas enemigas activas en la escena.
    /// </summary>
    void HardClearAllEnemiesAndEnemyBullets()
    {
        int cleared = 0;

        var enemies = FindObjectsByType<BulletSpawnerEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e)
            {
                Destroy(e.gameObject);
                cleared++;
            }
        }

        var bullets = FindObjectsByType<Bullet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var b in bullets)
        {
            if (b && b.owner == Bullet.Owner.Enemy)
            {
                Destroy(b.gameObject);
                cleared++;
            }
        }
    }

    /// <summary>
    /// Maneja el fin de juego cuando muere el jugador:
    /// desactiva todo y detiene el <see cref="TimeManager"/>.
    /// </summary>
    void HandlePlayerDied()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        CurrentPhase = Phase.End;
        if (bossRoot) bossRoot.SetActive(false);
        SetAllAreaSpawnersEnabled(false);
        HardClearAllEnemiesAndEnemyBullets();

        if (TimeManager.Instance) TimeManager.Instance.StopTimer();
    }

    /// <summary>
    /// Maneja el fin de juego cuando el boss es derrotado:
    /// desactiva todo y detiene el <see cref="TimeManager"/>.
    /// </summary>
    void HandleBossDefeated()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        CurrentPhase = Phase.End;
        if (bossRoot) bossRoot.SetActive(false);
        SetAllAreaSpawnersEnabled(false);
        HardClearAllEnemiesAndEnemyBullets();

        if (TimeManager.Instance) TimeManager.Instance.StopTimer();
    }
}
