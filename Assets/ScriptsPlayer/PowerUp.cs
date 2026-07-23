using UnityEngine;

// ============================================================================
// POWER-UPS PROCEDURALES
// Esferas brillantes que giran y flotan; al acercarte las recoges:
//   - Triple disparo (celeste): 3 pastas en abanico por 10 segundos
//   - Pasta rápida (naranja): dispara al doble de velocidad por 10 segundos
//   - Escudo de flúor (menta): inmunidad total por 8 segundos
// La recolección es POR DISTANCIA (no por trigger físico), porque el
// CharacterController del jugador no siempre dispara triggers estáticos.
// ============================================================================
public enum TipoPowerUp
{
    TripleDisparo,
    PastaRapida,
    EscudoFluor
}

public class PowerUp : MonoBehaviour
{
    private const float DURACION_EFECTO = 10f;
    private const float DURACION_ESCUDO = 8f;
    private const float SEGUNDOS_DE_VIDA = 20f;
    private const float RADIO_RECOLECCION = 1.7f;

    private TipoPowerUp tipo;
    private Vector3 posicionInicial;
    private bool recogido = false;
    private Transform jugador;

    // Crea un power-up al azar en la posición dada (donde murió un enemigo)
    public static void CrearAleatorio(Vector3 posicion)
    {
        int sorteo = Random.Range(0, 3);

        if (sorteo == 0) Crear(posicion, TipoPowerUp.TripleDisparo);
        else if (sorteo == 1) Crear(posicion, TipoPowerUp.PastaRapida);
        else Crear(posicion, TipoPowerUp.EscudoFluor);
    }

    public static void Crear(Vector3 posicion, TipoPowerUp tipo)
    {
        GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esfera.name = "PowerUp_" + tipo;
        esfera.transform.position = posicion + Vector3.up * 0.4f;
        esfera.transform.localScale = Vector3.one * 0.45f;

        // Sin colisión física: la recolección es por distancia
        Collider col = esfera.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        Color color = ColorDe(tipo);

        Renderer rend = esfera.GetComponent<Renderer>();
        rend.material.color = color;

        // Luz puntual: hace que el power-up se note desde lejos
        GameObject objetoLuz = new GameObject("Luz");
        objetoLuz.transform.SetParent(esfera.transform, false);
        Light luz = objetoLuz.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = color;
        luz.range = 4f;
        luz.intensity = 2f;

        // Etiqueta con el nombre, para saber qué es antes de tocarlo
        EtiquetaPickup.Crear(esfera.transform, NombreDe(tipo), color);

        PowerUp comp = esfera.AddComponent<PowerUp>();
        comp.tipo = tipo;
        comp.posicionInicial = esfera.transform.position;

        Object.Destroy(esfera, SEGUNDOS_DE_VIDA);
    }

    static Color ColorDe(TipoPowerUp tipo)
    {
        if (tipo == TipoPowerUp.TripleDisparo) return new Color(0.3f, 0.8f, 1f);   // celeste
        if (tipo == TipoPowerUp.PastaRapida) return new Color(1f, 0.6f, 0.15f);    // naranja
        return new Color(0.4f, 1f, 0.7f);                                          // menta (escudo)
    }

    static string NombreDe(TipoPowerUp tipo)
    {
        if (tipo == TipoPowerUp.TripleDisparo) return "TRIPLE DISPARO";
        if (tipo == TipoPowerUp.PastaRapida) return "PASTA RÁPIDA";
        return "ESCUDO DE FLÚOR";
    }

    void Update()
    {
        // Gira y flota: señal universal de "esto se recoge"
        transform.Rotate(Vector3.up, 130f * Time.deltaTime, Space.World);

        Vector3 pos = posicionInicial;
        pos.y += Mathf.Sin(Time.time * 2.5f) * 0.15f;
        transform.position = pos;

        // ----- Recolección por distancia -----
        if (recogido) return;

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

            if (jugador == null) return;
        }

        if (Vector3.Distance(transform.position, jugador.position) <= RADIO_RECOLECCION)
        {
            Recoger();
        }
    }

    void Recoger()
    {
        recogido = true;

        // OJO: SistemaDisparo vive en la Main Camera (hija del Jugador),
        // por eso hay que buscar en los hijos y no solo en la raíz
        SistemaDisparo disparo = jugador.GetComponentInChildren<SistemaDisparo>();
        VidaJugador vida = jugador.GetComponentInChildren<VidaJugador>();

        string aviso = NombreDe(tipo);
        Color color = ColorDe(tipo);

        if (tipo == TipoPowerUp.TripleDisparo && disparo != null)
        {
            disparo.ActivarTripleDisparo(DURACION_EFECTO);
        }
        else if (tipo == TipoPowerUp.PastaRapida && disparo != null)
        {
            disparo.ActivarPastaRapida(DURACION_EFECTO);
        }
        else if (tipo == TipoPowerUp.EscudoFluor && vida != null)
        {
            vida.ActivarEscudo(DURACION_ESCUDO);
        }

        TextoFlotante.Crear(transform.position, "¡" + aviso + "!", color);

        AudioClip clip = Resources.Load<AudioClip>("SFX_PositiveSound01");
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, 0.8f);
        }

        Destroy(gameObject);
    }
}
