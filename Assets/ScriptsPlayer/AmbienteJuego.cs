using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
// ATMÓSFERA AMBIENTAL
// Crea un sistema de partículas con motas suaves flotando en el aire
// (como gérmenes/polvo dentro de la boca) que sigue al jugador.
// Todo generado por código: no necesita prefabs ni assets.
// ============================================================================
public class AmbienteJuego : MonoBehaviour
{
    private static bool inicializado = false;

    private Transform jugador;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (inicializado) return;
        inicializado = true;

        GameObject objeto = new GameObject("MotasAmbiente");
        Object.DontDestroyOnLoad(objeto);
        AmbienteJuego comp = objeto.AddComponent<AmbienteJuego>();
        comp.ConfigurarParticulas();
    }

    void ConfigurarParticulas()
    {
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = 0.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
        main.startColor = new Color(1f, 0.9f, 0.9f, 0.35f);
        main.maxParticles = 300;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emision = ps.emission;
        emision.rateOverTime = 30f;

        ParticleSystem.ShapeModule forma = ps.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = new Vector3(30f, 8f, 30f);

        // Movimiento browniano suave: las motas "nadan" en el aire
        ParticleSystem.NoiseModule ruido = ps.noise;
        ruido.enabled = true;
        ruido.strength = 0.2f;
        ruido.frequency = 0.25f;

        // Aparecen y desaparecen suavemente
        ParticleSystem.ColorOverLifetimeModule colorVida = ps.colorOverLifetime;
        colorVida.enabled = true;

        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorVida.color = new ParticleSystem.MinMaxGradient(gradiente);

        // Material con puntito circular suave (generado por código)
        ParticleSystemRenderer rend = GetComponent<ParticleSystemRenderer>();
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = CrearTexturaCirculo(32);
        rend.material = material;
    }

    // Círculo blanco difuso para usar como sprite de partícula
    static Texture2D CrearTexturaCirculo(int tam)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        float centro = tam / 2f;

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float dx = (x - centro) / centro;
                float dy = (y - centro) / centro;
                float distancia = Mathf.Sqrt(dx * dx + dy * dy);
                float alfa = Mathf.Clamp01(1f - distancia);
                alfa = alfa * alfa; // borde más suave
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }

        tex.Apply();
        return tex;
    }

    void LateUpdate()
    {
        // El emisor sigue al jugador para que siempre haya motas a su alrededor
        if (jugador == null)
        {
            GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");

            if (objetoJugador == null)
            {
                objetoJugador = GameObject.Find("Jugador");
            }

            if (objetoJugador != null)
            {
                jugador = objetoJugador.transform;
            }
        }

        if (jugador != null)
        {
            transform.position = jugador.position;
        }
    }
}
