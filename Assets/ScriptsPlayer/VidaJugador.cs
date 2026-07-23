using UnityEngine;
using TMPro;
using UnityEngine.UI; // ¡NUEVO! Necesario para manejar Sliders e Imágenes
using UnityEngine.SceneManagement; 
using System.Collections; // ¡NUEVO! Necesario para ejecutar las Corrutinas

public class VidaJugador : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 5;
    private int vidaActual;

    [Header("Interfaz de Texto")]
    public TextMeshProUGUI textoVida;

    [Header("Nueva Interfaz Visual")]
    [Tooltip("Arrastra aquí el Canvas Image rojo/marrón")]
    public GameObject imagenDanio; 
    [Tooltip("Opcional: Arrastra aquí tu UI Slider si decides usar barra de vida")]
    public Slider barraVida; 

    [Header("UI de Game Over")]
    public GameObject panelGameOver; 

    private bool estaMuerto = false;

    // ----- Escudo de Flúor (power-up) -----
    private float escudoHasta = 0f;
    private GameObject burbujaEscudo;

    public void ActivarEscudo(float segundos)
    {
        escudoHasta = Time.time + segundos;

        if (burbujaEscudo == null)
        {
            CrearBurbujaEscudo();
        }

        burbujaEscudo.SetActive(true);
    }

    public bool EscudoActivo()
    {
        return Time.time < escudoHasta;
    }

    void CrearBurbujaEscudo()
    {
        burbujaEscudo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        burbujaEscudo.name = "BurbujaEscudo";

        Collider colBurbuja = burbujaEscudo.GetComponent<Collider>();
        if (colBurbuja != null) Destroy(colBurbuja);

        burbujaEscudo.transform.SetParent(transform, false);
        burbujaEscudo.transform.localPosition = Vector3.zero;
        burbujaEscudo.transform.localScale = Vector3.one * 2.4f;

        // Material translúcido simple (Sprites/Default respeta el alfa)
        Renderer rend = burbujaEscudo.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.4f, 1f, 0.7f, 0.18f);
        rend.material = mat;
    }

    void Update()
    {
        // Apagar la burbuja cuando el escudo expira
        if (burbujaEscudo != null && burbujaEscudo.activeSelf && !EscudoActivo())
        {
            burbujaEscudo.SetActive(false);
        }
    }

    void Start()
    {
        vidaActual = vidaMaxima;
        
        // 1. Configuramos la barra de vida al iniciar (si le asignaste una en Unity)
        if (barraVida != null)
        {
            barraVida.maxValue = vidaMaxima;
            barraVida.value = vidaActual;
        }

        // 2. Nos aseguramos de que la pantalla de infección esté apagada al inicio
        if (imagenDanio != null)
        {
            imagenDanio.SetActive(false);
        }

        ActualizarInterfaz();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false); 
        }
        
        Time.timeScale = 1f; 

        // Oculta y bloquea el mouse al iniciar para poder jugar bien
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RecibirDano(int cantidadDano)
    {
        if (estaMuerto) return;

        // El Escudo de Flúor bloquea todo el daño mientras dura
        if (EscudoActivo())
        {
            TextoFlotante.Crear(transform.position + transform.forward, "¡Escudo!", new Color(0.4f, 1f, 0.7f));
            return;
        }

        vidaActual -= cantidadDano;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        // Sacudida de cámara: el golpe se SIENTE, no solo se ve
        EfectosCamara.Sacudir(0.25f, 0.35f);

        ActualizarInterfaz();

        // 3. Activamos el parpadeo de infección cada vez que se recibe daño
        if (imagenDanio != null)
        {
            StartCoroutine(EfectoInfeccion());
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void Curar(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual += cantidad;
        // Mathf.Clamp asegura que la vida no pase del máximo que configuraste (5)
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        ActualizarInterfaz();
        
        Debug.Log("¡Vida recuperada! Vida actual: " + vidaActual);
    }

    void ActualizarInterfaz()
    {
        if (textoVida != null)
        {
            textoVida.text = "Vida: " + vidaActual + " / " + vidaMaxima;

            // El color avisa el peligro de un vistazo: verde -> amarillo -> rojo
            float fraccion = (float)vidaActual / vidaMaxima;

            if (fraccion > 0.6f)
            {
                textoVida.color = new Color(0.55f, 1f, 0.55f);
            }
            else if (fraccion > 0.3f)
            {
                textoVida.color = new Color(1f, 0.9f, 0.35f);
            }
            else
            {
                textoVida.color = new Color(1f, 0.35f, 0.3f);
            }
        }

        // 4. Actualizamos el relleno de la barra visual de vida
        if (barraVida != null)
        {
            barraVida.value = vidaActual;
        }
    }

    // ============================================================================
    // CORRUTINA: EFECTO DE PANTALLA ROJA (INFECCIÓN)
    // ============================================================================
    IEnumerator EfectoInfeccion()
    {
        imagenDanio.SetActive(true); // Enciende la imagen
        yield return new WaitForSeconds(0.2f); // Espera una fracción de segundo
        imagenDanio.SetActive(false); // Apaga la imagen
    }

    void Morir()
    {
        estaMuerto = true;

        Debug.Log("El jugador ha perdido.");

        if (textoVida != null)
        {
            textoVida.text = "Vida: 0 - Perdiste";
        }

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true); 
        }

        Time.timeScale = 0f; 

        // ========================================================
        // ¡ESTO DEVUELVE EL MOUSE A LA PANTALLA PARA DAR CLICK!
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;                  
        // ========================================================
    }

    // Esta función la usará tu botón de REINICIAR
    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ============================================================================
    // SISTEMA DE MUERTE INSTANTÁNEA AL TOCAR EL CUERPO DEL KAMIKAZE
    // ============================================================================

    private void OnCollisionEnter(Collision collision)
    {
        ComprobarMuerteKamikaze(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        ComprobarMuerteKamikaze(other.gameObject);
    }

    void ComprobarMuerteKamikaze(GameObject enemigo)
    {
        // El Escudo de Flúor también salva del toque mortal del kamikaze
        if (EscudoActivo()) return;

        // Si lo que nos toca tiene el script Kamikaze, o su padre lo tiene, o se llama Monster o Chest
        if (enemigo.GetComponent<BacteriaKamikaze>() != null || 
            enemigo.GetComponentInParent<BacteriaKamikaze>() != null || 
            enemigo.name.Contains("Monster") || 
            enemigo.name.Contains("Chest"))
        {
            Debug.Log("¡EL MONSTRUO TE TOCÓ EL CUERPO! Activando Game Over...");
            
            // CORREGIDO: Ya no se salta la pantalla. Ahora llama a tu función Morir()
            Morir();
        }
    }
}