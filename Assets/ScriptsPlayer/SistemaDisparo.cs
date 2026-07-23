using UnityEngine;

public class SistemaDisparo : MonoBehaviour
{
    public GameObject prefabPasta; // Aquí pondremos el molde azul
    public float fuerzaDisparo = 30f;

    [Header("Cadencia de Disparo")]
    public float tiempoEntreDisparos = 0.25f; // Evita el "modo dios" con autoclick
    private float proximoDisparo = 0f;

    [Header("Sonido de Disparo")]
    public AudioClip sonidoDisparo; // Si se deja vacío, usa el pop de Resources con tono agudo
    [Range(0f, 1f)] public float volumenDisparo = 0.4f;

    [Header("Estela del Proyectil")]
    public bool usarEstela = true;

    private Material materialEstela;

    // ----- Power-ups temporales -----
    private float tripleDisparoHasta = 0f;
    private float pastaRapidaHasta = 0f;

    public void ActivarTripleDisparo(float segundos)
    {
        tripleDisparoHasta = Time.time + segundos;
    }

    public void ActivarPastaRapida(float segundos)
    {
        pastaRapidaHasta = Time.time + segundos;
    }

    void Start()
    {
        // Si no asignaste un sonido en el Inspector, reutilizamos el pop del proyecto
        if (sonidoDisparo == null)
        {
            sonidoDisparo = Resources.Load<AudioClip>("pop_bacteria");
        }

        // Material simple para la estela (no necesita asset en disco)
        materialEstela = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        bool pastaRapidaActiva = Time.time < pastaRapidaHasta;

        // Con "pasta rápida" activa, la recarga es a la mitad de tiempo
        float cadenciaEfectiva = tiempoEntreDisparos;

        if (pastaRapidaActiva)
        {
            cadenciaEfectiva *= 0.45f;
        }

        // Con "pasta rápida": basta MANTENER el clic presionado (ametralladora).
        // Sin ella: un clic = un disparo, como siempre.
        bool quiereDisparar = pastaRapidaActiva
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (quiereDisparar && Time.time >= proximoDisparo)
        {
            proximoDisparo = Time.time + cadenciaEfectiva;
            Disparar();
        }
    }

    void Disparar()
    {
        if (Time.time < tripleDisparoHasta)
        {
            // Triple disparo: tres pastas en abanico
            GameObject balaIzq = DispararUna(Quaternion.AngleAxis(-8f, transform.up) * transform.forward);
            GameObject balaCentro = DispararUna(transform.forward);
            GameObject balaDer = DispararUna(Quaternion.AngleAxis(8f, transform.up) * transform.forward);

            // Las 3 nacen casi en el mismo punto: si no se ignoran entre sí,
            // chocan apenas salen y se destruyen mutuamente
            IgnorarColisionEntre(balaIzq, balaCentro);
            IgnorarColisionEntre(balaIzq, balaDer);
            IgnorarColisionEntre(balaCentro, balaDer);
        }
        else
        {
            DispararUna(transform.forward);
        }

        ReproducirSonidoDisparo();
    }

    void IgnorarColisionEntre(GameObject a, GameObject b)
    {
        if (a == null || b == null) return;

        Collider colA = a.GetComponent<Collider>();
        Collider colB = b.GetComponent<Collider>();

        if (colA != null && colB != null)
        {
            Physics.IgnoreCollision(colA, colB);
        }
    }

    GameObject DispararUna(Vector3 direccion)
    {
        // Crea una bola de pasta justo donde está la cámara
        GameObject bala = Instantiate(prefabPasta, transform.position + direccion, Quaternion.LookRotation(direccion));

        // Le da un empujón fuerte hacia adelante
        bala.GetComponent<Rigidbody>().AddForce(direccion * fuerzaDisparo, ForceMode.Impulse);

        if (usarEstela)
        {
            AgregarEstela(bala);
        }

        // Destruye la pasta después de 2 segundos si no le dio a nada para no llenar el piso de basura
        Destroy(bala, 2f);

        return bala;
    }

    void AgregarEstela(GameObject bala)
    {
        TrailRenderer estela = bala.AddComponent<TrailRenderer>();
        estela.time = 0.25f;
        estela.startWidth = 0.15f;
        estela.endWidth = 0f;
        estela.material = materialEstela;
        estela.startColor = new Color(1f, 1f, 1f, 0.8f);   // Blanco pasta
        estela.endColor = new Color(0.5f, 0.9f, 1f, 0f);   // Se desvanece a celeste menta
    }

    void ReproducirSonidoDisparo()
    {
        if (sonidoDisparo == null) return;

        // Creamos un AudioSource temporal para poder subirle el tono (pitch)
        // y que el pop suene como un disparo de pasta "squish"
        GameObject objetoSonido = new GameObject("SonidoDisparo");
        objetoSonido.transform.position = transform.position;

        AudioSource fuente = objetoSonido.AddComponent<AudioSource>();
        fuente.clip = sonidoDisparo;
        fuente.volume = volumenDisparo;
        fuente.pitch = Random.Range(1.5f, 1.9f); // Tono agudo y variado para que no canse
        fuente.spatialBlend = 0f;
        fuente.Play();

        Destroy(objetoSonido, sonidoDisparo.length + 0.1f);
    }
}
