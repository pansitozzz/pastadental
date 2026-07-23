using UnityEngine;
using TMPro;

public class Cronometro : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float tiempoRestante = 30f;

    [Header("UI")]
    public TextMeshProUGUI textoTiempo;

    private bool juegoTerminado = false;

    void Update()
    {
        if (juegoTerminado) return;

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            tiempoRestante = Mathf.Max(tiempoRestante, 0);

            ActualizarTexto();
        }
        else
        {
            GameOver();
        }
    }

    void ActualizarTexto()
    {
        if (textoTiempo != null)
        {
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString();
        }
    }

    void GameOver()
    {
        juegoTerminado = true;

        if (textoTiempo != null)
        {
            textoTiempo.text = "¡CARIES! Perdiste";
        }

        Debug.Log("Game Over: se acabó el tiempo.");
    }

    public void DetenerCronometroPorVictoria()
    {
        juegoTerminado = true;

        if (textoTiempo != null)
        {
            textoTiempo.text = "¡Diente limpio!";
        }
    }
}