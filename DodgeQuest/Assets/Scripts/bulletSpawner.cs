using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public enum SpawnerType { Fan, CircleBurst, RoseStar }

    [Header("Bullet Attributes")]
    public GameObject bullet;
    public float bulletLife = 3f; // backup
    public float speed = 6f;

    [Header("Spawner Attributes")]
    [SerializeField] private SpawnerType spawnerType = SpawnerType.Fan;
    [SerializeField] private float firingRate = 0.08f;

    [Header("Pattern Switching")]
    public float switchInterval = 10f;
    public SpawnerType[] cycle = new SpawnerType[] { SpawnerType.Fan, SpawnerType.CircleBurst, SpawnerType.RoseStar };
    public bool clearOnSwitch = false;

    [Header("Orientación global")]
    public float angleOffset = -90f; // -90 para que apunten hacia abajo

    [Header("Fan")]
    public int fanCount = 6;
    public float fanSpread = 160f;
    public float fanSwayAmplitude = 25f;
    public float fanSwaySpeed = 0.5f;

    [Header("Circle Burst")]
    public int circleCount = 20;
    public float circleFiringRate = 0.25f;

    [Header("Rose / Star")]
    public int rosePetals = 6;
    [Range(0f, 1f)]
    public float roseRadiusAmp = 0.273f;
    public float roseBaseRadius = 1.6f;
    public int roseSamples = 35;
    public float roseSpin = 140f;
    public float roseFiringRate = 0.3f;
    public int roseRingsPerTick = 1;

    private float timer;
    private int cycleIndex;
    private float rosePhaseDeg;

    void Start()
    {
        StartCoroutine(PatternSwitcher());
    }

    void Update()
    {
        if (spawnerType == SpawnerType.RoseStar)
            rosePhaseDeg += roseSpin * Time.deltaTime;

        float currentRate =
            (spawnerType == SpawnerType.CircleBurst) ? circleFiringRate :
            (spawnerType == SpawnerType.RoseStar)    ? roseFiringRate :
                                                        firingRate;

        timer += Time.deltaTime;
        if (timer >= currentRate)
        {
            Fire();
            timer = 0f;
        }
    }

    IEnumerator PatternSwitcher()
    {
        if (cycle == null || cycle.Length == 0) yield break;

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

        if (spawnerType == SpawnerType.RoseStar)
            rosePhaseDeg = 0f;

        if (clearOnSwitch) ClearExistingBullets();
    }

    private void Fire()
    {
        if (!bullet) return;

        switch (spawnerType)
        {
            case SpawnerType.Fan:         FireFan(); break;
            case SpawnerType.CircleBurst: FireCircleBurst(); break;
            case SpawnerType.RoseStar:    FireRoseStar(); break;
        }
    }

    // PATRONES
    private void FireFan()
    {
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * fanSwaySpeed) * fanSwayAmplitude;

        if (fanCount <= 1)
        {
            Quaternion rot = transform.rotation * Quaternion.Euler(0f, 0f, angleOffset + sway);
            SpawnBullet(transform.position, rot);
            return;
        }

        float step = fanSpread / (fanCount - 1);
        float start = -fanSpread * 0.5f;

        for (int i = 0; i < fanCount; i++)
        {
            float angle = start + step * i + angleOffset + sway;
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

    private void FireRoseStar()
    {
        if (roseSamples <= 0) return;

        float step = 360f / roseSamples;

        for (int ring = 0; ring < Mathf.Max(1, roseRingsPerTick); ring++)
        {
            for (int i = 0; i < roseSamples; i++)
            {
                float theta = i * step + rosePhaseDeg;
                float r     = roseBaseRadius * (1f + roseRadiusAmp *
                               Mathf.Sin(rosePetals * theta * Mathf.Deg2Rad));

                float shotAngle = angleOffset + theta;
                Quaternion rot  = transform.rotation * Quaternion.Euler(0f, 0f, shotAngle);

                Vector3 dir = rot * Vector3.right;
                Vector3 pos = transform.position + dir * r;

                SpawnBullet(pos, rot);
            }
        }
    }

    private void SpawnBullet(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(bullet, pos, rot);
        var b = go.GetComponent<Bullet>();
        if (b)
        {
            b.speed = speed;
            b.bulletLife = bulletLife;
            b.rotation = rot.eulerAngles.z;
        }
    }

    private void ClearExistingBullets()
    {
        var all = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (var bb in all)
            if (bb != null) Destroy(bb.gameObject);
    }
}
