using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
// MÚSICA DE FONDO POR NIVEL
// Se crea sola al arrancar el juego (no hay que agregarla a ninguna escena).
// Carga los clips desde Assets/Resources/Musica y cambia según el nivel.
// ============================================================================
public class MusicaFondo : MonoBehaviour
{
    private AudioSource fuente;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        GameObject objeto = new GameObject("MusicaFondo");
        Object.DontDestroyOnLoad(objeto);
        objeto.AddComponent<MusicaFondo>();
    }

    void Awake()
    {
        fuente = gameObject.AddComponent<AudioSource>();
        fuente.loop = true;
        fuente.volume = 0.2f;      // Bajita, para que no tape los efectos
        fuente.spatialBlend = 0f;  // Sonido 2D (no depende de la posición)

        SceneManager.sceneLoaded += AlCargarEscena;
        PonerMusicaDeEscena(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        PonerMusicaDeEscena(escena.name);
    }

    void PonerMusicaDeEscena(string nombreEscena)
    {
        string nombreClip;

        if (nombreEscena == "Nivel_1")
        {
            nombreClip = "Music_Suspenseful";   // Empieza la infección...
        }
        else if (nombreEscena == "Nivel_2")
        {
            nombreClip = "Music_Ethereal";      // Se pone serio
        }
        else if (nombreEscena == "Nivel_3")
        {
            nombreClip = "Music_Exciting";      // ¡Batalla final!
        }
        else
        {
            nombreClip = "Music_Suspenseful";
        }

        AudioClip clip = Resources.Load<AudioClip>("Musica/" + nombreClip);

        if (clip == null)
        {
            Debug.LogWarning("No se encontró la música '" + nombreClip + "' en Resources/Musica.");
            return;
        }

        // Solo cambiamos si es una canción distinta, para no reiniciarla entre reintentos
        if (fuente.clip != clip)
        {
            fuente.clip = clip;
            fuente.Play();
        }
    }
}
