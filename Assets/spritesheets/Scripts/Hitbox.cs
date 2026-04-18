// ============================================================
// ARCHIVO: Hitbox.cs
// PROPÓSITO: Detectar colisiones de ataque y aplicar daño, knockback
//            y ganancia de energía a jugadores y enemigos.
// RESPONSABILIDAD: Núcleo del sistema de daño por contacto en combate.
//                  Maneja bloqueos, cooldown de golpe y comunicación
//                  con los sistemas de salud y energía.
// SISTEMA: Combate — Detección de Golpes (Hitbox Principal)
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Hitbox es el componente que convierte un Collider2D en un área de daño activa.
 * Usa OnTriggerStay2D para detectar contacto continuo mientras el Collider está activo,
 * lo que permite que el golpe conecte aunque el personaje ya esté dentro del área.
 *
 * Implementa:
 *   - Cooldown por golpe (hitCooldown) para evitar daño excesivo por contacto prolongado.
 *   - Detección de bloqueo: si el objetivo bloquea, el atacante recibe un rebote.
 *   - Comunicación con HealthSystem, EnergySystem del atacante y del objetivo.
 *   - Guard contra autogolpe (ignora el propio personaje).
 *
 * Los hitboxes se activan/desactivan desde PlayerAttack.cs durante cada ataque.
 *
 * SISTEMA: Combate / Detección de Golpes
 * INTERACTÚA CON: PlayerAttack.cs, PlayerDefense.cs, HealthSystem.cs, EnergySystem.cs
 */

