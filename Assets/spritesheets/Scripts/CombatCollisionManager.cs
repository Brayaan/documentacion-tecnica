/// <summary>
/// Handles collision logic and physics configuration for combat entities,
/// particularly focusing on interactions between enemy units.
/// </summary>
/// <remarks>
/// File: CombatCollisionManager.cs
/// </remarks>
using UnityEngine;

/// <summary>
/// Manages the collision settings for all enemies in the scene upon startup.
/// Can be configured to allow or prevent enemies from pushing each other physically.
/// </summary>
public class CombatCollisionManager : MonoBehaviour
{
    /// <summary>
    /// Determines whether enemies can physically push each other.
    /// If false, physical collisions between enemies will be ignored.
    /// </summary>
    public bool enemiesPushEachOther = false;

    /// <summary>
    /// Called before the first frame update. 
    /// Initiates the configuration of all enemy units in the scene.
    /// </summary>
    void Start()
    {
        ConfigureAllEnemies();
    }

    /// <summary>
    /// Finds all GameObjects tagged as "Enemy" and applies the necessary physics configurations.
    /// Logs an error if the "Enemy" tag is not defined in the project settings.
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
        Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

        foreach (GameObject enemy in enemies)
        {
            ConfigureEnemyPhysics(enemy, allColliders);
        }
    }

    /// <summary>
    /// Applies specific physics settings to a single enemy unit based on the manager's configuration.
    /// </summary>
    /// <param name="enemy">The enemy GameObject to configure.</param>
    /// <param name="allColliders">An array containing all Collider2D components currently in the scene.</param>
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

                // Ignorar colisiones físicas entre todos los enemigos
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
}