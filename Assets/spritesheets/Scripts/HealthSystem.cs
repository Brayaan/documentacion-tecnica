// ============================================================
// ARCHIVO: HealthSystem.cs
// PROPÓSITO: Gestionar la vida del jugador, el daño recibido,
//            el knockback, la animación de hit y la UI de salud.
// RESPONSABILIDAD: Punto de entrada de daño para el jugador.
//                  Coordina física, animación e input durante el hit-stun.
// SISTEMA: Combate — Salud del Jugador
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script controla toda la lógica de salud del jugador.
 * Cuando un enemigo (o el hitbox de otro jugador) golpea al personaje,
 * llama a TakeDamage() para:
 *   1. Reducir la vida.
 *   2. Actualizar la UI (spritesheet de corazones).
 *   3. Aplicar knockback físico.
 *   4. Deshabilitar el input del jugador durante el hit-stun.
 *   5. Activar la animación de recibir golpe.
 *
 * Implementa un sistema de hit-stun (isHit) que bloquea golpes apilados
 * y restaura el control del jugador al expirar el timer.
 *
 * SISTEMA: Combate / Salud
 * INTERACTÚA CON: Hitbox.cs, PlayerMovement.cs, Animator
 */

using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // -------------------------
    // VARIABLES PÚBLICAS
    // -------------------------

    /// <summary>
    /// Vida máxima del jugador. Define el techo de salud posible.
    /// Configurable desde el Inspector. No debe ser 0 o negativo.
    /// </summary>
    public int maxHealth = 100;

    /// <summary>
    /// Vida actual del jugador en tiempo real.
    /// Se inicializa igual a maxHealth en Start().
    /// Visible en Inspector para debug durante la partida.
    /// </summary>
    public int currentHealth;

    /// <summary>
    /// Componente Image de Unity UI que muestra visualmente la barra de vida.
    /// Debe asignarse desde el Inspector. Si es null, la UI no se actualizará.
    /// </summary>
    public Image healthImage;

    /// <summary>
    /// Fuerza del empuje físico al recibir un golpe.
    /// Configurable desde el Inspector. Valor más alto = mayor desplazamiento.
    /// </summary>
    public float knockbackForce = 5f;

    /// <summary>
    /// Duración en segundos del hit-stun:
    /// tiempo mínimo antes de poder recibir otro knockback y tiempo
    /// que el input del jugador permanece deshabilitado.
    /// Recomendación: valor bajo (0.05f) para respuesta fluida,
    /// mayor si se desea un hit-stun más pronunciado.
    /// </summary>
    public float knockbackDuration = 0.05f;

    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Referencia al Rigidbody2D del jugador.
    /// Necesario para aplicar el knockback físico. Se obtiene en Start().
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Referencia al script PlayerMovement del jugador.
    /// Se deshabilita durante el hit-stun para bloquear el input.
    /// Se restaura al finalizar el hit-stun.
    /// </summary>
    private PlayerMovement movement;

    /// <summary>
    /// Referencia al Animator del jugador.
    /// Se usa para disparar el trigger "Hit" si el Animator lo tiene registrado.
    /// </summary>
    private Animator anim;

    /// <summary>
    /// Flag de hit-stun activo.
    /// true = el jugador está en hit-stun, no puede moverse ni recibir otro knockback.
    /// false = el jugador puede recibir daño con efecto físico completo.
    /// </summary>
    private bool isHit = false;

    /// <summary>
    /// Array de sprites cargados desde el spritesheet de corazones.
    /// Cargado automáticamente desde Resources/HeartCounter/heart_counter-Sheet.
    /// Cada elemento corresponde a un nivel de vida diferente.
    /// </summary>
    private Sprite[] healthSprites;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar la escena.
    /// Carga el spritesheet de vida, inicializa currentHealth al máximo,
    /// obtiene referencias a Rigidbody2D, PlayerMovement y Animator,
    /// y actualiza la UI al estado inicial.
    /// </summary>
    void Start()
    {
        // Cargar spritesheet de vida desde la carpeta Resources
        healthSprites = Resources.LoadAll<Sprite>("HeartCounter/heart_counter-Sheet");

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();

        UpdateHealthUI();
    }

    // ============================================================
    // MÉTODOS PRINCIPALES
    // ============================================================

    /// <summary>
    /// Punto de entrada de daño para el jugador.
    /// Reduce la vida, actualiza la UI y aplica el hit-stun con knockback.
    ///
    /// CUÁNDO SE LLAMA: Desde Hitbox.cs cuando el hitbox de un enemigo o jugador
    ///                  toca el collider del jugador.
    /// QUÉ AFECTA: currentHealth, UI de vida, física del Rigidbody2D,
    ///             input del jugador (PlayerMovement), Animator.
    /// </summary>
    /// <param name="damage">Cantidad de puntos de vida a restar.</param>
    /// <param name="attackerPosition">Posición del atacante para calcular dirección del knockback.</param>
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        currentHealth -= damage;

        // Clampear vida para no bajar de cero
        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log("Vida actual: " + currentHealth);

        UpdateHealthUI();

        ApplyHit(attackerPosition);
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    /// <summary>
    /// Aplica el hit-stun al jugador: deshabilita input, activa animación
    /// de golpe y aplica knockback físico.
    ///
    /// CUÁNDO SE EJECUTA: Llamado internamente desde TakeDamage().
    /// QUÉ AFECTA: PlayerMovement (deshabilitado), Animator (trigger "Hit"),
    ///             Rigidbody2D (velocidad y fuerza), flag isHit.
    ///
    /// NOTA: Si isHit ya está activo, el método retorna sin hacer nada.
    ///       Esto es intencional para evitar stacking de knockback.
    /// </summary>
    /// <param name="attackerPosition">Posición desde donde vino el ataque.</param>
    void ApplyHit(Vector2 attackerPosition)
    {
        // Descartar golpe si el hit-stun ya está activo
        if (isHit) return;

        isHit = true;

        // Deshabilitar input del jugador durante el hit-stun
        if (movement != null)
            movement.enabled = false;

        // Activar animación de recibir golpe si existe el parámetro en el Animator
        if (anim != null && TieneParametro("Hit"))
            anim.SetTrigger("Hit");

        // Calcular dirección del empuje opuesta al atacante
        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // El eje Y tiene un valor fijo de 2f para dar pequeña elevación al impacto
            rb.AddForce(new Vector2(direction.x * knockbackForce, 2f), ForceMode2D.Impulse);
        }

        // Mathf.Max evita que un knockbackDuration negativo rompa el Invoke
        Invoke(nameof(EndHit), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Verifica si el Animator tiene un parámetro registrado por nombre.
    /// Evita errores al llamar SetTrigger con un nombre que no existe.
    ///
    /// USO EXCLUSIVO: Solo llamado desde ApplyHit() para validar el parámetro "Hit".
    /// </summary>
    /// <param name="nombre">Nombre del parámetro a buscar en el Animator.</param>
    /// <returns>true si el parámetro existe; false si no.</returns>
    bool TieneParametro(string nombre)
    {
        foreach (var param in anim.parameters)
            if (param.name == nombre) return true;
        return false;
    }

    /// <summary>
    /// Finaliza el hit-stun: reactiva el input del jugador y restablece isHit.
    /// Se llama automáticamente mediante Invoke() al vencer knockbackDuration.
    /// </summary>
    void EndHit()
    {
        isHit = false;

        // Restaurar input del jugador al salir del hit-stun
        if (movement != null)
            movement.enabled = true;
    }

    /// <summary>
    /// Actualiza el sprite de la barra de vida en la UI según currentHealth.
    /// Calcula el índice proporcional en el spritesheet e invierte el orden
    /// para que el sprite 0 del array corresponda a vida llena.
    ///
    /// CUÁNDO SE EJECUTA: Llamado desde Start() y después de cada TakeDamage().
    ///
    /// LÓGICA DE INVERSIÓN:
    ///   index normal   = proporcional a la vida actual (0 = vacío, max = lleno)
    ///   index invertido = (Length-1) - index → el array comienza desde lleno
    /// </summary>
    void UpdateHealthUI()
    {
        // Guard: evitar división por cero si maxHealth es inválido
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

        // Calcular índice proporcional según la vida actual
        int index = Mathf.RoundToInt(((float)currentHealth / maxHealth) * (healthSprites.Length - 1));

        // Invertir: index 0 del array = barra llena
        index = (healthSprites.Length - 1) - index;

        healthImage.sprite = healthSprites[index];
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. Start() → carga sprites, inicializa vida al máximo, actualiza UI.
     * 2. Hitbox.cs detecta colisión → llama TakeDamage(damage, attackerPosition).
     * 3. TakeDamage() → descuenta vida, actualiza UI, llama ApplyHit().
     * 4. ApplyHit() → verifica isHit, deshabilita PlayerMovement,
     *    activa animación "Hit", aplica fuerza al Rigidbody2D.
     * 5. Después de knockbackDuration → EndHit() → reactiva PlayerMovement.
     * 6. Si currentHealth = 0, el jugador está muerto
     *    (no hay lógica de muerte aquí; extender si se requiere).
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * ES LLAMADO POR:
     *   - Hitbox.cs → TakeDamage() cuando el hitbox de un atacante toca al jugador.
     *
     * AFECTA A:
     *   - PlayerMovement.cs → se deshabilita/habilita durante el hit-stun.
     *   - Animator → trigger "Hit" para reproducir animación de golpe.
     *   - Rigidbody2D → aplicación de knockback.
     *   - healthImage (UI) → actualización visual de la barra de vida.
     *
     * NO GESTIONA:
     *   - Muerte del jugador (no hay lógica de Game Over aquí).
     *   - EnergySystem (la energía al recibir daño se gestiona en Hitbox.cs).
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ El spritesheet DEBE estar en Resources/HeartCounter/heart_counter-Sheet.
     *   Si la ruta o el nombre cambian, los sprites no se cargarán.
     *
     * ⚠ No hay lógica de muerte del jugador en este script.
     *   Si currentHealth llega a 0, nada ocurre más allá de la UI.
     *   Implementar Game Over o respawn externamente.
     *
     * ⚠ TieneParametro() itera todos los parámetros del Animator cada vez.
     *   En Animators muy grandes podría tener impacto mínimo. Si se necesita
     *   optimizar, cachear el resultado en Start().
     *
     * ⚠ knockbackDuration NO debe ser negativo. Mathf.Max lo protege,
     *   pero valores negativos en Inspector generan comportamiento inesperado.
     */
}
