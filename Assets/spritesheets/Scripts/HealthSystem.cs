//-----------------------------------------------------------------------
// <copyright file="HealthSystem.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
// <summary>
// Manages player or entity health, knockback, and interactions with the UI and CombatManager.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the health, damage processing, and death state of a character or entity.
/// Handles UI updates and communicates death events to the CombatManager.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    /// <summary>
    /// The maximum health of the character.
    /// </summary>
    public int maxHealth = 100;

    /// <summary>
    /// The current health of the character.
    /// </summary>
    public int currentHealth;

    /// <summary>
    /// The UI Image component representing the health bar or heart counter.
    /// </summary>
    public Image healthImage;

    /// <summary>
    /// Array of sprites representing different health states for the UI.
    /// </summary>
    private Sprite[] healthSprites;

    /// <summary>
    /// Force applied when the character takes knockback.
    /// </summary>
    public float knockbackForce = 5f;

    /// <summary>
    /// Duration of the knockback effect in seconds.
    /// </summary>
    public float knockbackDuration = 0.05f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private Animator anim;

    /// <summary>
    /// Indicates whether the character is currently in a hit-stun state.
    /// </summary>
    private bool isHit = false;

    // ── NUEVO: evitar que TakeDamage se llame tras la muerte ──
    /// <summary>
    /// Indicates whether the character is currently dead.
    /// </summary>
    private bool isDead = false;

    // Posición inicial para reiniciar en cada ronda
    /// <summary>
    /// The starting position to reset the character at the beginning of each round.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// Initializes components, loads UI resources, and sets starting values.
    /// </summary>
    void Start()
    {
        startPosition = transform.position;
        healthSprites = Resources.LoadAll<Sprite>("HeartCounter/heart_counter-Sheet");

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();

        UpdateHealthUI();
    }

    /// <summary>
    /// Applies damage to the character and triggers knockback if not dead.
    /// </summary>
    /// <param name="damage">The amount of damage to subtract from current health.</param>
    /// <param name="attackerPosition">The world position of the attacker for knockback calculation.</param>
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // ── NUEVO: ignorar daño si ya está muerto ──
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Vida actual: " + currentHealth);

        UpdateHealthUI();

        // ── NUEVO: verificar muerte antes de aplicar hit-stun ──
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        ApplyHit(attackerPosition);
    }

    /// <summary>
    /// Heals the character by a specified amount, up to the maximum health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[Healing] Vida restaurada: +{amount} → {currentHealth}/{maxHealth}");
        UpdateHealthUI();
    }

    // ── NUEVO: muerte del jugador ──
    /// <summary>
    /// Handles character death, triggers death animations, and notifies the CombatManager.
    /// </summary>
    void Die()
    {
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Disparar animación de recibir daño → Death fluirá desde el Animator
        if (anim != null)
        {
            if (TieneParametro("Hit"))
                anim.SetTrigger("Hit");

            // isDead como bool para que el Animator transite hacia Death
            if (TieneParametro("isDead"))
                anim.SetBool("isDead", true);
        }

        // Notificar al CombatManager que este jugador perdió
        if (CombatManager.Instance != null)
            CombatManager.Instance.NotifyPlayerDeath(this);

        Debug.Log($"[HealthSystem] {gameObject.name} murió.");
    }

    /// <summary>
    /// Applies the hit-stun effect and knockback force to the character.
    /// </summary>
    /// <param name="attackerPosition">The world position of the attacker to calculate knockback direction.</param>
    void ApplyHit(Vector2 attackerPosition)
    {
        if (isHit) return;

        isHit = true;

        if (movement != null)
            movement.enabled = false;

        if (anim != null && TieneParametro("Hit"))
            anim.SetTrigger("Hit");

        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction.x * knockbackForce, 0f), ForceMode2D.Impulse);
        }

        Invoke(nameof(EndHit), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Checks if a parameter exists within the Animator component.
    /// </summary>
    /// <param name="nombre">The name of the parameter to search for.</param>
    /// <returns>True if the parameter exists, false otherwise.</returns>
    bool TieneParametro(string nombre)
    {
        foreach (var param in anim.parameters)
            if (param.name == nombre) return true;
        return false;
    }

    /// <summary>
    /// Concludes the hit-stun state, restoring movement if applicable.
    /// </summary>
    void EndHit()
    {
        isHit = false;

        if (movement != null && !isDead)
        {
            if (CombatManager.Instance == null || !CombatManager.Instance.isCombatEnded)
            {
                movement.enabled = true;
            }
        }
    }

    /// <summary>
    /// Updates the health UI by determining the correct sprite to display based on current health percentage.
    /// </summary>
    void UpdateHealthUI()
    {
        if (maxHealth <= 0)
        {
            Debug.LogError("maxHealth debe ser mayor que cero", this);
            return;
        }

        if (healthImage == null)
        {
            Debug.LogError("healthImage no está asignada en el Inspector", this);
            return;
        }

        if (healthSprites == null || healthSprites.Length == 0)
        {
            Debug.LogError("No se cargaron los sprites", this);
            return;
        }

        int index = Mathf.RoundToInt(((float)currentHealth / maxHealth) * (healthSprites.Length - 1));
        
        // Volver a invertir porque el frame 0 es lleno y el frame 30 es vacío
        index = (healthSprites.Length - 1) - index;
        
        healthImage.sprite = healthSprites[index];
        
        // FORZAR VISIBILIDAD:
        healthImage.enabled = true;
        healthImage.color = Color.white;
    }

    // ── NUEVO: Reinicio para siguientes rondas ──
    /// <summary>
    /// Resets the character's health, position, and states for a new round.
    /// </summary>
    public void ResetPlayer()
    {
        isDead = false;
        isHit = false;
        currentHealth = maxHealth;
        transform.position = startPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        UpdateHealthUI();

        if (anim != null)
        {
            if (TieneParametro("isDead"))
                anim.SetBool("isDead", false);
            
            anim.Play("Idle", -1, 0f);
        }

        if (movement != null)
        {
            movement.enabled = true;
        }
    }
}