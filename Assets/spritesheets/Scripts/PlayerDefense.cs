/// <summary>
/// File: PlayerDefense.cs
/// Description: Handles the defensive mechanics of the player, including blocking and energy gain.
/// </summary>

using UnityEngine;

/// <summary>
/// Manages the blocking mechanics of the player.
/// Synchronizes blocking inputs with animations, hitbox activation, and energy recovery.
/// </summary>
public class PlayerDefense : MonoBehaviour
{
    /// <summary>Reference to the Animator component for controlling blocking animations.</summary>
    private Animator anim;

    /// <summary>Hitbox object used for blocking attacks.</summary>
    public GameObject blockHitbox;

    /// <summary>Indicates whether the player is currently blocking.</summary>
    private bool isBlocking = false;

    /// <summary>Key used to trigger a block.</summary>
    public KeyCode blockKey = KeyCode.L;

    /// <summary>
    /// Initializes references and ensures the block hitbox is deactivated on startup.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();

        // Desactivar hitbox de bloqueo al iniciar la escena
        if (blockHitbox != null)
            blockHitbox.SetActive(false);
    }

    /// <summary>
    /// Updates the defense logic, checking for blocking input and managing the corresponding state changes.
    /// </summary>
    void Update()
    {
        bool input = Input.GetKey(blockKey);

        // Solo ejecutar lógica cuando el estado de bloqueo cambia
        if (input != isBlocking)
        {
            isBlocking = input;

            // Otorgar energía únicamente al inicio del bloqueo
            if (isBlocking)
            {
                EnergySystem energy = GetComponent<EnergySystem>();
                if (energy != null)
                    energy.GainEnergyFromBlock();
            }

            // Sincronizar animación con el estado actual de bloqueo
            if (anim != null)
                anim.SetBool("isBlocking", isBlocking);

            // Activar o desactivar el hitbox de bloqueo según estado
            if (blockHitbox != null)
                blockHitbox.SetActive(isBlocking);
        }
    }

    /// <summary>
    /// Returns the current blocking state of the player.
    /// </summary>
    /// <returns>True if the player is currently blocking, false otherwise.</returns>
    public bool IsBlocking()
    {
        return isBlocking;
    }
}