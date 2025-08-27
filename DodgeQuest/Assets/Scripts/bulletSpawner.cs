using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public enum SpawnerType { SpiralArms, Fan, CircleBurst }

    [Header("Bullet Attributes")]
    public GameObject bullet;
    public float bulletLife = 1f;
    public float speed = 8f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType = SpawnerType.SpiralArms;
    [SerializeField] private float firingRate = 0.03f;

    [Header("Pattern Switching")]
    public float switchInterval = 10f;
    public SpawnerType[] cycle = new SpawnerType[] { SpawnerType.SpiralArms, SpawnerType.Fan, SpawnerType.CircleBurst };

    [Header("Fan")]
    public int   fanCount  = 9;
    public float fanSpread = 160f;

    [Header("Circle Burst")]
    public int circleCount = 20;

    [Header("Spiral Arms")]
    public int   arms       = 6;
    public float spinSpeed  = 180f;
    public float armSpread  = 0f;
    public float angleOffset = -90f;

    private float timer;
    private int   cycleIndex;
    private float baseAngle;

    void Start()
    {
        if (cycle != null && cycle.Length > 0)
            StartCoroutine(PatternSwitcher());
        else
            spawnerType = SpawnerType.SpiralArms;
    }

    void Update()
    {
        if (spawnerType == SpawnerType.SpiralArms)
            baseAngle += spinSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= firingRate)
        {
            Fire();
            timer = 0f;
        }
    }

    IEnumerator PatternSwitcher()
    {
        cycleIndex = 0;
        SetPattern(cycle[cycleIndex]);

        while (true)
        {
            yield return new WaitForSeconds(switchInterval);

            cycleIndex = (cycleIndex + 1) % cycle.Length;
            SetPattern(cycle[cycleIndex]);
        }
    }

    private void SetPattern(SpawnerType next)
    {
        spawnerType = next;

        timer = 0f;
        if (spawnerType == SpawnerType.SpiralArms)
            baseAngle = 0f;

    }

    private void Fire()
    {
        if (!bullet) return;

        switch (spawnerType)
        {
            case SpawnerType.Fan:
                FireFan();
                break;

            case SpawnerType.CircleBurst:
                FireCircleBurst();
                break;

            case SpawnerType.SpiralArms:
                FireSpiralArms();
                break;
        }
    }

    private void FireFan()
    {
        if (fanCount <= 1)
        {
            SpawnBullet(transform.position, transform.rotation);
            return;
        }

        float step  = fanSpread / (fanCount - 1);
        float start = -fanSpread * 0.5f;

        for (int i = 0; i < fanCount; i++)
        {
            float angle = start + step * i + angleOffset;
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(transform.position, rot);
        }
    }

    private void FireCircleBurst()
    {
        if (circleCount <= 0) return;

        float step = 360f / circleCount;
        for (int i = 0; i < circleCount; i++)
        {
            float angle = step * i + angleOffset;
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(transform.position, rot);
        }
    }

    private void FireSpiralArms()
    {
        float armStep = 360f / Mathf.Max(1, arms);

        for (int i = 0; i < arms; i++)
        {
            float angle = baseAngle + armStep * i + angleOffset;

            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(transform.position, rot);
        }
    }

    private void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(bullet, pos, rot);
        var b  = go.GetComponent<Bullet>();
        if (b)
        {
            b.speed      = speed;
            b.bulletLife = bulletLife;
            b.rotation   = rot.eulerAngles.z;
        }
    }

    private void ClearExistingBullets()
    {
        var all = FindObjectsOfType<Bullet>();
        foreach (var bb in all)
        {
            if (bb != null) Destroy(bb.gameObject);
        }
    }
}
