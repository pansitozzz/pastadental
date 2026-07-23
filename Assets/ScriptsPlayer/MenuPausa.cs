using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// ============================================================================
// MENÚ DE PAUSA (tecla ESC)
// Pausa el juego y muestra Reanudar / Reiniciar nivel / Salir.
// Se crea solo al arrancar el juego; no hay que agregarlo a ninguna escena.
// No interfiere con las pantallas de Game Over ni de Victoria (si el juego
// ya está pausado por otra razón, ESC no hace nada).
// ============================================================================
public class MenuPausa : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (FindFirstObjectByType<MenuPausa>() != null) return;

        GameObject objeto = new GameObject("MenuPausa");
        Object.DontDestroyOnLoad(objeto);
        objeto.AddComponent<MenuPausa>();
    }

    private bool pausado = false;
    private GameObject panel;

    void Awake()
    {
        ConstruirPanel();
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Al cambiar de escena, el juego arranca sin pausa
        pausado = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    void Update()
    {
        // Mientras está pausado, garantizamos que el cursor siga libre
        // (por si otro script intenta capturarlo)
        if (pausado)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ESC o P pausan. Nota: en el EDITOR de Unity, el primer ESC lo
        // consume el propio editor para liberar el mouse y el juego no lo
        // recibe; la tecla P siempre llega. En el build final ESC funciona.
        if (!Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.P)) return;

        // Si el juego está pausado por otra pantalla (Game Over / Victoria),
        // la pausa no debe interferir
        if (!pausado && Time.timeScale == 0f) return;

        if (pausado)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    void Pausar()
    {
        // Si el panel se perdió por cualquier motivo, lo reconstruimos
        if (panel == null)
        {
            ConstruirPanel();
        }

        pausado = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        panel.SetActive(true);
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        panel.SetActive(false);
    }

    void ReiniciarNivel()
    {
        pausado = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void CerrarAplicacion()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ---------- Construcción de la UI ----------

    void ConstruirPanel()
    {
        panel = new GameObject("PanelPausa");
        panel.transform.SetParent(transform, false);

        Canvas canvas = panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler escalador = panel.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);

        panel.AddComponent<GraphicRaycaster>();

        // Fondo semitransparente
        Image fondo = CrearImagen(panel.transform, "Fondo", new Color(0.02f, 0.05f, 0.08f, 0.8f));
        fondo.rectTransform.anchorMin = Vector2.zero;
        fondo.rectTransform.anchorMax = Vector2.one;
        fondo.rectTransform.offsetMin = Vector2.zero;
        fondo.rectTransform.offsetMax = Vector2.zero;

        CrearTexto(panel.transform, "Titulo", "PAUSA", 80, new Color(0.4f, 1f, 0.9f), new Vector2(0, 220));

        CrearBoton(panel.transform, "BotonReanudar", "Reanudar", new Vector2(0, 60), Reanudar);
        CrearBoton(panel.transform, "BotonReiniciar", "Reiniciar nivel", new Vector2(0, -60), ReiniciarNivel);
        CrearBoton(panel.transform, "BotonSalir", "Salir del juego", new Vector2(0, -180), CerrarAplicacion);

        panel.SetActive(false);
    }

    Image CrearImagen(Transform padre, string nombre, Color color)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        Image imagen = objeto.AddComponent<Image>();
        imagen.color = color;
        return imagen;
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
        rect.sizeDelta = new Vector2(1000, 110);

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

        TextMeshProUGUI texto = CrearTexto(objeto.transform, "Etiqueta", etiqueta, 38, Color.white, Vector2.zero);
        texto.rectTransform.sizeDelta = rect.sizeDelta;
    }
}
