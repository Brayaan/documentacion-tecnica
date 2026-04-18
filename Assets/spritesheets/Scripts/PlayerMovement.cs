// ============================================================
// ARCHIVO: PlayerMovement.cs
// PROPÓSITO: Controlar el movimiento completo del jugador:
//            desplazamiento horizontal, salto, agachado, volteo de sprite,
//            detección de suelo/paredes y sistema de knockback.
// RESPONSABILIDAD: Gestionar el input de movimiento y traducirlo en
//                  física y animaciones de forma coherente con el estado de combate.
// SISTEMA: Movimiento — Física y Control del Jugador
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script es el núcleo del control de movimiento del jugador.
 * Procesa el input de teclado para mover, saltar y agacharse,
 * ajustando el Collider2D dinámicamente al agacharse y detectando
 * colisiones con suelo, techo y paredes mediante raycasts y OverlapCircle.
 *
 * Implementa un sistema de knockback (ApplyKnockback) que deshabilita
 * temporalmente el script completo para que el movimiento no interfiera
 * con la física del golpe. Al terminar el knockback, el control se restaura.
 *
 * También detecta colisiones con otros personajes (characterLayer) para
 * evitar que los jugadores se empujen entre sí durante el movimiento normal.
 *
 * SISTEMA: Movimiento / Física del Jugador
 * INTERACTÚA CON: PlayerAttack.cs (isAttacking), Rigidbody2D, Animator
 */

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------
    // PARÁMETROS DE MOVIMIENTO
    // -------------------------

    /// <summary>
    /// Velocidad de desplazamiento horizontal normal.
    /// Configurable desde el Inspector.
    /// </summary>
    public float speed = 3f;

    /// <summary>
    /// Fuerza aplicada al Rigidbody2D al saltar.
    /// Configurable desde el Inspector. Valor más alto = mayor altura de salto.
    /// </summary>
    public float jumpForce = 10f;

    /// <summary>
    /// Velocidad de desplazamiento horizontal mientras el jugador está agachado.
    /// Normalmente menor que speed para reflejar la posición comprimida.
    /// </summary>
    public float crouchSpeed = 2f;

    // -------------------------
    // KNOCKBACK
    // -------------------------

    [Header("Knockback Settings")]

    /// <summary>
    /// Si es true, el jugador puede recibir knockback al ser golpeado.
    /// Si es false, el knockback se ignora completamente.
    /// Útil para momentos de invencibilidad o personajes especiales.
    /// </summary>
    public bool canBeKnockedBack = true;

    /// <summary>
    /// Fuerza del knockback que este jugador puede aplicar a otros personajes.
    /// También se usa en ApplyKnockback() como fuerza propia de salida.
    /// </summary>
    public float knockbackForce = 5f;

    /// <summary>
    /// Duración en segundos del estado de knockback.
    /// Durante este tiempo el script está deshabilitado (input bloqueado).
    /// </summary>
    public float knockbackDuration = 0.3f;

    /// <summary>
    /// Flag de knockback activo.
    /// true = el jugador está volando por un knockback, Update y FixedUpdate retornan.
    /// false = comportamiento de movimiento normal.
    /// </summary>
    private bool isKnockedBack = false;

    // -------------------------
    // DETECCIÓN DE ENTORNO
    // -------------------------

    /// <summary>
    /// Distancia máxima del raycast para detectar paredes laterales.
    /// Configurable desde el Inspector.
    /// </summary>
    public float wallCheckDistance = 0.2f;

    // -------------------------
    // REFERENCIAS DE COLISIÓN
    // -------------------------

    /// <summary>
    /// Rigidbody2D del jugador. Marcado [SerializeField] para asignarlo
    /// desde el Inspector o fallback a GetComponent en Start().
    /// </summary>
    [SerializeField] private Rigidbody2D rb;

    /// <summary>
    /// Transform del punto de detección de suelo.
    /// Debe ser un hijo del jugador posicionado en sus pies.
    /// Se usa con OverlapCircle para detectar si está en el suelo.
    /// </summary>
    public Transform groundCheck;

    /// <summary>
    /// Transform del punto de detección de techo.
    /// Debe ser un hijo del jugador posicionado en su cabeza.
    /// Bloquea el levantarse del agachado si hay geometría encima.
    /// </summary>
    public Transform ceilingCheck;

    /// <summary>
    /// Transform del punto de detección de pared lateral.
    /// Debe ser un hijo del jugador en su costado.
    /// Se usa con Raycast para detectar paredes y otros personajes.
    /// </summary>
    public Transform wallCheck;

    /// <summary>
    /// LayerMask de las capas que se consideran suelo y paredes.
    /// Usado en groundCheck, ceilingCheck y wallCheck para raycast/overlap.
    /// Configurar desde el Inspector con la capa Ground.
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// LayerMask de las capas que corresponden a personajes (Player, Duplicate).
    /// Usado en FixedUpdate para detectar colisiones con otros jugadores
    /// y detener el movimiento al hacer contacto.
    /// Debe incluir todas las capas de personajes jugables.
    /// </summary>
    public LayerMask characterLayer;

    // -------------------------
    // COLISIONADOR Y ANIMATOR
    // -------------------------

    /// <summary>
    /// BoxCollider2D del jugador.
    /// Se redimensiona dinámicamente al agacharse (crouchSize) y al levantarse (originalSize).
    /// Debe asignarse desde el Inspector. Es requerido: sin él el script se deshabilita en Start().
    /// </summary>
    public BoxCollider2D boxCollider;

    /// <summary>
    /// Animator del jugador.
    /// Se controla con parámetros: Speed (float), isJumping (bool), isCrouching (bool).
    /// Debe asignarse desde el Inspector.
    /// </summary>
    public Animator animator;

    // -------------------------
    // ESTADO PÚBLICO DE COMBATE
    // -------------------------

    /// <summary>
    /// true = el jugador está agachado actualmente.
    /// Accesible públicamente por si otros sistemas necesitan conocer este estado.
    /// </summary>
    public bool isCrouching;

    /// <summary>
    /// true = el jugador mira hacia la derecha; false = hacia la izquierda.
    /// Controla la inversión de escala al voltear el sprite con Flip().
    /// </summary>
    public bool facingRight = true;

    /// <summary>
    /// true = el jugador está ejecutando un ataque.
    /// PlayerAttack.cs lo escribe directamente en este campo.
    /// Disponible para que otros sistemas (movimiento, animaciones) sepan
    /// si el jugador está atacando.
    /// </summary>
    public bool isAttacking;

    // -------------------------
    // TECLAS DE INPUT
    // -------------------------

    /// <summary>Tecla para moverse a la izquierda. Por defecto: A.</summary>
    public KeyCode leftKey = KeyCode.A;

    /// <summary>Tecla para moverse a la derecha. Por defecto: D.</summary>
    public KeyCode rightKey = KeyCode.D;

    /// <summary>Tecla para saltar. Por defecto: Space.</summary>
    public KeyCode jumpKey = KeyCode.Space;

    /// <summary>Tecla para agacharse. Por defecto: S.</summary>
    public KeyCode crouchKey = KeyCode.S;

    // -------------------------
    // VARIABLES INTERNAS DE MOVIMIENTO
    // -------------------------

    /// <summary>
    /// Input horizontal procesado del frame actual (-1, 0 o 1 multiplicado por velocidad).
    /// Se calcula en Update() y se aplica en FixedUpdate() al Rigidbody2D.
    /// </summary>
    private float moveInput;

    /// <summary>true si el jugador toca el suelo detectado por groundCheck.</summary>
    private bool isGrounded;

    /// <summary>true si hay geometría sobre el jugador que bloquea levantarse del agachado.</summary>
    private bool isBlocked;

    /// <summary>true si el jugador está tocando una pared lateral en la dirección del input.</summary>
    private bool isTouchingWall;

    // -------------------------
    // DATOS DEL COLLIDER (TAMAÑOS)
    // -------------------------

    /// <summary>Tamaño original del BoxCollider2D guardado en Start().</summary>
    private Vector2 originalSize;

    /// <summary>Tamaño del BoxCollider2D reducido a la mitad vertical para el agachado.</summary>
    private Vector2 crouchSize;

    /// <summary>Offset original del BoxCollider2D guardado en Start().</summary>
    private Vector2 originalOffset;

    /// <summary>Offset ajustado del BoxCollider2D durante el agachado.</summary>
    private Vector2 crouchOffset;

    /// <summary>Escala local original del transform (usada para restaurar después de voltear).</summary>
    private Vector3 originalScale;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar.
    /// Verifica que boxCollider esté asignado (requerido), guarda las
    /// dimensiones originales del collider y calcula las del agachado.
    /// También obtiene el Rigidbody2D si no fue asignado en el Inspector.
    ///
    /// NOTA: Si boxCollider es null, el script se deshabilita automáticamente
    ///       para evitar NullReferenceExceptions en Update/FixedUpdate.
    /// </summary>
    void Start()
    {
        // Guard: boxCollider es necesario antes de leer su tamaño
        if (boxCollider == null)
        {
            Debug.LogError("boxCollider no está asignado en el Inspector de " + gameObject.name, this);
            enabled = false;
            return;
        }

        // Guardar dimensiones originales para restaurar al salir del agachado
        originalSize = boxCollider.size;
        originalOffset = boxCollider.offset;
        originalScale = transform.localScale;

        // Calcular dimensiones del collider en agachado (mitad de altura)
        crouchSize = new Vector2(originalSize.x, originalSize.y / 2);
        crouchOffset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y / 4));

        // Obtener Rigidbody2D si no fue asignado en el Inspector
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Update() — Se ejecuta cada frame.
    /// Procesa todo el input de movimiento, salto, agachado y volteo de sprite.
    /// Actualiza el estado del Animator y redimensiona el collider según el estado.
    ///
    /// CUÁNDO SE EJECUTA: Cada frame. Si isKnockedBack = true, retorna inmediatamente.
    /// QUÉ CONTROLA:
    ///   - Input horizontal → moveInput
    ///   - Detección de paredes y agachado
    ///   - Salto con verificación de suelo
    ///   - Volteo del sprite (Flip)
    ///   - Parámetros del Animator (Speed, isJumping, isCrouching)
    ///   - Redimensionado del BoxCollider2D al agacharse/levantarse
    /// </summary>
    void Update()
    {
        // Bloquear todo input durante el vuelo del knockback
        if (isKnockedBack) return;

        // Guard: verificar referencias críticas antes de ejecutar lógica
        if (wallCheck == null || groundCheck == null || ceilingCheck == null || boxCollider == null || animator == null)
        {
            Debug.LogError("Faltan referencias requeridas en el Inspector de " + gameObject.name, this);
            return;
        }

        float input = 0;

        if (Input.GetKey(leftKey))
            input = -1;
        else if (Input.GetKey(rightKey))
            input = 1;

        // Detectar pared lateral con raycast en la dirección del input
        if (input != 0)
            isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(input), wallCheckDistance, groundLayer);
        else
            isTouchingWall = false;

        // Determinar velocidad efectiva según estado del personaje
        if (isTouchingWall)
            moveInput = 0;
        else if (isCrouching)
            // Guard: evitar división por cero si speed es inválido
            moveInput = speed > 0 ? input * (crouchSpeed / speed) : 0;
        else
            moveInput = input;

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // Voltear el sprite si cambia la dirección horizontal
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();

        // Detectar si el personaje toca el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Detectar techo para bloquear levantarse del agachado
        isBlocked = Physics2D.OverlapCircle(ceilingCheck.position, 0.2f, groundLayer);

        if (Input.GetKeyDown(jumpKey) && isGrounded && !isCrouching)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        animator.SetBool("isJumping", !isGrounded);

        if (Input.GetKey(crouchKey))
            isCrouching = true;
        else if (!isBlocked)
            isCrouching = false;

        animator.SetBool("isCrouching", isCrouching);

        // Redimensionar el BoxCollider2D al agacharse y al levantarse
        if (isCrouching)
        {
            boxCollider.size = crouchSize;
            boxCollider.offset = crouchOffset;
        }
        else
        {
            boxCollider.size = originalSize;
            boxCollider.offset = originalOffset;
        }
    }

    /// <summary>
    /// FixedUpdate() — Se ejecuta en el ciclo de física (por defecto 50Hz).
    /// Aplica la velocidad horizontal calculada en Update() al Rigidbody2D.
    /// También detecta colisiones con otros personajes y detiene el movimiento
    /// al hacer contacto para evitar empujes físicos entre jugadores.
    ///
    /// CUÁNDO SE EJECUTA: Cada paso de física. Si isKnockedBack = true, retorna.
    /// QUÉ CONTROLA:
    ///   - Aplicación de velocidad horizontal al Rigidbody2D.
    ///   - Detección de otros personajes mediante RaycastAll con characterLayer.
    ///   - Bloqueo de movimiento al tocar a otro personaje.
    /// </summary>
    void FixedUpdate()
    {
        if (isKnockedBack) return;

        // wallCheck es requerido antes de ejecutar el raycast
        if (wallCheck == null) return;

        float move = moveInput;

        if (moveInput != 0)
        {
            // RaycastAll en characterLayer para detectar colisión con otro personaje
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                wallCheck.position,
                Vector2.right * Mathf.Sign(moveInput),
                0.1f,
                characterLayer);

            foreach (RaycastHit2D hit in hits)
            {
                // Saltar colliders que pertenecen a esta misma jerarquía (autocolisión)
                if (hit.collider.transform.root == transform.root) continue;

                bool hitsCharacter = ((1 << hit.collider.gameObject.layer) & characterLayer) != 0;

                if (hitsCharacter)
                {
                    // Detener este personaje al hacer contacto con el oponente
                    move = 0;
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                    // No zerear velocidad del oponente si está en knockback activo
                    Rigidbody2D otherRb = hit.collider.transform.root.GetComponent<Rigidbody2D>();
                    if (otherRb != null)
                    {
                        PlayerMovement otherMovement = hit.collider.transform.root.GetComponent<PlayerMovement>();
                        if (otherMovement == null || !otherMovement.isKnockedBack)
                            otherRb.linearVelocity = new Vector2(0, otherRb.linearVelocity.y);
                    }

                    break;
                }
            }
        }

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    /// <summary>
    /// Invierte la dirección del sprite horizontalmente.
    /// Cambia facingRight y niega la escala X del transform.
    ///
    /// CUÁNDO SE LLAMA: Desde Update() cuando moveInput cambia de signo.
    /// QUÉ AFECTA: transform.localScale.x y el flag facingRight.
    /// </summary>
    void Flip()
    {
        facingRight = !facingRight;

        // Invertir escala horizontal para voltear el sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ============================================================
    // API PÚBLICA — KNOCKBACK
    // ============================================================

    /// <summary>
    /// Aplica un knockback al jugador desde una posición de atacante.
    /// Deshabilita el script completo durante knockbackDuration para que
    /// el input no interfiera con el vuelo del knockback.
    ///
    /// CUÁNDO SE LLAMA: Externamente cuando el jugador recibe un golpe
    ///                  (no usado actualmente en el flujo principal; ver HealthSystem).
    /// QUÉ AFECTA: isKnockedBack, rb (fuerza), this.enabled (script deshabilitado).
    ///
    /// NOTA: canBeKnockedBack e isKnockedBack previenen knockback doble.
    /// </summary>
    /// <param name="attackerPosition">Posición del atacante para calcular dirección del vuelo.</param>
    public void ApplyKnockback(Vector2 attackerPosition)
    {
        if (!canBeKnockedBack || isKnockedBack) return;

        isKnockedBack = true;

        // Calcular dirección opuesta al atacante
        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // El eje Y tiene un valor fijo de 2f para pequeña elevación del vuelo
            rb.AddForce(new Vector2(direction.x * knockbackForce, 2f), ForceMode2D.Impulse);
        }

        // Deshabilitar script para que Update no interfiera durante el knockback
        this.enabled = false;

        // Mathf.Max evita que duration negativo rompa el Invoke
        Invoke(nameof(EndKnockback), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Finaliza el knockback y restaura el control del jugador.
    /// Se llama automáticamente mediante Invoke() al vencer knockbackDuration.
    /// Limpia la velocidad horizontal residual al aterrizar.
    /// </summary>
    private void EndKnockback()
    {
        isKnockedBack = false;

        // Restaurar control del jugador al terminar el knockback
        this.enabled = true;

        // Eliminar velocidad horizontal residual para aterrizar limpiamente
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * MOVIMIENTO NORMAL:
     * 1. Update() lee input → calcula moveInput según estado (agachado, pared, libre).
     * 2. Update() voltea sprite si cambia dirección, detecta suelo/techo, actualiza Animator.
     * 3. Update() redimensiona BoxCollider2D si cambia estado de agachado.
     * 4. FixedUpdate() aplica moveInput * speed al Rigidbody2D.
     * 5. FixedUpdate() detecta otros personajes → bloquea movimiento al contacto.
     *
     * KNOCKBACK:
     * 1. ApplyKnockback(position) → isKnockedBack = true, this.enabled = false.
     * 2. Update/FixedUpdate retornan inmediatamente → sin input.
     * 3. Rigidbody2D vuela libremente por la fuerza de knockback.
     * 4. Después de knockbackDuration → EndKnockback() → this.enabled = true.
     * 5. Velocidad horizontal residual limpiada → jugador recupera control.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * ES AFECTADO POR:
     *   - HealthSystem.cs → deshabilita/habilita este script durante hit-stun.
     *   - PlayerAttack.cs → escribe en isAttacking directamente.
     *
     * AFECTA A:
     *   - Rigidbody2D del jugador (velocidad y knockback).
     *   - BoxCollider2D (redimensionado en agachado).
     *   - Animator (Speed, isJumping, isCrouching).
     *   - Rigidbody2D del oponente (zereo de velocidad horizontal al contacto).
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ boxCollider ES OBLIGATORIO. Sin él, el script se deshabilita en Start()
     *   y el jugador quedará completamente inmovilizado.
     *
     * ⚠ characterLayer DEBE incluir tanto la capa "Player" como "Duplicate"
     *   (u otras capas de personajes). Si falta alguna capa, los personajes
     *   se empujarán físicamente en lugar de detenerse.
     *
     * ⚠ ApplyKnockback() en este script no es el sistema principal de knockback
     *   para el jugador. HealthSystem.cs gestiona el knockback del jugador
     *   deshabilitando directamente este script. ApplyKnockback() está disponible
     *   como alternativa pero actualmente no se llama en el flujo de combate.
     *
     * ⚠ Invoke() no se cancela si el script se deshabilita.
     *   EndKnockback() se ejecutará aunque el objeto esté desactivado.
     *   Agregar CancelInvoke(nameof(EndKnockback)) en OnDisable() si esto
     *   pudiera causar problemas en escenas con respawn.
     *
     * ⚠ La detección de personajes en FixedUpdate usa RaycastAll con distancia 0.1f.
     *   Si los personajes están muy separados o el frame rate es muy bajo,
     *   pueden "atravesarse" antes de que el raycast los detecte.
     */
}
