// ============================================================
// ARCHIVO: ExtendedHitbox.cs
// PROPÓSITO: Hitbox auxiliar que amplía el alcance de un ataque
//            reutilizando la configuración del Hitbox principal.
// RESPONSABILIDAD: Detectar enemigos en un área extendida durante ataques activos
//                  y aplicar daño con los mismos valores del Hitbox padre.
// SISTEMA: Combate — Detección de Golpes (Hitbox Extendido)
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * ExtendedHitbox es un hitbox secundario que complementa al Hitbox principal.
 * Comparte el valor de daño del Hitbox padre para garantizar consistencia,
 * pero tiene su propio Collider2D con mayor alcance (configurado en la escena).
 *
 * A diferencia de Hitbox.cs (que usa OnTriggerStay2D para daño continuo),
 * este script usa OnTriggerEnter2D para registrar el golpe solo al momento
 * del primer contacto.
 *
 * Solo procesa golpes cuando PlayerAttack reporta un ataque activo,
 * evitando detecciones falsas fuera de la animación de ataque.
 *
 * SISTEMA: Combate / Detección de Golpes
 * INTERACTÚA CON: Hitbox.cs, PlayerAttack.cs, EnemyHealthSystem.cs, EnergySystem.cs
 */

using UnityEngine;

public class ExtendedHitbox : MonoBehaviour
{
    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Referencia al Hitbox principal del mismo GameObject.
    /// Se usa para leer el valor de daño (originalHitbox.damage)
    /// en lugar de duplicarlo, manteniendo un único punto de configuración.
    /// </summary>
    private Hitbox originalHitbox;

    /// <summary>
    /// Referencia al PlayerAttack en la raíz de la jerarquía del jugador.
    /// Necesario para verificar si hay un ataque activo antes de procesar golpes.
    /// </summary>
    private PlayerAttack playerAttack;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al activarse el objeto.
    /// Obtiene referencias a Hitbox (mismo objeto) y PlayerAttack (raíz de jerarquía).
    ///
    /// NOTA: Si el objeto no es hijo del jugador, transform.root.GetComponent
    ///       puede devolver null y el hitbox quedará inactivo.
    /// </summary>
    void Start()
    {
        // Obtener el Hitbox principal del mismo GameObject
        originalHitbox = GetComponent<Hitbox>();

        // Obtener PlayerAttack desde la raíz de la jerarquía del jugador
        playerAttack = transform.root.GetComponent<PlayerAttack>();
    }

    /// <summary>
    /// OnTriggerEnter2D — Se ejecuta una vez al entrar en contacto con otro Collider2D.
    /// Registra el impacto al PRIMER frame de contacto (no continuo como OnTriggerStay2D).
    ///
    /// CUÁNDO SE EJECUTA: Cuando el Collider2D de este hitbox toca otro collider.
    /// QUÉ CONTROLA: Validaciones de ataque activo, aplicación de daño y ganancia de energía.
    ///
    /// FLUJO INTERNO:
    /// 1. Verificar que las referencias críticas existen.
    /// 2. Verificar que PlayerAttack está en estado de ataque activo.
    /// 3. Verificar que el collider tocado tiene el tag "Enemy".
    /// 4. Obtener EnemyHealthSystem y aplicar daño con el valor del Hitbox padre.
    /// 5. Otorgar energía al atacante por conectar el golpe.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Cancelar si faltan componentes críticos del sistema
        if (originalHitbox == null || playerAttack == null)
            return;

        // Solo procesar durante un ataque activo y con el hitbox habilitado
        if (!playerAttack.IsAttacking() || !gameObject.activeSelf)
            return;

        if (other.CompareTag("Enemy"))
        {
            EnemyHealthSystem enemyHealth = other.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                // Posición del atacante para calcular la dirección del knockback en el enemigo
                Vector2 attackerPosition = transform.root.position;

                // Usar el daño del Hitbox padre para no duplicar la configuración
                int damage = originalHitbox.damage;

                enemyHealth.TakeDamage(damage, attackerPosition);

                // Otorgar energía al atacante por conectar el golpe exitosamente
                EnergySystem attackerEnergy = transform.root.GetComponent<EnergySystem>();
                if (attackerEnergy != null)
                {
                    // "Puñetazo" es el tipo de ataque fijo para el hitbox extendido
                    attackerEnergy.GainEnergyFromAttack("Puñetazo");
                }

                Debug.Log($"Golpe a enemigo!");
            }
        }
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. PlayerAttack activa el hitbox extendido (SetActive(true)).
     * 2. El Collider2D entra en contacto con un enemigo → OnTriggerEnter2D().
     * 3. Se verifican: referencias, ataque activo, tag "Enemy".
     * 4. EnemyHealthSystem.TakeDamage() se llama con el daño del Hitbox padre.
     * 5. EnergySystem del atacante recibe +5 por "Puñetazo".
     * 6. PlayerAttack desactiva el hitbox (SetActive(false)) al terminar el ataque.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * DEPENDE DE:
     *   - Hitbox.cs (mismo objeto) → para leer el valor de daño.
     *   - PlayerAttack.cs (raíz de jerarquía) → para verificar estado de ataque.
     *   - EnemyHealthSystem.cs (en el enemigo) → para aplicar el daño.
     *   - EnergySystem.cs (raíz de jerarquía) → para dar energía al atacante.
     *
     * ES ACTIVADO/DESACTIVADO POR:
     *   - PlayerAttack.cs → ActivarHitbox() / DesactivarHitbox()
     *
     * DIFERENCIA CON Hitbox.cs:
     *   - Hitbox.cs usa OnTriggerStay2D (daño continuo mientras hay contacto).
     *   - ExtendedHitbox usa OnTriggerEnter2D (daño único al primer contacto).
     *   - ExtendedHitbox solo afecta a "Enemy", no a "Player".
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ El tipo de ataque para la energía está hardcodeado como "Puñetazo".
     *   Si este hitbox se reutiliza para patadas u otros ataques, la energía
     *   ganada siempre será la del puñetazo (+5). Considerar exponer
     *   attackName como variable pública para mayor flexibilidad.
     *
     * ⚠ No tiene cooldown propio (a diferencia de Hitbox.cs).
     *   Un enemigo puede recibir daño múltiple si múltiples hitboxes
     *   extendidos lo tocan en el mismo frame.
     *
     * ⚠ Si originalHitbox es null (porque el Hitbox.cs se eliminó),
     *   el script retorna silenciosamente sin daño. Verificar en el Inspector
     *   que ambos componentes estén en el mismo GameObject.
     */
}
