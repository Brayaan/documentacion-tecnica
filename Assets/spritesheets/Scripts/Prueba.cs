using UnityEngine;

/// <summary>
/// Clase de prueba para la documentación.
/// </summary>
public class Prueba : MonoBehaviour
{
    /// <summary>
    /// Velocidad del objeto.
    /// </summary>
    public float velocidad = 5f;

    /// <summary>
    /// Mueve el objeto hacia adelante.
    /// </summary>
    public void Mover()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }
}