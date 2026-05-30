//-----------------------------------------------------------------------
// <copyright file="ExtendedHitbox.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
// <summary>Handles collision detection for extended player hitboxes during attacks.</summary>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Handles collision detection for extended hitboxes during player attacks.
/// Evaluates interactions with enemy health systems and defense mechanics.
/// </summary>
public class ExtendedHitbox : MonoBehaviour
{
    /// <summary>
    /// The primary Hitbox component attached to the same GameObject.
    /// Provides attack metadata such as damage and name.
    /// </summary>
    private Hitbox originalHitbox;

    /// <summary>
    /// The PlayerAttack component located on the root of the attacker's hierarchy.
    /// Used to determine if an attack is currently active.
    /// </summary>
    private PlayerAttack playerAttack;

    /// <summary>
    /// Initializes references to necessary components on start.
    /// Retrieves the original hitbox and the player's attack controller.
    /// </summary>
    void Start()
    {
        // Obtener referencias desde el mismo objeto y la raíz
        originalHitbox = GetComponent<Hitbox>();
        playerAttack = transform.root.GetComponent<PlayerAttack>();
    }

    /// <summary>
    /// Triggered when another collider enters this trigger collider attached to the object.
    /// Evaluates if an active attack successfully hits a valid enemy target, applies damage,
    /// checks for blocking, triggers audio, and updates energy systems.
    /// </summary>
    /// <param name="other">The other Collider2D involved in this collision.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Cancelar si faltan componentes críticos del sistema
        if (originalHitbox == null || playerAttack == null)
            return;

        // Solo procesar durante un ataque activo
        if (!playerAttack.IsAttacking() || !gameObject.activeSelf)
            return;

        if (other.CompareTag("Enemy"))
        {
            EnemyHealthSystem enemyHealth = other.GetComponent<EnemyHealthSystem>();
            PlayerDefense defense = other.GetComponentInParent<PlayerDefense>();

            if (defense != null && defense.IsBlocking())
            {
                Debug.Log("Ataque bloqueado por " + other.name);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBlockSound();
                return;
            }

            if (enemyHealth != null)
            {
                Vector2 attackerPosition = transform.root.position;
                // Usar el daño del Hitbox padre, no un valor fijo
                int damage = originalHitbox.damage;

                enemyHealth.TakeDamage(damage, attackerPosition);
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound(originalHitbox.attackName);

                // Dar energía al atacante por conectar el golpe
                EnergySystem attackerEnergy = transform.root.GetComponent<EnergySystem>();
                if (attackerEnergy != null)
                {
                    attackerEnergy.GainEnergyFromAttack("Puñetazo");
                }

                Debug.Log($"Golpe a enemigo!");
            }
        }
    }
}