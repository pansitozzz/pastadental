using UnityEngine;
using System.Collections.Generic;

// ============================================================================
// BALIZAS SOBRE LOS ÚLTIMOS ENEMIGOS
// Cuando quedan 3 enemigos o menos en el nivel, aparece una columna de luz
// amarilla vertical sobre cada uno. Así nunca más habrá que buscar a la
// "última bacteria" por todo el mapa.
// ============================================================================
public class BalizaEnemigos : MonoBehaviour
{
    private const int UMBRAL_ENEMIGOS = 3;
    private const float INTERVALO_REVISION = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (FindFirstObjectByType<BalizaEnemigos>() != null) return;

        GameObject objeto = new GameObject("BalizaEnemigos");
        Object.DontDestroyOnLoad(objeto);
        objeto.AddComponent<BalizaEnemigos>();
    }

    // Cada enemigo marcado y su columna de luz
    private readonly Dictionary<Transform, GameObject> balizas = new Dictionary<Transform, GameObject>();
    private readonly List<Transform> paraQuitar = new List<Transform>();

    private Material materialBaliza;
    private float proximaRevision = 0f;

    void Update()
    {
        // Seguir a los enemigos y limpiar balizas de enemigos muertos
        paraQuitar.Clear();

        foreach (KeyValuePair<Transform, GameObject> par in balizas)
        {
            if (par.Key == null)
            {
                if (par.Value != null) Destroy(par.Value);
                paraQuitar.Add(par.Key);
            }
            else if (par.Value != null)
            {
                par.Value.transform.position = par.Key.position + Vector3.up * 8f;
            }
        }

        foreach (Transform t in paraQuitar)
        {
            balizas.Remove(t);
        }

        // Revisar el estado del nivel cada medio segundo
        if (Time.time < proximaRevision) return;
        proximaRevision = Time.time + INTERVALO_REVISION;

        List<Transform> enemigos = EnemigosVivos();

        if (enemigos.Count == 0 || enemigos.Count > UMBRAL_ENEMIGOS)
        {
            // Demasiados enemigos (o ninguno): sin balizas
            QuitarTodas();
            return;
        }

        // Crear baliza para cada enemigo que no tenga una
        foreach (Transform enemigo in enemigos)
        {
            if (!balizas.ContainsKey(enemigo))
            {
                balizas[enemigo] = CrearColumnaDeLuz();
            }
        }
    }

    List<Transform> EnemigosVivos()
    {
        List<Transform> lista = new List<Transform>();

        foreach (ControlBacteria b in FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None))
        {
            lista.Add(b.transform);
        }

        foreach (BacteriaKamikaze k in FindObjectsByType<BacteriaKamikaze>(FindObjectsSortMode.None))
        {
            lista.Add(k.transform);
        }

        foreach (EnemigoDisparador d in FindObjectsByType<EnemigoDisparador>(FindObjectsSortMode.None))
        {
            lista.Add(d.transform);
        }

        return lista;
    }

    GameObject CrearColumnaDeLuz()
    {
        GameObject columna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        columna.name = "BalizaEnemigo";

        Collider col = columna.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Cilindro delgado y muy alto (el cilindro base mide 2 de alto)
        columna.transform.localScale = new Vector3(0.18f, 8f, 0.18f);

        if (materialBaliza == null)
        {
            materialBaliza = new Material(Shader.Find("Sprites/Default"));
            materialBaliza.color = new Color(1f, 0.9f, 0.3f, 0.45f);
        }

        Renderer rend = columna.GetComponent<Renderer>();
        rend.material = materialBaliza;

        return columna;
    }

    void QuitarTodas()
    {
        foreach (KeyValuePair<Transform, GameObject> par in balizas)
        {
            if (par.Value != null) Destroy(par.Value);
        }

        balizas.Clear();
    }
}
