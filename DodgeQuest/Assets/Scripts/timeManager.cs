using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Control")]
    public bool autoStart = true;
    public float elapsed { get; private set; } = 0f;
    public bool running { get; private set; } = false;

    public static event Action<float> OnTick;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (autoStart) StartTimer();
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        OnTick?.Invoke(elapsed);
    }

    public void StartTimer() { running = true; }
    public void StopTimer()  { running = false; }
    public void ResetTimer() { elapsed = 0f; }

    public void ResetAndStart()
    {
        elapsed = 0f;
        running = true;
    }
}
