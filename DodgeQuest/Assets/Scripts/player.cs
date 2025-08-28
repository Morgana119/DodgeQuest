using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    [Range(0.1f, 1f)] public float slowMultiplier = 0.4f;
    public float clampPadding = 0.2f; // margen para no tocar bordes

    [Header("Disparo")]
    public GameObject playerBulletPrefab;   
    public float fireRate = 0.12f; // menos = más rápido
    public float bulletSpeed = 12f;
    public float bulletLife = 3f;
    public float shootAngle = 90f; // 90° = hacia arriba

    [Header("Cámara")]
    public Camera gameplayCam;
    
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
        ClampToCamera();
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
            b.gameplayCam = (gameplayCam != null) ? gameplayCam : Camera.main;
        }
    }

    void ClampToCamera()
    {
        var cam = (gameplayCam != null) ? gameplayCam : Camera.main;
        if (cam == null) return;

        Vector3 v = cam.WorldToViewportPoint(transform.position);
        v.x = Mathf.Clamp(v.x, 0f + clampPadding, 1f - clampPadding);
        v.y = Mathf.Clamp(v.y, 0f + clampPadding, 1f - clampPadding);
        transform.position = cam.ViewportToWorldPoint(v);
    }
}
