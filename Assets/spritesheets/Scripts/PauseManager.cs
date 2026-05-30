/// <summary>
/// Archivo: PauseManager.cs
/// Sistema de gestión de pausas, opciones en juego y configuración de audio global.
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Administra el estado de pausa del juego, la interfaz de usuario de pausa, 
/// submenús de opciones y el volumen a través del AudioMixer.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI Menú de Pausa")]
    /// <summary>Panel principal mostrado al pausar el juego.</summary>
    public GameObject pauseMenuUI;
    
    /// <summary>Panel secundario para mostrar ajustes y sliders.</summary>
    public GameObject optionsMenuUI;

    [Header("Configuración de Escenas")]
    /// <summary>Nombre de la escena de menú principal para usar al salir.</summary>
    public string mainMenuSceneName = "MenuPrincipal";

    [Header("Audio Mixer (Volumen Independiente)")]
    /// <summary>Referencia al AudioMixer que controla el volumen global.</summary>
    public AudioMixer audioMixer;
    
    /// <summary>Control deslizante para ajustar la música.</summary>
    public Slider musicSlider;
    
    /// <summary>Control deslizante para ajustar los efectos de sonido (SFX).</summary>
    public Slider sfxSlider;

    /// <summary>Indica si el juego se encuentra actualmente pausado.</summary>
    private bool isPaused = false;

    /// <summary>
    /// Método de Unity llamado en la inicialización.
    /// Oculta los menús de pausa y ajusta los sliders a los valores del AudioMixer.
    /// </summary>
    void Start()
    {
        // Asegurarnos de que el menú de pausa esté oculto al iniciar
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        // Inicializar sliders con los valores actuales del Mixer si están asignados
        if (audioMixer != null)
        {
            float musicVol;
            if (musicSlider != null && audioMixer.GetFloat("MusicVolume", out musicVol))
                musicSlider.value = Mathf.Pow(10, musicVol / 20); // Convertir DB a lineal

            float sfxVol;
            if (sfxSlider != null && audioMixer.GetFloat("SFXVolume", out sfxVol))
                sfxSlider.value = Mathf.Pow(10, sfxVol / 20); // Convertir DB a lineal
        }
    }

    /// <summary>
    /// Actualiza frame a frame, gestionando la entrada del usuario para activar/desactivar la pausa.
    /// </summary>
    void Update()
    {
        // CA-01: Activar pausa con la tecla Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // CA-06: Camino triste - No permitir pausa durante animación de victoria
            if (CombatManager.Instance != null && (!CombatManager.Instance.isRoundActive || CombatManager.Instance.isCombatEnded))
            {
                Debug.Log("No se puede pausar durante una transición o finalización de round.");
                return;
            }

            if (isPaused)
            {
                // Si estamos en el menú de opciones, Esc vuelve al menú de pausa principal
                if (optionsMenuUI != null && optionsMenuUI.activeSelf)
                {
                    CloseOptions();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Reanuda el juego, ocultando las interfaces de pausa y restaurando la escala de tiempo.
    /// </summary>
    // CA-02: Reanudar partida
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        Time.timeScale = 1f; // Descongelar el juego
        isPaused = false;
    }

    /// <summary>
    /// Pausa el juego, detiene la escala de tiempo y muestra el menú principal de pausa.
    /// </summary>
    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        Time.timeScale = 0f; // Congelar físicas, movimientos y timers
        isPaused = true;
    }

    // Submenú de Opciones

    /// <summary>
    /// Abre el menú de opciones desde el menú de pausa.
    /// </summary>
    public void OpenOptions()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(true);
    }

    /// <summary>
    /// Cierra el menú de opciones y vuelve al menú principal de pausa.
    /// </summary>
    public void CloseOptions()
    {
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    /// <summary>
    /// Reinicia la partida actual delegando al <see cref="CombatManager"/>.
    /// </summary>
    // CA-03: Reiniciar partida
    public void RestartMatch()
    {
        Resume(); // Asegurarnos de que el tiempo vuelva a la normalidad
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.RestartMatch(); // Reinicia el combate limpio
        }
    }

    /// <summary>
    /// Abandona la partida actual y carga la escena del menú principal.
    /// </summary>
    // CA-04: Volver al menú principal
    public void LoadMainMenu()
    {
        Resume(); // Descongelar antes de cambiar de escena
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ajusta el volumen de la música transformando escala lineal a decibelios logarítmicos.
    /// </summary>
    /// <param name="sliderValue">Valor del slider a transformar.</param>
    // CA-05: Ajuste de volumen (Música)
    public void SetMusicVolume(float sliderValue)
    {
        if (audioMixer != null)
        {
            // Convertir de escala lineal (0.0001 a 1) a logarítmica (Decibelios)
            float decibels = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            audioMixer.SetFloat("MusicVolume", decibels);
        }
    }

    /// <summary>
    /// Ajusta el volumen de efectos sonoros transformando escala lineal a decibelios logarítmicos.
    /// </summary>
    /// <param name="sliderValue">Valor del slider a transformar.</param>
    // CA-05: Ajuste de volumen (SFX)
    public void SetSFXVolume(float sliderValue)
    {
        if (audioMixer != null)
        {
            float decibels = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
            audioMixer.SetFloat("SFXVolume", decibels);
        }
    }
}
