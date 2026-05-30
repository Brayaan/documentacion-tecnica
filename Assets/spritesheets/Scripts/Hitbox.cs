//-----------------------------------------------------------------------
// <copyright file="Hitbox.cs">
//     Copyright (c) 2026. All rights reserved.
// </copyright>
// <summary>Handles collision detection and damage application for attacks.</summary>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Represents a damage-dealing area activated during an attack animation.
/// Detects collisions with enemies or players and applies damage, knockback, and energy changes.
/// </summary>
public class Hitbox : MonoBehaviour
{
    /// <summary>
    /// The name of the attack associated with this hitbox (e.g., "Punch", "Kick").
    /// </summary>
    public string attackName;

    /// <summary>
    /// The amount of health points deducted when this hitbox strikes a valid target.
    /// </summary>
    public int damage = 1;

    /// <summary>
    /// Reference to the PlayerAttack component on the root object to check attack state.
    /// </summary>
    private PlayerAttack attack;

    /// <summary>
    /// The timestamp of the last successful hit to enforce the hit cooldown.
    /// </summary>
    private float lastHitTime = float.NegativeInfinity;

    /// <summary>
    /// The minimum time interval (in seconds) between consecutive hits from this hitbox.
    /// </summary>
    // Intervalo mínimo entre golpes por activación del hitbox
    public float hitCooldown = 0.2f;

    /// <summary>
    /// Called every frame a collider remains within the trigger zone.
    /// Validates the target, checks blocking states, applies damage, and updates energy.
    /// </summary>
    /// <param name="other">The collider interacting with the hitbox.</param>
    // OnTriggerStay2D se ejecuta cada frame mientras el collider permanece dentro
    private void OnTriggerStay2D(Collider2D other)
    {
        // Cancelar si el ataque no está activo actualmente
        if (attack == null || !attack.IsAttacking() || !gameObject.activeSelf)
            return;

        if (other.transform.root.CompareTag("Player") || other.transform.root.CompareTag("Enemy"))
        {
            // Evitar que el hitbox golpee al propio personaje
            if (other.transform.root == transform.root)
                return;

            PlayerDefense defense = other.GetComponentInParent<PlayerDefense>();

            // Si el objetivo bloquea, empujar al atacante de vuelta
            if (defense != null && defense.IsBlocking())
            {
                Debug.Log(other.name + " bloqueó el ataque");
                
                // CA-03: Sonido de bloqueo
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBlockSound();

                Rigidbody2D attackerRb = transform.root.GetComponent<Rigidbody2D>();

                if (attackerRb != null)
                {
                    // Dirección de rebote desde el bloqueador hacia el atacante
                    Vector2 direction = (transform.root.position - other.transform.position).normalized;

                    attackerRb.linearVelocity = Vector2.zero;
                    attackerRb.AddForce(new Vector2(direction.x * 4f, 1.5f), ForceMode2D.Impulse);
                }

                return;
            }

            HealthSystem health = other.GetComponentInParent<HealthSystem>();

            if (health != null)
            {
                // Mathf.Max garantiza que hitCooldown negativo se trate como cero
                if (Time.time - lastHitTime < Mathf.Max(0f, hitCooldown))
                    return;

                lastHitTime = Time.time;

                Vector2 attackerPosition = transform.root.position;

                health.TakeDamage(damage, attackerPosition);

                // CA-01: Sonido de impacto (Puño o Patada)
                if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound(attackName);

                // Dar energía al objetivo por absorber el impacto
                EnergySystem targetEnergy = other.GetComponentInParent<EnergySystem>();
                if (targetEnergy != null)
                    targetEnergy.GainEnergyFromDamage();

                // Dar energía al atacante por conectar el golpe
                EnergySystem attackerEnergy = transform.root.GetComponent<EnergySystem>();
                if (attackerEnergy != null)
                    attackerEnergy.GainEnergyFromAttack(attackName);

                Debug.Log("Golpeaste a: " + other.name + " con: " + attackName);
            }
        }
    }

    /// <summary>
    /// Initializes references to required components.
    /// </summary>
    void Start()
    {
        // Buscar PlayerAttack en la raíz de la jerarquía
        attack = transform.root.GetComponent<PlayerAttack>();
    }

    /// <summary>
    /// Resets the hitbox state when deactivated to ensure the next attack sequence starts fresh.
    /// </summary>
    void OnDisable()
    {
        // Resetear cooldown para que el próximo ataque siempre conecte
        lastHitTime = float.NegativeInfinity;
    }
}