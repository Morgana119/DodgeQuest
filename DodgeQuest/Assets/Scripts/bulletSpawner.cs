using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public enum SpawnerType { SpiralArms, Fan, CircleBurst }

    [Header("Bullet Attributes")]
    public GameObject bullet;          
    public float bulletLife = 10f; // backup
    public float speed = 8f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType = SpawnerType.SpiralArms;
    [SerializeField] private float firingRate = 0.03f; // menor = más densidad

    [Header("Pattern Switching")]
    public float switchInterval = 10f; // Duracion de cada patron
    public SpawnerType[] cycle = new SpawnerType[] { SpawnerType.SpiralArms, SpawnerType.Fan, SpawnerType.CircleBurst };

    [Header("Fan")]
    public int fanCount = 9;
    public float fanSpread = 160f;

    [Header("Circle Burst")]
    public int circleCount = 20;

    [Header("Spiral Arms")]
    public int arms = 6; // nº de brazos
    public float spinSpeed = 180f; // grados/seg (giro)
    public float angleOffset = -90f;   

    private float timer;
    private int cycleIndex;
    private float baseAngle;

    void OnValidate()
    {
        var def = new SpawnerType[] { SpawnerType.SpiralArms, SpawnerType.Fan, SpawnerType.CircleBurst };
        if (cycle == null || cycle.Length != def.Length)
        {
            cycle = def;
        }
        else
        {
            for (int i = 0; i < cycle.Length; i++) cycle[i] = def[i];
        }
    }

    void Start()
    {
        StartCoroutine(PatternSwitcher());
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
        if (spawnerType == SpawnerType.SpiralArms) baseAngle = 0f;
    }

    private void Fire()
    {
        if (!bullet) return;

        switch (spawnerType)
        {
            case SpawnerType.Fan: FireFan(); break;
            case SpawnerType.CircleBurst: FireCircleBurst(); break;
            case SpawnerType.SpiralArms: FireSpiralArms(); break;
        }
    }

    // PATRONES
    private void FireFan()
    {
        if (fanCount <= 1)
        {
            SpawnBullet(transform.position, transform.rotation);
            return;
        }

        float step = fanSpread / (fanCount - 1);
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
            b.speed = speed;
            b.bulletLife = bulletLife; // backup
            b.rotation = rot.eulerAngles.z;
        }
    }
}
