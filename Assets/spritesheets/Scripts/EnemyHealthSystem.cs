//-----------------------------------------------------------------------
// <copyright file="EnemyHealthSystem.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Manages the health and hit reactions (such as knockback) for an enemy entity.
/// </summary>
public class EnemyHealthSystem : MonoBehaviour
{
    /// <summary>
    /// The maximum health of the enemy.
    /// </summary>
    public int maxHealth = 50;

    /// <summary>
    /// The current health of the enemy.
    /// </summary>
    public int currentHealth;

    /// <summary>
    /// The force applied to the enemy when taking a hit.
    /// </summary>
    public float knockbackForce = 5f;

    /// <summary>
    /// The duration for which the enemy remains in a 'hit' state, preventing further knockbacks.
    /// </summary>
    public float knockbackDuration = 0.3f;

    /// <summary>
    /// The Rigidbody2D component attached to the enemy, used for applying physics forces.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// A flag indicating whether the enemy is currently in a 'hit' state.
    /// </summary>
    private bool isHit = false;

    /// <summary>
    /// Initializes the health system, setting current health to max health and retrieving required components.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Applies damage to the enemy and triggers a hit reaction. If health drops to zero or below, the enemy dies.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <param name="attackerPosition">The world position of the attacker, used to calculate knockback direction.</param>
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log($"Enemigo vida: {currentHealth}");

        ApplyHit(attackerPosition);

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Applies a knockback effect to the enemy based on the attacker's position.
    /// </summary>
    /// <param name="attackerPosition">The world position of the attacker to determine the knockback direction.</param>
    void ApplyHit(Vector2 attackerPosition)
    {
        if (isHit) return;

        isHit = true;

        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.AddForce(new Vector2(direction.x * knockbackForce, 0f), ForceMode2D.Impulse);
        }

        Invoke(nameof(EndHit), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Resets the hit state, allowing the enemy to take knockback again.
    /// </summary>
    void EndHit()
    {
        isHit = false;
    }

    /// <summary>
    /// Handles the enemy's death sequence, destroying the game object.
    /// </summary>
    void Die()
    {
        Debug.Log("Enemigo derrotado");
        Destroy(gameObject, 0.5f);
    }
}