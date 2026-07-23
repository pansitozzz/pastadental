using UnityEngine;

public class ImpactoPastaContraEnemigo : MonoBehaviour
{
    [Header("Daño de la pasta")]
    public int dano = 1;

    private bool yaImpacto = false;

    void OnCollisionEnter(Collision colision)
    {
        if (yaImpacto) return;

        // Una pasta nunca explota contra otra pasta
        if (colision.gameObject.GetComponent<ImpactoPasta>() != null) return;

        yaImpacto = true;

        EnemigoDisparador enemigo = colision.gameObject.GetComponent<EnemigoDisparador>();

        if (enemigo == null)
        {
            enemigo = colision.gameObject.GetComponentInParent<EnemigoDisparador>();
        }

        if (enemigo != null)
        {
            enemigo.RecibirDano(dano);
        }

        Destroy(gameObject);
    }
}