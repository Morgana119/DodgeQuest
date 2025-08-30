using UnityEngine;

public class LevelController : MonoBehaviour
{
    public enum Phase { None, Phase1, Boss, End }
    bool gameOverTriggered = false;

    [Header("Spawners de Fase 1 (AreaEnemy)")]
    public AreaEnemy[] spawners;

    [Header("Temporización exacta (segundos)")]
    public float phase1EndAt = 30f;
    public float bossDelay   = 1.0f; 
    public float gameEndAt   = 60f;   

    [Header("Boss (Fase 2)")]
    public GameObject bossRoot;

    [Header("Debug")]
    public bool logTransitions = true;

    public Phase CurrentPhase { get; private set; } = Phase.None;

    bool phase1Started, bossTransitionDone, bossShown, gameEnded;
    float bossEnableAt = -1f;

    void Start()
    {
        // Timer global
        if (!TimeManager.Instance) gameObject.AddComponent<TimeManager>();
        TimeManager.Instance.ResetAndStart();

        // Arranca en fase 1
        CurrentPhase        = Phase.Phase1;
        phase1Started       = true;
        bossShown           = false;
        bossTransitionDone  = false;
        gameEnded           = false;

        SetAllAreaSpawnersEnabled(true);
        if (bossRoot) bossRoot.SetActive(false);

        Player.OnPlayerDied += HandlePlayerDied;
        BulletSpawnerBoss.OnBossDefeated += HandleBossDefeated;
    }

    void OnDestroy()
    {
        Player.OnPlayerDied -= HandlePlayerDied;
        BulletSpawnerBoss.OnBossDefeated -= HandleBossDefeated;
    }

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

    void SetAllAreaSpawnersEnabled(bool enabled)
    {
        if (spawners != null)
            foreach (var s in spawners)
                if (s) s.gameObject.SetActive(enabled);

        var all = FindObjectsByType<AreaEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var s in all)
            if (s) s.gameObject.SetActive(enabled);
    }

    // Destruye enemigos y balas enemigas visibles
    void HardClearAllEnemiesAndEnemyBullets()
    {
        int cleared = 0;

        // Enemigos normales
        var enemies = FindObjectsByType<BulletSpawnerEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e)
            {
                Destroy(e.gameObject);
                cleared++;
            }
        }

        // Balas enemigas
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

    // Fin por muerte del boss
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
