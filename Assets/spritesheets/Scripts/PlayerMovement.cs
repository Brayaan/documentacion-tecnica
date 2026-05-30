/// <summary>
/// File: PlayerMovement.cs
/// Description: Handles all aspects of player movement including walking, jumping, crouching, and knockback handling.
/// </summary>

using UnityEngine;

/// <summary>
/// Manages player movement physics, input handling, and animation synchronization.
/// Also handles collision detection for grounded states and obstacle checking.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /// <summary>Base movement speed of the player.</summary>
    public float speed = 3f;
    
    /// <summary>Force applied when the player jumps.</summary>
    public float jumpForce = 10f;

    [Header("Knockback Settings")]
    /// <summary>Determines whether the player can be knocked back.</summary>
    public bool canBeKnockedBack = true;
    
    /// <summary>The force applied when the player receives a knockback.</summary>
    public float knockbackForce = 5f;
    
    /// <summary>The duration in seconds of the knockback effect.</summary>
    public float knockbackDuration = 0.3f;
    
    /// <summary>Internal flag to track if the player is currently in a knockback state.</summary>
    // Bloquea Update y FixedUpdate mientras el personaje vuela
    private bool isKnockedBack = false;

    /// <summary>Distance to check for walls in front of the player.</summary>
    public float wallCheckDistance = 0.2f;
    
    /// <summary>Movement speed when the player is crouching.</summary>
    public float crouchSpeed = 2f;

    /// <summary>Reference to the player's Rigidbody2D component.</summary>
    [SerializeField] private Rigidbody2D rb;
    
    /// <summary>Transform used to check if the player is on the ground.</summary>
    public Transform groundCheck;
    
    /// <summary>Transform used to check if there is a ceiling above the player.</summary>
    public Transform ceilingCheck;
    
    /// <summary>Layer mask representing what surfaces are considered ground.</summary>
    public LayerMask groundLayer;
    
    /// <summary>Layer mask representing what colliders belong to characters.</summary>
    // Debe incluir las capas Player y Duplicate
    public LayerMask characterLayer;
    
    /// <summary>Transform used to check for walls in front of the player.</summary>
    public Transform wallCheck;

    /// <summary>Reference to the player's primary BoxCollider2D.</summary>
    public BoxCollider2D boxCollider;
    
    /// <summary>Reference to the Animator component for controlling movement animations.</summary>
    public Animator animator;

    /// <summary>Indicates whether the player is currently crouching.</summary>
    public bool isCrouching;
    
    /// <summary>Indicates whether the player is facing to the right.</summary>
    public bool facingRight = true;
    
    /// <summary>Flag to track if the player is currently attacking.</summary>
    public bool isAttacking;

    /// <summary>Key used to move left.</summary>
    public KeyCode leftKey = KeyCode.A;
    
    /// <summary>Key used to move right.</summary>
    public KeyCode rightKey = KeyCode.D;
    
    /// <summary>Key used to jump.</summary>
    public KeyCode jumpKey = KeyCode.Space;
    
    /// <summary>Key used to crouch.</summary>
    public KeyCode crouchKey = KeyCode.S;

    /// <summary>The calculated horizontal movement input value.</summary>
    private float moveInput;
    
    /// <summary>Indicates whether the player is currently touching the ground.</summary>
    private bool isGrounded;
    
    /// <summary>Indicates whether standing up is blocked by an overhead obstacle.</summary>
    private bool isBlocked;
    
    /// <summary>Indicates whether the player is touching a wall.</summary>
    private bool isTouchingWall;

    /// <summary>Original size of the box collider.</summary>
    private Vector2 originalSize;
    
    /// <summary>Modified size of the box collider when crouching.</summary>
    private Vector2 crouchSize;

    /// <summary>Original offset of the box collider.</summary>
    private Vector2 originalOffset;
    
    /// <summary>Modified offset of the box collider when crouching.</summary>
    private Vector2 crouchOffset;
    
    /// <summary>Original scale of the player transform.</summary>
    private Vector3 originalScale;

    /// <summary>
    /// Initializes necessary components and dimensions required for movement and crouching.
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

        // Guardar dimensiones para restaurar al salir del agachado
        originalSize = boxCollider.size;
        originalOffset = boxCollider.offset;
        originalScale = transform.localScale;

        crouchSize = new Vector2(originalSize.x, originalSize.y / 2);
        crouchOffset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y / 4));

        // Obtener Rigidbody2D si no fue asignado en el Inspector
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Processes player input, manages physical states (grounded, blocked), and updates animations.
    /// </summary>
    void Update()
    {
        // Bloquear todo input durante el vuelo del knockback
        if (isKnockedBack) return;

        // Guard: verificar referencias críticas antes de usar en Update
        if (wallCheck == null || groundCheck == null || ceilingCheck == null || boxCollider == null || animator == null)
        {
            Debug.LogError("Faltan referencias requeridas en el Inspector de " + gameObject.name, this);
            return;
        }

        float input = 0;

        if (Input.GetKey(leftKey))
        {
            input = -1;
        }
        else if (Input.GetKey(rightKey))
        {
            input = 1;
        }

        // Detectar pared lateral con raycast en la dirección del input
        if (input != 0)
        {
            isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * Mathf.Sign(input), wallCheckDistance, groundLayer);
        }
        else
        {
            isTouchingWall = false;
        }

        // Determinar velocidad según pared, agachado o movimiento libre
        if (isTouchingWall)
        {
            moveInput = 0;
        }
        else if (isCrouching)
        {
            // Guard: evitar división por cero si speed es inválido
            moveInput = speed > 0 ? input * (crouchSpeed / speed) : 0;
        }
        else
        {
            moveInput = input;
        }

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // Voltear el sprite si cambia la dirección horizontal
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }

        // Detectar si el personaje toca el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Detectar techo para bloquear levantarse del agachado
        isBlocked = Physics2D.OverlapCircle(ceilingCheck.position, 0.2f, groundLayer);

        if (Input.GetKeyDown(jumpKey) && isGrounded && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        animator.SetBool("isJumping", !isGrounded);

        if (Input.GetKey(crouchKey))
        {
            isCrouching = true;
        }
        else if (!isBlocked)
        {
            isCrouching = false;
        }

        animator.SetBool("isCrouching", isCrouching);

        // Redimensionar el collider físico al agacharse y al levantarse
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
    /// Applies physics calculations, handling movement and character pushing logic.
    /// </summary>
    void FixedUpdate()
    {
        if (isKnockedBack) return;

        // wallCheck es requerido antes de ejecutar el raycast
        if (wallCheck == null) return;

        float move = moveInput;

        if (moveInput != 0)
        {
            // RaycastAll filtra colliders propios por raíz de jerarquía
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                wallCheck.position,
                Vector2.right * Mathf.Sign(moveInput),
                0.1f,
                characterLayer);

            foreach (RaycastHit2D hit in hits)
            {
                // Saltar colliders que pertenecen a esta misma jerarquía
                if (hit.collider.transform.root == transform.root) continue;

                bool hitsCharacter = ((1 << hit.collider.gameObject.layer) & characterLayer) != 0;

                if (hitsCharacter)
                {
                    // Detener este personaje al tocar al oponente
                    move = 0;
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                    // No zerear velocidad del oponente si está en knockback
                    Rigidbody2D otherRb = hit.collider.transform.root.GetComponent<Rigidbody2D>();
                    if (otherRb != null)
                    {
                        PlayerMovement otherMovement = hit.collider.transform.root.GetComponent<PlayerMovement>();
                        if (otherMovement == null || !otherMovement.isKnockedBack)
                        {
                            otherRb.linearVelocity = new Vector2(0, otherRb.linearVelocity.y);
                        }
                    }

                    break;
                }
            }
        }

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Flips the character's facing direction by modifying local scale.
    /// </summary>
    void Flip()
    {
        facingRight = !facingRight;

        // Invertir escala horizontal para voltear el sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Applies knockback force to the player away from an attacker's position.
    /// </summary>
    /// <param name="attackerPosition">The origin position of the attack causing the knockback.</param>
    public void ApplyKnockback(Vector2 attackerPosition)
    {
        if (!canBeKnockedBack || isKnockedBack) return;

        isKnockedBack = true;

        // Calcular dirección del vuelo opuesta al atacante
        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction.x * knockbackForce, 2f), ForceMode2D.Impulse);
        }

        // Deshabilitar script para que Update no interfiera con el vuelo
        this.enabled = false;

        // Mathf.Max evita que duration negativo colapse el knockback
        Invoke(nameof(EndKnockback), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Ends the knockback effect and restores player control.
    /// </summary>
    private void EndKnockback()
    {
        isKnockedBack = false;
        // Restaurar control del jugador al terminar el knockback
        this.enabled = true;

        // Eliminar velocidad horizontal residual al aterrizar
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}