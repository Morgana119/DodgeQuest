using System.Collections.Generic;
using UnityEngine;

public class AreaEnemy : MonoBehaviour
{
    public BulletSpawnerEnemy enemyPrefab;
    public Vector2 xRange = new Vector2(-6f, 6f);
    public Vector2 yRange = new Vector2(1f, 4f);

    public int targetAlive = 3;
    public float respawnDelay = 0.5f;

    readonly List<BulletSpawnerEnemy> alive = new();

    void OnEnable()
    {
        alive.Clear();
        FillToTarget();
    }

    void OnDisable()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i]) Destroy(alive[i].gameObject);
        }
        alive.Clear();
        CancelInvoke();
    }

    void Update()
    {
        bool removed = alive.RemoveAll(e => e == null) > 0;
        if (removed) Invoke(nameof(FillToTarget), respawnDelay);
        else if (alive.Count < targetAlive) FillToTarget();
    }

    void FillToTarget()
    {
        if (!enemyPrefab) return;
        while (alive.Count < Mathf.Max(0, targetAlive))
        {
            float x = Random.Range(xRange.x, xRange.y);
            float y = Random.Range(yRange.x, yRange.y);
            var pos = new Vector3(x, y, 0f);

            var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
            alive.Add(e);
        }
    }

    // Gizmos para ver el área (Scene view)
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Vector3 p1 = new Vector3(xRange.x, yRange.x, 0f);
    //     Vector3 p2 = new Vector3(xRange.y, yRange.x, 0f);
    //     Vector3 p3 = new Vector3(xRange.y, yRange.y, 0f);
    //     Vector3 p4 = new Vector3(xRange.x, yRange.y, 0f);
    //     Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p3);
    //     Gizmos.DrawLine(p3, p4); Gizmos.DrawLine(p4, p1);

    //     var c = new Vector3((xRange.x + xRange.y) * 0.5f, (yRange.x + yRange.y) * 0.5f, 0f);
    //     Gizmos.DrawSphere(c, 0.06f);
    // }
}