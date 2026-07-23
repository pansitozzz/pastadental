using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================================
// OJOS PROCEDURALES PARA LAS BACTERIAS
// Al cargar cada escena, agrega un par de ojos (esfera blanca + pupila negra)
// a cada bacteria. Los ojos siguen al jugador con la mirada: convierte
// esferas verdes en personajes con carisma, sin tocar ningún prefab.
// ============================================================================
public class DecoradorBacterias : MonoBehaviour
{
    private static bool inicializado = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (inicializado) return;
        inicializado = true;

        SceneManager.sceneLoaded += AlCargarEscena;
        DecorarTodas();
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        DecorarTodas();
    }

    static void DecorarTodas()
    {
        ControlBacteria[] bacterias = Object.FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None);

        foreach (ControlBacteria bacteria in bacterias)
        {
            if (bacteria.transform.Find("Ojos") == null)
            {
                CrearOjos(bacteria.transform);
            }
        }
    }

    static void CrearOjos(Transform bacteria)
    {
        // Contenedor que gira para "mirar" al jugador
        GameObject contenedor = new GameObject("Ojos");
        contenedor.transform.SetParent(bacteria, false);
        contenedor.transform.localPosition = Vector3.zero;
        contenedor.AddComponent<MiradaBacteria>();

        CrearOjo(contenedor.transform, new Vector3(-0.18f, 0.14f, 0.40f));
        CrearOjo(contenedor.transform, new Vector3(0.18f, 0.14f, 0.40f));
    }

    static void CrearOjo(Transform padre, Vector3 posicionLocal)
    {
        // Globo ocular blanco
        GameObject ojo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ojo.name = "Ojo";
        Object.Destroy(ojo.GetComponent<SphereCollider>()); // sin colisión

        ojo.transform.SetParent(padre, false);
        ojo.transform.localPosition = posicionLocal;
        ojo.transform.localScale = Vector3.one * 0.24f;

        Renderer rendOjo = ojo.GetComponent<Renderer>();
        rendOjo.material.color = Color.white;

        // Pupila negra, mirando hacia adelante
        GameObject pupila = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupila.name = "Pupila";
        Object.Destroy(pupila.GetComponent<SphereCollider>());

        pupila.transform.SetParent(ojo.transform, false);
        pupila.transform.localPosition = new Vector3(0f, 0f, 0.38f);
        pupila.transform.localScale = Vector3.one * 0.45f;

        Renderer rendPupila = pupila.GetComponent<Renderer>();
        rendPupila.material.color = new Color(0.05f, 0.05f, 0.08f);
    }
}

// Componente que hace que los ojos sigan al jugador con la mirada
public class MiradaBacteria : MonoBehaviour
{
    private const float VELOCIDAD_GIRO = 4f;

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 direccion = cam.transform.position - transform.position;

        if (direccion.sqrMagnitude < 0.01f) return;

        Quaternion objetivo = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, VELOCIDAD_GIRO * Time.deltaTime);
    }
}
