//-----------------------------------------------------------------------
// <copyright file="CharacterData.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
// <summary>
//     Contains the definition for the CharacterData ScriptableObject.
// </summary>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Represents the data defining a character in the fighting game.
/// This ScriptableObject holds information such as the character's name,
/// animations, description, and prefab.
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Juego Pelea/Personaje")]
public class CharacterData : ScriptableObject
{
    /// <summary>
    /// The display name of the character.
    /// </summary>
    public string characterName;
    
    /// <summary>
    /// The sequence of sprites that make up the character's idle animation.
    /// Used to create a GIF-like animation effect.
    /// </summary>
    [Header("Animación (Estilo GIF)")]
    public Sprite[] idleAnimation; // Varios PNGs para hacer la animación
    
    /// <summary>
    /// The time delay in seconds between each frame of the animation.
    /// For example, 0.1 means a new frame every 0.1 seconds.
    /// </summary>
    public float animationSpeed = 0.1f; // Velocidad (ej: 0.1 segundos por frame)
    
    /// <summary>
    /// A short description or lore for the character.
    /// </summary>
    [TextArea(3, 5)]
    public string description = "Descripción del personaje aquí...";
    
    /// <summary>
    /// An optional reference to the character's prefab, which can be 
    /// instantiated during combat.
    /// </summary>
    // Prefab del jugador para instanciar en combate (opcional para el futuro)
    public GameObject characterPrefab;
}
