using TMPro;
using UnityEngine;

public class BulletCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    void OnEnable()
    {
        UpdateAll(Bullet.ActiveCount, Bullet.ActivePlayerCount, Bullet.ActiveEnemyCount);
        Bullet.OnCountsChanged += UpdateAll;
    }

    void OnDisable()
    {
        Bullet.OnCountsChanged -= UpdateAll;
    }

    private void UpdateAll(int total, int player, int enemy)
    {
        if (label != null)
            label.text = $"Balas activas: {total}\nJugador: {player}   Enemigo: {enemy}";
    }
}
