using UnityEngine;

public class VidaBacteria : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 3f;  // Cuántos disparos de pasta aguanta antes de morir
    private float vidaActual;

    [Header("Configuración de Recompensas (Loot)")]
    public GameObject prefabVidaExtra; // Arrastra aquí tu prefab de la pastilla en Unity

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    // FUNCIÓN PÚBLICA: Tu pasta dental llamará a esta función para hacerle daño
    public void RecibirDano(float cantidadDano)
    {
        // Si ya está muerta, ignoramos el daño extra
        if (vidaActual <= 0) return;

        vidaActual -= cantidadDano;
        
        Debug.Log(gameObject.name + " recibió daño. Vida restante: " + vidaActual);

        // Verificamos si la bacteria debe morir
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("¡La bacteria ha sido eliminada por tus disparos!");

        // Aquí puedes agregar un efecto visual de explosión de pasta si tienes, por ejemplo:
        // Instantiate(efectoExplosion, transform.position, Quaternion.identity);

        // --- SISTEMA DE LOOT (VIDA EXTRA) ---
        // Genera un número aleatorio del 1 al 100. Si sale 30 o menos (30% de probabilidad), suelta la cura.
        if (prefabVidaExtra != null && Random.Range(1, 101) <= 30)
        {
            Instantiate(prefabVidaExtra, transform.position, Quaternion.identity);
            Debug.Log("¡La bacteria soltó una pastilla de Vida Extra!");
        }

        // El objeto de la bacteria desaparece del mapa para siempre
        Destroy(gameObject); 
    }
}