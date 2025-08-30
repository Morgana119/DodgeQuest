using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador de un área de spawn para enemigos.
/// Mantiene un número objetivo de enemigos activos dentro de un rango de coordenadas.
/// Si un enemigo muere, se respawnea uno nuevo tras un retardo.
/// </summary>
public class AreaEnemy : MonoBehaviour
{
    /// <summary>Prefab del enemigo a instanciar en esta área.</summary>
    public BulletSpawnerEnemy enemyPrefab;

    /// <summary>Rango de posiciones posibles en el eje X para el spawn.</summary>
    public Vector2 xRange = new Vector2(-6f, 6f);

    /// <summary>Rango de posiciones posibles en el eje Y para el spawn.</summary>
    public Vector2 yRange = new Vector2(1f, 4f);

    /// <summary>Número objetivo de enemigos vivos simultáneamente.</summary>
    public int targetAlive = 3;

    /// <summary>Tiempo de espera (segundos) antes de respawnear tras una baja.</summary>
    public float respawnDelay = 0.5f;

    /// <summary>Lista interna con las referencias a los enemigos vivos en esta área.</summary>
    private readonly List<BulletSpawnerEnemy> alive = new();

    /// <summary>
    /// Al habilitar el área, limpia la lista y llena hasta el número objetivo de enemigos.
    /// </summary>
    void OnEnable()
    {
        alive.Clear();
        FillToTarget();
    }

    /// <summary>
    /// Al deshabilitar el área, destruye todos los enemigos vivos y limpia la lista.
    /// </summary>
    void OnDisable()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i]) Destroy(alive[i].gameObject);
        }
        alive.Clear();
        CancelInvoke();
    }

    /// <summary>
    /// Monitorea la lista de enemigos: si alguno fue destruido, 
    /// programa respawn; si faltan enemigos, los crea directamente.
    /// </summary>
    void Update()
    {
        bool removed = alive.RemoveAll(e => e == null) > 0;

        if (removed)
            Invoke(nameof(FillToTarget), respawnDelay);
        else if (alive.Count < targetAlive)
            FillToTarget();
    }

    /// <summary>
    /// Spawnea enemigos hasta alcanzar el número objetivo configurado.
    /// Elige posiciones aleatorias dentro de los rangos <see cref="xRange"/> y <see cref="yRange"/>.
    /// </summary>
    void FillToTarget()
    {
        if (!enemyPrefab) return;

        while (alive.Count < Mathf.Max(0, targetAlive))
        {
            float x = Random.Range(xRange.x, xRange.y);
            float y = Random.Range(yRange.x, yRange.y);
            var pos = new Vector3(x, y, 0f);

            var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
            Debug.Log($"[AreaEnemy] Spawned {e.name} at {pos}", this);
            alive.Add(e);
        }
    }
}