/// <summary>
/// Archivo: JumpWhitStrike.cs
/// Implementa la mecánica de un ataque con salto o ataque aéreo.
/// </summary>

using UnityEngine;
using System.Collections;

/// <summary>
/// Clase responsable de gestionar el impulso y lógica de ataque aéreo de un personaje.
/// </summary>
public class JumpWhitStrike : MonoBehaviour
{
    /// <summary>
    /// Componente Rigidbody2D asociado al personaje para la aplicación de fuerzas.
    /// </summary>
    public Rigidbody2D rb;

    /// <summary>
    /// Fuerza base aplicada durante saltos normales (opcional o de referencia).
    /// </summary>
    public float fuerzaSalto = 8f;

    /// <summary>
    /// Impulso horizontal aplicado al personaje al realizar el golpe aéreo.
    /// </summary>
    public float impulsoHorizontal = 5f;

    /// <summary>Estado interno para verificar si el ataque ya fue usado en el aire.</summary>
    private bool ataqueUsado = false;
    
    /// <summary>Estado interno para verificar si el personaje se encuentra en el suelo.</summary>
    private bool enSuelo = true;

    /// <summary>
    /// Método de Unity llamado en cada frame.
    /// Detecta la entrada del jugador para ejecutar el ataque aéreo si las condiciones se cumplen.
    /// </summary>
    void Update()
    {
        // Ataque aéreo (tecla J)
        if (Input.GetKeyDown(KeyCode.J) && !enSuelo && !ataqueUsado)
        {
            StartCoroutine(GolpeConSalto());
        }
    }

    /// <summary>
    /// Corrutina que ejecuta la lógica y física del golpe aéreo.
    /// </summary>
    /// <returns>Retorna un IEnumerator para el manejo del tiempo de espera.</returns>
    IEnumerator GolpeConSalto()
    {
        ataqueUsado = true;

        // Impulso hacia adelante
        rb.AddForce(Vector2.right * impulsoHorizontal, ForceMode2D.Impulse);

        // Activar animación de ataque aquí
        // animator.SetTrigger("Ataque");

        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// Detecta la colisión inicial con objetos físicos, restaurando estados como el de estar en el suelo.
    /// </summary>
    /// <param name="collision">Información sobre la colisión producida.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = true;
            ataqueUsado = false; // Reinicia el ataque al aterrizar
        }
    }

    /// <summary>
    /// Detecta cuando el personaje deja de hacer contacto con objetos físicos.
    /// </summary>
    /// <param name="collision">Información sobre la colisión finalizada.</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = false;
        }
    }
}
