/// <summary>
/// Archivo: NoPushOnHit.cs
/// Lógica para evitar el desplazamiento físico del personaje durante estados de hit-stun.
/// </summary>

using UnityEngine;

/// <summary>
/// Clase que gestiona la inmovilización física (hit-stun) de un objeto al recibir un golpe,
/// modificando temporalmente su masa e ignorando ciertas colisiones.
/// </summary>
public class NoPushOnHit : MonoBehaviour
{
    /// <summary>Componente Rigidbody2D del objeto.</summary>
    private Rigidbody2D rb;
    
    /// <summary>Masa original guardada para ser restaurada tras el impacto.</summary>
    private float originalMass;
    
    /// <summary>Bandera que indica si el objeto está actualmente en estado de hit-stun.</summary>
    private bool isHit = false;

    /// <summary>
    /// Método de Unity llamado en el primer frame.
    /// Inicializa las variables y guarda la masa original.
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
    /// Inicia el estado de hit-stun, incrementando drásticamente la masa para evitar empujes físicos.
    /// </summary>
    public void OnHitStart()
    {
        if (rb != null)
        {
            // Masa extrema para evitar ser desplazado por física de contacto
            rb.mass = 1000f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        isHit = true;
    }

    /// <summary>
    /// Termina el estado de hit-stun, restaurando la masa original del objeto.
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

    /// <summary>
    /// Se ejecuta al detectar colisiones. Si está en estado hit-stun, ignora a los enemigos.
    /// </summary>
    /// <param name="collision">Información de la colisión detectada.</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Durante el hit-stun ignorar colisiones con enemigos
        if (isHit && (collision.gameObject.CompareTag("Enemy")))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
        }
    }
}
