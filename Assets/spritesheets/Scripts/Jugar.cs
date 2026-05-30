/// <summary>
/// Archivo: Jugar.cs
/// Contiene la lógica para el menú principal, opciones y configuración de audio.
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Clase responsable de gestionar el menú principal, permitiendo iniciar el juego,
/// cambiar configuraciones de audio, visualizar controles y salir de la aplicación.
/// </summary>
public class Jugar : MonoBehaviour
{
    /// <summary>
    /// Nombre de la escena de batalla o juego a cargar.
    /// </summary>
    public string batalla;

    [Header("Botones del Menú Principal")]
    /// <summary>Referencia al botón para iniciar el juego.</summary>
    public Button botonJugar;
    
    /// <summary>Referencia al botón para abrir el menú de opciones.</summary>
    public Button botonOpciones;
    
    /// <summary>Referencia al botón para salir del juego.</summary>
    public Button botonSalir;

    /// <summary>
    /// Carga la escena del juego especificada en <see cref="batalla"/>.
    /// </summary>
    public void CargarEscena()
    {
        SceneManager.LoadScene(batalla);
    }

    /// <summary>
    /// Finaliza la ejecución del juego. Funciona tanto en el editor como en la build final.
    /// </summary>
    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit(); // Cierra el juego exportado (.exe)

        // Detiene el juego si estás probando dentro del editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ==========================================
    // SECCIÓN DE OPCIONES (AUDIO Y CONTROLES)
    // ==========================================
    
    [Header("Paneles Principales")]
    /// <summary>Panel principal que contiene las opciones iniciales del menú.</summary>
    public GameObject panelPrincipal;

    [Header("Paneles de Opciones")]
    /// <summary>Panel base de opciones que agrupa las categorías (Audio, Controles).</summary>
    public GameObject panelOpcionesBase;
    
    /// <summary>Panel que muestra el tutorial y configuración de controles.</summary>
    public GameObject panelTutorial;     
    
    /// <summary>Panel dedicado a la configuración de volumen.</summary>
    public GameObject panelAudio;        

    [Header("Configuración de Audio")]
    /// <summary>Mezclador de audio utilizado para controlar niveles de volumen generales.</summary>
    public AudioMixer audioMixer;
    
    /// <summary>Control deslizante (Slider) para ajustar el volumen de la música.</summary>
    public Slider musicSlider;
    
    /// <summary>Control deslizante (Slider) para ajustar el volumen de los efectos de sonido (SFX).</summary>
    public Slider sfxSlider;

    /// <summary>
    /// Método de Unity llamado antes del primer frame.
    /// Configura el estado inicial de los paneles y actualiza los sliders con el volumen actual del AudioMixer.
    /// </summary>
    private void Start()
    {
        // Asegurarnos de que los menús estén en el estado correcto al iniciar
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(false);
        if (panelTutorial != null) panelTutorial.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(false);

        // Inicializar sliders con el volumen guardado en el Mixer
        if (audioMixer != null)
        {
            float musicVol;
            if (musicSlider != null && audioMixer.GetFloat("MusicVolume", out musicVol))
                musicSlider.value = Mathf.Pow(10, musicVol / 20);

            float sfxVol;
            if (sfxSlider != null && audioMixer.GetFloat("SFXVolume", out sfxVol))
                sfxSlider.value = Mathf.Pow(10, sfxVol / 20);
        }
    }

    /// <summary>
    /// Oculta el panel principal y muestra el panel base de opciones.
    /// </summary>
    public void AbrirOpcionesBase()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false); // Oculta el panel principal
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(true); // Muestra el panel de opciones
    }

    /// <summary>
    /// Oculta el panel base de opciones y vuelve al panel principal.
    /// </summary>
    public void CerrarOpcionesBase()
    {
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(false); // Oculta el panel de opciones
        if (panelPrincipal != null) panelPrincipal.SetActive(true); // Vuelve a mostrar el panel principal
    }

    /// <summary>
    /// Oculta el panel base de opciones y muestra el panel de tutorial/controles.
    /// </summary>
    public void AbrirTutorial()
    {
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(false);
        if (panelTutorial != null) panelTutorial.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel de tutorial/controles y regresa al panel base de opciones.
    /// </summary>
    public void CerrarTutorial()
    {
        if (panelTutorial != null) panelTutorial.SetActive(false);
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(true); // Volver atrás
    }

    /// <summary>
    /// Oculta el panel base de opciones y muestra el panel de ajustes de audio.
    /// </summary>
    public void AbrirAudio()
    {
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel de ajustes de audio y regresa al panel base de opciones.
    /// </summary>
    public void CerrarAudio()
    {
        if (panelAudio != null) panelAudio.SetActive(false);
        if (panelOpcionesBase != null) panelOpcionesBase.SetActive(true); // Volver atrás
    }

    // ==========================================
    // LOGICA DE VOLUMEN (Mixer)
    // ==========================================

    /// <summary>
    /// Ajusta el volumen de la música en el AudioMixer basado en el valor de un slider.
    /// </summary>
    /// <param name="sliderValue">Valor lineal del slider a convertir a decibelios.</param>
    public void SetMusicVolume(float sliderValue)
    {
        if (audioMixer != null)
        {
            float decibels = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            audioMixer.SetFloat("MusicVolume", decibels);
        }
    }

    /// <summary>
    /// Ajusta el volumen de los efectos de sonido (SFX) en el AudioMixer basado en el valor de un slider.
    /// </summary>
    /// <param name="sliderValue">Valor lineal del slider a convertir a decibelios.</param>
    public void SetSFXVolume(float sliderValue)
    {
        if (audioMixer != null)
        {
            float decibels = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            audioMixer.SetFloat("SFXVolume", decibels);
        }
    }
}