using UnityEngine;
using UnityEngine.SceneManagement; 

public class ControlNiveles : MonoBehaviour
{
    [Header("Condición para Pasar de Nivel")]
    [Tooltip("Si está activo, también hay que eliminar a los enemigos disparadores para avanzar")]
    public bool exigirEliminarDisparadores = true;

    private bool cambiandoDeNivel = false;

    void Update()
    {
        if (cambiandoDeNivel) return;

        // Si ya estamos en el Nivel 3, este script no hace nada.
        // De eso ya se encarga el ControlFinalJuego del DirectorDeJuego.
        if (SceneManager.GetActiveScene().name == "Nivel_3") return;

        // BUSCAMOS EN TIEMPO REAL SI QUEDAN BACTERIAS VIVAS
        ControlBacteria[] bacteriasNormales = FindObjectsByType<ControlBacteria>(FindObjectsSortMode.None);
        BacteriaKamikaze[] bacteriasKamikazes = FindObjectsByType<BacteriaKamikaze>(FindObjectsSortMode.None);

        // Los disparadores también cuentan (antes se podía avanzar con ellos vivos)
        int disparadoresVivos = 0;
        if (exigirEliminarDisparadores)
        {
            disparadoresVivos = FindObjectsByType<EnemigoDisparador>(FindObjectsSortMode.None).Length;
        }

        // Si ya no queda ningún rastro de bacterias... ¡Pasamos de nivel!
        if (bacteriasNormales.Length == 0 && bacteriasKamikazes.Length == 0 && disparadoresVivos == 0)
        {
            PasarSiguienteNivel();
        }
    }

    void PasarSiguienteNivel()
    {
        cambiandoDeNivel = true;
        
        int nivelActual = SceneManager.GetActiveScene().buildIndex;
        int siguienteNivel = nivelActual + 1;

        // Verificamos si existe una escena en la lista antes de cargarla
        if (siguienteNivel < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("¡Nivel completado! Cargando el siguiente escenario...");
            SceneManager.LoadScene(siguienteNivel);
        }
        else
        {
            Debug.LogWarning("No hay más niveles registrados en el Build Settings.");
        }
    }
}