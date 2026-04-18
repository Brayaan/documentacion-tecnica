// ============================================================
// ARCHIVO: PlayerDefense.cs
// PROPÓSITO: Gestionar el sistema de bloqueo del jugador:
//            input de bloqueo, animación, hitbox de guardia y ganancia de energía.
// RESPONSABILIDAD: Detectar cuándo el jugador bloquea, activar el hitbox
//                  de bloqueo, sincronizar la animación y dar energía al inicio.
// SISTEMA: Combate — Defensa / Bloqueo
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script controla la mecánica de defensa del jugador.
 * Mientras se mantiene presionada la tecla de bloqueo (L por defecto),
 * el jugador entra en estado de guardia:
 *   - Se activa el hitbox de bloqueo (que Hitbox.cs detecta para rebotar al atacante).
 *   - Se dispara la animación de bloqueo en el Animator.
 *   - Se otorga energía al inicio del bloqueo (solo una vez por activación).
 *
 * Al soltar la tecla, todo vuelve al estado normal.
 * La lógica solo se ejecuta cuando el estado cambia (no cada frame si ya está bloqueando),
 * lo que evita llamadas innecesarias a EnergySystem cada frame.
 *
 * SISTEMA: Combate / Defensa
 * INTERACTÚA CON: Hitbox.cs, EnergySystem.cs, Animator
 */

using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    // -------------------------
    // REFERENCIAS EXTERNAS
    // -------------------------

    /// <summary>
    /// GameObject del hitbox de bloqueo.
    /// Cuando está activo, Hitbox.cs lo detecta mediante PlayerDefense.IsBlocking()
    /// y aplica el rebote al atacante en lugar de dañar al bloqueador.
    /// Debe asignarse desde el Inspector.
    /// </summary>
    public GameObject blockHitbox;

    // -------------------------
    // TECLAS DE INPUT
    // -------------------------

    /// <summary>
    /// Tecla asignada al bloqueo. Por defecto: L.
    /// Configurable desde el Inspector para remapeo de controles.
    /// Usa GetKey (no GetKeyDown) para detectar pulsación continua.
    /// </summary>
    public KeyCode blockKey = KeyCode.L;

    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Estado actual del bloqueo.
    /// true = el jugador está bloqueando activamente.
    /// false = el jugador no está en guardia.
    /// Este valor es el que Hitbox.cs consulta vía IsBlocking().
    /// </summary>
    private bool isBlocking = false;

    /// <summary>
    /// Referencia al Animator del jugador.
    /// Se obtiene en Start(). Necesario para sincronizar la animación de bloqueo.
    /// </summary>
    private Animator anim;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar.
    /// Obtiene el Animator y desactiva el hitbox de bloqueo para asegurar
    /// que el personaje comienza sin guardia activa.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();

        // Desactivar hitbox de bloqueo al iniciar para evitar guardia permanente
        if (blockHitbox != null)
            blockHitbox.SetActive(false);
    }

    /// <summary>
    /// Update() — Se ejecuta cada frame.
    /// Lee el input de bloqueo y compara con el estado anterior (isBlocking).
    /// Solo ejecuta la lógica de cambio de estado cuando el input cambia,
    /// evitando llamadas repetidas a EnergySystem y al Animator cada frame.
    ///
    /// CUÁNDO SE EJECUTA: Cada frame durante la partida.
    /// QUÉ CONTROLA: Detección de cambio de estado de bloqueo,
    ///               ganancia de energía al inicio, animación y hitbox.
    ///
    /// FLUJO DE CAMBIO DE ESTADO:
    ///   Input presionado (isBlocking = false → true):
    ///     → EnergySystem.GainEnergyFromBlock() (solo una vez)
    ///     → Animator.SetBool("isBlocking", true)
    ///     → blockHitbox.SetActive(true)
    ///   Input soltado (isBlocking = true → false):
    ///     → Animator.SetBool("isBlocking", false)
    ///     → blockHitbox.SetActive(false)
    /// </summary>
    void Update()
    {
        bool input = Input.GetKey(blockKey);

        // Solo ejecutar lógica cuando el estado de bloqueo cambia (no cada frame)
        if (input != isBlocking)
        {
            isBlocking = input;

            // Otorgar energía solo al inicio del bloqueo (transición false → true)
            if (isBlocking)
            {
                EnergySystem energy = GetComponent<EnergySystem>();
                if (energy != null)
                    energy.GainEnergyFromBlock();
            }

            // Sincronizar animación con el estado actual de bloqueo
            if (anim != null)
                anim.SetBool("isBlocking", isBlocking);

            // Activar o desactivar el hitbox de bloqueo según el estado
            if (blockHitbox != null)
                blockHitbox.SetActive(isBlocking);
        }
    }

    // ============================================================
    // API PÚBLICA
    // ============================================================

    /// <summary>
    /// Expone el estado de bloqueo para que Hitbox.cs pueda consultarlo
    /// durante la detección de colisiones y decidir si aplicar daño
    /// o hacer rebotar al atacante.
    /// </summary>
    /// <returns>true si el jugador está bloqueando; false si no.</returns>
    public bool IsBlocking()
    {
        return isBlocking;
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * ACTIVAR BLOQUEO:
     * 1. GetKey(L) = true + isBlocking = false → cambio de estado.
     * 2. isBlocking = true.
     * 3. EnergySystem.GainEnergyFromBlock() → +3 de energía (una sola vez).
     * 4. Animator.SetBool("isBlocking", true) → animación de guardia.
     * 5. blockHitbox.SetActive(true) → hitbox de bloqueo activo.
     *
     * MIENTRAS SE BLOQUEA:
     * 6. input == isBlocking (ambos true) → no se ejecuta nada más.
     * 7. Hitbox.cs detecta colisión → IsBlocking() = true → rebote al atacante.
     *
     * DESACTIVAR BLOQUEO:
     * 8. GetKey(L) = false + isBlocking = true → cambio de estado.
     * 9. isBlocking = false.
     * 10. Animator.SetBool("isBlocking", false) → animación normal.
     * 11. blockHitbox.SetActive(false) → hitbox desactivado.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * AFECTA A:
     *   - EnergySystem.cs → GainEnergyFromBlock() al inicio del bloqueo.
     *   - Animator → SetBool("isBlocking") para controlar animación.
     *   - blockHitbox (GameObject) → SetActive on/off.
     *
     * ES CONSULTADO POR:
     *   - Hitbox.cs → IsBlocking() para determinar si aplicar daño o rebote.
     *
     * NO DEPENDE DE ningún otro sistema de combate en runtime.
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ La ganancia de energía (+3) ocurre SOLO al inicio del bloqueo
     *   (transición false → true), no mientras se mantiene bloqueando.
     *   Esto es intencional para no recompensar el bloqueo continuo.
     *
     * ⚠ El parámetro "isBlocking" del Animator debe existir como Bool
     *   en el Animator Controller. Si no existe, Unity lanzará un warning
     *   pero el sistema seguirá funcionando.
     *
     * ⚠ Este script no bloquea el input de ataque ni de movimiento mientras
     *   se está bloqueando. Si se desea que el bloqueo cancele ataques,
     *   añadir lógica en PlayerAttack.cs para verificar PlayerDefense.IsBlocking().
     *
     * ⚠ blockHitbox debe tener un Collider2D en modo Trigger para que
     *   Hitbox.cs pueda detectar el bloqueo correctamente mediante
     *   OnTriggerStay2D. Sin esto, el rebote al atacante no funcionará.
     */
}
