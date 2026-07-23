using UnityEngine;

// ============================================================================
// AYUDANTE DE EFECTOS
// Carga el VFX de limpieza y el sonido pop desde la carpeta Resources,
// para que cualquier script pueda usarlos sin tener que asignarlos
// a mano en el Inspector de cada prefab.
// ============================================================================
public static class EfectosJuego
{
    private static GameObject prefabLimpieza;
    private static AudioClip clipPop;

    static GameObject PrefabLimpieza
    {
        get
        {
            if (prefabLimpieza == null)
            {
                prefabLimpieza = Resources.Load<GameObject>("VFX_LimpiezaPasta");
            }
            return prefabLimpieza;
        }
    }

    static AudioClip ClipPop
    {
        get
        {
            if (clipPop == null)
            {
                clipPop = Resources.Load<AudioClip>("pop_bacteria");
            }
            return clipPop;
        }
    }

    // Explosión de partículas de limpieza. escala 1 = tamaño normal,
    // 0.3 = salpicadura pequeña (impacto contra pared)
    public static void ExplosionLimpieza(Vector3 posicion, float escala)
    {
        if (PrefabLimpieza == null) return;

        GameObject efecto = Object.Instantiate(PrefabLimpieza, posicion, Quaternion.identity);

        // Ajustamos el tamaño y velocidad de las partículas directamente,
        // porque escalar el transform no siempre afecta a los ParticleSystem
        if (!Mathf.Approximately(escala, 1f))
        {
            foreach (ParticleSystem ps in efecto.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = ps.main;
                main.startSizeMultiplier *= escala;
                main.startSpeedMultiplier *= escala;
            }
        }

        Object.Destroy(efecto, 2f);
    }

    // Salpicadura pequeña cuando la pasta choca contra paredes o dientes
    public static void SplatPasta(Vector3 posicion)
    {
        ExplosionLimpieza(posicion, 0.3f);
    }

    // Sonido pop con tono ajustable (pitch alto = sonido pequeño y agudo)
    public static void SonidoPop(Vector3 posicion, float volumen, float pitch)
    {
        if (ClipPop == null) return;

        GameObject objetoSonido = new GameObject("SonidoPop");
        objetoSonido.transform.position = posicion;

        AudioSource fuente = objetoSonido.AddComponent<AudioSource>();
        fuente.clip = ClipPop;
        fuente.volume = volumen;
        fuente.pitch = pitch;
        fuente.spatialBlend = 0f;
        fuente.Play();

        Object.Destroy(objetoSonido, ClipPop.length / Mathf.Max(pitch, 0.1f) + 0.1f);
    }
}
