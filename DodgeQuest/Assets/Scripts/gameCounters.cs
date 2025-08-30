using TMPro;
using UnityEngine;

/// <summary>
/// Muestra en pantalla los contadores de juego en la UI.
/// Incluye: balas activas, enemigos activos, HP del jugador,
/// HP del boss y el tiempo de partida.
/// </summary>
public class GameCountersUI : MonoBehaviour
{
    /// <summary>Etiqueta de texto donde se imprimen los valores.</summary>
    [SerializeField] private TMP_Text label;

    // --- Estado interno ---
    int bulletsTotal, bulletsPlayer, bulletsEnemy;
    int enemiesActive;
    int playerHP;
    int bossHP, bossMax;

    /// <summary>
    /// Suscribe los eventos de balas, enemigos, jugador y boss.
    /// Se ejecuta automáticamente cuando el objeto se habilita.
    /// </summary>
    void OnEnable()
    {
        UpdateBullets(Bullet.ActiveCount, Bullet.ActivePlayerCount, Bullet.ActiveEnemyCount);
        UpdateEnemies(BulletSpawnerEnemy.ActiveCount);

        Bullet.OnCountsChanged += UpdateBullets;
        BulletSpawnerEnemy.OnEnemyCountChanged += UpdateEnemies;

        Player.OnPlayerHealthChanged += UpdatePlayerHP;
        BulletSpawnerBoss.OnBossHealthChanged += UpdateBossHP;
    }

    /// <summary>
    /// Se ejecuta al deshabilitar el objeto, 
    /// eliminando las suscripciones a eventos.
    /// </summary>
    void OnDisable()
    {
        Bullet.OnCountsChanged -= UpdateBullets;
        BulletSpawnerEnemy.OnEnemyCountChanged -= UpdateEnemies;

        Player.OnPlayerHealthChanged -= UpdatePlayerHP;
        BulletSpawnerBoss.OnBossHealthChanged -= UpdateBossHP;
    }

    /// <summary>
    /// Actualiza la interfaz en cada frame.
    /// </summary>
    void Update()
    {
        Redraw();
    }

    /// <summary>
    /// Actualiza el estado de los contadores de balas.
    /// </summary>
    /// <param name="total">Balas totales activas.</param>
    /// <param name="player">Balas disparadas por el jugador.</param>
    /// <param name="enemy">Balas disparadas por los enemigos.</param>
    void UpdateBullets(int total, int player, int enemy)
    {
        bulletsTotal = total;
        bulletsPlayer = player;
        bulletsEnemy = enemy;
    }

    /// <summary>
    /// Actualiza el número de enemigos activos en pantalla.
    /// </summary>
    /// <param name="count">Cantidad de enemigos activos.</param>
    void UpdateEnemies(int count)
    {
        enemiesActive = count;
    }

    /// <summary>
    /// Actualiza el HP del jugador.
    /// </summary>
    /// <param name="hp">HP actual del jugador.</param>
    void UpdatePlayerHP(int hp)
    {
        playerHP = hp;
    }

    /// <summary>
    /// Actualiza el HP del boss.
    /// </summary>
    /// <param name="hp">HP actual del boss.</param>
    /// <param name="max">HP máximo del boss.</param>
    void UpdateBossHP(int hp, int max)
    {
        bossHP = hp;
        bossMax = max;
    }

    /// <summary>
    /// Redibuja los valores en pantalla.
    /// Incluye tiempo, balas, enemigos y vida.
    /// </summary>
    void Redraw()
    {
        if (!label) return;

        string timeStr = "";
        if (TimeManager.Instance)
        {
            float t = TimeManager.Instance.elapsed;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            timeStr = $"\nTime: {m:00}:{s:00}";
        }

        string hpPlayerStr = $"Player HP: {playerHP}";
        string hpBossStr   = (bossMax > 0) ? $"Boss HP: {bossHP}/{bossMax}"
                                            : "Boss HP: --";

        label.text =
            $"Active Bullets: {bulletsTotal}\n" +
            $"Player: {bulletsPlayer}\n" +
            $"Enemy: {bulletsEnemy}\n\n" +
            $"Active Enemies: {enemiesActive}\n\n" +
            $"{hpPlayerStr}\n" +
            $"{hpBossStr}\n" +
            timeStr;
    }
}
