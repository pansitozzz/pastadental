using UnityEngine;

// ============================================================================
// RESCATE AUTOMÁTICO DE ENEMIGOS ATASCADOS
// Revisa si el centro real del collider de un enemigo está dentro de
// geometría sólida ajena. Ignora sus propios colliders, otros enemigos,
// triggers y al jugador.
// ============================================================================
public class DesatascadorEnemigos : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        // Evita crear más de un desatascador
        if (FindFirstObjectByType<DesatascadorEnemigos>() != null)
        {
            return;
        }

        GameObject objeto = new GameObject("DesatascadorEnemigos");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<DesatascadorEnemigos>();
    }

    private const float RADIO_REVISION = 0.15f;
    private const float PASO_EMPUJE = 0.5f;
    private const int MAX_INTENTOS = 20;

    private float proximaRevision;

    void Update()
    {
        if (Time.time < proximaRevision)
        {
            return;
        }

        proximaRevision = Time.time + 2f;

        foreach (BacteriaKamikaze enemigo in
                 FindObjectsByType<BacteriaKamikaze>(
                     FindObjectsSortMode.None))
        {
            RevisarYLiberar(enemigo.transform);
        }

        foreach (EnemigoDisparador enemigo in
                 FindObjectsByType<EnemigoDisparador>(
                     FindObjectsSortMode.None))
        {
            RevisarYLiberar(enemigo.transform);
        }
    }

    void RevisarYLiberar(Transform enemigo)
    {
        if (enemigo == null)
        {
            return;
        }

        Collider colliderPrincipal =
            enemigo.GetComponentInChildren<Collider>();

        // Calculamos la diferencia entre el pivote del enemigo
        // y el centro verdadero de su collider.
        Vector3 desplazamientoCentro = Vector3.zero;

        if (colliderPrincipal != null)
        {
            desplazamientoCentro =
                colliderPrincipal.bounds.center - enemigo.position;
        }

        if (EstaLibre(
                enemigo,
                enemigo.position,
                desplazamientoCentro))
        {
            return;
        }

        Vector3 nuevaPosicion = enemigo.position;
        int intentos = 0;

        while (
            intentos < MAX_INTENTOS &&
            !EstaLibre(
                enemigo,
                nuevaPosicion,
                desplazamientoCentro
            )
        )
        {
            nuevaPosicion += Vector3.up * PASO_EMPUJE;
            intentos++;
        }

        if (intentos >= MAX_INTENTOS)
        {
            Debug.LogError(
                "[Desatascador] No se pudo liberar a '" +
                enemigo.name +
                "' después de " +
                MAX_INTENTOS +
                " intentos."
            );

            return;
        }

        Rigidbody rb = enemigo.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.position = nuevaPosicion;
        }
        else
        {
            enemigo.position = nuevaPosicion;
        }

        // El kamikaze tiene su propia lógica de raycast hacia el suelo
        // (más completa que la de este script) y sabe autodestruirse si
        // de verdad no hay suelo real debajo. Reutilizamos esa lógica en
        // vez de dejarlo flotando en el primer punto "libre" que se
        // encontró al empujarlo hacia arriba (eso fue lo que causó que un
        // enemigo terminara flotando sobre la boca, invisible, en el Nivel 3).
        BacteriaKamikaze kamikaze = enemigo.GetComponent<BacteriaKamikaze>();

        if (kamikaze != null)
        {
            kamikaze.ActualizarAlturaFija(nuevaPosicion.y);
            kamikaze.ReasentarEnSuelo();

            Debug.LogWarning(
                "[Desatascador] '" + enemigo.name +
                "' estaba dentro de geometría. Se liberó y se le pidió " +
                "reasentarse sobre suelo real (o autodestruirse si no lo hay)."
            );

            return;
        }

        // Para el resto de enemigos (disparador): verificamos que exista
        // suelo real debajo antes de dejarlo ahí. Si no lo hay, no tiene
        // sentido dejarlo flotando para siempre: se elimina.
        if (!HaySueloRealDebajo(enemigo, nuevaPosicion, out float alturaSuelo))
        {
            Debug.LogWarning(
                "[Desatascador] '" + enemigo.name +
                "' quedó sin suelo real tras liberarlo de un muro. " +
                "Se elimina para no dejarlo flotando para siempre."
            );

            Destroy(enemigo.gameObject);
            return;
        }

        nuevaPosicion.y = alturaSuelo;

        if (rb != null)
        {
            rb.position = nuevaPosicion;
        }
        else
        {
            enemigo.position = nuevaPosicion;
        }

        Debug.LogWarning(
            "[Desatascador] '" +
            enemigo.name +
            "' estaba realmente dentro de geometría. " +
            "Se liberó y se asentó sobre suelo real."
        );
    }

    // Busca el primer suelo real (no pared) debajo de una posición.
    // Devuelve la altura Y donde debería apoyarse el pivote del enemigo,
    // asumiendo que su pivote está a nivel de su base (como el disparador).
    bool HaySueloRealDebajo(Transform enemigo, Vector3 posicion, out float alturaSuelo)
    {
        RaycastHit[] golpes = Physics.RaycastAll(
            posicion + Vector3.up * 0.3f,
            Vector3.down,
            100f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        bool encontrado = false;
        float distanciaMasCorta = float.MaxValue;
        float mejorY = posicion.y;

        foreach (RaycastHit golpe in golpes)
        {
            Collider c = golpe.collider;

            if (c == null) continue;
            if (c.transform == enemigo || c.transform.IsChildOf(enemigo)) continue;
            if (c.CompareTag("Player") || c.transform.root.CompareTag("Player")) continue;
            if (golpe.normal.y < 0.5f) continue; // ignorar paredes verticales

            if (golpe.distance < distanciaMasCorta)
            {
                distanciaMasCorta = golpe.distance;
                mejorY = golpe.point.y;
                encontrado = true;
            }
        }

        alturaSuelo = mejorY;
        return encontrado;
    }

    bool EstaLibre(
        Transform enemigo,
        Vector3 posicionRaiz,
        Vector3 desplazamientoCentro)
    {
        Vector3 centroRevision =
            posicionRaiz + desplazamientoCentro;

        Collider[] encontrados = Physics.OverlapSphere(
            centroRevision,
            RADIO_REVISION,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        Rigidbody rbEnemigo =
            enemigo.GetComponentInParent<Rigidbody>();

        foreach (Collider c in encontrados)
        {
            if (c == null)
            {
                continue;
            }

            // Ignorar colliders pertenecientes al propio enemigo
            if (c.transform == enemigo ||
                c.transform.IsChildOf(enemigo))
            {
                continue;
            }

            // Ignorar colliders unidos al mismo Rigidbody
            if (rbEnemigo != null &&
                c.attachedRigidbody == rbEnemigo)
            {
                continue;
            }

            // Ignorar al jugador
            if (c.CompareTag("Player") ||
                c.transform.root.CompareTag("Player"))
            {
                continue;
            }

            // Ignorar otros enemigos
            if (c.GetComponentInParent<EnemigoDisparador>() != null)
            {
                continue;
            }

            if (c.GetComponentInParent<BacteriaKamikaze>() != null)
            {
                continue;
            }

            /*
             * ClosestPoint devuelve el mismo punto cuando este se
             * encuentra dentro del collider o exactamente en su superficie.
             */
            Vector3 puntoMasCercano =
                c.ClosestPoint(centroRevision);

            float distanciaCuadrada =
                (puntoMasCercano - centroRevision).sqrMagnitude;

            bool centroDentroDelCollider =
                distanciaCuadrada < 0.000001f;

            if (centroDentroDelCollider)
            {
                return false;
            }
        }

        return true;
    }

    void OnDrawGizmos()
    {
        // Este objeto se crea automáticamente y normalmente
        // no necesita dibujar nada.
    }
}