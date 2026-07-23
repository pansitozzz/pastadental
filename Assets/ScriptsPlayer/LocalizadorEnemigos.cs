using UnityEngine;

// ============================================================================
// HERRAMIENTA DE DIAGNÓSTICO: localizar enemigos restantes
// Presiona F1 durante la partida para ver en la Consola de Unity la posición
// exacta y la distancia al jugador de cada bacteria/kamikaze/disparador vivo.
// Útil para encontrar al último enemigo cuando el contador no llega a cero
// pero no se ve nada en el mapa.
// ============================================================================
public class LocalizadorEnemigos : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        GameObject objeto = new GameObject("LocalizadorEnemigos");
        Object.DontDestroyOnLoad(objeto);
        objeto.AddComponent<LocalizadorEnemigos>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LocalizarTodos();
        }
    }

    void LocalizarTodos()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        Vector3 posJugador = jugador != null ? jugador.transform.position : Vector3.zero;

        Debug.Log("========== ENEMIGOS RESTANTES (F1) ==========");

        int total = 0;

        foreach (ControlBacteria b in FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None))
        {
            ReportarPosicion("Bacteria: " + b.name, b.transform.position, posJugador);
            total++;
        }

        foreach (BacteriaKamikaze k in FindObjectsByType<BacteriaKamikaze>(FindObjectsSortMode.None))
        {
            ReportarPosicion("Kamikaze: " + k.name, k.transform.position, posJugador);
            total++;
        }

        foreach (EnemigoDisparador d in FindObjectsByType<EnemigoDisparador>(FindObjectsSortMode.None))
        {
            ReportarPosicion("Disparador: " + d.name, d.transform.position, posJugador);
            total++;
        }

        Debug.Log("Total restante: " + total);
        Debug.Log("==============================================");
    }

    void ReportarPosicion(string etiqueta, Vector3 posicion, Vector3 posJugador)
    {
        float distancia = Vector3.Distance(posicion, posJugador);
        Debug.Log(etiqueta + " -> posición " + posicion + "  (a " + distancia.ToString("F1") + " metros de ti)");
    }
}
