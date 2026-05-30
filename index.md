# Documentación Técnica

API Reference & Guía de Desarrollo

---

## Estructura del Proyecto

| Carpeta | Descripción |
|---------|-------------|
| Assets/spritesheets/Scripts/ | Scripts principales del juego |
| Assets/Tests/PlayMode/ | Pruebas en modo Play |

---

## Clases Principales

| Clase | Descripción |
|-------|-------------|
| CombatCollisionManager | Gestiona colisiones de combate |
| HealthSystem | Controla salud y daño |
| PlayerMovement | Maneja movimiento del jugador |
| EnergySystem | Gestiona energía del personaje |

---

## Guía Rápida

**Ver todas las clases**
Haz clic en la pestaña "Clases" en el menú superior.

**Explorar archivos**
Haz clic en "Archivos" para ver todos los scripts.

**Buscar algo específico**
Usa la barra de búsqueda en la parte superior.

---

## Ejemplos de Código

Crear una instancia de HealthSystem

    HealthSystem health = gameObject.AddComponent<HealthSystem>();
    health.maxHealth = 100f;
    health.currentHealth = 100f;

Configurar movimiento del jugador

    PlayerMovement movement = GetComponent<PlayerMovement>();
    movement.speed = 5f;
    movement.jumpForce = 10f;

---

## Recursos Adicionales

- https://docs.unity3d.com/Manual/index.html
- https://docs.unity3d.com/ScriptReference/index.html
- https://docs.microsoft.com/es-es/dotnet/csharp/

---

## Convenciones de Documentación

Usa comentarios XML en tu código:

    /// <summary>
    /// Descripcion breve de la clase o metodo
    /// </summary>
    /// <param name="parametro">Descripcion del parametro</param>
    /// <returns>Descripcion del valor de retorno</returns>

---

## Contribución

Regenera la documentación con:

    doxygen Doxyfile_completo

---

*Documentación generada con Doxygen*