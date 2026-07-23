using UnityEngine;

public class RecogerVida : MonoBehaviour
{
    public int cantidadCura = 1;

    [Header("Apariencia de Ítem")]
    public float velocidadGiro = 90f;      // Grados por segundo (para que se note que es un pickup)
    public float segundosDeVida = 25f;     // Después de este tiempo desaparece sola

    private bool recogida = false;

    void Start()
    {
        // La pastilla desaparece sola pasado un tiempo,
        // para no dejar "bolitas" tiradas el resto de la partida
        Destroy(gameObject, segundosDeVida);

        // Etiqueta flotante para que se sepa qué es antes de tocarla
        EtiquetaPickup.Crear(transform, "+1 VIDA", new Color(1f, 0.4f, 0.45f));
    }

    void Update()
    {
        // Gira sobre sí misma: señal visual clásica de "esto se puede recoger"
        transform.Rotate(Vector3.up, velocidadGiro * Time.deltaTime, Space.World);
    }

    // Detecta si el jugador la pisa en el suelo (Colisión física)
    private void OnCollisionEnter(Collision collision)
    {
        EvaluarRecoleccion(collision.gameObject);
    }

    // Por si acaso el jugador la toca en el aire antes de caer (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        EvaluarRecoleccion(other.gameObject);
    }

    private void EvaluarRecoleccion(GameObject objetoContacto)
    {
        if (recogida) return;

        VidaJugador jugador = objetoContacto.GetComponent<VidaJugador>();

        if (jugador != null)
        {
            recogida = true;
            jugador.Curar(cantidadCura);
            ReproducirSonidoRecogida();
            Destroy(gameObject); // La pastilla desaparece al ser recogida
        }
    }

    void ReproducirSonidoRecogida()
    {
        AudioClip clip = Resources.Load<AudioClip>("SFX_PositiveSound01");

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, 0.7f);
        }
    }
}