using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // -------------------------
    // VARIABLES PÚBLICAS
    // -------------------------

    /// <summary>
    /// Nombre del ataque asociado a este hitbox (ej: "Puñetazo", "Patada").
    /// Se usa para calcular la ganancia de energía en EnergySystem.GainEnergyFromAttack().
    /// Configurable desde el Inspector por tipo de hitbox.
    /// </summary>
    public string attackName;

    /// <summary>
    /// Cantidad de daño que este hitbox inflige por golpe.
    /// Configurable desde el Inspector. También lo lee ExtendedHitbox.cs.
    /// </summary>
    public int damage = 1;

    /// <summary>
    /// Tiempo mínimo en segundos entre golpes consecutivos mientras el Collider
    /// permanece en contacto. Evita que un solo ataque quite vida múltiple
    /// veces por frame durante el contacto.
    /// </summary>
    public float hitCooldown = 0.2f;

    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Referencia al PlayerAttack en la raíz de la jerarquía.
    /// Necesaria para verificar si hay un ataque activo antes de aplicar daño.
    /// </summary>
    private PlayerAttack attack;

    /// <summary>
    /// Timestamp del último golpe registrado (Time.time en el momento del impacto).
    /// Inicializado en float.NegativeInfinity para que el primer golpe siempre conecte.
    /// Usado para calcular el cooldown entre golpes en OnTriggerStay2D.
    /// </summary>
    private float lastHitTime = float.NegativeInfinity;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al activarse el hitbox en escena.
    /// Obtiene la referencia a PlayerAttack desde la raíz de la jerarquía.
    ///
    /// NOTA: El hitbox generalmente es hijo del jugador, por lo que
    ///       transform.root apunta al GameObject raíz del jugador.
    /// </summary>
    void Start()
    {
        // Buscar PlayerAttack en la raíz de la jerarquía del jugador
        attack = transform.root.GetComponent<PlayerAttack>();
    }

    /// <summary>
    /// OnDisable() — Se ejecuta cuando el hitbox se desactiva (SetActive(false)).
    /// Resetea el cooldown para que el próximo ataque siempre conecte
    /// en el primer frame, independientemente del tiempo transcurrido.
    ///
    /// IMPORTANTE: Sin este reset, si el jugador ataca rápido, el cooldown
    ///             podría bloquear el golpe del siguiente ataque.
    /// </summary>
    void OnDisable()
    {
        // Resetear cooldown para que el próximo ataque siempre conecte
        lastHitTime = float.NegativeInfinity;
    }

    /// <summary>
    /// OnTriggerStay2D — Se ejecuta cada frame mientras otro Collider2D
    /// permanece dentro del área del hitbox.
    ///
    /// CUÁNDO SE EJECUTA: Cada frame de contacto con un collider en trigger mode.
    /// QUÉ CONTROLA: Validaciones de ataque activo, bloqueo del objetivo,
    ///               cooldown de golpe, aplicación de daño y energía.
    ///
    /// FLUJO INTERNO:
    /// 1. Verificar que PlayerAttack existe y hay ataque activo.
    /// 2. Verificar tag del objetivo ("Player" o "Enemy").
    /// 3. Ignorar autogolpe (mismo root GameObject).
    /// 4. Verificar si el objetivo está bloqueando → rebote al atacante.
    /// 5. Verificar cooldown de golpe.
    /// 6. Aplicar daño vía HealthSystem.
    /// 7. Dar energía al objetivo (GainEnergyFromDamage) y al atacante (GainEnergyFromAttack).
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Cancelar si el ataque no está activo o el hitbox fue desactivado
        if (attack == null || !attack.IsAttacking() || !gameObject.activeSelf)
            return;

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Evitar que el hitbox golpee al propio personaje que lo contiene
            if (other.gameObject == transform.root.gameObject)
                return;

            PlayerDefense defense = other.GetComponent<PlayerDefense>();

            // Si el objetivo está bloqueando: rebote al atacante (no al objetivo)
            if (defense != null && defense.IsBlocking())
            {
                Debug.Log(other.name + " bloqueó el ataque");

                Rigidbody2D attackerRb = transform.root.GetComponent<Rigidbody2D>();

                if (attackerRb != null)
                {
                    // Dirección del rebote: desde el bloqueador hacia el atacante
                    Vector2 direction = (transform.root.position - other.transform.position).normalized;

                    attackerRb.linearVelocity = Vector2.zero;
                    attackerRb.AddForce(new Vector2(direction.x * 4f, 1.5f), ForceMode2D.Impulse);
                }

                return; // Interrumpir: no aplicar daño al bloqueador
            }

            HealthSystem health = other.GetComponent<HealthSystem>();

            if (health != null)
            {
                // Verificar cooldown: evitar golpes múltiples en contacto continuo
                // Mathf.Max garantiza que hitCooldown negativo se trate como cero
                if (Time.time - lastHitTime < Mathf.Max(0f, hitCooldown))
                    return;

                lastHitTime = Time.time;

                Vector2 attackerPosition = transform.root.position;

                health.TakeDamage(damage, attackerPosition);

                // Energía al objetivo: ganar por absorber el impacto
                EnergySystem targetEnergy = other.GetComponent<EnergySystem>();
                if (targetEnergy != null)
                    targetEnergy.GainEnergyFromDamage();

                // Energía al atacante: ganar por conectar el golpe
                EnergySystem attackerEnergy = transform.root.GetComponent<EnergySystem>();
                if (attackerEnergy != null)
                    attackerEnergy.GainEnergyFromAttack(attackName);

                Debug.Log("Golpeaste a: " + other.name + " con: " + attackName);
            }
        }
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. PlayerAttack activa el hitbox (SetActive(true)) al inicio del ataque.
     * 2. Si hay un Collider2D dentro del área → OnTriggerStay2D() cada frame.
     * 3. Se validan: ataque activo, tag, autogolpe, bloqueo, cooldown.
     * 4. Si pasa todas las validaciones → HealthSystem.TakeDamage() en el objetivo.
     * 5. EnergySystem del objetivo recibe +2 (GainEnergyFromDamage).
     * 6. EnergySystem del atacante recibe +5 o +10 según attackName.
     * 7. PlayerAttack desactiva el hitbox → OnDisable() resetea lastHitTime.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * DEPENDE DE:
     *   - PlayerAttack.cs → IsAttacking() para validar estado del ataque.
     *   - PlayerDefense.cs → IsBlocking() para detectar bloqueos.
     *   - HealthSystem.cs → TakeDamage() para aplicar daño al objetivo.
     *   - EnergySystem.cs → GainEnergyFromDamage() y GainEnergyFromAttack().
     *
     * ES ACTIVADO/DESACTIVADO POR:
     *   - PlayerAttack.cs → ActivarHitbox() / DesactivarHitbox()
     *                     → ActivarKickHitbox() / DesactivarKickHitbox()
     *
     * SU VALOR damage ES LEÍDO POR:
     *   - ExtendedHitbox.cs → originalHitbox.damage
     *
     * AFECTA A:
     *   - Rigidbody2D del atacante (rebote al bloquear).
     *   - HealthSystem del objetivo (daño).
     *   - EnergySystem del objetivo y del atacante.
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ attackName DEBE coincidir exactamente con los strings en EnergySystem
     *   ("Puñetazo" o "Patada"). Un typo en el Inspector causará que
     *   la energía del atacante no aumente, sin ningún error visible.
     *
     * ⚠ OnTriggerStay2D se ejecuta en PhysicsUpdate, no en Update.
     *   Si el físico está a menor framerate, el cooldown puede comportarse
     *   de forma diferente al esperado a FPS muy bajos.
     *
     * ⚠ El rebote al bloquear tiene fuerza fija (4f, 1.5f) hardcodeada.
     *   Considerar exponer estas variables en el Inspector para balanceo.
     *
     * ⚠ Este script solo afecta a GameObjects con HealthSystem.
     *   Enemigos con EnemyHealthSystem NO son dañados por este Hitbox;
     *   para ellos se usa ExtendedHitbox.cs.
     */
}
