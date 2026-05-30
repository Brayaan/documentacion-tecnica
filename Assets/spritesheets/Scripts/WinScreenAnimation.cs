/// -----------------------------------------------------------------------------
/// <file>WinScreenAnimation.cs</file>
/// <summary>
/// Contiene la clase WinScreenAnimation.
/// </summary>
/// -----------------------------------------------------------------------------

using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Gestiona la animación de la pantalla de victoria mediante un CanvasGroup y escalado de texto.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class WinScreenAnimation : MonoBehaviour
{
    /// <summary>
    /// Referencia al componente CanvasGroup usado para modificar la opacidad de la pantalla.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Componente de texto que muestra el resultado y se anima al aparecer la pantalla.
    /// </summary>
    public TMP_Text resultText;

    /// <summary>
    /// Se llama al inicializar el script antes de que inicie el juego para obtener componentes necesarios.
    /// </summary>
    void Awake()
    {
        // Obtenemos el CanvasGroup al iniciar
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Se llama cada vez que el objeto es habilitado o activado.
    /// Reinicia los valores de transparencia y escala del texto y lanza la animación.
    /// </summary>
    void OnEnable()
    {
        // Cuando el panel se enciende, reiniciamos su estado para animarlo
        canvasGroup.alpha = 0f; // Panel invisible al inicio

        // Texto pequeño listo para crecer
        if (resultText != null)
            resultText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        StartCoroutine(AnimateUI()); // Lanzamos la animación
    }

    /// <summary>
    /// Corrutina que anima la interfaz de usuario con un Fade In de la pantalla oscura y escalado del texto de resultado.
    /// </summary>
    /// <returns>El enumerador para la corrutina.</returns>
    IEnumerator AnimateUI()
    {
        float duration = 0.6f; // Duración total de la animación
        float elapsed = 0f;    // Tiempo transcurrido

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; // Progreso de 0 a 1

            // 1. Efecto Fade In (Aparece suavemente la pantalla oscura)
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t * 1.5f);

            // 2. Efecto de crecimiento y "golpe" en el texto
            if (resultText != null)
            {
                // Curva Ease-Out (Rápido al inicio, lento al final)
                float scaleT = 1f - Mathf.Pow(1f - t, 3f);
                resultText.transform.localScale = Vector3.Lerp(new Vector3(0.1f, 0.1f, 0.1f), Vector3.one, scaleT);
            }

            yield return null; // Esperamos al siguiente frame
        }

        // Asegurarnos de que termine exacto en tamaño 1
        if (resultText != null)
            resultText.transform.localScale = Vector3.one;
    }
}