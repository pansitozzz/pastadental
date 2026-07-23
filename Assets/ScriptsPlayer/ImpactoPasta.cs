using UnityEngine;

public class ImpactoPasta : MonoBehaviour
{
    private bool yaImpacto = false;

    void OnCollisionEnter(Collision colision)
    {
        if (yaImpacto) return;

        // Una pasta NUNCA explota contra otra pasta (pasa con el triple
        // disparo y la ráfaga rápida, que lanzan varias muy juntas)
        if (colision.gameObject.GetComponent<ImpactoPasta>() != null) return;

        yaImpacto = true;

        // 1. INTENTAR BUSCAR BACTERIA NORMAL
        ControlBacteria bacteria = colision.gameObject.GetComponent<ControlBacteria>();

        if (bacteria == null)
        {
            bacteria = colision.gameObject.GetComponentInParent<ControlBacteria>();
        }

        if (bacteria != null)
        {
            bacteria.RecibirDano();
        }

        // 2. INTENTAR BUSCAR MONSTRUO KAMIKAZE
        BacteriaKamikaze kamikaze = colision.gameObject.GetComponent<BacteriaKamikaze>();

        if (kamikaze == null)
        {
            kamikaze = colision.gameObject.GetComponentInParent<BacteriaKamikaze>();
        }

        if (kamikaze != null)
        {
            // El kamikaze vale más puntos porque es más peligroso
            int ganancia = PuntajeJuego.SumarEliminacion(25);
            TextoFlotante.Crear(kamikaze.transform.position, "+" + ganancia, new Color(1f, 0.7f, 0.4f));

            // Efecto y sonido de muerte (antes moría en silencio)
            EfectosJuego.ExplosionLimpieza(kamikaze.transform.position, 1.2f);
            EfectosJuego.SonidoPop(kamikaze.transform.position, 1f, 0.9f);

            // Destruimos el objeto que TIENE el script kamikaze (si la pasta
            // golpea una pata del modelo, igual muere el monstruo completo)
            Destroy(kamikaze.gameObject);
        }

        // 3. SI NO LE DIMOS A NADA VIVO: salpicadura de pasta contra la superficie
        bool leDioAlDisparador = colision.gameObject.GetComponentInParent<EnemigoDisparador>() != null;

        if (bacteria == null && kamikaze == null && !leDioAlDisparador)
        {
            EfectosJuego.SplatPasta(transform.position);
        }

        // Siempre destruimos el proyectil de pasta al chocar
        Destroy(gameObject);
    }
}
