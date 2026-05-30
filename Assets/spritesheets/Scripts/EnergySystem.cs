// -----------------------------------------------------------------------
// <copyright file="EnergySystem.cs">
// Copyright (c) Standard Company. All rights reserved.
// </copyright>
// <author>Auto-generated</author>
// <summary>Sistema de energía para el jugador.</summary>
// -----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el sistema de energía del personaje, incluyendo la ganancia de energía
/// por diversas acciones y la actualización de la interfaz de usuario.
/// </summary>
public class EnergySystem : MonoBehaviour
{
    /// <summary>
    /// La cantidad máxima de energía que el personaje puede acumular.
    /// </summary>
    public int maxEnergy = 100;

    /// <summary>
    /// La cantidad de energía actual del personaje.
    /// </summary>
    public int currentEnergy;

    /// <summary>
    /// Componente de imagen de la interfaz de usuario utilizado para mostrar la barra de energía.
    /// </summary>
    public Image energyImage;

    /// <summary>
    /// Arreglo de sprites que representan los diferentes niveles de la barra de energía.
    /// </summary>
    private Sprite[] energySprites;

    /// <summary>
    /// Inicializa el sistema de energía cargando los sprites y reiniciando la energía actual.
    /// </summary>
    void Start()
    {
        // Cargar spritesheet de energía desde la carpeta Resources
        energySprites = Resources.LoadAll<Sprite>("EnergyBar/emerald_counter-Sheet");

        currentEnergy = 0;
        UpdateEnergyUI();
    }

    /// <summary>
    /// Incrementa la energía cuando el personaje recibe daño del oponente.
    /// </summary>
    public void GainEnergyFromDamage()
    {
        currentEnergy += 2;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    /// <summary>
    /// Incrementa la energía cuando el personaje conecta un ataque, dependiendo del tipo de ataque.
    /// </summary>
    /// <param name="attackName">El nombre del ataque realizado.</param>
    public void GainEnergyFromAttack(string attackName)
    {
        int gain = 0;
        if (attackName == "Puñetazo") gain = 5;
        else if (attackName == "Patada") gain = 10;

        currentEnergy += gain;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

        UpdateEnergyUI();
    }

    /// <summary>
    /// Incrementa la energía cuando el personaje bloquea un ataque exitosamente.
    /// </summary>
    public void GainEnergyFromBlock()
    {
        currentEnergy += 3;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

        UpdateEnergyUI();
    }

    /// <summary>
    /// Actualiza la imagen de la interfaz de usuario para reflejar el nivel de energía actual.
    /// </summary>
    void UpdateEnergyUI()
    {
        // Guard: evitar división por cero si maxEnergy es inválido
        if (maxEnergy <= 0)
        {
            Debug.LogError("maxEnergy debe ser mayor que cero", this);
            return;
        }

        if (energyImage == null)
        {
            Debug.LogError("energyImage no está asignada en el Inspector", this);
            return;
        }

        if (energySprites == null || energySprites.Length == 0)
        {
            Debug.LogError("No se cargaron los sprites de energía", this);
            return;
        }

        // Calcular índice: 0 = vacío, último = lleno. Invertimos porque el sprite 0 es el lleno.
        int index = Mathf.RoundToInt(((float)currentEnergy / maxEnergy) * (energySprites.Length - 1));
        index = Mathf.Clamp(index, 0, energySprites.Length - 1);
        
        index = (energySprites.Length - 1) - index;
        
        energyImage.sprite = energySprites[index];
        energyImage.enabled = true;
        energyImage.color = Color.white;
    }

    /// <summary>
    /// Comprueba si la barra de energía está completamente llena.
    /// </summary>
    /// <returns>Verdadero si la energía actual es igual o mayor a la energía máxima.</returns>
    public bool IsFull()
    {
        return currentEnergy >= maxEnergy;
    }

    /// <summary>
    /// Consume una cantidad específica de energía.
    /// </summary>
    /// <param name="amount">La cantidad de energía a consumir.</param>
    public void ConsumeEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        UpdateEnergyUI();
    }
}