using UnityEngine;

public class EnemigoDisparador : MonoBehaviour
{
    [Header("Vida del Enemigo")]
    public int vidaMaxima = 3;
    private int vidaActual;
    private bool estaMuerto = false;

    [Header("Jugador")]
    public string tagJugador = "Player";
    public Transform jugador;

    [Header("Rangos")]
    public float rangoDeteccion = 12f;
    public float rangoDisparo = 8f;
    public float distanciaMinimaAlJugador = 3f;

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 4f;
    public float velocidadRotacion = 6f;

    [Header("Patrulla")]
    public Transform[] puntosPatrulla;
    public float distanciaParaCambiarPunto = 0.3f;
    private int puntoActual = 0;

    [Header("Disparo")]
    public GameObject balaEnemigaPrefab;
    public Transform puntoDisparo;
    public float velocidadBala = 15f;
    public float tiempoEntreDisparos = 1.5f;
    private float tiempoSiguienteDisparo = 0f;

    [Header("Efectos")]
    public GameObject efectoMuerte;
    public AudioClip sonidoMuerte;
    [Range(0f, 1f)] public float volumenMuerte = 1f;

    [Header("Recompensas (Loot)")]
    public GameObject prefabVidaExtra; // Casilla para tu pastilla de vida

    void Start()
    {
        vidaActual = vidaMaxima;

        if (jugador == null)
        {
            GameObject jugadorEncontrado = GameObject.FindGameObjectWithTag(tagJugador);

            if (jugadorEncontrado != null)
            {
                jugador = jugadorEncontrado.transform;
            }
            else
            {
                Debug.LogWarning("No se encontró al jugador. Revisa el tag del Player.");
            }
        }
    }

    private float proximaRevisionVisual = 0f;

    void Update()
    {
        if (estaMuerto) return;

        // Red de seguridad: si cayó fuera del mapa, se elimina para no bloquear la partida
        if (transform.position.y < -15f)
        {
            Destroy(gameObject);
            return;
        }

        // Red de seguridad: un enemigo NUNCA puede ser invisible.
        // (El diseño original ocultaba la cápsula del disparador y usaba al
        // monstruo cofre como cuerpo visible; al morir el cofre, la cápsula
        // quedaba invisible pero seguía disparando. Ahora se muestra sola.)
        if (Time.time >= proximaRevisionVisual)
        {
            proximaRevisionVisual = Time.time + 1f;
            AsegurarVisibilidad();
        }

        if (jugador == null)
        {
            Patrullar();
            return;
        }

        float distanciaJugador = Vector3.Distance(transform.position, jugador.position);

        if (distanciaJugador <= rangoDeteccion)
        {
            PerseguirJugador(distanciaJugador);

            if (distanciaJugador <= rangoDisparo)
            {
                IntentarDisparar();
            }
        }
        else
        {
            Patrullar();
        }
    }

    void AsegurarVisibilidad()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                return; // hay al menos una parte visible: todo bien
            }
        }

        // Nada visible: reactivamos los renderers para que el jugador lo vea
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        if (renderers.Length > 0)
        {
            Debug.LogWarning("[EnemigoDisparador] '" + name + "' era invisible. Se reactivó su renderer.");
        }
    }

    void Patrullar()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;

        Transform puntoObjetivo = puntosPatrulla[puntoActual];

        if (puntoObjetivo == null) return;

        Vector3 posicionObjetivo = new Vector3(
            puntoObjetivo.position.x,
            transform.position.y,
            puntoObjetivo.position.z
        );

        MoverHacia(posicionObjetivo, velocidadPatrulla);

        float distanciaAlPunto = Vector3.Distance(transform.position, posicionObjetivo);

        if (distanciaAlPunto <= distanciaParaCambiarPunto)
        {
            puntoActual++;

            if (puntoActual >= puntosPatrulla.Length)
            {
                puntoActual = 0;
            }
        }
    }

    void PerseguirJugador(float distanciaJugador)
    {
        Vector3 posicionJugador = new Vector3(
            jugador.position.x,
            transform.position.y,
            jugador.position.z
        );

        MirarHacia(posicionJugador);

        if (distanciaJugador > distanciaMinimaAlJugador)
        {
            MoverHacia(posicionJugador, velocidadPersecucion);
        }
    }

    void MoverHacia(Vector3 destino, float velocidad)
    {
        MirarHacia(destino);

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );
    }

    void MirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;

        if (direccion == Vector3.zero) return;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    void IntentarDisparar()
    {
        if (Time.time >= tiempoSiguienteDisparo)
        {
            Disparar();
            tiempoSiguienteDisparo = Time.time + tiempoEntreDisparos;
        }
    }

    void Disparar()
    {
        if (balaEnemigaPrefab == null || puntoDisparo == null) return;

        Vector3 objetivo = jugador.position + Vector3.up * 1f;
        Vector3 direccion = objetivo - puntoDisparo.position;

        GameObject bala = Instantiate(
            balaEnemigaPrefab,
            puntoDisparo.position,
            Quaternion.LookRotation(direccion)
        );

        BalaEnemiga scriptBala = bala.GetComponent<BalaEnemiga>();

        if (scriptBala != null)
        {
            scriptBala.Inicializar(direccion, velocidadBala, gameObject);
        }
    }

    public void RecibirDano(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;

        Debug.Log("Vida del enemigo: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        estaMuerto = true;

        // El disparador es el enemigo más difícil (3 de vida), vale más puntos
        int ganancia = PuntajeJuego.SumarEliminacion(50);
        TextoFlotante.Crear(transform.position, "+" + ganancia, new Color(1f, 0.55f, 0.55f));

        // 25% de probabilidad de soltar un power-up
        if (Random.Range(1, 101) <= 25)
        {
            PowerUp.CrearAleatorio(transform.position);
        }

        if (sonidoMuerte != null)
        {
            AudioSource.PlayClipAtPoint(sonidoMuerte, transform.position, volumenMuerte);
        }
        else
        {
            // Si no se asignó sonido en el Inspector, usamos el pop grave
            EfectosJuego.SonidoPop(transform.position, volumenMuerte, 0.7f);
        }

        if (efectoMuerte != null)
        {
            GameObject efecto = Instantiate(efectoMuerte, transform.position, Quaternion.identity);
            Destroy(efecto, 2f);
        }
        else
        {
            // Si no se asignó efecto en el Inspector, usamos la explosión de limpieza grande
            EfectosJuego.ExplosionLimpieza(transform.position, 1.5f);
        }

        // --- SISTEMA DE LOOT (VIDA EXTRA) ---
        // Si no se asignó la pastilla en el Inspector, la cargamos de Resources
        GameObject pastilla = prefabVidaExtra != null ? prefabVidaExtra : Resources.Load<GameObject>("vidaExtra");

        // Genera un número aleatorio del 1 al 100. Si sale 30 o menos (30% de probabilidad), instancia la pastilla.
        if (pastilla != null && Random.Range(1, 101) <= 30)
        {
            Instantiate(pastilla, transform.position, Quaternion.identity);
            Debug.Log("¡Enemigo soltó una pastilla de Vida Extra!");
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDisparo);
    }
}