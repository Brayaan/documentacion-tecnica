// ============================================================
// ARCHIVO: PlayerAttack.cs
// PROPÓSITO: Gestionar el sistema de ataques del jugador:
//            puñetazo y patada, sus hitboxes, cooldowns y animaciones.
// RESPONSABILIDAD: Capturar input de ataque, activar hitboxes durante
//                  la ventana de daño y sincronizar el estado de ataque
//                  con PlayerMovement.
// SISTEMA: Combate — Input y Control de Ataques
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script es el controlador central de los ataques del jugador.
 * Escucha el input de teclado para puñetazo (J) y patada (K),
 * dispara las animaciones correspondientes, activa el hitbox durante
 * la ventana de daño (attackDuration) y lo desactiva al terminar.
 *
 * Implementa:
 *   - Cooldown independiente para puñetazo y patada.
 *   - Flag isAttacking para evitar ataques superpuestos.
 *   - Sincronización del estado isAttacking con PlayerMovement.
 *   - API pública (IsAttacking()) para que Hitbox y ExtendedHitbox
 *     verifiquen si deben aplicar daño.
 *
 * SISTEMA: Combate / Input de Ataque
 * INTERACTÚA CON: PlayerMovement.cs, Hitbox.cs, ExtendedHitbox.cs, Animator
 */

