using UnityEngine;

public class LevelController : MonoBehaviour
{
    public enum Phase { None, Phase1, Boss, End }
    public static System.Action OnCullAllEnemies;
    
    [Header("Spawners de Fase 1 (AreaEnemy)")]
    public AreaEnemy[] spawners;

    [Header("Temporización exacta (segundos)")]
    public float phase1EndAt = 30f;
    public float bossDelay   = 1.0f;
    public float gameEndAt   = 60f;

    [Header("Transición")]
    public bool clearOnSwitch = true;

    [Header("Boss (Fase 2)")]
    public GameObject bossRoot;

    [Header("Debug")]
    public bool logTransitions = true;

    public Phase CurrentPhase { get; private set; } = Phase.None;

    bool phase1Started, bossTransitionDone, bossShown, gameEnded;
    float bossEnableAt = -1f;

    void Start()
    {
        SetSpawnersEnabled(false);
        if (bossRoot) bossRoot.SetActive(false);

        if (!TimeManager.Instance) gameObject.AddComponent<TimeManager>();
        TimeManager.Instance.ResetAndStart();
    }

    void Update()
    {
        float t = TimeManager.Instance ? TimeManager.Instance.elapsed : 0f;

        if (!phase1Started && t >= 0f)
        {
            phase1Started = true;
            CurrentPhase = Phase.Phase1;
            SetSpawnersEnabled(true);
            if (logTransitions) Debug.Log("[Level] Phase 1 ON");
        }

        if (!bossTransitionDone && t >= phase1EndAt)
        {
            bossTransitionDone = true;
            CurrentPhase = Phase.Boss;

            SetSpawnersEnabled(false);

            if (clearOnSwitch) ClearRemaining();

            bossEnableAt = t + Mathf.Max(0f, bossDelay);
            if (logTransitions) Debug.Log($"[Level] Boss transition @ {t:F2}s, will show at {bossEnableAt:F2}s");
        }

        if (!bossShown && bossEnableAt > 0f && t >= bossEnableAt)
        {
            bossShown = true;
            if (bossRoot) bossRoot.SetActive(true);
            if (logTransitions) Debug.Log($"[Level] Boss ON @ {t:F2}s");
        }

        if (!gameEnded && t >= gameEndAt)
        {
            gameEnded = true;
            CurrentPhase = Phase.End;

            if (bossRoot) bossRoot.SetActive(false);
            SetSpawnersEnabled(false);
            ClearRemaining();

            TimeManager.Instance.StopTimer();
            if (logTransitions) Debug.Log($"[Level] GAME OVER @ {t:F2}s");
        }
    }

    void SetSpawnersEnabled(bool enabled)
    {
        if (spawners != null)
            foreach (var s in spawners)
                if (s) s.gameObject.SetActive(enabled);
    }

    void ClearRemaining()
    {
        OnCullAllEnemies?.Invoke();

        int cleared = 0;
    }
}
