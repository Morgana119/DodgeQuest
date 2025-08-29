using TMPro;
using UnityEngine;

public class GameCountersUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    int bulletsTotal, bulletsPlayer, bulletsEnemy;
    int enemiesActive;
    int playerHP, playerMax;
    int bossHP, bossMax;

    void OnEnable()
    {
        UpdateBullets(Bullet.ActiveCount, Bullet.ActivePlayerCount, Bullet.ActiveEnemyCount);
        UpdateEnemies(BulletSpawnerEnemy.ActiveCount);

        Bullet.OnCountsChanged += UpdateBullets;
        BulletSpawnerEnemy.OnEnemyCountChanged += UpdateEnemies;

        Player.OnPlayerHealthChanged += UpdatePlayerHP;
        BulletSpawnerBoss.OnBossHealthChanged += UpdateBossHP;
    }

    void OnDisable()
    {
        Bullet.OnCountsChanged -= UpdateBullets;
        BulletSpawnerEnemy.OnEnemyCountChanged -= UpdateEnemies;

        Player.OnPlayerHealthChanged -= UpdatePlayerHP;
        BulletSpawnerBoss.OnBossHealthChanged -= UpdateBossHP;
    }

    void Update()
    {
        Redraw();
    }

    void UpdateBullets(int total, int player, int enemy)
    {
        bulletsTotal = total;
        bulletsPlayer = player;
        bulletsEnemy = enemy;
    }

    void UpdateEnemies(int count)
    {
        enemiesActive = count;
    }

    void UpdatePlayerHP(int hp, int max)
    {
        playerHP = hp;
        playerMax = max;
    }

    void UpdateBossHP(int hp, int max)
    {
        bossHP = hp;
        bossMax = max;
    }

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

        string hpPlayerStr = (playerMax > 0) ? $"Player HP: {playerHP}/{playerMax}" : "Player HP: --";
        string hpBossStr   = (bossMax > 0)   ? $"Boss HP: {bossHP}/{bossMax}"       : "Boss HP: --";

        label.text =
            $"Active Bullets: {bulletsTotal}\n" +
            $"Player: {bulletsPlayer}\n" +
            $"Enemy: {bulletsEnemy}\n" + "\n" +
            $"Active Enemies: {enemiesActive}\n" + "\n" +
            $"{hpPlayerStr}\n" +
            $"{hpBossStr}" + "\n" +
            timeStr;
    }
}
