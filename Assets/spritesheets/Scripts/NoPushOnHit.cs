// ============================================================
// ARCHIVO: NoPushOnHit.cs
// PROPÓSITO: Evitar que el jugador sea desplazado físicamente
//            por colisiones con enemigos durante el hit-stun.
// RESPONSABILIDAD: Aumentar temporalmente la masa del Rigidbody2D
//                  para resistir empujes físicos al recibir un golpe.
// SISTEMA: Combate — Física de Impacto / Hit-Stun
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Cuando el jugador recibe un golpe y entra en hit-stun, otros enemigos
 * que lo toquen físicamente podrían empujarlo accidentalmente, acumulando
 * desplazamientos no deseados. Este script resuelve ese problema aumentando
 * la masa del Rigidbody2D a un valor extremo (1000f) durante el hit-stun,
 * lo que hace que las fuerzas de contacto sean prácticamente inefectivas.
 *
 * Al terminar el hit-stun, la masa se restaura al valor original.
 * También ignora colisiones físicas con enemigos mientras isHit está activo.
 *
 * SISTEMA: Combate / Física
 * INTERACTÚA CON: HealthSystem.cs (debe llamar OnHitStart/OnHitEnd manualmente)
 *
 * NOTA: Este script expone OnHitStart() y OnHitEnd() como API pública.
 *       Deben ser llamados externamente por el sistema que gestiona el hit-stun.
 */

using UnityEngine;

public class NoPushOnHit : MonoBehaviour
{
    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Referencia al Rigidbody2D del jugador.
    /// Se obtiene en Start(). Necesario para modificar la masa y las constraints.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Masa original del Rigidbody2D antes de entrar en hit-stun.
    /// Se guarda en Start() y se restaura en OnHitEnd() para no perder
    /// la configuración del Inspector.
    /// </summary>
    private float originalMass;

    /// <summary>
    /// Flag de estado de hit-stun activo.
    /// true = el jugador está en hit-stun; colisiones con enemigos se ignoran.
    /// false = comportamiento físico normal.
    /// </summary>
    private bool isHit = false;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar.
    /// Obtiene el Rigidbody2D y guarda la masa original como referencia.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Guardar masa original para restaurarla al terminar el golpe
            originalMass = rb.mass;
        }
    }

    /// <summary>
    /// OnCollisionEnter2D — Se ejecuta al entrar en colisión con otro Collider2D
    /// (colisión sólida, no trigger).
    ///
    /// CUÁNDO SE EJECUTA: Al primer frame de contacto físico con otro collider.
    /// QUÉ CONTROLA: Durante el hit-stun, ignora colisiones físicas con enemigos
    ///               para evitar que lo empujen mientras ya está volando por el knockback.
    ///
    /// NOTA: Physics2D.IgnoreCollision persiste entre activaciones del objeto.
    ///       Si el enemigo cambia de estado, la colisión permanece ignorada
    ///       hasta que se reactive explícitamente.
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Durante el hit-stun: ignorar colisiones físicas con cualquier Enemy
        if (isHit && collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
        }
    }

    // ============================================================
    // MÉTODOS PRINCIPALES — API PÚBLICA
    // ============================================================

    /// <summary>
    /// Inicia el estado de hit-stun para este personaje.
    /// Aumenta la masa del Rigidbody2D a 1000f para resistir empujes físicos
    /// de contacto con otros colliders, y congela la rotación.
    ///
    /// CUÁNDO SE LLAMA: Debe ser llamado externamente por el sistema de hit-stun
    ///                  (HealthSystem.cs o similar) al inicio de un golpe recibido.
    /// QUÉ AFECTA: rb.mass (modificado a 1000f), rb.constraints, flag isHit.
    ///
    /// ADVERTENCIA: Si OnHitEnd() nunca se llama, el jugador quedará con
    ///              masa de 1000f permanentemente, rompiendo su física.
    /// </summary>
    public void OnHitStart()
    {
        if (rb != null)
        {
            // Masa extrema para resistir desplazamientos por contacto físico
            rb.mass = 1000f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        isHit = true;
    }

    /// <summary>
    /// Finaliza el estado de hit-stun y restaura la física al estado normal.
    /// Restaura la masa original guardada en Start().
    ///
    /// CUÁNDO SE LLAMA: Debe ser llamado externamente al terminar el hit-stun.
    ///                  Típicamente mediante Invoke() o una coroutine desde el
    ///                  script que llamó OnHitStart().
    /// QUÉ AFECTA: rb.mass (restaurado), flag isHit.
    /// </summary>
    public void OnHitEnd()
    {
        if (rb != null)
        {
            // Restaurar masa original al salir del hit-stun
            rb.mass = originalMass;
        }
        isHit = false;
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. Start() → guarda originalMass del Rigidbody2D.
     * 2. HealthSystem (o sistema externo) detecta un golpe → llama OnHitStart().
     * 3. OnHitStart() → rb.mass = 1000f, isHit = true.
     * 4. Durante el hit-stun: si un enemigo colisiona → OnCollisionEnter2D()
     *    → Physics2D.IgnoreCollision activo para ese enemigo.
     * 5. Al terminar el hit-stun → OnHitEnd() → rb.mass = originalMass, isHit = false.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * ES LLAMADO POR:
     *   - HealthSystem.cs (o el sistema que gestione el hit-stun del jugador)
     *     debe llamar OnHitStart() y OnHitEnd() manualmente.
     *
     * AFECTA A:
     *   - Rigidbody2D del jugador (masa y constraints).
     *   - Sistema de colisiones de Physics2D (IgnoreCollision con enemigos).
     *
     * NOTA IMPORTANTE: En la versión actual del proyecto, HealthSystem.cs
     *   NO llama a OnHitStart/OnHitEnd. Este script está preparado como
     *   extensión pero requiere integración manual para funcionar.
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ CRÍTICO: OnHitStart() y OnHitEnd() deben llamarse en par.
     *   Si OnHitEnd() no se llama (por error o excepción), el jugador
     *   quedará con rb.mass = 1000f permanentemente, haciendo que la
     *   física de movimiento y salto sea incorrecta.
     *
     * ⚠ Physics2D.IgnoreCollision() en OnCollisionEnter2D persiste
     *   indefinidamente. Si el mismo enemigo vuelve a colisionar después
     *   del hit-stun, la colisión seguirá ignorada hasta que se reactive
     *   con Physics2D.IgnoreCollision(col1, col2, false).
     *
     * ⚠ Este script no gestiona el timer del hit-stun.
     *   El control de duración debe hacerse desde el script que lo llama.
     *
     * ⚠ La masa de 1000f es un valor hardcodeado. Si la masa original del
     *   Rigidbody2D es muy alta (ej: 500f), la diferencia puede no ser
     *   suficiente. Considerar usar rb.isKinematic = true en su lugar.
     */
}
