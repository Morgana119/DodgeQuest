using System;
using UnityEngine;

/// <summary>
/// Gestiona el tiempo global del juego.
/// Permite iniciar, detener y reiniciar el cronómetro, además de notificar a otros sistemas mediante eventos.
/// Implementa el patrón Singleton para facilitar su acceso desde cualquier parte del juego.
/// </summary>
public class TimeManager : MonoBehaviour
{
    /// <summary>Instancia única (singleton) del <see cref="TimeManager"/> en escena.</summary>
    public static TimeManager Instance { get; private set; }

    [Header("Control")]
    /// <summary>
    /// Si es true, el temporizador comenzará automáticamente al iniciar la escena.
    /// </summary>
    public bool autoStart = true;

    /// <summary>
    /// Tiempo transcurrido desde que se inició el temporizador, en segundos.
    /// </summary>
    public float elapsed { get; private set; } = 0f;

    /// <summary>
    /// Indica si el temporizador está corriendo actualmente.
    /// </summary>
    public bool running { get; private set; } = false;

    /// <summary>
    /// Evento que se dispara cada frame mientras el temporizador está corriendo.
    /// Entrega como parámetro el tiempo total transcurrido.
    /// </summary>
    public static event Action<float> OnTick;

    /// <summary>
    /// Configura el Singleton. Si ya existe otra instancia, se destruye este objeto.
    /// </summary>
    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Inicia el temporizador automáticamente si <see cref="autoStart"/> es true.
    /// </summary>
    void Start()
    {
        if (autoStart) StartTimer();
    }

    /// <summary>
    /// Incrementa el tiempo si el temporizador está activo e invoca el evento <see cref="OnTick"/>.
    /// </summary>
    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        OnTick?.Invoke(elapsed);
    }

    /// <summary>
    /// Comienza o reanuda el temporizador desde el tiempo actual.
    /// </summary>
    public void StartTimer() { running = true; }

    /// <summary>
    /// Detiene el temporizador sin reiniciar el tiempo acumulado.
    /// </summary>
    public void StopTimer()  { running = false; }

    /// <summary>
    /// Reinicia el tiempo acumulado a 0 pero no inicia el temporizador automáticamente.
    /// </summary>
    public void ResetTimer() { elapsed = 0f; }

    /// <summary>
    /// Reinicia el tiempo acumulado a 0 e inicia el temporizador automáticamente.
    /// </summary>
    public void ResetAndStart()
    {
        elapsed = 0f;
        running = true;
    }
}