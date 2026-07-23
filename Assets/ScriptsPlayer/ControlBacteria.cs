using UnityEngine;

public class ControlBacteria : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vida = 1;
    public GameObject efectoLimpieza;

    [Header("Sonidos")]
    public AudioClip sonidoExplosion;
    [Range(0f, 1f)] public float volumenExplosion = 1f;

    [Header("Movimiento Limitado")]
    public float velocidad = 2f;
    public float rangoMovimiento = 0.4f;

    private Vector3 posicionInicial;
    private Vector3 escalaInicial;
    private Renderer renderBacteria;
    private bool estaMuerta = false;
    private float faseAleatoria; // Para que no pulsen todas al mismo tiempo

    void Start()
    {
        posicionInicial = transform.localPosition;
        escalaInicial = transform.localScale;
        renderBacteria = GetComponent<Renderer>();
        faseAleatoria = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (estaMuerta) return;

        float offset = Mathf.Sin(Time.time * velocidad) * rangoMovimiento;

        transform.localPosition = new Vector3(
            posicionInicial.x + offset,
            posicionInicial.y,
            posicionInicial.z
        );

        // Pulsación orgánica: la bacteria "respira" agrandándose y encogiéndose un poco
        float pulso = 1f + Mathf.Sin(Time.time * 3f + faseAleatoria) * 0.06f;
        transform.localScale = escalaInicial * pulso;
    }

    public void RecibirDano()
    {
        if (estaMuerta) return;

        vida--;

        if (vida <= 0)
        {
            Morir();
        }
        else
        {
            CambiarColorPorDano();
        }
    }

    void Morir()
    {
        estaMuerta = true;

        // Suma puntos (con multiplicador de combo) y los muestra flotando
        int ganancia = PuntajeJuego.SumarEliminacion(10);
        TextoFlotante.Crear(transform.position, "+" + ganancia, new Color(1f, 0.95f, 0.5f));

        if (sonidoExplosion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoExplosion, transform.position, volumenExplosion);
        }

        if (efectoLimpieza != null)
        {
            GameObject efecto = Instantiate(efectoLimpieza, transform.position, Quaternion.identity);
            Destroy(efecto, 2f);
        }

        // --- LOOT: 15% de probabilidad de soltar una pastilla de vida ---
        GameObject pastilla = Resources.Load<GameObject>("vidaExtra");
        if (pastilla != null && Random.Range(1, 101) <= 15)
        {
            Instantiate(pastilla, transform.position, Quaternion.identity);
        }

        // --- POWER-UP: 12% de probabilidad (independiente de la vida) ---
        if (Random.Range(1, 101) <= 12)
        {
            PowerUp.CrearAleatorio(transform.position);
        }

        Destroy(gameObject);
    }

    void CambiarColorPorDano()
    {
        if (renderBacteria != null)
        {
            renderBacteria.material.color *= 0.8f;
        }
    }
}
