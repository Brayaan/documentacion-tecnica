// ============================================================
// ARCHIVO: EnemyHealthSystem.cs
// PROPÓSITO: Gestionar la vida, el daño recibido, el knockback
//            y la muerte de un enemigo individual.
// RESPONSABILIDAD: Punto de entrada de daño para todos los enemigos.
// SISTEMA: Combate — Salud y Reacción al Daño (Enemigos)
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script controla la salud de cada enemigo de forma independiente.
 * Cuando el jugador golpea a un enemigo, llama a TakeDamage() para
 * reducir su vida, aplicar un knockback físico y verificar si debe morir.
 *
 * Implementa un sistema de hit-stun (isHit) que previene que golpes
 * muy rápidos apilen fuerzas de knockback simultáneas, manteniendo
 * el comportamiento físico predecible y justo.
 *
 * SISTEMA: Combate / Salud de Enemigos
 * INTERACTÚA CON: Hitbox.cs, ExtendedHitbox.cs
 */

using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    // -------------------------
    // VARIABLES PÚBLICAS
    // -------------------------

    /// <summary>
    /// Vida máxima del enemigo. Define cuántos puntos de daño puede absorber
    /// antes de morir. Configurable por tipo de enemigo desde el Inspector.
    /// </summary>
    public int maxHealth = 50;

    /// <summary>
    /// Vida actual del enemigo en tiempo real.
    /// Se inicializa igual a maxHealth en Start().
    /// Visible en el Inspector para debug durante la partida.
    /// </summary>
    public int currentHealth;

    /// <summary>
    /// Fuerza del empuje físico (knockback) que recibe el enemigo al ser golpeado.
    /// Configurable desde el Inspector. Valor más alto = mayor desplazamiento.
    /// </summary>
    public float knockbackForce = 5f;

    /// <summary>
    /// Duración en segundos del hit-stun: tiempo mínimo que debe pasar
    /// antes de que el enemigo pueda recibir otro knockback.
    /// Evita que golpes rápidos apilen fuerzas y lo lancen fuera de la pantalla.
    /// </summary>
    public float knockbackDuration = 0.3f;

    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Referencia al Rigidbody2D del enemigo.
    /// Se obtiene automáticamente en Start(). Necesario para aplicar knockback.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Flag de hit-stun activo.
    /// true = el enemigo está en hit-stun y no puede recibir otro knockback.
    /// false = el enemigo puede recibir daño con efecto físico completo.
    /// Previene el "stacking" de fuerzas por golpes consecutivos rápidos.
    /// </summary>
    private bool isHit = false;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al activarse el enemigo en escena.
    /// Inicializa la vida al máximo y obtiene el Rigidbody2D.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    // ============================================================
    // MÉTODOS PRINCIPALES
    // ============================================================

    /// <summary>
    /// Punto de entrada de daño para el enemigo.
    /// Reduce la vida, aplica knockback y verifica si el enemigo debe morir.
    ///
    /// CUÁNDO SE LLAMA: Desde Hitbox.cs o ExtendedHitbox.cs cuando un golpe conecta.
    /// QUÉ AFECTA: currentHealth, física del Rigidbody2D, y destrucción del objeto.
    /// </summary>
    /// <param name="damage">Cantidad de puntos de vida a restar.</param>
    /// <param name="attackerPosition">Posición del atacante para calcular dirección del knockback.</param>
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        currentHealth -= damage;

        // Clampear vida para no bajar de cero (evita valores negativos en UI o lógica)
        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log($"Enemigo vida: {currentHealth}");

        ApplyHit(attackerPosition);

        // Verificar muerte después de aplicar el golpe para permitir animación de hit
        if (currentHealth <= 0)
            Die();
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    /// <summary>
    /// Aplica el knockback físico al enemigo cuando recibe un golpe.
    /// Controla el hit-stun para evitar que fuerzas se acumulen.
    ///
    /// CUÁNDO SE EJECUTA: Llamado internamente desde TakeDamage().
    /// QUÉ AFECTA: Rigidbody2D (velocidad y fuerza), flag isHit.
    ///
    /// NOTA: Si isHit ya está activo, el método retorna sin hacer nada.
    ///       Esto es intencional para evitar stacking de knockback.
    /// </summary>
    /// <param name="attackerPosition">Posición desde donde vino el ataque.</param>
    void ApplyHit(Vector2 attackerPosition)
    {
        // Ignorar golpe si el hit-stun aún está activo
        if (isHit) return;

        isHit = true;

        // Dirección del empuje: opuesta al atacante para alejarse de él
        Vector2 direction = (transform.position - (Vector3)attackerPosition).normalized;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            // El eje Y tiene un valor fijo de 2f para dar una pequeña elevación al golpe
            rb.AddForce(new Vector2(direction.x * knockbackForce, 2f), ForceMode2D.Impulse);
        }

        // Mathf.Max evita que un knockbackDuration negativo rompa el Invoke
        Invoke(nameof(EndHit), Mathf.Max(0f, knockbackDuration));
    }

    /// <summary>
    /// Finaliza el hit-stun y permite que el enemigo reciba knockback nuevamente.
    /// Se llama automáticamente mediante Invoke() al vencer knockbackDuration.
    /// </summary>
    void EndHit()
    {
        isHit = false;
    }

    /// <summary>
    /// Destruye el enemigo al quedarse sin vida.
    /// Usa un delay de 0.5 segundos para permitir efectos visuales de muerte
    /// (animaciones, partículas, sonidos) antes de eliminar el objeto.
    ///
    /// CUÁNDO SE EJECUTA: Llamado desde TakeDamage() cuando currentHealth llega a 0.
    /// </summary>
    void Die()
    {
        Debug.Log("Enemigo derrotado");
        // Delay para permitir efectos visuales antes de destruir
        Destroy(gameObject, 0.5f);
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. Un Hitbox o ExtendedHitbox detecta colisión con el enemigo.
     * 2. Llaman a TakeDamage(damage, attackerPosition).
     * 3. Se descuenta vida y se llama ApplyHit().
     * 4. ApplyHit() verifica isHit → si false, aplica knockback y activa hit-stun.
     * 5. Después de knockbackDuration segundos → EndHit() desactiva isHit.
     * 6. Si currentHealth <= 0 → Die() destruye el GameObject con 0.5s de delay.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * ES LLAMADO POR:
     *   - Hitbox.cs → TakeDamage() cuando el hitbox de ataque toca al enemigo.
     *   - ExtendedHitbox.cs → TakeDamage() para ataques con hitbox extendido.
     *
     * AFECTA A:
     *   - Rigidbody2D del enemigo (knockback físico).
     *   - El GameObject completo (lo destruye al morir).
     *
     * NO AFECTA:
     *   - EnergySystem (la energía se gestiona en Hitbox.cs, no aquí).
     *   - UI de vida (el enemigo no tiene barra de vida en este sistema).
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ Si el enemigo no tiene Rigidbody2D, el knockback no se aplica
     *   pero el daño y la muerte funcionan correctamente.
     *
     * ⚠ Die() tiene un delay de 0.5s. Durante ese tiempo el enemigo sigue
     *   presente en escena y puede recibir más golpes (aunque currentHealth = 0).
     *   Considerar agregar un flag isDead para ignorar daño adicional.
     *
     * ⚠ knockbackDuration NO debe ser negativo. Mathf.Max lo protege,
     *   pero configurar valores negativos en el Inspector genera comportamiento
     *   inesperado silencioso.
     */
}
