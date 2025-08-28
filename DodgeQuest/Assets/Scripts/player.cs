using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    [Range(0.1f, 1f)] public float slowMultiplier = 0.4f;

    [Header("Límites de movimiento")]
    public float minX = -4.5f;
    public float maxX =  1.3f;
    public float minY = -4.5f;
    public float maxY =  2.5f;

    [Header("Disparo")]
    public GameObject playerBulletPrefab;
    public float fireRate = 0.12f;
    public float bulletSpeed = 12f;
    public float bulletLife = 3f;
    public float shootAngle = 90f; // 90° = hacia arriba
    
    private InputAction moveAction;
    private InputAction fireAction;
    private InputAction slowAction;

    float fireTimer;

    void Awake()
    {
        // Movimiento: WASD + Flechas
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

        // Modo lento: Shift
        slowAction = new InputAction("Slow");
        slowAction.AddBinding("<Keyboard>/leftShift");
        slowAction.AddBinding("<Keyboard>/rightShift");
    }

    void OnEnable()
    {
        moveAction.Enable();
        fireAction.Enable();
        slowAction.Enable();
    }

    void OnDisable()
    {
        fireAction.Disable();
        slowAction.Disable();
        moveAction.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        Vector2 dir = moveAction.ReadValue<Vector2>().normalized;
        bool slow = slowAction.IsPressed();
        float speed = moveSpeed * (slow ? slowMultiplier : 1f);

        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        ClampToWorldLimits();

    }

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

    void ClampToWorldLimits()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
