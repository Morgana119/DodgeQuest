using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Contador de balas
    public static int ActiveCount { get; private set; }
    public static event Action<int> OnActiveCountChanged;
    
    public float bulletLife = 1f;
    public float rotation = 0f;
    public float speed = 1f;

    private Vector2 spawnPoint;
    private float timer = 0f;

    void OnEnable()
    {
        ActiveCount++;
        OnActiveCountChanged?.Invoke(ActiveCount);
    }

    void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
        OnActiveCountChanged?.Invoke(ActiveCount);
    }
    
    void Start()
    {
        spawnPoint = new Vector2(transform.position.x, transform.position.y);
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= bulletLife)
        {
            Destroy(this.gameObject);
            return;
        }

        transform.position = Movement(timer);
    }

    private Vector2 Movement(float t)
    {
        float x = t * speed * transform.right.x;
        float y = t * speed * transform.right.y;
        return new Vector2(x + spawnPoint.x, y + spawnPoint.y);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
