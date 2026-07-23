using UnityEngine;
using TMPro;

// ============================================================================
// TEXTO FLOTANTE EN EL MUNDO ("+30", "¡Triple disparo!"...)
// Sube lentamente, mira siempre a la cámara y se desvanece solo.
// Se crea por código: TextoFlotante.Crear(posicion, "+10", color);
// ============================================================================
public class TextoFlotante : MonoBehaviour
{
    private const float DURACION = 1.2f;
    private const float VELOCIDAD_SUBIDA = 1.1f;

    private TextMeshPro texto;
    private float tiempoCreacion;

    public static void Crear(Vector3 posicionMundo, string contenido, Color color)
    {
        GameObject objeto = new GameObject("TextoFlotante");
        objeto.transform.position = posicionMundo + Vector3.up * 0.6f;

        TextMeshPro tmp = objeto.AddComponent<TextMeshPro>();
        tmp.text = contenido;
        tmp.fontSize = 5;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect = tmp.rectTransform;
        rect.sizeDelta = new Vector2(4f, 2f);

        TextoFlotante instancia = objeto.AddComponent<TextoFlotante>();
        instancia.texto = tmp;
        instancia.tiempoCreacion = Time.time;

        Destroy(objeto, DURACION + 0.1f);
    }

    void Update()
    {
        // Sube lentamente
        transform.position += Vector3.up * VELOCIDAD_SUBIDA * Time.deltaTime;

        // Mira siempre a la cámara (billboard)
        Camera cam = Camera.main;

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }

        // Se desvanece en la segunda mitad de su vida
        if (texto != null)
        {
            float transcurrido = Time.time - tiempoCreacion;
            float mitad = DURACION * 0.5f;

            if (transcurrido > mitad)
            {
                texto.alpha = 1f - (transcurrido - mitad) / mitad;
            }
        }
    }
}