using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // -------------------------
    // REFERENCIAS EXTERNAS
    // -------------------------

    /// <summary>
    /// Referencia al script de movimiento del jugador.
    /// Necesaria para sincronizar el flag isAttacking con PlayerMovement,
    /// que puede usar ese dato para modificar el comportamiento del movimiento.
    /// Debe asignarse desde el Inspector.
    /// </summary>
    public PlayerMovement playerMovement;

    // -------------------------
    // HITBOXES
    // -------------------------

    /// <summary>
    /// GameObject del hitbox de puñetazo.
    /// Se activa/desactiva durante la ventana de daño del puñetazo.
    /// Debe asignarse desde el Inspector. El GameObject debe tener un
    /// Collider2D en modo Trigger y el script Hitbox.cs.
    /// </summary>
    public GameObject punchHitbox;

    /// <summary>
    /// GameObject del hitbox de patada.
    /// Se activa/desactiva durante la ventana de daño de la patada.
    /// Debe asignarse desde el Inspector. El GameObject debe tener un
    /// Collider2D en modo Trigger y el script Hitbox.cs.
    /// </summary>
    public GameObject kickHitbox;

    // -------------------------
    // TECLAS DE INPUT
    // -------------------------

    /// <summary>
    /// Tecla asignada al puñetazo. Por defecto: J.
    /// Configurable desde el Inspector para remapeo de controles.
    /// </summary>
    public KeyCode punchKey = KeyCode.J;

    /// <summary>
    /// Tecla asignada a la patada. Por defecto: K.
    /// Configurable desde el Inspector para remapeo de controles.
    /// </summary>
    public KeyCode kickKey = KeyCode.K;

    // -------------------------
    // COOLDOWNS Y ESTADO
    // -------------------------

    /// <summary>
    /// Flag que indica si la patada está disponible para ejecutarse.
    /// Se pone en false al atacar y vuelve a true después de kickCooldown segundos.
    /// </summary>
    private bool kickReady = true;

    /// <summary>
    /// Tiempo en segundos que debe pasar antes de poder usar la patada de nuevo.
    /// Configurable desde el Inspector.
    /// </summary>
    public float kickCooldown = 0.5f;

    /// <summary>
    /// Flag que indica si el puñetazo está disponible para ejecutarse.
    /// Se pone en false al atacar y vuelve a true después de punchCooldown segundos.
    /// </summary>
    private bool punchReady = true;

    /// <summary>
    /// Tiempo en segundos que debe pasar antes de poder usar el puñetazo de nuevo.
    /// Configurable desde el Inspector.
    /// </summary>
    public float punchCooldown = 0.3f;

    /// <summary>
    /// Flag global de ataque activo.
    /// true = el jugador está ejecutando un ataque y no puede iniciar otro.
    /// false = el jugador puede iniciar un nuevo ataque.
    /// También se sincroniza con PlayerMovement.isAttacking.
    /// </summary>
    private bool isAttacking = false;

    /// <summary>
    /// Tiempo en segundos que el hitbox permanece activo durante un ataque.
    /// Define la "ventana de daño": cuánto tiempo puede conectar el golpe.
    /// También controla la duración del estado isAttacking.
    /// </summary>
    private float attackDuration = 0.3f;

    // -------------------------
    // REFERENCIAS INTERNAS
    // -------------------------

    /// <summary>
    /// Referencia al Animator del jugador.
    /// Se obtiene en Start(). Necesario para disparar triggers de animación.
    /// </summary>
    private Animator anim;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar.
    /// Obtiene el Animator y asegura que ambos hitboxes están desactivados.
    /// Los hitboxes SIEMPRE deben comenzar desactivados para evitar
    /// detecciones falsas al iniciar la escena.
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
    /// Update() — Se ejecuta cada frame.
    /// Lee el input de teclado para puñetazo y patada.
    /// Verifica cooldown, flag de ataque activo y disponibilidad antes de atacar.
    ///
    /// CUÁNDO SE EJECUTA: Cada frame durante la partida.
    /// QUÉ CONTROLA: Input de ataques, inicio de StartAttack(), activación de hitboxes,
    ///               triggers de animación y cooldowns.
    ///
    /// ORDEN DE VERIFICACIÓN (puñetazo):
    ///   1. Input detectado (GetKeyDown).
    ///   2. No hay ataque en curso (!isAttacking).
    ///   3. Puñetazo disponible (punchReady).
    ///   → StartAttack() + animación + hitbox activo + cooldown.
    /// </summary>
    void Update()
    {
        // PUÑETAZO: verificar input, estado de ataque y cooldown
        if (Input.GetKeyDown(punchKey) && !isAttacking && punchReady)
        {
            StartAttack();
            anim.SetTrigger("punch");
            ActivarHitbox();
            Invoke(nameof(DesactivarHitbox), attackDuration);

            punchReady = false;
            Invoke(nameof(ResetPunch), punchCooldown);
        }

        // PATADA: verificar cooldown, input y estado de ataque
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

    // ============================================================
    // MÉTODOS PRINCIPALES
    // ============================================================

    /// <summary>
    /// Inicia el estado de ataque global.
    /// Pone isAttacking en true y lo sincroniza con PlayerMovement.
    /// Programa EndAttack() para que se ejecute después de attackDuration.
    ///
    /// CUÁNDO SE LLAMA: Al inicio de cada ataque (puñetazo o patada).
    /// QUÉ AFECTA: isAttacking, PlayerMovement.isAttacking.
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
    /// Finaliza el estado de ataque y libera el sistema para el próximo ataque.
    /// Se ejecuta automáticamente mediante Invoke() al vencer attackDuration.
    ///
    /// CUÁNDO SE LLAMA: Después de attackDuration segundos desde StartAttack().
    /// QUÉ AFECTA: isAttacking, PlayerMovement.isAttacking.
    /// </summary>
    void EndAttack()
    {
        isAttacking = false;

        // Limpiar flag de ataque en PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.isAttacking = false;
        }
    }

    // ============================================================
    // API PÚBLICA
    // ============================================================

    /// <summary>
    /// Expone el estado de ataque para que Hitbox y ExtendedHitbox
    /// puedan verificar si deben procesar colisiones.
    /// </summary>
    /// <returns>true si hay un ataque en curso; false en caso contrario.</returns>
    public bool IsAttacking()
    {
        return isAttacking;
    }

    // ============================================================
    // MÉTODOS DE HITBOX
    // ============================================================

    /// <summary>Activa el hitbox de puñetazo. Llamado al inicio del puñetazo.</summary>
    public void ActivarHitbox()
    {
        if (punchHitbox != null)
            punchHitbox.SetActive(true);
    }

    /// <summary>Desactiva el hitbox de puñetazo. Llamado al terminar la ventana de daño.</summary>
    public void DesactivarHitbox()
    {
        if (punchHitbox != null)
            punchHitbox.SetActive(false);
    }

    /// <summary>Activa el hitbox de patada. Llamado al inicio de la patada.</summary>
    public void ActivarKickHitbox()
    {
        if (kickHitbox != null)
            kickHitbox.SetActive(true);
    }

    /// <summary>Desactiva el hitbox de patada. Llamado al terminar la ventana de daño.</summary>
    public void DesactivarKickHitbox()
    {
        if (kickHitbox != null)
            kickHitbox.SetActive(false);
    }

    // ============================================================
    // MÉTODOS AUXILIARES — RESET DE COOLDOWNS
    // ============================================================

    /// <summary>
    /// Reactiva la disponibilidad de la patada después de kickCooldown segundos.
    /// Se ejecuta mediante Invoke() desde Update().
    /// </summary>
    void ResetKick()
    {
        kickReady = true;
    }

    /// <summary>
    /// Reactiva la disponibilidad del puñetazo después de punchCooldown segundos.
    /// Se ejecuta mediante Invoke() desde Update().
    /// </summary>
    void ResetPunch()
    {
        punchReady = true;
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * PUÑETAZO:
     * 1. GetKeyDown(J) + !isAttacking + punchReady → StartAttack().
     * 2. anim.SetTrigger("punch") → inicia animación.
     * 3. punchHitbox.SetActive(true) → ventana de daño abierta.
     * 4. Después de attackDuration → DesactivarHitbox() + EndAttack().
     * 5. Después de punchCooldown → ResetPunch() → punchReady = true.
     *
     * PATADA: mismo flujo con kickHitbox, trigger "kick" y kickCooldown.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * AFECTA A:
     *   - PlayerMovement.isAttacking → sincronización de estado.
     *   - Hitbox.cs → lee IsAttacking() para validar golpes.
     *   - ExtendedHitbox.cs → lee IsAttacking() para validar golpes.
     *   - Animator → triggers "punch" y "kick".
     *   - punchHitbox / kickHitbox → SetActive on/off.
     *
     * NO DEPENDE DE ningún sistema de combate en runtime
     * (solo necesita PlayerMovement, Animator e Inspector).
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ attackDuration controla tanto la ventana de daño del hitbox
     *   como la duración del estado isAttacking. Si se necesita que
     *   la ventana de daño sea diferente a la duración del estado de ataque,
     *   separar en dos variables distintas.
     *
     * ⚠ Invoke() no se cancela si el script se deshabilita durante el ataque.
     *   Si PlayerAttack se deshabilita a mitad del ataque, EndAttack() y
     *   los reset de hitboxes podrían no ejecutarse, dejando el sistema en
     *   estado inválido. Considerar usar CancelInvoke() en OnDisable().
     *
     * ⚠ Los triggers de Animator "punch" y "kick" deben existir en el
     *   Animator Controller. Si no existen, Unity lanzará un warning
     *   y la animación no se reproducirá, pero el resto del sistema seguirá
     *   funcionando normalmente.
     */
}
