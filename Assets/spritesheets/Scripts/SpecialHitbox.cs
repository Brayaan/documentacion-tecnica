/// -----------------------------------------------------------------------------
/// <file>SpecialHitbox.cs</file>
/// <summary>
/// Contiene la clase SpecialHitbox.
/// </summary>
/// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controla la hitbox de un ataque especial, incluyendo la lógica de daño y empuje.
/// </summary>
public class SpecialHitbox : MonoBehaviour
{
    /// <summary>
    /// Cantidad de daño que inflige el ataque especial.
    /// </summary>
    [HideInInspector]
    public int damage = 20;

    /// <summary>
    /// Fuerza de empuje aplicada al objetivo al recibir el ataque especial.
    /// </summary>
    public float specialKnockbackForce = 10f;

    /// <summary>
    /// Tiempo en el que se registró el último impacto.
    /// </summary>
    private float lastHitTime = float.NegativeInfinity;

    /// <summary>
    /// Tiempo de espera entre impactos sucesivos (cooldown).
    /// </summary>
    public float hitCooldown = 0.15f;

    /// <summary>
    /// Referencia al GameObject dueño de esta hitbox.
    /// </summary>
    public GameObject owner;

    /// <summary>
    /// Se llama cuando el objeto es habilitado. Reinicia el tiempo del último impacto.
    /// </summary>
    void OnEnable()
    {
        lastHitTime = float.NegativeInfinity;
    }

    /// <summary>
    /// Se llama cuando otro collider entra en la zona del trigger.
    /// </summary>
    /// <param name="other">El collider que entró al trigger.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerStay2D(other);
    }

    /// <summary>
    /// Se llama mientras otro collider permanece en la zona del trigger, manejando la lógica de colisión y daño.
    /// </summary>
    /// <param name="other">El collider que se mantiene en el trigger.</param>
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] Detectó colisión con: {other.name} (Root: {other.transform.root.name})");

        // Determinar el root del atacante
        Transform attackerRoot = owner != null ? owner.transform.root : transform.root;

        // Ignorar todo lo que no sea Enemy o Player
        if (!other.transform.root.CompareTag("Enemy") && !other.transform.root.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Ignorado porque {other.transform.root.name} no es Enemy ni Player.");
            return;
        }

        // Evitar golpearse a sí mismo
        if (other.transform.root == attackerRoot)
        {
            Debug.Log($"[{gameObject.name}] Ignorado porque es el mismo atacante.");
            return;
        }

        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        Vector2 attackerPos = attackerRoot.position;

        // Intentar EnemyHealthSystem primero (enemigos reales)
        EnemyHealthSystem enemyHealth = other.GetComponentInParent<EnemyHealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, attackerPos);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound("Special");
            ApplyExtraKnockback(other, attackerPos);
            Debug.Log($"[Especial] Golpe a {other.name} — daño: {damage}");
            return;
        }

        // Fallback: HealthSystem (Player o Duplicate)
        HealthSystem health = other.GetComponentInParent<HealthSystem>();

        if (health != null)
        {
            PlayerDefense defense = other.GetComponentInParent<PlayerDefense>();

            if (defense != null && defense.IsBlocking())
            {
                Debug.Log("Ataque especial bloqueado por " + other.name);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBlockSound();
                return;
            }

            health.TakeDamage(damage, attackerPos);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound("Special");
            ApplyExtraKnockback(other, attackerPos);

            EnergySystem targetEnergy = other.GetComponentInParent<EnergySystem>();
            if (targetEnergy != null)
                targetEnergy.GainEnergyFromDamage();

            Debug.Log($"[Especial] Golpe a {other.name} — daño: {damage}");
        }
    }

    /// <summary>
    /// Aplica una fuerza de empuje extra al objetivo especificado.
    /// </summary>
    /// <param name="target">El collider del objetivo al que se aplicará el empuje.</param>
    /// <param name="attackerPos">La posición del atacante para determinar la dirección del empuje.</param>
    void ApplyExtraKnockback(Collider2D target, Vector2 attackerPos)
    {
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        Vector2 dir = ((Vector3)target.transform.position - (Vector3)attackerPos).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * specialKnockbackForce, 3f), ForceMode2D.Impulse);
    }
}