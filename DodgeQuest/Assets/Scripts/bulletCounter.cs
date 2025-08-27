using TMPro;
using UnityEngine;

public class BulletCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    void OnEnable()
    {
        UpdateLabel(Bullet.ActiveCount);
        Bullet.OnActiveCountChanged += UpdateLabel;
    }

    void OnDisable()
    {
        Bullet.OnActiveCountChanged -= UpdateLabel;
    }

    private void UpdateLabel(int count)
    {
        if (label != null)
            label.text = $"Balas activas: {count}";
    }
}
