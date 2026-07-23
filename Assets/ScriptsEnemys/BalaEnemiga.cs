using UnityEngine;

public class BalaEnemiga : MonoBehaviour
{
    [Header("Daño")]
    public int dano = 1;

    [Header("Tiempo de vida")]
    public float tiempoDeVida = 5f;

    private Rigidbody rb;
    private GameObject dueno;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Inicializar(Vector3 direccion, float velocidad, GameObject enemigoQueDispara)
    {
        dueno = enemigoQueDispara;

        // Ignora colisiones con el enemigo que disparó
        Collider colliderBala = GetComponent<Collider>();
        Collider[] collidersDueno = dueno.GetComponentsInChildren<Collider>();

        foreach (Collider col in collidersDueno)
        {
            Physics.IgnoreCollision(colliderBala, col);
        }

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = direccion.normalized * velocidad;
        }

        Destroy(gameObject, tiempoDeVida);
    }

    void OnTriggerEnter(Collider other)
    {
        // Si toca al mismo enemigo que la disparó, no hace nada
        if (dueno != null && other.transform.IsChildOf(dueno.transform))
        {
            return;
        }

        // Si toca al jugador, le hace daño
        if (other.CompareTag("Player"))
        {
            VidaJugador vidaJugador = other.GetComponent<VidaJugador>();

            if (vidaJugador != null)
            {
                vidaJugador.RecibirDano(dano);
            }

            Destroy(gameObject);
        }
        else
        {
            // Si toca pared, piso u otro objeto, se destruye
            Destroy(gameObject);
        }
    }
}