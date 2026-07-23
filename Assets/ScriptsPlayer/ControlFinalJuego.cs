using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ControlFinalJuego : MonoBehaviour
{
    [Header("UI de Victoria (opcional)")]
    [Tooltip("Ya no es necesario: la pantalla final ahora se genera por código con estadísticas")]
    public GameObject panelVictoria;

    [Header("Condición de Victoria")]
    [Tooltip("Si está activo, también hay que eliminar a los enemigos disparadores para ganar")]
    public bool exigirEliminarDisparadores = true;

    private bool juegoTerminado = false;

    void Start()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }

        // ============================================================================
        // TRUCO DEFENSIVO: Si Unity crea EventSystems de más al iniciar, ¡los borramos!
        // ============================================================================
        CorregirEventSystemsDuplicados();
    }

    void Update()
    {
        if (juegoTerminado) return;

        // 1. BUSCAR BACTERIAS NORMALES (Buscando su script de control)
        ControlBacteria[] bacteriasNormales = FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None);

        // 2. BUSCAR BACTERIAS KAMIKAZES (Buscando su script)
        BacteriaKamikaze[] bacteriasKamikazes = FindObjectsByType<BacteriaKamikaze>(FindObjectsSortMode.None);

        // 3. BUSCAR ENEMIGOS DISPARADORES (antes no contaban y se podía ganar con ellos vivos)
        int disparadoresVivos = 0;
        if (exigirEliminarDisparadores)
        {
            disparadoresVivos = FindObjectsByType<EnemigoDisparador>(FindObjectsSortMode.None).Length;
        }

        // Si ya no queda absolutamente ninguno en la escena... ¡GANASTE!
        if (bacteriasNormales.Length == 0 && bacteriasKamikazes.Length == 0 && disparadoresVivos == 0)
        {
            TerminarJuego();
        }
    }

    void TerminarJuego()
    {
        juegoTerminado = true;
        Debug.Log("¡Victoria total! Mostrando pantalla final...");

        // Por si volvieron a nacer EventSystems fantasmas al final, los volvemos a limpiar
        CorregirEventSystemsDuplicados();

        MostrarPantallaVictoria();

        // Bloqueamos el juego y soltamos el mouse
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ============================================================================
    // PANTALLA DE VICTORIA CON ESTADÍSTICAS (generada por código)
    // Reemplaza el antiguo Application.Quit() automático que cerraba el juego
    // ============================================================================
    void MostrarPantallaVictoria()
    {
        GameObject objetoCanvas = new GameObject("CanvasVictoria");
        Canvas canvas = objetoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20; // Por encima de cualquier otra UI

        CanvasScaler escalador = objetoCanvas.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);

        objetoCanvas.AddComponent<GraphicRaycaster>();

        // Fondo oscuro que cubre toda la pantalla
        Image fondo = CrearImagen(objetoCanvas.transform, "Fondo", new Color(0.05f, 0.1f, 0.15f, 0.92f));
        EstirarATodaLaPantalla(fondo.rectTransform);

        // Título
        CrearTexto(objetoCanvas.transform, "Titulo", "¡BOCA LIMPIA!", 90, new Color(0.4f, 1f, 0.9f), new Vector2(0, 280));

        // Estadísticas de la partida
        string estadisticas =
            "Puntaje final: " + PuntajeJuego.puntos + "\n" +
            "Bacterias eliminadas: " + PuntajeJuego.bacteriasEliminadas + "\n" +
            "Tiempo total: " + PuntajeJuego.TiempoFormateado();

        TextMeshProUGUI textoStats = CrearTexto(objetoCanvas.transform, "Estadisticas", estadisticas, 44, Color.white, new Vector2(0, 60));
        textoStats.rectTransform.sizeDelta = new Vector2(900, 300);

        // Botones
        CrearBoton(objetoCanvas.transform, "BotonReiniciar", "Jugar de nuevo", new Vector2(0, -180), JugarDeNuevo);
        CrearBoton(objetoCanvas.transform, "BotonSalir", "Salir", new Vector2(0, -300), CerrarAplicacion);
    }

    public void JugarDeNuevo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Vuelve al primer nivel (el HUD reinicia el puntaje solo)
    }

    // --------- Ayudantes para construir la UI ---------

    Image CrearImagen(Transform padre, string nombre, Color color)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        Image imagen = objeto.AddComponent<Image>();
        imagen.color = color;
        return imagen;
    }

    void EstirarATodaLaPantalla(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, float tamano, Color color, Vector2 posicion)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);

        TextMeshProUGUI texto = objeto.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = TextAlignmentOptions.Center;

        RectTransform rect = texto.rectTransform;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(1200, 120);

        return texto;
    }

    void CrearBoton(Transform padre, string nombre, string etiqueta, Vector2 posicion, UnityEngine.Events.UnityAction accion)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);

        Image imagen = objeto.AddComponent<Image>();
        imagen.color = new Color(0.15f, 0.55f, 0.5f, 1f);

        Button boton = objeto.AddComponent<Button>();
        boton.onClick.AddListener(accion);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(420, 90);

        TextMeshProUGUI texto = CrearTexto(objeto.transform, "Etiqueta", etiqueta, 40, Color.white, Vector2.zero);
        texto.rectTransform.sizeDelta = rect.sizeDelta;
    }

    void CorregirEventSystemsDuplicados()
    {
        // Buscamos TODOS los EventSystems que existan en la escena ahora mismo
        UnityEngine.EventSystems.EventSystem[] todosLosEventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);

        // Si hay más de uno saboteando el juego...
        if (todosLosEventSystems.Length > 1)
        {
            Debug.LogWarning("¡Se detectaron " + todosLosEventSystems.Length + " EventSystems! Destruyendo clones para evitar errores de UI.");

            // Dejamos el primero vivo (índice 0) y destruimos todos los demás (del 1 en adelante)
            for (int i = 1; i < todosLosEventSystems.Length; i++)
            {
                Destroy(todosLosEventSystems[i].gameObject);
            }
        }
    }

    void CerrarAplicacion()
    {
        Debug.Log("Cerrando el juego de forma definitiva.");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
