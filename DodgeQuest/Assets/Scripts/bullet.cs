using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum Owner { Enemy, Player }

    // CONTADORES
    public static int ActiveCount { get; private set; }           
    public static int ActivePlayerCount { get; private set; }
    public static int ActiveEnemyCount  { get; private set; }

    public static event Action<int> OnActiveCountChanged;        
    public static event Action<int,int,int> OnCountsChanged;

    [Header("Bullet Attributes")]
    public Owner owner = Owner.Enemy;
    public float bulletLife = 3f; // backup: tiempo máximo de vida
    public float rotation = 0f;
    public float speed = 1f;

    [Header("Viewport Kill")]
    [Tooltip("Margen extra fuera de pantalla (0.1 = 10% del viewport)")]
    public float offscreenPadding = 0.12f;

    private Vector2 spawnPoint;
    private float timer = 0f;

    void OnEnable()
    {
        ActiveCount++;
        if (owner == Owner.Player) ActivePlayerCount++;
        else ActiveEnemyCount++;

        OnActiveCountChanged?.Invoke(ActiveCount);
        OnCountsChanged?.Invoke(ActiveCount, ActivePlayerCount, ActiveEnemyCount);

        timer = 0f;
    }

    void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        if (owner == Owner.Player)
            ActivePlayerCount = Mathf.Max(0, ActivePlayerCount - 1);
        else
            ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);

        OnActiveCountChanged?.Invoke(ActiveCount);
        OnCountsChanged?.Invoke(ActiveCount, ActivePlayerCount, ActiveEnemyCount);
    }

    void Start()
    {
        spawnPoint = transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    void Update()
    {
        transform.position = Movement(timer);

        // Backup por tiempo
        timer += Time.deltaTime;
        if (timer >= bulletLife)
        {
            Destroy(gameObject);
            return;
        }

        // Destrucción por viewport
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 v = cam.WorldToViewportPoint(transform.position);
            float pad = offscreenPadding;

            if (v.x < -pad || v.x > 1f + pad ||
                v.y < -pad || v.y > 1f + pad)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private Vector2 Movement(float t)
    {
        float x = t * speed * transform.right.x;
        float y = t * speed * transform.right.y;
        return new Vector2(x + spawnPoint.x, y + spawnPoint.y);
    }
}