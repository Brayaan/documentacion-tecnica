// ============================================================
// ARCHIVO: EnergySystem.cs
// PROPÓSITO: Gestionar la barra de energía del jugador,
//            incluyendo ganancia por combate y consumo para habilidades.
// RESPONSABILIDAD: Acumular y gastar energía; actualizar la UI visual.
// SISTEMA: Combate — Energía / Recursos del Jugador
// ============================================================

/*
 * DESCRIPCIÓN DEL SISTEMA
 * -----------------------
 * Este script controla la energía del jugador, un recurso que se carga
 * durante el combate (atacando, bloqueando o recibiendo daño) y puede
 * consumirse para activar habilidades especiales.
 *
 * La UI se representa como un spritesheet de frames cargado desde Resources,
 * donde cada frame corresponde a un nivel de energía distinto.
 * El índice se invierte para que el frame 0 represente la barra llena.
 *
 * SISTEMA: Combate / Recursos
 * INTERACTÚA CON: Hitbox.cs, ExtendedHitbox.cs, PlayerDefense.cs
 */

using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    // -------------------------
    // VARIABLES PÚBLICAS
    // -------------------------

    /// <summary>
    /// Energía máxima acumulable. Define el techo del recurso.
    /// Configurable desde el Inspector. No debe ser 0 o negativo.
    /// </summary>
    public int maxEnergy = 100;

    /// <summary>
    /// Energía actual del jugador en tiempo real.
    /// Se inicializa en 0 al comenzar la partida.
    /// Visible en Inspector para debug.
    /// </summary>
    public int currentEnergy;

    /// <summary>
    /// Componente Image de Unity UI que muestra visualmente la barra de energía.
    /// Debe asignarse desde el Inspector. Si es null, la UI no se actualiza.
    /// </summary>
    public Image energyImage;

    // -------------------------
    // VARIABLES PRIVADAS
    // -------------------------

    /// <summary>
    /// Array de sprites cargados desde el spritesheet de energía.
    /// Se cargan automáticamente desde Resources/EnergyBar/emerald_counter-Sheet.
    /// Cada elemento corresponde a un frame del estado de la barra.
    /// </summary>
    private Sprite[] energySprites;

    // ============================================================
    // EVENTOS DE UNITY
    // ============================================================

    /// <summary>
    /// Start() — Se ejecuta una vez al iniciar.
    /// Carga el spritesheet de energía desde Resources,
    /// inicializa currentEnergy en 0 y actualiza la UI.
    ///
    /// DEPENDENCIA: El archivo debe existir en Resources/EnergyBar/emerald_counter-Sheet.
    /// </summary>
    void Start()
    {
        // Cargar spritesheet de energía desde la carpeta Resources
        energySprites = Resources.LoadAll<Sprite>("EnergyBar/emerald_counter-Sheet");

        currentEnergy = 0;
        UpdateEnergyUI();
    }

    // ============================================================
    // MÉTODOS PRINCIPALES — GANANCIA DE ENERGÍA
    // ============================================================

    /// <summary>
    /// Otorga energía al jugador cuando recibe un golpe del oponente.
    /// Incremento fijo de 2 puntos. Representa la mecánica de "cargarse
    /// al absorber daño", común en juegos de pelea.
    ///
    /// CUÁNDO SE LLAMA: Desde Hitbox.cs cuando un golpe conecta en el jugador.
    /// </summary>
    public void GainEnergyFromDamage()
    {
        currentEnergy += 2;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateEnergyUI();
    }

    /// <summary>
    /// Otorga energía al jugador según el tipo de ataque que conectó.
    /// Cada tipo de ataque da una cantidad diferente:
    ///   - "Puñetazo" → +5 de energía
    ///   - "Patada"   → +10 de energía
    ///
    /// CUÁNDO SE LLAMA: Desde Hitbox.cs y ExtendedHitbox.cs al conectar un golpe.
    /// </summary>
    /// <param name="attackName">Nombre del ataque ejecutado ("Puñetazo" o "Patada").</param>
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
    /// Otorga energía al jugador al activar el bloqueo (+3 puntos).
    /// Se llama solo al inicio del bloqueo, no mientras se mantiene presionado.
    ///
    /// CUÁNDO SE LLAMA: Desde PlayerDefense.cs en el frame en que isBlocking cambia a true.
    /// </summary>
    public void GainEnergyFromBlock()
    {
        currentEnergy += 3;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

        UpdateEnergyUI();
    }

    // ============================================================
    // MÉTODOS PRINCIPALES — ESTADO Y CONSUMO
    // ============================================================

    /// <summary>
    /// Devuelve true si la barra de energía está completamente llena.
    /// Útil para verificar si el jugador puede activar una habilidad especial.
    /// </summary>
    public bool IsFull()
    {
        return currentEnergy >= maxEnergy;
    }

    /// <summary>
    /// Consume una cantidad de energía al usar una habilidad especial.
    /// Clampea currentEnergy a 0 para evitar valores negativos.
    /// Actualiza la UI inmediatamente después.
    ///
    /// CUÁNDO SE LLAMA: Por scripts de habilidades o ataques especiales.
    /// </summary>
    /// <param name="amount">Cantidad de energía a consumir.</param>
    public void ConsumeEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        UpdateEnergyUI();
    }

    // ============================================================
    // MÉTODOS AUXILIARES
    // ============================================================

    /// <summary>
    /// Actualiza el sprite de la barra de energía en la UI según currentEnergy.
    /// Calcula el índice proporcional en el spritesheet e invierte el orden
    /// para que el sprite 0 del array corresponda a energía llena.
    ///
    /// CUÁNDO SE EJECUTA: Llamado internamente después de cualquier cambio en currentEnergy.
    ///
    /// LÓGICA DE INVERSIÓN:
    ///   index normal  = 0  → energía vacía (sprite de barra vacía)
    ///   index invertido = (Length-1) - index → invierte el mapeo
    ///   Resultado: index 0 del array = barra llena
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

        // Calcular índice proporcional según la energía actual
        int index = Mathf.RoundToInt(((float)currentEnergy / maxEnergy) * (energySprites.Length - 1));
        index = Mathf.Clamp(index, 0, energySprites.Length - 1);

        // Invertir: sprite 0 del array = barra llena
        index = (energySprites.Length - 1) - index;
        energyImage.sprite = energySprites[index];
    }

    // ============================================================
    // FLUJO DEL SISTEMA
    // ============================================================
    /*
     * 1. Start() → carga sprites y establece currentEnergy = 0.
     * 2. Durante el combate, otros scripts llaman GainEnergyFrom*().
     * 3. Cada ganancia acumula energía y llama UpdateEnergyUI().
     * 4. UpdateEnergyUI() calcula el frame correcto y lo muestra en pantalla.
     * 5. ConsumeEnergy() reduce la energía (habilidades especiales) → UpdateEnergyUI().
     * 6. IsFull() permite verificar si se puede activar una habilidad especial.
     */

    // ============================================================
    // INTERACCIONES CON OTROS SISTEMAS
    // ============================================================
    /*
     * ES LLAMADO POR:
     *   - Hitbox.cs → GainEnergyFromAttack() y GainEnergyFromDamage()
     *   - ExtendedHitbox.cs → GainEnergyFromAttack()
     *   - PlayerDefense.cs → GainEnergyFromBlock()
     *
     * AFECTA A:
     *   - energyImage (UI) — se actualiza en cada cambio.
     *
     * NO AFECTA directamente a ningún otro sistema de combate.
     */

    // ============================================================
    // NOTAS / ADVERTENCIAS
    // ============================================================
    /*
     * ⚠ El spritesheet DEBE estar en Resources/EnergyBar/emerald_counter-Sheet.
     *   Si la ruta o el nombre cambian, los sprites no se cargarán y
     *   la UI quedará en blanco sin lanzar error en tiempo de ejecución.
     *
     * ⚠ maxEnergy nunca debe ser 0. Si se configura en 0 desde el Inspector,
     *   UpdateEnergyUI() lanzará un error y la UI no se actualizará.
     *
     * ⚠ Los nombres de ataque en GainEnergyFromAttack() son strings literales
     *   ("Puñetazo", "Patada"). Si Hitbox.cs cambia el attackName,
     *   la energía de ese ataque dejará de acumularse silenciosamente.
     *   Considerar usar un enum en el futuro para evitar este acoplamiento frágil.
     */
}
