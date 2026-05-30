/**
 * @file CombatManager.cs
 * @brief Manages the combat rounds, player scores, UI updates, and match states.
 * 
 * This script is responsible for handling the overall flow of the combat match,
 * including starting rounds, ending rounds, checking for match win conditions,
 * and managing player inputs during transitions.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Singleton class that manages the combat flow, round logic, scoring, and UI representation
/// in a fighting game context.
/// </summary>
public class CombatManager : MonoBehaviour
{
    /// <summary>
    /// Global instance accessible from any script.
    /// </summary>
    public static CombatManager Instance { get; private set; }

    [Header("Jugadores")]
    /// <summary>Reference to the HealthSystem of Player 1.</summary>
    public HealthSystem player1Health;
    /// <summary>Reference to the HealthSystem of Player 2.</summary>
    public HealthSystem player2Health;

    [Header("Nombres en pantalla")]
    /// <summary>Display name for Player 1.</summary>
    public string player1Name = "Jugador 1";
    /// <summary>Display name for Player 2.</summary>
    public string player2Name = "Jugador 2";

    [Header("Componentes a desactivar al terminar/iniciar round")]
    /// <summary>Movement component for Player 1.</summary>
    public PlayerMovement player1Movement;
    /// <summary>Movement component for Player 2.</summary>
    public PlayerMovement player2Movement;
    /// <summary>Attack component for Player 1.</summary>
    public PlayerAttack player1Attack;
    /// <summary>Attack component for Player 2.</summary>
    public PlayerAttack player2Attack;
    /// <summary>Special attack component for Player 1.</summary>
    public PlayerSpecialAttack player1Special;
    /// <summary>Special attack component for Player 2.</summary>
    public PlayerSpecialAttack player2Special;

    [Header("UI de resultado final")]
    /// <summary>UI panel displayed at the end of a round or match.</summary>
    public GameObject resultPanel;
    /// <summary>Text component displaying the result (e.g., winner name or tie).</summary>
    public TMP_Text resultText;

    [Header("Sistema de Rondas (NUEVO)")]
    /// <summary>Text component displaying the current score.</summary>
    public TMP_Text scoreText; // Asignar el texto en el inspector para el marcador
    /// <summary>Number of rounds a player needs to win to win the match.</summary>
    public int roundsToWin = 2; // Mejor de 3 rounds
    /// <summary>Time duration between the end of a round and the start of the next one.</summary>
    public float timeBetweenRounds = 2f;

    private int player1Wins = 0;
    private int player2Wins = 0;
    private bool roundActive = false;
    private bool isFirstRound = true;

    private bool combatEnded = false;

    /// <summary>Indicates whether the overall combat match has ended.</summary>
    public bool isCombatEnded => combatEnded;

    /// <summary>Indicates whether a round is currently active and playable.</summary>
    public bool isRoundActive => roundActive;

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        // Singleton: solo existe una instancia a la vez
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Delays initialization by one frame to ensure dependencies are set up,
    /// then starts the match.
    /// </summary>
    IEnumerator Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        // Esperar 1 frame para que HealthSystem y EnergySystem ejecuten sus Start()
        yield return null;

        StartMatch();
    }

    /// <summary>
    /// Resets match scores and starts the first round.
    /// </summary>
    public void StartMatch()
    {
        player1Wins = 0;
        player2Wins = 0;
        UpdateScoreUI();
        StartCoroutine(StartNewRoundCoroutine());
    }

    /// <summary>
    /// Coroutine that handles the initialization and countdown of a new round.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator StartNewRoundCoroutine()
    {
        roundActive = false;
        combatEnded = false;

        // Ocultar mensaje de victoria al iniciar un nuevo round
        if (resultPanel != null) resultPanel.SetActive(false);

        // Tarea: Bloquear inputs antes de iniciar
        DisableAllControls();

        // CA-02: Reiniciar posiciones y vida (100%)
        if (player1Health != null) player1Health.ResetPlayer();
        if (player2Health != null) player2Health.ResetPlayer();

        // Desactivar controles nuevamente por seguridad (si ResetPlayer activó movement)
        DisableAllControls();

        // Calcular el tiempo que deben estar bloqueados dependiendo del audio
        float currentWaitTime = timeBetweenRounds;
        if (isFirstRound && AudioManager.Instance != null && AudioManager.Instance.matchStartJingle != null)
        {
            // En el primer round, esperar a que termine el jingle completo
            currentWaitTime = Mathf.Max(timeBetweenRounds, AudioManager.Instance.matchStartJingle.length);
            isFirstRound = false;
        }
        else if (AudioManager.Instance != null && AudioManager.Instance.roundStartSound != null)
        {
            // En los siguientes rounds, esperar a que termine el sonido de "Fight!"
            currentWaitTime = Mathf.Max(timeBetweenRounds, AudioManager.Instance.roundStartSound.length);
        }

        // Esperar antes de iniciar el combate bloqueando todo
        yield return new WaitForSeconds(currentWaitTime);

        // Desbloquear inputs y comenzar
        EnableAllControls();
        roundActive = true;
    }

    /// <summary>
    /// Called when a player's health drops to zero, triggering round end logic.
    /// </summary>
    /// <param name="deadPlayer">The HealthSystem of the player who died.</param>
    public void NotifyPlayerDeath(HealthSystem deadPlayer)
    {
        if (combatEnded) return; // Evitar que se ejecute ms de una vez
        
        // Reproducir sonido de muerte
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeathSound();

        // CA-04: Esperamos al final del frame por si ambos mueren simultáneamente
        StartCoroutine(HandleRoundEndCoroutine());
    }

    /// <summary>
    /// Coroutine that resolves round outcomes, displays results, and either
    /// progresses to the next round or ends the match.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator HandleRoundEndCoroutine()
    {
        roundActive = false;
        combatEnded = true; 
        
        // Tarea: Bloquear inputs al terminar cada round
        DisableAllControls();

        // Esperar al final del frame para confirmar empates
        yield return new WaitForEndOfFrame();

        if (player1Health == null || player2Health == null)
        {
            Debug.LogError("ERROR: player1Health o player2Health no están asignados en el CombatManager.");
            yield break; // Detiene la corrutina para evitar el error
        }

        bool p1Dead = player1Health.currentHealth <= 0;
        bool p2Dead = player2Health.currentHealth <= 0;

        // CA-04: Empate simultáneo
        if (p1Dead && p2Dead)
        {
            player1Wins++;
            player2Wins++;
            ShowRoundResult("¡Empate Simultáneo!");
        }
        // CA-01: Gana P2
        else if (p1Dead)
        {
            player2Wins++;
            ShowRoundResult($"¡{player2Name} Gana el Round!");
        }
        // CA-01: Gana P1
        else if (p2Dead)
        {
            player1Wins++;
            ShowRoundResult($"¡{player1Name} Gana el Round!");
        }

        UpdateScoreUI();

        // Pausa para que se reproduzca la animación de muerte
        yield return new WaitForSeconds(2f);

        // CA-03: Evaluar si alguien alcanzó 2 victorias
        if (player1Wins >= roundsToWin || player2Wins >= roundsToWin)
        {
            EndMatch();
        }
        else
        {
            // Iniciar siguiente round
            StartCoroutine(StartNewRoundCoroutine());
        }
    }

    /// <summary>
    /// Displays the specified result text on the result panel.
    /// </summary>
    /// <param name="text">The text to display.</param>
    private void ShowRoundResult(string text)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = text;
    }

    /// <summary>
    /// Ends the match and displays the overall winner or a tie.
    /// </summary>
    private void EndMatch()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        // Mostrar pantalla de resultado final
        if (player1Wins >= roundsToWin && player2Wins >= roundsToWin)
        {
            if (resultText != null) resultText.text = "¡EMPATE ÉPICO!";
        }
        else if (player1Wins >= roundsToWin)
        {
            if (resultText != null) resultText.text = $"¡{player1Name} GANA LA PARTIDA!";
        }
        else if (player2Wins >= roundsToWin)
        {
            if (resultText != null) resultText.text = $"¡{player2Name} GANA LA PARTIDA!";
        }
    }

    /// <summary>
    /// Restarts the match from the beginning, resetting scores and state.
    /// </summary>
    public void RestartMatch()
    {
        // Reiniciar contadores y UI
        player1Wins = 0;
        player2Wins = 0;
        isFirstRound = true; // Reiniciar estado del primer round
        UpdateScoreUI();
        
        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultText != null) resultText.text = "";
        
        combatEnded = false;
        
        // CA-03: Reiniciar combate desde el round 1 con vida completa
        if (player1Health != null) player1Health.ResetPlayer();
        if (player2Health != null) player2Health.ResetPlayer();
        
        StartCoroutine(StartNewRoundCoroutine());
    }

    /// <summary>
    /// For backwards compatibility with older code implementations.
    /// </summary>
    /// <param name="loserName">The name of the loser.</param>
    public void EndCombat(string loserName)
    {
        // ...
    }

    /// <summary>
    /// Updates the UI displaying the current match scores and player names.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // Formato profesional tipo e-Sports/Arcade:
            // Nombres en color, números gigantes en dorado, y un pequeño 'VS' en el centro.
            scoreText.text = $"<color=#FF4444>{player1Name.ToUpper()}</color>  <size=150%><color=#FFD700>{player1Wins}</color></size>  <color=#FFFFFF><size=60%>V S</size></color>  <size=150%><color=#FFD700>{player2Wins}</color></size>  <color=#4444FF>{player2Name.ToUpper()}</color>";
        }
    }

    /// <summary>
    /// Disables all player inputs (movement, attacks, special attacks).
    /// </summary>
    void DisableAllControls()
    {
        if (player1Movement != null) player1Movement.enabled = false;
        if (player2Movement != null) player2Movement.enabled = false;
        if (player1Attack   != null) player1Attack.enabled   = false;
        if (player2Attack   != null) player2Attack.enabled   = false;
        if (player1Special  != null) player1Special.enabled  = false;
        if (player2Special  != null) player2Special.enabled  = false;
    }

    /// <summary>
    /// Enables all player inputs (movement, attacks, special attacks).
    /// </summary>
    void EnableAllControls()
    {
        if (player1Movement != null) player1Movement.enabled = true;
        if (player2Movement != null) player2Movement.enabled = true;
        if (player1Attack   != null) player1Attack.enabled   = true;
        if (player2Attack   != null) player2Attack.enabled   = true;
        if (player1Special  != null) player1Special.enabled  = true;
        if (player2Special  != null) player2Special.enabled  = true;
    }
}