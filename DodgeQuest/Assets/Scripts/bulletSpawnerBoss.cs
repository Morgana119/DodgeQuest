using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el comportamiento del jefe (Boss).
/// Se encarga de sus patrones de disparo, su ciclo de vida (HP),
/// y de notificar cuando ha sido derrotado.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BulletSpawnerBoss : MonoBehaviour
{
    /// <summary>
    /// Evento que se dispara cuando cambia la vida del Boss.
    /// Parámetros: vida actual, vida máxima.
    /// </summary>
    public static event Action<int, int> OnBossHealthChanged;

    /// <summary>
    /// Evento que se dispara cuando el Boss ha sido derrotado.
    /// </summary>
    public static event Action OnBossDefeated;

    [Header("HP (Boss)")]
    /// <summary>Vida máxima del Boss.</summary>
    public int maxHP = 120;
    /// <summary>Vida actual del Boss.</summary>
    public int currentHP;

    /// <summary>
    /// Tipos de patrones de disparo del Boss.
    /// </summary>
    public enum SpawnerType { Fan, CircleBurst, RoseStar }

    [Header("Bullet Attributes")]
    /// <summary>Prefab de la bala que dispara el Boss.</summary>
    public GameObject bullet;
    /// <summary>Tiempo de vida de las balas.</summary>
    public float bulletLife = 3f;
    /// <summary>Velocidad de las balas.</summary>
    public float speed = 6f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType = SpawnerType.CircleBurst;
    [SerializeField] private float firingRate = 0.08f;

    [Header("Pattern Switching")]
    /// <summary>Tiempo (segundos) que dura cada patrón antes de cambiar.</summary>
    public float switchInterval = 10f;
    /// <summary>Ciclo de patrones que seguirá el Boss.</summary>
    public SpawnerType[] cycle = new SpawnerType[] { SpawnerType.Fan, SpawnerType.CircleBurst, SpawnerType.RoseStar };
    /// <summary>Si es true, limpia todas las balas al cambiar de patrón.</summary>
    public bool clearOnSwitch = false;

    [Header("Orientación global")]
    /// <summary>Offset en grados para alinear disparos. (-90 apunta hacia abajo).</summary>
    public float angleOffset = -90f;

    [Header("Fan")]
    public int fanCount = 6;
    public float fanSpread = 160f;
    public float fanSwayAmplitude = 25f;
    public float fanSwaySpeed = 0.5f;

    [Header("Circle Burst")]
    public int circleCount = 20;
    public float circleFiringRate = 0.25f;

    [Header("Rose / Star")]
    public int rosePetals = 6;
    [Range(0f, 1f)] public float roseRadiusAmp = 0.273f;
    public float roseBaseRadius = 1.6f;
    public int roseSamples = 35;
    public float roseSpin = 140f;
    public float roseFiringRate = 0.3f;
    public int roseRingsPerTick = 1;

    // ===== Variables internas =====
    private float timer;
    private int cycleIndex;
    private float rosePhaseDeg;
    private Coroutine switcher;
    private bool isActive = false;

    /// <summary>
    /// Configura el Boss al iniciar (collider como trigger).
    /// </summary>
    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Inicializa la vida y arranca el ciclo de patrones.
    /// </summary>
    void OnEnable()
    {
        currentHP = Mathf.Max(1, maxHP);
        OnBossHealthChanged?.Invoke(currentHP, maxHP);

        timer = 0f;
        cycleIndex = 0;
        isActive = true;

        if (cycle != null && cycle.Length > 0)
        {
            SetPattern(cycle[cycleIndex]);
            switcher = StartCoroutine(PatternSwitcher());
        }

        Player.OnPlayerDied += HandlePlayerDiedStop;
    }

    /// <summary>
    /// Limpia corrutinas y desuscripciones al desactivar.
    /// </summary>
    void OnDisable()
    {
        isActive = false;

        if (switcher != null) StopCoroutine(switcher);
        switcher = null;

        Player.OnPlayerDied -= HandlePlayerDiedStop;
    }

    /// <summary>
    /// Maneja la lógica de disparo del Boss en cada frame.
    /// </summary>
    void Update()
    {
        if (!isActive) return;

        if (spawnerType == SpawnerType.RoseStar)
            rosePhaseDeg += roseSpin * Time.deltaTime;

        float currentRate =
            (spawnerType == SpawnerType.CircleBurst) ? circleFiringRate :
            (spawnerType == SpawnerType.RoseStar)    ? roseFiringRate   :
                                                        firingRate;

        timer += Time.deltaTime;
        if (timer >= currentRate)
        {
            Fire();
            timer = 0f;
        }
    }

    /// <summary>
    /// Corrutina que alterna los patrones de disparo del Boss.
    /// </summary>
    IEnumerator PatternSwitcher()
    {
        if (cycle == null || cycle.Length == 0) yield break;

        while (isActive)
        {
            yield return new WaitForSeconds(switchInterval);
            if (!isActive) yield break;

            cycleIndex = (cycleIndex + 1) % cycle.Length;
            SetPattern(cycle[cycleIndex]);
        }
    }

    /// <summary>
    /// Cambia el patrón de disparo actual.
    /// </summary>
    private void SetPattern(SpawnerType next)
    {
        spawnerType = next;
        timer = 0f;

        if (spawnerType == SpawnerType.RoseStar)
            rosePhaseDeg = 0f;

        if (clearOnSwitch) ClearExistingBullets();
    }

    /// <summary>
    /// Ejecuta el disparo según el patrón activo.
    /// </summary>
    private void Fire()
    {
        if (!bullet || !isActive) return;

        switch (spawnerType)
        {
            case SpawnerType.Fan: FireFan(); break;
            case SpawnerType.CircleBurst: FireCircleBurst(); break;
            case SpawnerType.RoseStar: FireRoseStar(); break;
        }
    }

    // ===== PATRONES DE DISPARO =====

    /// <summary>
    /// Disparo en abanico (fan) con efecto de oscilación (sway).
    /// </summary>
    private void FireFan()
    {
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * fanSwaySpeed) * fanSwayAmplitude;

        if (fanCount <= 1)
        {
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angleOffset + sway);
            SpawnBullet(transform.position, rot);
            return;
        }

        float step = fanSpread / (fanCount - 1);
        float start = -fanSpread * 0.5f;

        for (int i = 0; i < fanCount; i++)
        {
            float angle = start + step * i + angleOffset + sway;
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(transform.position, rot);
        }
    }

    /// <summary>
    /// Disparo circular completo (circle burst).
    /// </summary>
    private void FireCircleBurst()
    {
        if (circleCount <= 0) return;

        float step = 360f / circleCount;
        for (int i = 0; i < circleCount; i++)
        {
            float angle = step * i + angleOffset;
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(transform.position, rot);
        }
    }

    /// <summary>
    /// Disparo en patrón de "flor/estrella" basado en curvas sinusoidales.
    /// </summary>
    private void FireRoseStar()
    {
        if (roseSamples <= 0) return;

        float step = 360f / roseSamples;

        for (int ring = 0; ring < Mathf.Max(1, roseRingsPerTick); ring++)
        {
            for (int i = 0; i < roseSamples; i++)
            {
                float theta = i * step + rosePhaseDeg;
                float r     = roseBaseRadius * (1f + roseRadiusAmp *
                                Mathf.Sin(rosePetals * theta * Mathf.Deg2Rad));

                float shotAngle = angleOffset + theta;
                Quaternion rot  = transform.rotation * Quaternion.Euler(0f, 0f, shotAngle);

                Vector3 dir = rot * Vector3.right;
                Vector3 pos = transform.position + dir * r;

                SpawnBullet(pos, rot);
            }
        }
    }

    /// <summary>
    /// Instancia una bala en la posición y rotación indicadas.
    /// </summary>
    private void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(bullet, pos, rot);
        var b = go.GetComponent<Bullet>();
        if (b)
        {
            b.owner = Bullet.Owner.Enemy;
            b.speed = speed;
            b.bulletLife = bulletLife;
            b.rotation = rot.eulerAngles.z;
        }
    }

    /// <summary>
    /// Elimina todas las balas enemigas en pantalla.
    /// </summary>
    private void ClearExistingBullets()
    {
        var all = FindObjectsByType<Bullet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var bb in all)
        {
            if (bb != null && bb.owner == Bullet.Owner.Enemy)
                Destroy(bb.gameObject);
        }
    }

    /// <summary>
    /// Aplica daño al Boss cuando recibe una bala del jugador.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        var bullet = other.GetComponent<Bullet>();
        if (bullet != null && bullet.owner == Bullet.Owner.Player)
        {
            currentHP = Mathf.Max(0, currentHP - 1);
            OnBossHealthChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0)
            {
                OnBossDefeated?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Maneja la lógica de detener al Boss si el jugador muere.
    /// </summary>
    void HandlePlayerDiedStop()
    {
        isActive = false; 
        ClearExistingBullets();
    }
}