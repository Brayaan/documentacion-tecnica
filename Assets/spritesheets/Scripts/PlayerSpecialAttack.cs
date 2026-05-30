/// <summary>
/// File: PlayerSpecialAttack.cs
/// Description: Manages special attack inputs, energy consumption, and executing special moves including healing and damage.
/// </summary>

using System.Collections;
using UnityEngine;

/// <summary>
/// Container class for defining the properties of a special attack.
/// Can represent either a damaging attack or a healing move.
/// </summary>
[System.Serializable]
public class SpecialAttackData
{
    /// <summary>Name identifying the special attack.</summary>
    public string attackName;
    
    /// <summary>Key input required to trigger the attack.</summary>
    public KeyCode key;
    
    /// <summary>Amount of energy consumed by this attack.</summary>
    public int energyCost;
    
    /// <summary>Damage dealt if it's an offensive attack.</summary>
    public int damage;
    
    /// <summary>Total duration in seconds of the special attack sequence.</summary>
    public float duration;
    
    /// <summary>Animation trigger parameter name.</summary>
    public string animTrigger;
    
    /// <summary>Hitbox object associated with this special attack.</summary>
    public GameObject hitbox;
    
    [Tooltip("Tiempo que tarda en activarse el golpe después de presionar la tecla (para sincronizar con la animación)")]
    /// <summary>Delay in seconds before the hitbox activates or healing applies.</summary>
    public float hitDelay = 0.15f;

    [Space]
    /// <summary>Flag indicating if this special attack is a healing move instead of offensive.</summary>
    public bool isHealing;
    
    [Range(0f, 1f)]
    /// <summary>Percentage of maximum health to recover if this is a healing move.</summary>
    public float healPercent;
}

/// <summary>
/// Component that handles execution of special attacks, UI updates for power readiness,
/// and interactions with energy, health, and movement systems.
/// </summary>
public class PlayerSpecialAttack : MonoBehaviour
{
    [Header("Referencias")]
    /// <summary>Reference to the energy system for consuming energy.</summary>
    public EnergySystem energySystem;
    
    /// <summary>Reference to the base attack component.</summary>
    public PlayerAttack playerAttack;
    
    /// <summary>Reference to the movement component to lock movement during specials.</summary>
    public PlayerMovement playerMovement;
    
    /// <summary>Reference to the health system for healing effects.</summary>
    public HealthSystem playerHealth;
    
    /// <summary>Reference to the animator component.</summary>
    private Animator anim;

    [Header("UI — Barra de Poderes")]
    /// <summary>UI element displaying when the player has full power/energy.</summary>
    public GameObject powerBarUI;

    [Header("Poderes Especiales (configurar los 4 en el Inspector)")]
    /// <summary>Array of configurable special attacks.</summary>
    public SpecialAttackData[] specialAttacks = new SpecialAttackData[4];

    /// <summary>Flag to track if a special attack is currently active.</summary>
    private bool isUsingSpecial = false;
    
    /// <summary>Flag tracking if energy was fully charged in the previous frame.</summary>
    private bool wasEnergyFull  = false;
    
    /// <summary>Stores the special attack that has been queued for execution.</summary>
    private SpecialAttackData pendingSpecial = null;

    /// <summary>
    /// Initializes references, ensures hitboxes are disabled, and hides power UI.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();

        if (powerBarUI != null)
            powerBarUI.SetActive(false);

