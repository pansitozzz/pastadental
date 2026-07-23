using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BacteriaKamikaze : MonoBehaviour
{
    [Header("Configuración de persecución")]
    public float velocidadCarga = 8f;
    public float distanciaDeteccion = 15f;

    [Header("Ajuste automático al suelo")]
    [Tooltip("Altura adicional para evitar que el modelo quede visualmente enterrado.")]
    public float elevacionExtra = 0.10f;

    [Tooltip("Distancia máxima para buscar suelo debajo del enemigo.")]
    public float distanciaBusquedaSuelo = 100f;

    [Tooltip("Capas que pueden considerarse suelo.")]
    public LayerMask capasSuelo = ~0;

    private Transform jugador;
    private Rigidbody rb;
    private Collider colliderPrincipal;

    private float alturaFija;
    private bool colocadoCorrectamente;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliderPrincipal = GetComponent<Collider>();

        /*
         * Este enemigo es controlado por código.
         * No necesitamos que la gravedad modifique su altura.
         */
        rb.useGravity = false;

        /*
         * Puede moverse en X y Z.
         * Su posición vertical queda bloqueada.
         * Puede girar únicamente alrededor del eje Y.
         */
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        BuscarJugador();
        AjustarAlSuelo();
        AsegurarVisibilidad();
    }

    // Un kamikaze invisible es una trampa injusta: si no tiene ningún
    // renderer visible, reactivamos los suyos.
    void AsegurarVisibilidad()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                return;
            }
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        if (renderers.Length > 0)
        {
            Debug.LogWarning("[BacteriaKamikaze] '" + name + "' era invisible. Se reactivó su renderer.");
        }
    }

    void BuscarJugador()
    {
        /*
         * Primero buscamos mediante el tag Player.
         */
        GameObject objetoJugador =
            GameObject.FindGameObjectWithTag("Player");

        /*
         * Si no lo encuentra mediante el tag,
         * intentamos buscarlo mediante el nombre.
         */
        if (objetoJugador == null)
        {
            objetoJugador = GameObject.Find("Jugador");
        }

        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
        }
        else
        {
            Debug.LogWarning(
                "[BacteriaKamikaze] No se encontró al jugador."
            );
        }
    }

    void AjustarAlSuelo()
    {
        if (colliderPrincipal == null)
        {
            Debug.LogWarning(
                "[BacteriaKamikaze] " + name +
                " no tiene Collider."
            );

            alturaFija = transform.position.y;
            colocadoCorrectamente = true;
            return;
        }

        /*
         * Actualiza los colliders antes de medir sus límites.
         */
        Physics.SyncTransforms();

        /*
         * Calculamos la distancia vertical desde el pivote
         * hasta la parte inferior real del collider.
         *
         * Ejemplo:
         * pivote Y = 0
         * parte inferior Y = -0.5
         * distancia = 0.5
         */
        float distanciaPivoteHastaBase =
            transform.position.y -
            colliderPrincipal.bounds.min.y;

        /*
         * Comenzamos el rayo ligeramente por encima
         * de la parte superior del collider.
         */
        Vector3 origenRaycast = new Vector3(
            colliderPrincipal.bounds.center.x,
            colliderPrincipal.bounds.max.y + 0.5f,
            colliderPrincipal.bounds.center.z
        );

        RaycastHit[] golpes = Physics.RaycastAll(
            origenRaycast,
            Vector3.down,
            distanciaBusquedaSuelo,
            capasSuelo,
            QueryTriggerInteraction.Ignore
        );

        bool sueloEncontrado = false;
        RaycastHit mejorGolpe = new RaycastHit();
        float distanciaMasCorta = float.MaxValue;

        foreach (RaycastHit golpe in golpes)
        {
            Collider colliderGolpeado = golpe.collider;

            if (colliderGolpeado == null)
            {
                continue;
            }

            /*
             * Ignorar el collider del propio monstruo.
             */
            if (colliderGolpeado == colliderPrincipal)
            {
                continue;
            }

            /*
             * Ignorar cualquier collider conectado
             * al mismo Rigidbody.
             */
            if (colliderGolpeado.attachedRigidbody == rb)
            {
                continue;
            }

            /*
             * Ignorar objetos hijos del propio enemigo.
             */
            if (colliderGolpeado.transform == transform ||
                colliderGolpeado.transform.IsChildOf(transform))
            {
                continue;
            }

            /*
             * Ignorar al jugador.
             */
            if (colliderGolpeado.CompareTag("Player"))
            {
                continue;
            }

            /*
             * Ignorar otros objetos móviles.
             * El suelo normalmente no tiene Rigidbody.
             */
            if (colliderGolpeado.attachedRigidbody != null)
            {
                continue;
            }

            /*
             * Evita considerar paredes verticales como suelo.
             */
            if (golpe.normal.y < 0.5f)
            {
                continue;
            }

            if (golpe.distance < distanciaMasCorta)
            {
                distanciaMasCorta = golpe.distance;
                mejorGolpe = golpe;
                sueloEncontrado = true;
            }
        }

        if (!sueloEncontrado)
        {
            // Si no hay suelo real debajo, dejarlo "flotando" a su altura
            // actual lo convierte en un enemigo fantasma: invisible o
            // inalcanzable, e imposible de completar el nivel. Es más seguro
            // eliminarlo directamente.
            Debug.LogWarning(
                "[BacteriaKamikaze] No se encontró suelo debajo de '" +
                name + "'. Se elimina para no dejar un enemigo fantasma."
            );

            Destroy(gameObject);
            return;
        }

        Vector3 nuevaPosicion = rb.position;

        /*
         * Ahora colocamos la BASE del collider sobre el suelo,
         * no el pivote del enemigo.
         */
        nuevaPosicion.y =
            mejorGolpe.point.y +
            distanciaPivoteHastaBase +
            elevacionExtra;

        rb.position = nuevaPosicion;

        alturaFija = nuevaPosicion.y;
        colocadoCorrectamente = true;

        Physics.SyncTransforms();

        Debug.Log(
            "[BacteriaKamikaze] '" + name +
            "' colocado correctamente sobre el suelo. " +
            "Altura final: " + alturaFija
        );
    }

    /*
     * Llamado por DesatascadorEnemigos cuando lo reposiciona.
     * Sin esto, FixedUpdate volvería a bajarlo a la altura
     * vieja (alturaFija) en el siguiente tick de física,
     * deshaciendo el rescate.
     */
    public void ActualizarAlturaFija(float nuevaAltura)
    {
        alturaFija = nuevaAltura;
    }

    /*
     * Llamado por DesatascadorEnemigos después de empujarlo fuera de
     * un muro: vuelve a buscar suelo real desde la posición actual y
     * se coloca sobre él (o se autodestruye si sigue sin haber suelo,
     * en vez de quedar flotando para siempre).
     */
    public void ReasentarEnSuelo()
    {
        AjustarAlSuelo();
    }

    void FixedUpdate()
    {
        if (!colocadoCorrectamente)
        {
            return;
        }

        /*
         * Red de seguridad por si sale completamente del mapa.
         */
        if (rb.position.y < -15f)
        {
            Debug.LogWarning(
                gameObject.name +
                " cayó fuera del mapa y fue eliminado."
            );

            Destroy(gameObject);
            return;
        }

        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        Vector3 posicionActual = rb.position;

        /*
         * Calculamos la distancia solamente en X y Z.
         * La diferencia de altura no afecta la detección.
         */
        Vector3 direccionJugador =
            jugador.position - posicionActual;

        direccionJugador.y = 0f;

        float distanciaHorizontal =
            direccionJugador.magnitude;

        if (distanciaHorizontal > distanciaDeteccion)
        {
            return;
        }

        /*
         * Girar hacia el jugador.
         */
        if (direccionJugador.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(
                    direccionJugador.normalized
                );

            rb.MoveRotation(rotacionObjetivo);
        }

        /*
         * Moverse hacia el jugador manteniendo
         * permanentemente la altura correcta.
         */
        Vector3 posicionObjetivo = jugador.position;
        posicionObjetivo.y = alturaFija;

        Vector3 nuevaPosicion = Vector3.MoveTowards(
            posicionActual,
            posicionObjetivo,
            velocidadCarga * Time.fixedDeltaTime
        );

        nuevaPosicion.y = alturaFija;

        rb.MovePosition(nuevaPosicion);
    }
}