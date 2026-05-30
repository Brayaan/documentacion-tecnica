// -----------------------------------------------------------------------------------------
// <copyright file="CharacterSelectionManager.cs" company="TU_COMPANY">
//     Copyright (c) TU_COMPANY. All rights reserved.
// </copyright>
// <summary>
//     Gestor de la pantalla de selección de personajes con interfaz estilo carrusel.
// </summary>
// -----------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la lógica de la interfaz de usuario para la selección de personajes.
/// Implementa un carrusel circular ("estilo Netflix") y permite animar al personaje central.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Base de Datos")]
    /// <summary>
    /// Lista de datos de los personajes disponibles para la selección.
    /// </summary>
    public List<CharacterData> availableCharacters;

    [Header("Imágenes del Carrusel (Estilo Netflix)")]
    /// <summary>
    /// Imagen de vista previa del personaje a la izquierda del seleccionado.
    /// </summary>
    public Image leftImage;

    /// <summary>
    /// Imagen del personaje actual o seleccionado (en el centro).
    /// </summary>
    public Image centerImage;

    /// <summary>
    /// Imagen de vista previa del personaje a la derecha del seleccionado.
    /// </summary>
    public Image rightImage;

    [Header("Textos")]
    /// <summary>
    /// Texto que muestra el nombre del personaje seleccionado.
    /// </summary>
    public TMP_Text nameText;

    /// <summary>
    /// Texto que muestra la descripción o historia del personaje seleccionado.
    /// </summary>
    public TMP_Text descriptionText;

    [Header("Botones y Flechas")]
    /// <summary>
    /// Botón para desplazarse al personaje anterior (izquierda).
    /// </summary>
    public Button leftArrow;

    /// <summary>
    /// Botón para desplazarse al personaje siguiente (derecha).
    /// </summary>
    public Button rightArrow;

    /// <summary>
    /// Botón para confirmar la selección del personaje.
    /// </summary>
    public Button selectButton;

    [Header("Escenas")]
    /// <summary>
    /// Nombre de la escena a cargar para iniciar la batalla.
    /// </summary>
    public string battleSceneName = "Batalla";

    /// <summary>
    /// Nombre de la escena del menú principal.
    /// </summary>
    public string menuSceneName = "Menu";

    /// <summary>
    /// Índice actual del personaje seleccionado en la lista <see cref="availableCharacters"/>.
    /// </summary>
    private int currentIndex = 0;
    
    /// <summary>
    /// Temporizador para controlar el avance de fotogramas de la animación.
    /// </summary>
    private float animationTimer = 0f;

    /// <summary>
    /// Índice del fotograma actual de la animación del personaje central.
    /// </summary>
    private int currentAnimationFrame = 0;

    /// <summary>
    /// Método de inicialización de Unity.
    /// Valida la disponibilidad de personajes y configura la UI del carrusel inicial.
    /// </summary>
    void Start()
    {
        // Validación CA-04: Sin personajes disponibles
        if (availableCharacters == null || availableCharacters.Count == 0)
        {
            Debug.LogError("No hay personajes en la lista. Bloqueando el menú.");
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            if (selectButton != null) selectButton.gameObject.SetActive(false);
            return;
        }

        // Inicializar el carrusel en el primer personaje
        currentIndex = 0;
        UpdateCarouselUI();
    }

    /// <summary>
    /// Método de actualización de Unity.
    /// Escucha la entrada del usuario (teclado) y actualiza la animación del personaje central.
    /// </summary>
    void Update()
    {
        // Permitir usar flechas del teclado
        if (availableCharacters != null && availableCharacters.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) NextCharacter();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) PreviousCharacter();
            if (Input.GetKeyDown(KeyCode.Return)) SelectCharacter();

            // Lógica de Animación (Reproducir los PNGs como GIF solo para el del centro)
            if (centerImage != null && centerImage.gameObject.activeSelf)
            {
                CharacterData currentCharacter = availableCharacters[currentIndex];
                if (currentCharacter.idleAnimation != null && currentCharacter.idleAnimation.Length > 0)
                {
                    animationTimer += Time.deltaTime;
                    if (animationTimer >= currentCharacter.animationSpeed)
                    {
                        animationTimer = 0f;
                        currentAnimationFrame = (currentAnimationFrame + 1) % currentCharacter.idleAnimation.Length;
                        centerImage.sprite = currentCharacter.idleAnimation[currentAnimationFrame];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Avanza al siguiente personaje en el carrusel.
    /// El carrusel es circular, por lo que vuelve al principio si se pasa del final.
    /// </summary>
    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % availableCharacters.Count;
        UpdateCarouselUI();
    }

    /// <summary>
    /// Retrocede al personaje anterior en el carrusel.
    /// El carrusel es circular, por lo que va al final si se retrocede desde el principio.
    /// </summary>
    public void PreviousCharacter()
    {
        currentIndex = (currentIndex - 1 + availableCharacters.Count) % availableCharacters.Count;
        UpdateCarouselUI();
    }

    /// <summary>
    /// Actualiza la interfaz de usuario del carrusel con la información e imágenes del personaje seleccionado,
    /// así como los personajes adyacentes a la izquierda y derecha.
    /// </summary>
    private void UpdateCarouselUI()
    {
        CharacterData currentCharacter = availableCharacters[currentIndex];

        // Mostrar textos con formato profesional (Rich Text de TextMeshPro)
        if (nameText != null) 
            nameText.text = $"<color=#FFD700>»</color> {currentCharacter.characterName.ToUpper()} <color=#FFD700>«</color>";
            
        if (descriptionText != null) 
            descriptionText.text = $"<color=#888888><size=80%>DATOS DEL LUCHADOR</size></color>\n<color=#FFFFFF>{currentCharacter.description}</color>";
        
        // Reiniciar la animación al cambiar de personaje para el del centro
        currentAnimationFrame = 0;
        animationTimer = 0f;
        
        // --- IMAGEN CENTRAL (El seleccionado / animado) ---
        if (centerImage != null)
        {
            centerImage.gameObject.SetActive(true);
            if (currentCharacter.idleAnimation != null && currentCharacter.idleAnimation.Length > 0)
                centerImage.sprite = currentCharacter.idleAnimation[0]; 
        }

        // Calcular índices para hacer el carrusel infinito (circular)
        int leftIndex = (currentIndex - 1 + availableCharacters.Count) % availableCharacters.Count;
        int rightIndex = (currentIndex + 1) % availableCharacters.Count;

        // --- IMAGEN IZQUIERDA (Estática) ---
        if (leftImage != null)
        {
            leftImage.gameObject.SetActive(true);
            CharacterData leftChar = availableCharacters[leftIndex];
            if (leftChar.idleAnimation != null && leftChar.idleAnimation.Length > 0)
                leftImage.sprite = leftChar.idleAnimation[0];
        }

        // --- IMAGEN DERECHA (Estática) ---
        if (rightImage != null)
        {
            rightImage.gameObject.SetActive(true);
            CharacterData rightChar = availableCharacters[rightIndex];
            if (rightChar.idleAnimation != null && rightChar.idleAnimation.Length > 0)
                rightImage.sprite = rightChar.idleAnimation[0];
        }

        // Flechas siempre activas porque el carrusel ahora es infinito
        if (leftArrow != null) leftArrow.interactable = true;
        if (rightArrow != null) rightArrow.interactable = true;
    }

    /// <summary>
    /// Confirma la selección del personaje actual y carga la escena de batalla.
    /// </summary>
    public void SelectCharacter()
    {
        // Como es solo el catálogo por ahora, el botón Seleccionar simplemente
        // avanza al combate según el flujo: menú -> carrusel -> combate.
        Debug.Log("Saliendo del carrusel, cargando escena de batalla...");
        SceneManager.LoadScene(battleSceneName);
    }

    /// <summary>
    /// Vuelve a la escena del menú principal.
    /// </summary>
    public void VolverAlMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene(menuSceneName);
    }
}
