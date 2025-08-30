using System;
using UnityEngine;

/// <summary>
/// Controla a un enemigo que dispara patrones de balas.
/// Administra su ciclo de vida, patrones de disparo, colisiones y notifica
/// al HUD mediante eventos estáticos.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BulletSpawnerEnemy : MonoBehaviour
{
    // ================== EVENTOS Y CONTADORES GLOBALES ==================

    /// <summary>Número de enemigos activos en la escena.</summary>
    public static int ActiveCount { get; private set; }

    /// <summary>Evento disparado cuando cambia el número de enemigos activos.</summary>
    public static event Action<int> OnActiveCountChanged;

    /// <summary>Evento disparado para actualizar contadores globales de enemigos.</summary>
    public static event Action<int> OnEnemyCountChanged;

    /// <summary>Evento disparado cuando un enemigo es destruido por el jugador.</summary>
    public static event Action OnEnemyKilled;

    // ================== CONFIGURACIÓN ==================

    [Header("Disparo - Prefab de bala")]
    /// <summary>Prefab de la bala que dispara este enemigo.</summary>
    public GameObject bulletPrefab;
    /// <summary>Velocidad de las balas disparadas.</summary>
    public float bulletSpeed = 7f;
    /// <summary>Duración máxima de vida de las balas disparadas.</summary>
    public float bulletLife  = 3.5f;

    [Header("Patrones (duración por fase)")]
    /// <summary>Duración del patrón de disparo simple.</summary>
    public float singleDuration = 4f;
    /// <summary>Duración del patrón de abanico.</summary>
    public float fanDuration    = 6f;
    /// <summary>Duración del patrón circular.</summary>
    public float circleDuration = 6f;
    /// <summary>Indica si los patrones deben repetirse en bucle.</summary>
    public bool loopPatterns = true;

    [Header("Fire Rates por patrón")]
    /// <summary>Cadencia de fuego para el patrón simple.</summary>
    public float fireRateSingle = 0.25f;
    /// <summary>Cadencia de fuego para el patrón de abanico.</summary>
    public float fireRateFan    = 0.40f;
    /// <summary>Cadencia de fuego para el patrón circular.</summary>
    public float fireRateCircle = 0.65f;

    [Header("Parámetros de patrón")]
    /// <summary>Ángulo de separación en el patrón abanico.</summary>
    public float fanAngleStep = 12f;
    /// <summary>Número de balas en el patrón circular.</summary>
    public int circleCount  = 20;

    [Header("Vida")]
    /// <summary>Indica si el enemigo puede recibir daño.</summary>
    public bool destructible = true;
    /// <summary>Puntos de vida del enemigo.</summary>
    public int hp = 3;

    [Header("Seguridad")]
    /// <summary>
    /// Margen fuera de la pantalla que provoca que el enemigo se destruya
    /// si sale del viewport.
    /// </summary>
    public float offscreenPadding = 0.20f;

    // ================== ESTADO INTERNO ==================

    /// <summary>Patrones de disparo disponibles.</summary>
    enum Pattern { Single, Fan3, Circle }

    Pattern current = Pattern.Single;
    float patternTimer = 0f;
    float fireTimer = 0f;
    Transform player;
    float aliveTime = 0f;

    /// <summary>Tiempo de gracia tras aparecer, antes de aplicar destrucción fuera de pantalla.</summary>
    public float spawnGrace = 0.75f;

    // ================== UNITY LIFECYCLE ==================

    void Awake()
    {
        // Configurar Rigidbody2D
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Configurar Collider2D
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable()
    {
        // Recolocar en Z = 0
        var p = transform.position;
        transform.position = new Vector3(p.x, p.y, 0f);
        aliveTime = 0f;

        // Contadores
        ActiveCount++;
        OnActiveCountChanged?.Invoke(ActiveCount);
        OnEnemyCountChanged?.Invoke(ActiveCount);

        // Referencia al jugador
        var pl = GameObject.FindWithTag("Player");
        if (pl) player = pl.transform;

        patternTimer = 0f;
        fireTimer = 0f;
        current = Pattern.Single;
    }

    void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        OnActiveCountChanged?.Invoke(ActiveCount);
        OnEnemyCountChanged?.Invoke(ActiveCount);
    }

    void Update()
    {
        aliveTime += Time.deltaTime;

        // Seguridad: destruir si está fuera de la cámara
        if (aliveTime >= spawnGrace && IsOutsideViewport())
        {
            Destroy(gameObject);
            return;
        }

        float dt = Time.deltaTime;
        patternTimer += dt;
        fireTimer    += dt;

        // Cambio de patrón
        if (patternTimer >= GetCurrentPatternDuration())
        {
            patternTimer = 0f;
            if (current == Pattern.Single) current = Pattern.Fan3;
            else if (current == Pattern.Fan3) current = Pattern.Circle;
            else current = loopPatterns ? Pattern.Single : Pattern.Circle;
        }

        // Disparo
        float rate = GetCurrentFireRate();
        if (fireTimer >= rate)
        {
            FireCurrentPattern();
            fireTimer = 0f;
        }
    }

    // ================== PATRONES DE DISPARO ==================

    /// <summary>
    /// Ejecuta el patrón de disparo actual.
    /// </summary>
    void FireCurrentPattern()
    {
        if (!bulletPrefab) return;

        switch (current)
        {
            case Pattern.Single: FireSingleAimed();     break;
            case Pattern.Fan3:   FireFan3Aimed();       break;
            case Pattern.Circle: FireCircleOriented();  break;
        }
    }

    /// <summary>Dispara una bala hacia el jugador.</summary>
    void FireSingleAimed() => ShootAtAngle(AimToPlayerDeg());

    /// <summary>Dispara 3 balas en abanico hacia el jugador.</summary>
    void FireFan3Aimed()
    {
        float baseAng = AimToPlayerDeg();
        ShootAtAngle(baseAng);
        ShootAtAngle(baseAng - fanAngleStep);
        ShootAtAngle(baseAng + fanAngleStep);
    }

    /// <summary>Dispara un círculo completo de balas, orientado hacia el jugador.</summary>
    void FireCircleOriented()
    {
        if (circleCount <= 0) return;

        float baseAng = AimToPlayerDeg();
        float step = 360f / circleCount;

        int nearestIdx = Mathf.RoundToInt(baseAng / step);
        float start = nearestIdx * step;

        for (int i = 0; i < circleCount; i++)
            ShootAtAngle(start + step * i);
    }

    // ================== AUXILIARES ==================

    /// <summary>
    /// Calcula el ángulo en grados hacia el jugador.
    /// </summary>
    float AimToPlayerDeg()
    {
        if (!player) return -90f;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Instancia una bala en un ángulo dado.
    /// </summary>
    void ShootAtAngle(float zDeg)
    {
        Quaternion rot = Quaternion.Euler(0f, 0f, zDeg);
        GameObject go = Instantiate(bulletPrefab, transform.position, rot);
        var b = go.GetComponent<Bullet>();
        if (b)
        {
            b.owner      = Bullet.Owner.Enemy;
            b.speed      = bulletSpeed;
            b.bulletLife = bulletLife;
            b.rotation   = zDeg;
        }
    }

    /// <summary>Devuelve la duración del patrón actual.</summary>
    float GetCurrentPatternDuration()
    {
        return current switch {
            Pattern.Single => singleDuration,
            Pattern.Fan3   => fanDuration,
            _              => circleDuration,
        };
    }

    /// <summary>Devuelve la cadencia de fuego del patrón actual.</summary>
    float GetCurrentFireRate()
    {
        return current switch {
            Pattern.Single => fireRateSingle,
            Pattern.Fan3   => fireRateFan,
            _              => fireRateCircle,
        };
    }

    /// <summary>
    /// Determina si el enemigo se encuentra fuera de la pantalla.
    /// </summary>
    bool IsOutsideViewport()
    {
        var cam = Camera.main;
        if (!cam) return false;
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        float pad = offscreenPadding;
        return (v.x < -pad || v.x > 1f + pad || v.y < -pad || v.y > 1f + pad);
    }

    // ================== COLISIONES ==================

    /// <summary>
    /// Aplica daño cuando recibe una bala del jugador.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!destructible) return;

        var bullet = other.GetComponent<Bullet>();
        if (bullet && bullet.owner == Bullet.Owner.Player)
        {
            hp--;
            if (hp <= 0)
            {
                OnEnemyKilled?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}