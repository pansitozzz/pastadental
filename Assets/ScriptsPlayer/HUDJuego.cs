using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// ============================================================================
// HUD DE PUNTAJE Y BACTERIAS RESTANTES
// Se crea solo al arrancar el juego (no hay que agregarlo a ninguna escena).
// Muestra en la esquina superior derecha:
//   - Puntos acumulados
//   - Bacterias que faltan por eliminar en el nivel
// ============================================================================
public class HUDJuego : MonoBehaviour
{
    private TextMeshProUGUI textoPuntos;
    private TextMeshProUGUI textoBacterias;
    private TextMeshProUGUI textoCombo;
    private TextMeshProUGUI textoNivel;
    private UnityEngine.UI.Image imagenFade;
    private Canvas canvas;

    private float proximaActualizacion = 0f;
    private float tiempoLetreroNivel = 0f;    // Momento en que apareció el letrero
    private const float DURACION_LETRERO = 3f;

    private float tiempoInicioFade = 0f;      // Fundido negro de entrada al nivel
    private const float DURACION_FADE = 0.9f;

    // Unity llama a esto automáticamente al iniciar el juego, en cualquier escena
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        GameObject objeto = new GameObject("HUDJuego");
        Object.DontDestroyOnLoad(objeto);
        objeto.AddComponent<HUDJuego>();
    }

    void Awake()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
        PuntajeJuego.ReiniciarPartida();
        ConstruirInterfaz();
        MostrarLetreroNivel(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Si volvemos al primer nivel, empieza una partida nueva desde cero
        if (escena.buildIndex == 0)
        {
            PuntajeJuego.ReiniciarPartida();
        }

        MostrarLetreroNivel(escena.name);
        tiempoInicioFade = Time.unscaledTime; // reinicia el fundido de entrada
    }

    // ============================================================================
    // LETRERO "NIVEL X" AL EMPEZAR CADA NIVEL (se desvanece solo)
    // ============================================================================
    void MostrarLetreroNivel(string nombreEscena)
    {
        if (textoNivel == null) return;

        // Convierte "Nivel_1" en "NIVEL 1"
        string titulo = nombreEscena.Replace("_", " ").ToUpper();

        textoNivel.text = titulo;
        tiempoLetreroNivel = Time.unscaledTime;
        textoNivel.alpha = 1f;
        textoNivel.gameObject.SetActive(true);
    }

    void ConstruirInterfaz()
    {
        // Canvas propio para no depender del Canvas de cada escena
        GameObject objetoCanvas = new GameObject("CanvasHUD");
        objetoCanvas.transform.SetParent(transform, false);

        canvas = objetoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        UnityEngine.UI.CanvasScaler escalador = objetoCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        escalador.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);

        textoPuntos = CrearTexto(objetoCanvas.transform, "TextoPuntos", new Vector2(-30, -30), 36, Color.white);
        textoBacterias = CrearTexto(objetoCanvas.transform, "TextoBacterias", new Vector2(-30, -80), 30, new Color(0.6f, 1f, 0.6f));
        textoCombo = CrearTexto(objetoCanvas.transform, "TextoCombo", new Vector2(-30, -125), 32, new Color(1f, 0.6f, 0.2f));

        // Letrero grande del nivel, centrado en pantalla
        GameObject objetoNivel = new GameObject("TextoNivel");
        objetoNivel.transform.SetParent(objetoCanvas.transform, false);
        textoNivel = objetoNivel.AddComponent<TextMeshProUGUI>();
        textoNivel.fontSize = 110;
        textoNivel.color = new Color(0.4f, 1f, 0.9f);
        textoNivel.alignment = TextAlignmentOptions.Center;
        textoNivel.raycastTarget = false;
        RectTransform rectNivel = textoNivel.rectTransform;
        rectNivel.anchoredPosition = new Vector2(0, 150);
        rectNivel.sizeDelta = new Vector2(1400, 160);
        textoNivel.gameObject.SetActive(false);

        // Fundido negro de entrada: la escena "abre los ojos" al cargar
        GameObject objetoFade = new GameObject("FadeEntrada");
        objetoFade.transform.SetParent(objetoCanvas.transform, false);
        imagenFade = objetoFade.AddComponent<UnityEngine.UI.Image>();
        imagenFade.color = Color.black;
        imagenFade.raycastTarget = false;
        RectTransform rectFade = imagenFade.rectTransform;
        rectFade.anchorMin = Vector2.zero;
        rectFade.anchorMax = Vector2.one;
        rectFade.offsetMin = Vector2.zero;
        rectFade.offsetMax = Vector2.zero;
        tiempoInicioFade = Time.unscaledTime;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string nombre, Vector2 posicion, float tamano, Color color)
    {
        GameObject objetoTexto = new GameObject(nombre);
        objetoTexto.transform.SetParent(padre, false);

        TextMeshProUGUI texto = objetoTexto.AddComponent<TextMeshProUGUI>();
        texto.fontSize = tamano;
        texto.color = color;
        texto.alignment = TextAlignmentOptions.TopRight;
        texto.raycastTarget = false;

        // Anclado a la esquina superior derecha de la pantalla
        RectTransform rect = texto.rectTransform;
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(500, 50);

        return texto;
    }

    void Update()
    {
        // Ocultamos el HUD cuando el juego está pausado (Game Over / Victoria)
        if (canvas != null)
        {
            canvas.enabled = Time.timeScale > 0f;
        }

        // Fundido negro de entrada al nivel
        if (imagenFade != null)
        {
            float f = (Time.unscaledTime - tiempoInicioFade) / DURACION_FADE;
            imagenFade.color = new Color(0f, 0f, 0f, Mathf.Clamp01(1f - f));
            imagenFade.enabled = f < 1f;
        }

        // Texto de combo (solo visible con combo x2 o más)
        if (textoCombo != null)
        {
            int combo = PuntajeJuego.ComboVigente();

            if (combo >= 2)
            {
                textoCombo.text = "Combo x" + Mathf.Min(combo, 5);
                // Pulso sutil para que llame la atención
                float pulso = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.08f;
                textoCombo.transform.localScale = Vector3.one * pulso;
            }
            else
            {
                textoCombo.text = "";
            }
        }

        // Desvanecer el letrero del nivel después de unos segundos
        if (textoNivel != null && textoNivel.gameObject.activeSelf)
        {
            float transcurrido = Time.unscaledTime - tiempoLetreroNivel;

            if (transcurrido > DURACION_LETRERO)
            {
                textoNivel.gameObject.SetActive(false);
            }
            else if (transcurrido > DURACION_LETRERO - 1f)
            {
                // El último segundo se va desvaneciendo
                textoNivel.alpha = DURACION_LETRERO - transcurrido;
            }
        }

        // Actualizamos solo 4 veces por segundo: contar enemigos cada frame es caro
        if (Time.unscaledTime < proximaActualizacion) return;
        proximaActualizacion = Time.unscaledTime + 0.25f;

        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + PuntajeJuego.puntos;
        }

        if (textoBacterias != null)
        {
            int restantes = ContarEnemigosVivos();
            textoBacterias.text = "Bacterias: " + restantes;
        }
    }

    int ContarEnemigosVivos()
    {
        int normales = Object.FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None).Length;
        int kamikazes = Object.FindObjectsByType<BacteriaKamikaze>(FindObjectsSortMode.None).Length;
        int disparadores = Object.FindObjectsByType<EnemigoDisparador>(FindObjectsSortMode.None).Length;
        return normales + kamikazes + disparadores;
    }
}
