using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
// GAME FEEL DE CÁMARA Y SONIDO DEL JUGADOR
// Se agrega solo al Jugador al cargar cada escena. Aporta:
//   - Sacudida de cámara al recibir daño (screen shake)
//   - Golpe de FOV al hacer dash (sensación de velocidad)
//   - Balanceo suave de cámara al caminar (head bobbing)
//   - Sonidos de pasos y de aterrizaje
// ============================================================================
public class EfectosCamara : MonoBehaviour
{
    private static EfectosCamara instancia;
    private static bool inicializado = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (inicializado) return;
        inicializado = true;

        SceneManager.sceneLoaded += AlCargarEscena;
        Adjuntar();
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Adjuntar();
    }

    static void Adjuntar()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");

        if (jugador == null)
        {
            jugador = GameObject.Find("Jugador");
        }

        if (jugador != null && jugador.GetComponent<EfectosCamara>() == null)
        {
            jugador.AddComponent<EfectosCamara>();
        }
    }

    // ---------- Llamadas externas ----------

    // VidaJugador llama a esto al recibir daño
    public static void Sacudir(float intensidad, float duracion)
    {
        if (instancia == null) return;
        instancia.intensidadShake = intensidad;
        instancia.tiempoShakeRestante = duracion;
        instancia.duracionShake = duracion;
    }

    // MovimientoJugador llama a esto al hacer dash
    public static void AvisarDash(float duracion)
    {
        if (instancia == null) return;
        instancia.dashHasta = Time.time + duracion;
    }

    // ---------- Estado interno ----------

    private Camera camara;
    private CharacterController controller;
    private AudioSource fuentePasos;

    private Vector3 posicionBaseCamara;
    private float fovBase;

    private float intensidadShake;
    private float tiempoShakeRestante;
    private float duracionShake = 1f;

    private float dashHasta;

    private float faseBob;
    private float proximoPaso;
    private bool estabaEnSuelo = true;
    private float velocidadYPrevia;

    private AudioClip[] clipsPasos;
    private AudioClip clipAterrizaje;

    void Awake()
    {
        instancia = this;

        camara = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();

        if (camara != null)
        {
            posicionBaseCamara = camara.transform.localPosition;
            fovBase = camara.fieldOfView;
        }

        fuentePasos = gameObject.AddComponent<AudioSource>();
        fuentePasos.spatialBlend = 0f;
        fuentePasos.volume = 0.22f;

        clipsPasos = new AudioClip[]
        {
            Resources.Load<AudioClip>("Pasos/Player_Footstep_01"),
            Resources.Load<AudioClip>("Pasos/Player_Footstep_02"),
            Resources.Load<AudioClip>("Pasos/Player_Footstep_03")
        };

        clipAterrizaje = Resources.Load<AudioClip>("Pasos/Player_Land");
    }

    void Update()
    {
        if (camara == null || controller == null) return;

        Vector3 velocidad = controller.velocity;
        Vector3 velocidadHorizontal = new Vector3(velocidad.x, 0f, velocidad.z);
        float rapidez = velocidadHorizontal.magnitude;
        bool enSuelo = controller.isGrounded;

        // ----- Balanceo al caminar (head bobbing) -----
        Vector3 offsetBob = Vector3.zero;

        if (enSuelo && rapidez > 1f)
        {
            faseBob += rapidez * Time.deltaTime * 1.6f;
            offsetBob.y = Mathf.Sin(faseBob) * 0.045f;
            offsetBob.x = Mathf.Cos(faseBob * 0.5f) * 0.025f;
        }
        else
        {
            faseBob = 0f;
        }

        // ----- Sacudida (screen shake) -----
        Vector3 offsetShake = Vector3.zero;

        if (tiempoShakeRestante > 0f)
        {
            tiempoShakeRestante -= Time.deltaTime;
            float fuerza = intensidadShake * (tiempoShakeRestante / duracionShake);
            offsetShake = Random.insideUnitSphere * fuerza;
            offsetShake.z = 0f;
        }

        camara.transform.localPosition = posicionBaseCamara + offsetBob + offsetShake;

        // ----- Golpe de FOV en el dash -----
        float fovObjetivo = fovBase + (Time.time < dashHasta ? 10f : 0f);
        camara.fieldOfView = Mathf.Lerp(camara.fieldOfView, fovObjetivo, 12f * Time.deltaTime);

        // ----- Sonido de pasos -----
        if (enSuelo && rapidez > 2f && Time.time >= proximoPaso)
        {
            ReproducirPaso();
            proximoPaso = Time.time + Mathf.Clamp(3.4f / rapidez, 0.28f, 0.6f);
        }

        // ----- Sonido de aterrizaje -----
        if (!estabaEnSuelo && enSuelo && velocidadYPrevia < -4f && clipAterrizaje != null)
        {
            fuentePasos.pitch = 1f;
            fuentePasos.PlayOneShot(clipAterrizaje, 0.8f);
        }

        estabaEnSuelo = enSuelo;
        velocidadYPrevia = velocidad.y;
    }

    void ReproducirPaso()
    {
        if (clipsPasos == null || clipsPasos.Length == 0) return;

        AudioClip clip = clipsPasos[Random.Range(0, clipsPasos.Length)];
        if (clip == null) return;

        fuentePasos.pitch = Random.Range(0.92f, 1.08f);
        fuentePasos.PlayOneShot(clip);
    }
}
