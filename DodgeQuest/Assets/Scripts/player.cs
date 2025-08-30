using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla al jugador: movimiento, disparo y puntos de vida (HP).
/// También gestiona la lógica de daño, curación y muerte.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    /// <summary>Evento disparado cuando la vida del jugador cambia.</summary>
    public static event Action<int> OnPlayerHealthChanged;

    /// <summary>Evento disparado cuando el jugador muere.</summary>
    public static event Action OnPlayerDied;

    [Header("Movimiento")]
    /// <summary>Velocidad base de movimiento del jugador.</summary>
    public float moveSpeed = 6f;

    /// <summary>Multiplicador aplicado al moverse en modo lento.</summary>
    [Range(0.1f, 1f)] public float slowMultiplier = 0.4f;

    [Header("Límites de movimiento")]
    /// <summary>Límite mínimo en eje X.</summary>
    public float minX = -4.5f;
    /// <summary>Límite máximo en eje X.</summary>
    public float maxX = 1.3f;
    /// <summary>Límite mínimo en eje Y.</summary>
    public float minY = -4.5f;
    /// <summary>Límite máximo en eje Y.</summary>
    public float maxY = 2.5f;

    [Header("Disparo")]
    /// <summary>Prefab de la bala que dispara el jugador.</summary>
    public GameObject playerBulletPrefab;

    /// <summary>Tiempo mínimo entre disparos.</summary>
    public float fireRate = 0.12f;

    /// <summary>Velocidad de las balas disparadas.</summary>
    public float bulletSpeed = 12f;

    /// <summary>Duración máxima de vida de cada bala.</summary>
    public float bulletLife = 3f;

    /// <summary>Ángulo en grados en el que se dispara la bala (90° = hacia arriba).</summary>
    public float shootAngle = 90f; 

    [Header("HP")]
    /// <summary>HP inicial del jugador al comenzar la partida.</summary>
    public int startHP = 30;

    /// <summary>HP actual del jugador (sin límite máximo).</summary>
    public int currentHP;

    /// <summary>Tiempo de invulnerabilidad tras recibir daño.</summary>
    public float hitInvuln = 0.5f; 

    // --- Controles ---
    private InputAction moveAction;
    private InputAction fireAction;
    private InputAction slowAction;

    // --- Estado interno ---
    float fireTimer;
    float lastHitTime = -999f;

    /// <summary>
    /// Configura los bindings de teclado para movimiento, disparo y modo lento.
    /// También ajusta Rigidbody2D y Collider2D.
    /// </summary>
    void Awake()
    {
        // Movimiento: WASD y flechas
        moveAction = new InputAction("Move");
        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w");
        wasd.With("Down", "<Keyboard>/s");
        wasd.With("Left", "<Keyboard>/a");
        wasd.With("Right", "<Keyboard>/d");

        var arrows = moveAction.AddCompositeBinding("2DVector");
        arrows.With("Up", "<Keyboard>/upArrow");
        arrows.With("Down", "<Keyboard>/downArrow");
        arrows.With("Left", "<Keyboard>/leftArrow");
        arrows.With("Right", "<Keyboard>/rightArrow");

        // Disparo: Z o Space
        fireAction = new InputAction("Fire");
        fireAction.AddBinding("<Keyboard>/z");
        fireAction.AddBinding("<Keyboard>/space");

        // Movimiento lento: Shift
        slowAction = new InputAction("Slow");
        slowAction.AddBinding("<Keyboard>/leftShift");
        slowAction.AddBinding("<Keyboard>/rightShift");

        // Rigidbody y Collider
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Inicializa el HP del jugador y suscribe eventos al habilitarse.
    /// </summary>
    void OnEnable()
    {
        moveAction.Enable();
        fireAction.Enable();
        slowAction.Enable();

        currentHP = startHP;
        OnPlayerHealthChanged?.Invoke(currentHP);

        BulletSpawnerEnemy.OnEnemyKilled += HandleEnemyKilledHeal;
    }

    /// <summary>
    /// Limpia las suscripciones y desactiva controles al deshabilitarse.
    /// </summary>
    void OnDisable()
    {
        BulletSpawnerEnemy.OnEnemyKilled -= HandleEnemyKilledHeal;
        
        fireAction.Disable();
        slowAction.Disable();
        moveAction.Disable();
    }

    /// <summary>
    /// Lógica de curación: cada vez que muere un enemigo se recupera HP.
    /// </summary>
    void HandleEnemyKilledHeal()
    {
        Heal(2);
    }

    /// <summary>
    /// Lógica principal de actualización (movimiento y disparo).
    /// </summary>
    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    /// <summary>
    /// Gestiona el movimiento del jugador dentro de los límites del mundo.
    /// </summary>
    void HandleMovement()
    {
        Vector2 dir = moveAction.ReadValue<Vector2>().normalized;
        bool slow = slowAction.IsPressed();
        float speed = moveSpeed * (slow ? slowMultiplier : 1f);

        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        ClampToWorldLimits();
    }

    /// <summary>
    /// Controla la cadencia de disparo del jugador.
    /// </summary>
    void HandleShooting()
    {
        fireTimer += Time.deltaTime;
        if (!fireAction.IsPressed()) return;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            ShootOne();
        }
    }

    /// <summary>
    /// Instancia una bala disparada por el jugador.
    /// </summary>
    void ShootOne()
    {
        if (playerBulletPrefab == null) return;

        Quaternion rot = Quaternion.Euler(0f, 0f, shootAngle);
        GameObject go = Instantiate(playerBulletPrefab, transform.position, rot);

        var b = go.GetComponent<Bullet>();
        if (b != null)
        {
            b.owner       = Bullet.Owner.Player;
            b.speed       = bulletSpeed;
            b.bulletLife  = bulletLife;
            b.rotation    = rot.eulerAngles.z;
        }
    }

    /// <summary>
    /// Restringe la posición del jugador a los límites de la cámara.
    /// </summary>
    void ClampToWorldLimits()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    /// <summary>
    /// Detecta colisiones con balas enemigas y aplica daño.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        var bullet = other.GetComponent<Bullet>();
        if (bullet != null && bullet.owner == Bullet.Owner.Enemy)
        {
            TryTakeDamage(1);
        }
    }

    /// <summary>
    /// Aplica daño al jugador respetando la invulnerabilidad tras ser golpeado.
    /// </summary>
    /// <param name="dmg">Cantidad de daño recibido.</param>
    public void TryTakeDamage(int dmg)
    {
        if (Time.time - lastHitTime < hitInvuln) return;
        lastHitTime = Time.time;

        currentHP = Mathf.Max(0, currentHP - dmg);
        OnPlayerHealthChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Cura al jugador en una cantidad específica de HP.
    /// No hay límite superior de vida.
    /// </summary>
    /// <param name="amount">Cantidad de HP a recuperar.</param>
    public void Heal(int amount)
    {
        currentHP += amount;
        OnPlayerHealthChanged?.Invoke(currentHP);
    }
}