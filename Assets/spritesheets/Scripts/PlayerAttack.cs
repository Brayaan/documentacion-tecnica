/// <summary>
/// File: PlayerAttack.cs
/// Description: Handles player attacking mechanics including punches and kicks.
/// </summary>

using UnityEngine;

/// <summary>
/// Manages the attack mechanics of the player.
/// Controls input for basic attacks, hitbox activation, and synchronization with animations and movement.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    /// <summary>Reference to the Animator component for controlling attack animations.</summary>
    private Animator anim;

    /// <summary>Reference to the PlayerMovement script to synchronize states during attacks.</summary>
    public PlayerMovement playerMovement;

    /// <summary>Hitbox object used for punch attacks.</summary>
    public GameObject punchHitbox;
    
    /// <summary>Hitbox object used for kick attacks.</summary>
    public GameObject kickHitbox;

    /// <summary>Key used to trigger a punch attack.</summary>
    public KeyCode punchKey = KeyCode.J;
    
    /// <summary>Key used to trigger a kick attack.</summary>
    public KeyCode kickKey = KeyCode.K;

    /// <summary>Indicates whether a kick attack can be performed.</summary>
    private bool kickReady = true;
    
    /// <summary>Cooldown duration in seconds between consecutive kick attacks.</summary>
    public float kickCooldown = 0.5f;

    /// <summary>Indicates whether a punch attack can be performed.</summary>
    private bool punchReady = true;
    
    /// <summary>Cooldown duration in seconds between consecutive punch attacks.</summary>
    public float punchCooldown = 0.3f;

    /// <summary>Indicates whether the player is currently executing an attack.</summary>
    private bool isAttacking = false;
    
    /// <summary>Duration in seconds that an attack hitbox remains active.</summary>
    // Tiempo que el hitbox permanece activo por ataque
    private float attackDuration = 0.3f;

    /// <summary>
    /// Initializes references and ensures hitboxes are deactivated on startup.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();

        // Asegurar que los hitboxes están desactivados al iniciar
        if (punchHitbox != null)
            punchHitbox.SetActive(false);

        if (kickHitbox != null)
            kickHitbox.SetActive(false);
    }

    /// <summary>
    /// Updates the attack logic, checking for player input and managing attack cooldowns.
    /// </summary>
    void Update()
    {
        // Puñetazo: verificar input, cooldown y estado de ataque
        if (Input.GetKeyDown(punchKey) && !isAttacking && punchReady)
        {
            StartAttack();
            anim.SetTrigger("punch");
            ActivarHitbox();
            Invoke(nameof(DesactivarHitbox), attackDuration);

            punchReady = false;
            Invoke(nameof(ResetPunch), punchCooldown);
        }

        // Patada: verificar cooldown, input y estado de ataque
        if (Input.GetKeyDown(kickKey) && kickReady && !isAttacking)
        {
            StartAttack();
            anim.SetTrigger("kick");
            ActivarKickHitbox();
            Invoke(nameof(DesactivarKickHitbox), attackDuration);

            kickReady = false;
            Invoke(nameof(ResetKick), kickCooldown);
        }
    }

    /// <summary>
    /// Begins the attack sequence, locking further attacks and notifying the movement script.
    /// </summary>
    void StartAttack()
    {
        isAttacking = true;

        // Sincronizar flag de ataque con PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.isAttacking = true;
        }

        Invoke(nameof(EndAttack), attackDuration);
    }

    /// <summary>
    /// Ends the current attack sequence, freeing the player to attack or move again.
    /// </summary>
    void EndAttack()
    {
        isAttacking = false;

        // Limpiar flag de ataque en PlayerMovement también
        if (playerMovement != null)
        {
            playerMovement.isAttacking = false;
        }
    }

    /// <summary>
    /// Returns the current attacking state of the player.
    /// </summary>
    /// <returns>True if the player is currently attacking, false otherwise.</returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// Activates the punch hitbox.
    /// </summary>
    public void ActivarHitbox()
    {
        if (punchHitbox != null)
            punchHitbox.SetActive(true);
    }

    /// <summary>
    /// Deactivates the punch hitbox.
    /// </summary>
    public void DesactivarHitbox()
    {
        if (punchHitbox != null)
            punchHitbox.SetActive(false);
    }

    /// <summary>
    /// Activates the kick hitbox.
    /// </summary>
    public void ActivarKickHitbox()
    {
        if (kickHitbox != null)
            kickHitbox.SetActive(true);
    }

    /// <summary>
    /// Deactivates the kick hitbox.
    /// </summary>
    public void DesactivarKickHitbox()
    {
        if (kickHitbox != null)
            kickHitbox.SetActive(false);
    }

    /// <summary>
    /// Resets the kick cooldown, allowing the player to kick again.
    /// </summary>
    void ResetKick()
    {
        kickReady = true;
    }

    /// <summary>
    /// Resets the punch cooldown, allowing the player to punch again.
    /// </summary>
    void ResetPunch()
    {
        punchReady = true;
    }
}