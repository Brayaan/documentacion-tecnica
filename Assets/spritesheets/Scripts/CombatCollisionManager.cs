// ============================================================
// ARCHIVO: CombatCollisionManager.cs
// PROPÓSITO: Gestionar las colisiones físicas entre enemigos
//            en el sistema de combate del juego.
// RESPONSABILIDAD: Configurar la física de los enemigos al iniciar
//                  la escena para evitar que se empujen entre sí.
// SISTEMA: Combate — Física y Colisiones
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script actúa como inicializador global del comportamiento físico
 * de todos los enemigos en la escena. Resuelve el problema de que los
 * enemigos se empujen mutuamente al acumularse cerca del jugador,
 * lo que rompe la lógica de combate.
 *
 * Se ejecuta una sola vez en Start() y configura las colisiones
 * entre todos los GameObjects con el tag "Enemy" usando Physics2D.IgnoreCollision.
 *
 * SISTEMA: Combate / Físicas
 * DEPENDE DE: Collider2D y Rigidbody2D en cada enemigo
 */

using UnityEngine;

public class CombatCollisionManager : MonoBehaviour
{
    // -------------------------
    // VARIABLES PÚBLICAS
    // -------------------------

    /// <summary>
    /// Si es true, los enemigos podrán empujarse físicamente entre sí.
    /// Si es false (por defecto), sus colisiones físicas se ignorarán.
    /// Configurable desde el Inspector para ajustar el diseño de gameplay.
    /// </summary>
    public bool enemiesPushEachOther = false;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una sola vez al iniciar la escena.
    /// Dispara la configuración global de física para todos los enemigos.
    /// Es el punto de entrada único de este sistema.
    /// </summary>
    void Start()
    {
        ConfigureAllEnemies();
    }

    // ============================================================
    // MÉTODOS PRINCIPALES
    // ============================================================

    /// <summary>
    /// Busca todos los GameObjects con el tag "Enemy" en la escena
    /// y les aplica la configuración de físicas correspondiente.
    ///
    /// CUÁNDO SE EJECUTA: Una sola vez al inicio, invocado desde Start().
    /// QUÉ AFECTA: Todos los enemigos activos en la escena al momento de Start().
    ///
    /// NOTA: Los enemigos instanciados después de Start() NO serán configurados.
    ///       Si el juego usa spawn dinámico, llamar ConfigureEnemyPhysics()
    ///       manualmente al instanciar cada enemigo.
    /// </summary>
    private void ConfigureAllEnemies()
    {
        // FindGameObjectsWithTag lanza excepción si el tag no existe
        GameObject[] enemies;
        try
        {
            enemies = GameObject.FindGameObjectsWithTag("Enemy");
        }
        catch (UnityEngine.UnityException)
        {
            Debug.LogError("El tag 'Enemy' no está registrado en Project Settings > Tags", this);
            return;
        }

        // Obtener todos los colliders una sola vez antes del loop
        // para evitar llamadas repetidas costosas a FindObjectsByType dentro del bucle
        Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

        foreach (GameObject enemy in enemies)
        {
            ConfigureEnemyPhysics(enemy, allColliders);
        }
    }

    /// <summary>
    /// Configura las físicas de un enemigo individual:
    /// - Bloquea la rotación del Rigidbody2D.
    /// - Hace que este enemigo ignore colisiones físicas con todos los demás enemigos.
    ///
    /// QUÉ HACE: Itera sobre todos los Collider2D de la escena
    ///           y aplica Physics2D.IgnoreCollision entre enemigos.
    /// CUÁNDO SE EJECUTA: Una vez por enemigo, llamado desde ConfigureAllEnemies().
    /// QUÉ AFECTA: Rigidbody2D y Collider2D del enemigo recibido como parámetro.
    /// </summary>
    /// <param name="enemy">El GameObject del enemigo a configurar.</param>
    /// <param name="allColliders">Lista de todos los Collider2D activos en la escena.</param>
    private void ConfigureEnemyPhysics(GameObject enemy, Collider2D[] allColliders)
    {
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            if (!enemiesPushEachOther)
            {
                // Prevenir que la física rote al Rigidbody
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                Collider2D myCollider = enemy.GetComponent<Collider2D>();

                // Saltar enemigos sin Collider2D para evitar NullReferenceException
                if (myCollider == null)
                {
                    Debug.LogWarning("El enemigo " + enemy.name + " no tiene Collider2D", enemy);
                    return;
                }

                // Ignorar colisiones físicas entre todos los enemigos de la escena
                foreach (Collider2D other in allColliders)
                {
                    if (other.gameObject.CompareTag("Enemy") && other.gameObject != enemy)
                    {
                        Physics2D.IgnoreCollision(myCollider, other, true);
                    }
                }
            }
        }
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. Start() → ConfigureAllEnemies()
     * 2. Se obtienen todos los Enemy tags y todos los Collider2D de la escena.
     * 3. Por cada enemigo → ConfigureEnemyPhysics()
     * 4. Se congela la rotación del Rigidbody2D del enemigo.
     * 5. Se llama Physics2D.IgnoreCollision entre el enemigo y cada otro enemigo.
     * 6. El sistema queda configurado estáticamente para toda la partida.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * DEPENDE DE:
     *   - Rigidbody2D en cada enemigo (requerido para configurar constraints).
     *   - Collider2D en cada enemigo (requerido para IgnoreCollision).
     *   - Tag "Enemy" registrado en Project Settings > Tags.
     *
     * AFECTA A:
     *   - EnemyHealthSystem: al eliminar el empuje entre enemigos,
     *     el knockback recibido no se ve interferido por colisiones de otros enemigos.
     *
     * NO ES AFECTADO POR ningún otro script en runtime.
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ ADVERTENCIA: Solo configura enemigos presentes en escena al momento de Start().
     *   Los enemigos generados dinámicamente deben configurarse manualmente.
     *
     * ⚠ Si se agrega un nuevo tag para variantes de enemigos (ej: "BossEnemy"),
     *   este script no los cubrirá automáticamente. Extender ConfigureAllEnemies()
     *   para incluir los tags adicionales.
     *
     * ⚠ NO modificar enemiesPushEachOther en runtime; el sistema no reaplica
     *   la configuración después de Start().
     */
}
