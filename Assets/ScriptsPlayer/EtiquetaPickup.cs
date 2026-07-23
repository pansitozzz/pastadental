using UnityEngine;
using TMPro;

// ============================================================================
// ETIQUETA FLOTANTE PARA PICKUPS ("+1 VIDA", "TRIPLE DISPARO"...)
// Texto pequeño que flota sobre el objeto y siempre mira a la cámara,
// para que el jugador sepa qué está recogiendo antes de tocarlo.
// ============================================================================
public class EtiquetaPickup : MonoBehaviour
{
    public static void Crear(Transform padre, string texto, Color color)
    {
        GameObject objeto = new GameObject("Etiqueta");

        // No la hacemos hija directa para que la rotación del pickup
        // (que gira sobre sí mismo) no arrastre al texto.
        EtiquetaPickup comp = objeto.AddComponent<EtiquetaPickup>();
        comp.objetivo = padre;

        TextMeshPro tmp = objeto.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = 3f;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);

        // Contorno sutil para que se lea sobre cualquier fondo
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(0, 0, 0, 200);
    }

    private Transform objetivo;

    void LateUpdate()
    {
        // Si el pickup fue recogido o destruido, la etiqueta muere con él
        if (objetivo == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = objetivo.position + Vector3.up * 0.75f;

        Camera cam = Camera.main;

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }
}
