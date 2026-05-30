//-----------------------------------------------------------------------
// <copyright file="GameData.cs">
//     Copyright (c). All rights reserved.
// </copyright>
// <summary>Defines the GameData class.</summary>
//-----------------------------------------------------------------------

/// <summary>
/// A static class used to store global game data that needs to persist across different scenes.
/// Aquí guardaremos las selecciones para que la escena de Batalla las lea.
/// </summary>
public static class GameData
{
    /// <summary>
    /// The CharacterData selected for Player 1.
    /// </summary>
    public static CharacterData player1Character;

    /// <summary>
    /// The CharacterData selected for Player 2.
    /// </summary>
    public static CharacterData player2Character;
}
