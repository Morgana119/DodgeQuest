using System;
using UnityEngine;

/// <summary>
/// Representa una bala en el juego. 
/// Puede pertenecer al jugador o a los enemigos.
/// Controla su movimiento, destrucción y contadores globales.
/// </summary>
public class Bullet : MonoBehaviour
{
    /// <summary>
    /// Indica el dueño de la bala.
    /// </summary>
    public enum Owner { Enemy, Player }

    // ================= CONTADORES GLOBALES =================

    /// <summary>
    /// Número total de balas activas en la escena.
    /// </summary>
    public static int ActiveCount { get; private set; }

    /// <summary>
    /// Número de balas activas disparadas por el jugador.
    /// </summary>
    public static int ActivePlayerCount { get; private set; }

    /// <summary>
    /// Número de balas activas disparadas por los enemigos.
    /// </summary>
    public static int ActiveEnemyCount { get; private set; }

    /// <summary>
    /// Evento que se dispara cuando cambia la cantidad total de balas activas.
    /// </summary>
    public static event Action<int> OnActiveCountChanged;

    /// <summary>
    /// Evento que se dispara cuando cambian los contadores de balas (total, jugador, enemigo).
    /// </summary>
    public static event Action<int, int, int> OnCountsChanged;

    // ================= ATRIBUTOS =================

    [Header("Bullet Attributes")]
    /// <summary>
    /// Dueño de la bala (jugador o enemigo).
    /// </summary>
    public Owner owner = Owner.Enemy;

    /// <summary>
    /// Tiempo máximo de vida de la bala, en segundos.
    /// </summary>
    public float bulletLife = 3f;

    /// <summary>
    /// Rotación inicial de la bala en grados.
    /// </summary>
    public float rotation = 0f;

    /// <summary>
    /// Velocidad de movimiento de la bala.
    /// </summary>
    public float speed = 1f;

    [Header("Viewport Kill")]
    /// <summary>
    /// Margen adicional fuera de la pantalla. 
    /// Si la bala se sale del viewport más este margen, se destruye.
    /// </summary>
    [Tooltip("Margen extra fuera de pantalla (0.1 = 10% del viewport)")]
    public float offscreenPadding = 0.12f;

    // ================= VARIABLES INTERNAS =================

    private Vector2 spawnPoint;
    private float timer = 0f;

    // ================= MÉTODOS DE UNITY =================

    /// <summary>
    /// Se llama cuando la bala se habilita en la escena.
    /// Incrementa los contadores globales y notifica eventos.
    /// </summary>
    void OnEnable()
    {
        ActiveCount++;
        if (owner == Owner.Player) ActivePlayerCount++;
        else ActiveEnemyCount++;

        OnActiveCountChanged?.Invoke(ActiveCount);
        OnCountsChanged?.Invoke(ActiveCount, ActivePlayerCount, ActiveEnemyCount);

        timer = 0f;
    }

    /// <summary>
    /// Se llama cuando la bala se destruye o desactiva.
    /// Decrementa los contadores globales y notifica eventos.
    /// </summary>
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

    /// <summary>
    /// Inicializa la posición de spawn y la rotación de la bala.
    /// </summary>
    void Start()
    {
        spawnPoint = transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    /// <summary>
    /// Actualiza el movimiento y gestiona destrucción por tiempo o por salir de la pantalla.
    /// </summary>
    void Update()
    {
        transform.position = Movement(timer);

        // Destrucción por tiempo
        timer += Time.deltaTime;
        if (timer >= bulletLife)
        {
            Destroy(gameObject);
            return;
        }

        // Destrucción por salir del viewport
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

    // ================= MÉTODOS AUXILIARES =================

    /// <summary>
    /// Calcula la posición de la bala en función del tiempo transcurrido.
    /// </summary>
    /// <param name="t">Tiempo desde que se creó la bala.</param>
    /// <returns>Posición calculada.</returns>
    private Vector2 Movement(float t)
    {
        float x = t * speed * transform.right.x;
        float y = t * speed * transform.right.y;
        return new Vector2(x + spawnPoint.x, y + spawnPoint.y);
    }
}