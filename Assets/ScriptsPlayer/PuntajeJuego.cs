using UnityEngine;

// ============================================================================
// PUNTAJE GLOBAL DE LA PARTIDA
// Es una clase estática: no vive en ninguna escena, así que el puntaje
// sobrevive cuando pasas de Nivel_1 a Nivel_2 y Nivel_3.
// ============================================================================
public static class PuntajeJuego
{
    public static int puntos = 0;
    public static int bacteriasEliminadas = 0;

    private static float tiempoInicio = 0f;

    // ----- Sistema de combo -----
    // Matar varios enemigos en cadena (dentro de la ventana de tiempo)
    // multiplica los puntos: x2, x3... hasta x5.
    private const float VENTANA_COMBO = 3f;
    private const int COMBO_MAXIMO = 5;

    private static int comboActual = 0;
    private static float ultimaEliminacion = -999f;

    // Se llama al empezar una partida nueva (al cargar el primer nivel)
    public static void ReiniciarPartida()
    {
        puntos = 0;
        bacteriasEliminadas = 0;
        tiempoInicio = Time.time;
        comboActual = 0;
        ultimaEliminacion = -999f;
    }

    // Cada enemigo llama a esto al morir con los puntos base que vale.
    // Devuelve los puntos realmente ganados (con el multiplicador de combo),
    // para poder mostrarlos flotando sobre el enemigo.
    public static int SumarEliminacion(int puntosBase)
    {
        if (Time.time - ultimaEliminacion <= VENTANA_COMBO)
        {
            comboActual++;
        }
        else
        {
            comboActual = 1;
        }

        ultimaEliminacion = Time.time;

        int multiplicador = Mathf.Min(comboActual, COMBO_MAXIMO);
        int ganancia = puntosBase * multiplicador;

        puntos += ganancia;
        bacteriasEliminadas++;

        return ganancia;
    }

    // Combo vigente ahora mismo (0 si la ventana ya expiró). El HUD lo muestra.
    public static int ComboVigente()
    {
        if (Time.time - ultimaEliminacion <= VENTANA_COMBO)
        {
            return comboActual;
        }

        return 0;
    }

    public static float TiempoTranscurrido()
    {
        return Time.time - tiempoInicio;
    }

    // Formato mm:ss para mostrar en pantalla
    public static string TiempoFormateado()
    {
        int totalSegundos = Mathf.FloorToInt(TiempoTranscurrido());
        int minutos = totalSegundos / 60;
        int segundos = totalSegundos % 60;
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}
