//-----------------------------------------------------------------------
// <copyright file="AudioManager.cs" company="None">
//     Copyright (c) None. All rights reserved.
// </copyright>
// <author>Antigravity</author>
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages the audio playback in the scene, including background music, sound effects, and voice overs.
/// Implements a simple Singleton pattern for easy access.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the AudioManager.
    /// </summary>
    public static AudioManager Instance;

    [Header("Audio Mixer Groups")]
    /// <summary>
    /// The AudioMixerGroup for music tracks.
    /// </summary>
    public AudioMixerGroup musicGroup;

    /// <summary>
    /// The AudioMixerGroup for sound effects (SFX).
    /// </summary>
    public AudioMixerGroup sfxGroup;

    [Header("Audio Clips (Efectos)")]
    /// <summary>
    /// The audio clip to play when a punch attack lands.
    /// </summary>
    public AudioClip punchSound;

    /// <summary>
    /// The audio clip to play when a kick attack lands.
    /// </summary>
    public AudioClip kickSound;

    /// <summary>
    /// The audio clip to play when an attack is blocked.
    /// </summary>
    public AudioClip blockSound;

    /// <summary>
    /// The audio clip to play when a character dies.
    /// </summary>
    public AudioClip deathSound;

    /// <summary>
    /// The audio clip to play when a round starts (e.g., "Fight!").
    /// </summary>
    public AudioClip roundStartSound; // "Fight!"

    [Header("Audio Clips (Música)")]
    /// <summary>
    /// The audio clip to play as an intro jingle before the background music starts.
    /// </summary>
    public AudioClip matchStartJingle; // Música/sonido intro del Round 1

    /// <summary>
    /// The main background music to loop during the scene.
    /// </summary>
    public AudioClip backgroundMusic;

    [Header("Ajustes SFX")]
    /// <summary>
    /// The minimum pitch variation for sound effects.
    /// </summary>
    [Range(0.8f, 1.2f)]
    public float pitchMin = 0.9f;

    /// <summary>
    /// The maximum pitch variation for sound effects.
    /// </summary>
    [Range(0.8f, 1.2f)]
    public float pitchMax = 1.1f;

    // Reproductores de Audio Internos
    private AudioSource musicSource;
    private AudioSource sfxSource;

    /// <summary>
    /// Initializes the singleton instance and sets up the audio sources.
    /// </summary>
    private void Awake()
    {
        // Al quitar DontDestroyOnLoad, este AudioManager morirá al salir de la escena de pelea,
        // permitiendo que la escena del menú vuelva a tocar su propia música limpia.
        Instance = this;
        SetupAudioSources();
    }

    /// <summary>
    /// Creates and configures the AudioSources for music and SFX, and handles the initial music playback logic.
    /// </summary>
    private void SetupAudioSources()
    {
        // Crear el reproductor de música
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Crear el reproductor de SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.outputAudioMixerGroup = sfxGroup;
        sfxSource.playOnAwake = false;

        // Lógica de intro musical
        if (matchStartJingle != null)
        {
            // Reproducir el jingle en el canal de música o sfx (usaremos sfx para que no lo corte el loop)
            sfxSource.PlayOneShot(matchStartJingle);
            
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.PlayDelayed(matchStartJingle.length); // La música espera a que termine el jingle
            }
        }
        else if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Plays an attack hit sound based on the attack name, with a slight pitch variation.
    /// </summary>
    /// <param name="attackName">The name of the attack (e.g., "Kick", "Punch") to determine the sound clip.</param>
    // CA-01: Reproducir sonido de golpe según tipo (con Pitch Variation)
    public void PlayHitSound(string attackName)
    {
        AudioClip clipToPlay = punchSound; // Por defecto

        // Detectar si el ataque se llama Kick, Patada, etc.
        if (attackName.ToLower().Contains("kick") || attackName.ToLower().Contains("patada"))
        {
            clipToPlay = kickSound;
        }

        if (clipToPlay != null)
        {
            // Pitch Variation: cambia sutilmente el tono para que no suene repetitivo
            sfxSource.pitch = Random.Range(pitchMin, pitchMax);
            sfxSource.PlayOneShot(clipToPlay);
        }
    }

    /// <summary>
    /// Plays the block sound effect with a slight pitch variation.
    /// </summary>
    // CA-03: Sonido de bloqueo
    public void PlayBlockSound()
    {
        if (blockSound != null)
        {
            // El bloqueo suele tener un pitch más constante, pero variarlo levemente es bueno
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(blockSound);
        }
    }

    /// <summary>
    /// Plays the round start announcement sound (e.g., "Fight!").
    /// </summary>
    // CA-04: Sonido de inicio de partida
    public void PlayRoundStartSound()
    {
        if (roundStartSound != null)
        {
            sfxSource.pitch = 1f; // El anunciador siempre debe sonar normal
            sfxSource.PlayOneShot(roundStartSound);
        }
    }

    /// <summary>
    /// Plays the death sound effect when a character is defeated.
    /// </summary>
    // Sonido al morir
    public void PlayDeathSound()
    {
        if (deathSound != null)
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(deathSound);
        }
    }
}
