using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BulletSpawnerEnemy : MonoBehaviour
{
    public static int ActiveCount { get; private set; }
    public static event Action<int> OnActiveCountChanged;
    public static event Action<int> OnEnemyCountChanged;

    [Header("Disparo - Prefab de bala")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 7f;
    public float bulletLife  = 3.5f;

    [Header("Patrones (duración por fase)")]
    public float singleDuration = 4f;
    public float fanDuration    = 6f;
    public float circleDuration = 6f;
    public bool  loopPatterns   = true;

    [Header("Fire Rates por patrón")]
    public float fireRateSingle = 0.25f;
    public float fireRateFan    = 0.40f;
    public float fireRateCircle = 0.65f;

    [Header("Parámetros de patrón")]
    public float fanAngleStep = 12f;
    public int   circleCount  = 20;

    [Header("Vida")]
    public bool destructible = true;
    public int  hp = 3;

    [Header("Seguridad")]
    public float offscreenPadding = 0.20f;

    enum Pattern { Single, Fan3, Circle }
    Pattern current = Pattern.Single;
    float   patternTimer = 0f;
    float   fireTimer    = 0f;
    Transform player;
    bool cullHookSet = false;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable()
    {
        var p = transform.position;
        transform.position = new Vector3(p.x, p.y, 0f);

        ActiveCount++;
        OnActiveCountChanged?.Invoke(ActiveCount);
        OnEnemyCountChanged?.Invoke(ActiveCount);

        var pl = GameObject.FindWithTag("Player");
        if (pl) player = pl.transform;

        patternTimer = 0f;
        fireTimer = 0f;
        current = Pattern.Single;
        
        if (!cullHookSet)
        {
            LevelController.OnCullAllEnemies += HandleCullAll;
            cullHookSet = true;
        }
    }

    void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        OnActiveCountChanged?.Invoke(ActiveCount);
        OnEnemyCountChanged?.Invoke(ActiveCount);

        if (cullHookSet)
        {
            LevelController.OnCullAllEnemies -= HandleCullAll;
            cullHookSet = false;
        }
    }

    void Update()
    {
        if (IsOutsideViewport())
        {
            Destroy(gameObject);
            return;
        }

        float dt = Time.deltaTime;
        patternTimer += dt;
        fireTimer    += dt;

        if (patternTimer >= GetCurrentPatternDuration())
        {
            patternTimer = 0f;
            if (current == Pattern.Single) current = Pattern.Fan3;
            else if (current == Pattern.Fan3) current = Pattern.Circle;
            else current = loopPatterns ? Pattern.Single : Pattern.Circle;
        }

        float rate = GetCurrentFireRate();
        if (fireTimer >= rate)
        {
            FireCurrentPattern();
            fireTimer = 0f;
        }
    }

    void FireCurrentPattern()
    {
        if (!bulletPrefab) return;

        switch (current)
        {
            case Pattern.Single: FireSingleAimed(); break;
            case Pattern.Fan3:   FireFan3Aimed();   break;
            case Pattern.Circle: FireCircleOriented(); break;
        }
    }

    void FireSingleAimed()
    {
        ShootAtAngle(AimToPlayerDeg());
    }

    void FireFan3Aimed()
    {
        float baseAng = AimToPlayerDeg();
        ShootAtAngle(baseAng);
        ShootAtAngle(baseAng - fanAngleStep);
        ShootAtAngle(baseAng + fanAngleStep);
    }

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

    float AimToPlayerDeg()
    {
        if (!player) return -90f;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

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

    float GetCurrentPatternDuration()
    {
        return current switch {
            Pattern.Single => singleDuration,
            Pattern.Fan3   => fanDuration,
            _              => circleDuration,
        };
    }

    float GetCurrentFireRate()
    {
        return current switch {
            Pattern.Single => fireRateSingle,
            Pattern.Fan3   => fireRateFan,
            _              => fireRateCircle,
        };
    }

    bool IsOutsideViewport()
    {
        var cam = Camera.main;
        if (!cam) return false;
        Vector3 v = cam.WorldToViewportPoint(transform.position);
        float pad = offscreenPadding;
        return (v.x < -pad || v.x > 1f + pad || v.y < -pad || v.y > 1f + pad);
    }

    // Daño por balas del player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!destructible) return;

        var bullet = other.GetComponent<Bullet>();
        if (bullet && bullet.owner == Bullet.Owner.Player)
        {
            hp--;
            if (hp <= 0) Destroy(gameObject);
        }
    }

    void HandleCullAll()
    {
        if (this) Destroy(gameObject);
    }
}
