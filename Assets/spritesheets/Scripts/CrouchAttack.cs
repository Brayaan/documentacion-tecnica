//-----------------------------------------------------------------------
// <copyright file="CrouchAttack.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
// <summary>Handles character crouch and crouch attack actions.</summary>
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the crouching state and crouch attack mechanics of the character.
/// Listens to user input to trigger crouch animations and corresponding attack states.
/// </summary>
public class CrouchAttack : MonoBehaviour
{
    /// <summary>
    /// Reference to the character's Animator component to handle animation transitions.
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Tracks if the character is currently in a crouching position.
    /// </summary>
    private bool estaAgachado;

    /// <summary>
    /// Tracks if the character is currently performing an attack.
    /// </summary>
    private bool atacando = false;

    /// <summary>
    /// Called once per frame. Updates crouching state based on player input 
    /// and triggers the crouch attack if conditions are met.
    /// </summary>
    void Update()
    {
        // Detectar si está agachado
        estaAgachado = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        // Actualizar Animator
        animator.SetBool("isCrouching", estaAgachado);

        // Ataque agachado con Q
        if (estaAgachado && Input.GetKeyDown(KeyCode.Q) && !atacando)
        {
            StartCoroutine(RealizarAtaque());
        }
    }

    /// <summary>
    /// Executes the crouch attack sequence, including triggering the animation
    /// and managing the attack state duration.
    /// </summary>
    /// <returns>An IEnumerator used for coroutine timing.</returns>
    IEnumerator RealizarAtaque()
    {
        atacando = true;

        animator.SetTrigger("CrouchAttack");

        // Duración de la animación (ajústala)
        yield return new WaitForSeconds(0.5f);

        atacando = false;
    }
}