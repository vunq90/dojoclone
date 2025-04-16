using System.Collections;
using System.Collections.Generic;
// EnemyProjectile.cs
// Fast projectile that damages the player on contact (unless invincible)

using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float speed = 5f;
    private float lifetime = 3f;
    private float rotateSpeed = 720f; // degrees per second

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // auto-cleanup
    }

    void Update()
    {
        // Rotate the projectile sprite
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsInvincible())
            {
                Debug.Log("Player hit by projectile.");
                // You can add death logic here
                player.Die();
            }
        }
    }

    public void SetDirection(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction.normalized * speed;
    }
}