        foreach (SpecialAttackData sa in specialAttacks)
            if (sa.hitbox != null)
                sa.hitbox.SetActive(false);
    }

    /// <summary>
    /// Updates the logic for checking special attack inputs and power bar status.
    /// </summary>
    void Update()
    {
        bool energyFull = energySystem != null && energySystem.IsFull();

        if (energyFull != wasEnergyFull)
        {
            wasEnergyFull = energyFull;
            if (powerBarUI != null)
                powerBarUI.SetActive(energyFull);
        }

        if (isUsingSpecial) return;

        for (int i = 0; i < specialAttacks.Length; i++)
        {
            SpecialAttackData sa = specialAttacks[i];

            // Ignorar ataques no configurados (sin nombre o sin tecla asignada)
            if (string.IsNullOrEmpty(sa.attackName) || sa.key == KeyCode.None)
                continue;

            if (Input.GetKeyDown(sa.key))
            {
                if (energySystem == null || energySystem.currentEnergy < sa.energyCost)
                {
                    Debug.Log($"[Especial] Energía insuficiente para {sa.attackName}. Necesitás {sa.energyCost}.");
                    break;
                }

                pendingSpecial = sa;
                break;
            }
        }

        if (pendingSpecial != null && (playerAttack == null || !playerAttack.IsAttacking()))
        {
            ExecuteSpecialAttack(pendingSpecial);
            pendingSpecial = null;
        }
    }

    /// <summary>
    /// Executes the specified special attack logic.
    /// </summary>
    /// <param name="sa">The special attack data to execute.</param>
    void ExecuteSpecialAttack(SpecialAttackData sa)
    {
        isUsingSpecial = true;
        Debug.Log($"[Especial] Ejecutando: {sa.attackName}");

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (anim != null && !string.IsNullOrEmpty(sa.animTrigger))
            anim.SetTrigger(sa.animTrigger);

        if (sa.isHealing)
        {
            if (playerHealth != null)
            {
                int amount = Mathf.RoundToInt(playerHealth.maxHealth * sa.healPercent);
                StartCoroutine(DelayedHeal(amount, sa.hitDelay));
            }
            else
            {
                Debug.LogWarning("[Healing] playerHealth no está asignado en el Inspector.");
            }
        }
        else if (sa.hitbox != null)
        {
            SpecialHitbox sh = sa.hitbox.GetComponent<SpecialHitbox>();
            if (sh == null)
            {
                sh = sa.hitbox.AddComponent<SpecialHitbox>();
            }
            
            sh.damage = sa.damage;

            StartCoroutine(DelayedHitbox(sa.hitbox, sa.hitDelay));
        }

        // Retrasar el consumo de energía para sincronizarlo con la animación y el golpe
        StartCoroutine(DelayedConsumeEnergy(sa.energyCost, sa.hitDelay));

        StartCoroutine(EndSpecialAfterDuration(sa));
    }

    /// <summary>
    /// Coroutine to consume energy after a specified delay.
    /// </summary>
    /// <param name="amount">Amount of energy to consume.</param>
    /// <param name="delay">Delay in seconds before consumption.</param>
    /// <returns>IEnumerator for coroutine sequencing.</returns>
    IEnumerator DelayedConsumeEnergy(int amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (energySystem != null)
            energySystem.ConsumeEnergy(amount);
    }

    /// <summary>
    /// Coroutine to activate a hitbox after a specified delay.
    /// </summary>
    /// <param name="hitbox">The GameObject containing the hitbox to activate.</param>
    /// <param name="delay">Delay in seconds before activation.</param>
    /// <returns>IEnumerator for coroutine sequencing.</returns>
    IEnumerator DelayedHitbox(GameObject hitbox, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hitbox == null)
        {
            Debug.LogError("¡El hitbox es nulo! Revisa el Inspector.");
            yield break;
        }

        Debug.Log("Activando hitbox: " + hitbox.name + " en " + gameObject.name);

        Collider2D col = hitbox.GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning($"⚠️ El hitbox {hitbox.name} NO tiene un Collider2D. ¡No podrá detectar golpes!");
        else
        {
            if (!col.isTrigger) Debug.LogWarning($"⚠️ El Collider de {hitbox.name} no tiene marcado 'Is Trigger'. Deberías marcarlo.");
            col.enabled = true; // <-- Asegurar que el collider esté encendido
        }

        SpecialHitbox sh = hitbox.GetComponent<SpecialHitbox>();
        if (sh == null)
        {
            Debug.LogWarning($"El objeto {hitbox.name} no tenía el script SpecialHitbox. Se lo agregué automáticamente.");
            sh = hitbox.AddComponent<SpecialHitbox>();
        }
        sh.enabled = true; // <-- Asegurar que el script esté encendido
        
        sh.owner = transform.root.gameObject; 

        hitbox.SetActive(true);
    }

    /// <summary>
    /// Coroutine to heal the player after a specified delay.
    /// </summary>
    /// <param name="amount">Health points to restore.</param>
    /// <param name="delay">Delay in seconds before healing.</param>
    /// <returns>IEnumerator for coroutine sequencing.</returns>
    IEnumerator DelayedHeal(int amount, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerHealth != null)
            playerHealth.Heal(amount);
    }

    /// <summary>
    /// Coroutine to handle cleanup and state resetting after a special attack finishes.
    /// </summary>
    /// <param name="sa">The special attack data that was executed.</param>
    /// <returns>IEnumerator for coroutine sequencing.</returns>
    IEnumerator EndSpecialAfterDuration(SpecialAttackData sa)
    {
        // Si la duración es menor que el retraso del golpe (0.15f), el hitbox se apagará antes de encenderse o instantáneamente.
        // Forzamos un mínimo de tiempo para que la física tenga tiempo de reaccionar.
        float actualDuration = Mathf.Max(sa.duration, 0.25f); 

        yield return new WaitForSeconds(actualDuration);

        if (sa.hitbox != null)
        {
            Debug.Log($"Apagando hitbox: {sa.hitbox.name} en {gameObject.name} después de {actualDuration} segundos.");
            sa.hitbox.SetActive(false);
        }

        if (playerMovement != null)
            playerMovement.enabled = true;

        isUsingSpecial = false;
    }

    /// <summary>
    /// Returns whether the player is currently executing a special attack.
    /// </summary>
    /// <returns>True if a special attack is active, false otherwise.</returns>
    public bool IsUsingSpecial() => isUsingSpecial;
}