using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// ============================================================================
// TEXTURIZADOR PROCEDURAL v2
// Crea materiales completos en runtime (con textura generada por código) y
// los asigna DIRECTAMENTE a cada renderer de la escena, según el nombre del
// material que tenga asignado. Se ejecuta en cada carga de escena.
//
// Esto es a prueba de todo: no depende de que Unity importe texturas desde
// disco ni de modificar los assets de material compartidos.
// ============================================================================
public class TexturizadorAmbiente : MonoBehaviour
{
    private static bool inicializado = false;

    private static Material matBacteria;
    private static Material matDiente;
    private static Material matLengua;      // con papilas: para el piso (Plane)
    private static Material matLenguaLisa;  // suave sin papilas: para las mallas grandes de lengua

    // Materiales del pipeline viejo (Standard) ya convertidos a URP,
    // para no convertir el mismo dos veces
    private static readonly Dictionary<Material, Material> convertidos = new Dictionary<Material, Material>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Inicializar()
    {
        if (inicializado) return;
        inicializado = true;

        SceneManager.sceneLoaded += AlCargarEscena;
        AplicarATodaLaEscena();
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AplicarATodaLaEscena();
    }

    static void AplicarATodaLaEscena()
    {
        CrearMaterialesSiFaltan();

        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in renderers)
        {
            Material[] materiales = r.sharedMaterials;
            bool huboCambio = false;

            for (int i = 0; i < materiales.Length; i++)
            {
                Material m = materiales[i];
                if (m == null) continue;

                string nombre = m.name;

                if (nombre.StartsWith("VerdeBacteria") || nombre.StartsWith("MaterialBacteria"))
                {
                    materiales[i] = matBacteria;
                    huboCambio = true;
                }
                else if (nombre.StartsWith("BlancoDiente"))
                {
                    materiales[i] = matDiente;
                    huboCambio = true;
                }
                else if (nombre.StartsWith("RojoLengua"))
                {
                    materiales[i] = matLengua;
                    huboCambio = true;
                }
                else if (nombre.StartsWith("Color_Lengua"))
                {
                    // Las mallas grandes de lengua (Lengua_Completa, Lados)
                    // usan la versión LISA: la textura de papilas se deforma
                    // feo sobre geometría enorme con UVs de FBX
                    materiales[i] = matLenguaLisa;
                    huboCambio = true;
                }
                else if (NecesitaConversionUrp(m))
                {
                    // Material del pipeline viejo (se veía MAGENTA/rosado):
                    // lo convertimos a URP conservando su textura y color
                    if (!convertidos.ContainsKey(m))
                    {
                        convertidos[m] = ConvertirAUrp(m);
                    }

                    materiales[i] = convertidos[m];
                    huboCambio = true;
                }
                else if (nombre == "Lit" || nombre == "Default-Material")
                {
                    // Objetos con el material gris por defecto: les damos la
                    // textura que corresponde por su nombre, para que TODOS
                    // los niveles se vean igual de vestidos
                    string nombreObjeto = r.gameObject.name;

                    if (nombreObjeto.StartsWith("Plane"))
                    {
                        materiales[i] = matLengua;
                        huboCambio = true;
                    }
                    else if (nombreObjeto.StartsWith("Cube"))
                    {
                        materiales[i] = matDiente;
                        huboCambio = true;
                    }
                }
            }

            if (huboCambio)
            {
                r.sharedMaterials = materiales;
            }
        }
    }

    // Detecta materiales con shaders del pipeline viejo, que URP no puede
    // dibujar (los pinta magenta/rosado)
    static bool NecesitaConversionUrp(Material m)
    {
        if (m.shader == null) return false;

        string nombreShader = m.shader.name;

        return nombreShader == "Standard"
            || nombreShader.StartsWith("Legacy Shaders/")
            || nombreShader == "Hidden/InternalErrorShader"
            || !m.shader.isSupported;
    }

    static Material ConvertirAUrp(Material original)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            return original; // no hay URP: dejarlo como está
        }

        Material nuevo = new Material(shader);
        nuevo.name = original.name + "_URP";

        // Rescatar la textura principal del material viejo
        Texture textura = null;

        if (original.HasProperty("_MainTex")) textura = original.GetTexture("_MainTex");
        if (textura == null && original.HasProperty("_BaseMap")) textura = original.GetTexture("_BaseMap");

        if (textura != null)
        {
            nuevo.SetTexture("_BaseMap", textura);
            nuevo.SetTexture("_MainTex", textura);
        }

        // Rescatar el color base
        Color color = Color.white;

        if (original.HasProperty("_Color")) color = original.GetColor("_Color");
        else if (original.HasProperty("_BaseColor")) color = original.GetColor("_BaseColor");

        nuevo.SetColor("_BaseColor", color);
        nuevo.SetColor("_Color", color);

        return nuevo;
    }

    static void CrearMaterialesSiFaltan()
    {
        if (matBacteria != null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard"); // por si el proyecto no fuera URP
        }

        matBacteria = CrearMaterial(shader, GenerarTexturaBacteria(256), 1f, 0.35f);
        matDiente = CrearMaterial(shader, GenerarTexturaDiente(256), 1f, 0.55f);
        matLengua = CrearMaterial(shader, GenerarTexturaLengua(256), 6f, 0.45f);
        matLenguaLisa = CrearMaterial(shader, GenerarTexturaLenguaLisa(256), 1f, 0.5f);
    }

    static Material CrearMaterial(Shader shader, Texture2D textura, float tiling, float suavidad)
    {
        Material m = new Material(shader);
        m.name = "Runtime_" + textura.name;

        // URP usa _BaseMap; Standard usa _MainTex. Cubrimos ambos.
        if (m.HasProperty("_BaseMap"))
        {
            m.SetTexture("_BaseMap", textura);
            m.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
        }

        if (m.HasProperty("_MainTex"))
        {
            m.SetTexture("_MainTex", textura);
            m.SetTextureScale("_MainTex", new Vector2(tiling, tiling));
        }

        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
        if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", suavidad);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", suavidad);

        return m;
    }

    // ---------- Generadores de texturas ----------

    static Texture2D GenerarTexturaBacteria(int tam)
    {
        Color baseColor = new Color(0.43f, 0.75f, 0.31f);
        Color[] pixeles = LlenarFondo(tam, baseColor);

        System.Random rnd = new System.Random(1);

        // Moteado de fondo (manchas suaves de verde variado)
        for (int i = 0; i < 40; i++)
        {
            int cx = rnd.Next(tam), cy = rnd.Next(tam);
            int r = rnd.Next(20, 45);
            Color c = new Color(baseColor.r * 0.85f, Mathf.Min(baseColor.g * 1.1f, 1f), baseColor.b * 0.85f);
            PintarCirculo(pixeles, tam, cx, cy, r, c, 0.25f);
        }

        // Organelos oscuros
        for (int i = 0; i < 14; i++)
        {
            int cx = rnd.Next(tam), cy = rnd.Next(tam);
            int r = rnd.Next(10, 22);
            PintarCirculo(pixeles, tam, cx, cy, r, new Color(0.22f, 0.45f, 0.18f), 0.9f);
        }

        // Núcleo central grande
        PintarCirculo(pixeles, tam, (int)(tam * 0.62f), (int)(tam * 0.58f), (int)(tam * 0.15f), new Color(0.16f, 0.35f, 0.14f), 0.95f);

        // Brillos pequeños
        for (int i = 0; i < 60; i++)
        {
            int cx = rnd.Next(tam), cy = rnd.Next(tam);
            PintarCirculo(pixeles, tam, cx, cy, rnd.Next(1, 3), new Color(0.75f, 0.95f, 0.6f), 0.5f);
        }

        return CrearTextura(tam, pixeles, "Textura_Bacteria");
    }

    static Texture2D GenerarTexturaDiente(int tam)
    {
        Color[] pixeles = new Color[tam * tam];

        for (int y = 0; y < tam; y++)
        {
            float f = (float)y / tam;
            Color fila = Color.Lerp(new Color(0.98f, 0.97f, 0.93f), new Color(0.86f, 0.80f, 0.65f), f * 0.3f);

            for (int x = 0; x < tam; x++)
            {
                float ruido = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.06f;
                pixeles[y * tam + x] = fila + new Color(ruido, ruido, ruido * 0.8f);
            }
        }

        return CrearTextura(tam, pixeles, "Textura_Diente");
    }

    static Texture2D GenerarTexturaLengua(int tam)
    {
        Color baseColor = new Color(0.78f, 0.36f, 0.4f);
        Color[] pixeles = LlenarFondo(tam, baseColor);

        int columnas = 18;
        int paso = tam / columnas;
        System.Random rnd = new System.Random(2);

        // Rejilla de papilas (sombra + centro claro), con jitter
        for (int fy = 0; fy < columnas; fy++)
        {
            for (int fx = 0; fx < columnas; fx++)
            {
                int cx = fx * paso + paso / 2 + rnd.Next(-3, 4);
                int cy = fy * paso + paso / 2 + rnd.Next(-3, 4);
                int r = Mathf.Max(paso / 3, 2);

                PintarCirculo(pixeles, tam, cx + 1, cy + 1, r, new Color(0.55f, 0.2f, 0.24f), 0.6f);
                PintarCirculo(pixeles, tam, cx, cy, Mathf.Max(r - 1, 1), new Color(0.88f, 0.5f, 0.53f), 0.75f);
            }
        }

        return CrearTextura(tam, pixeles, "Textura_Lengua");
    }

    // Lengua lisa: solo el color carne con moteado muy suave, sin papilas.
    // Pensada para mallas grandes donde cualquier patrón repetido se deforma.
    static Texture2D GenerarTexturaLenguaLisa(int tam)
    {
        Color baseColor = new Color(0.8f, 0.4f, 0.44f);
        Color[] pixeles = new Color[tam * tam];

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                // Moteado orgánico suave con ruido Perlin, sin bordes duros
                float ruido = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                Color c = Color.Lerp(baseColor, new Color(0.87f, 0.5f, 0.53f), ruido * 0.5f);
                pixeles[y * tam + x] = c;
            }
        }

        return CrearTextura(tam, pixeles, "Textura_LenguaLisa");
    }

    // ---------- Utilidades de dibujo ----------

    static Color[] LlenarFondo(int tam, Color color)
    {
        Color[] pixeles = new Color[tam * tam];
        for (int i = 0; i < pixeles.Length; i++) pixeles[i] = color;
        return pixeles;
    }

    static void PintarCirculo(Color[] pixeles, int tam, int cx, int cy, int radio, Color color, float intensidad)
    {
        int r2 = radio * radio;

        for (int y = -radio; y <= radio; y++)
        {
            int py = cy + y;
            if (py < 0 || py >= tam) continue;

            for (int x = -radio; x <= radio; x++)
            {
                int px = cx + x;
                if (px < 0 || px >= tam) continue;
                if (x * x + y * y > r2) continue;

                int idx = py * tam + px;
                pixeles[idx] = Color.Lerp(pixeles[idx], color, intensidad);
            }
        }
    }

    static Texture2D CrearTextura(int tam, Color[] pixeles, string nombre)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, true);
        tex.name = nombre;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(pixeles);
        tex.Apply();
        return tex;
    }
}
