using UnityEngine;
using System.Collections;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 6f;
    public float gravedad = -9.81f;

    [Header("Configuración Súper Salto")]
    public float fuerzaSaltoBase = 5f;       // La fuerza del primer salto
    public float multiplicadorPorSalto = 1.5f; // Cuanta más fuerza ganas por cada espacio extra
    public int saltosMaximos = 2;            // 1 salto desde el suelo + 1 en el aire (doble salto)
    private int contadorSaltos = 0;          // Cuenta los saltos seguidos que llevas

    [Header("Cámara")]
    public float sensibilidadRaton = 2f;

    [Header("Dash")]
    public float velocidadDash = 20f;
    public float duracionDash = 0.2f;
    public float cooldownDash = 1f;

    private CharacterController controller;
    private Transform camara;

    private float rotacionX = 0f;
    private float velocidadY;

    private bool puedeDash = true;
    private bool haciendoDash = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camara = GetComponentInChildren<Camera>()?.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controller == null)
        {
            Debug.LogError("Falta el CharacterController en el jugador.");
        }

        if (camara == null)
        {
            Debug.LogError("Falta una cámara como hija del jugador.");
        }
    }

    void Update()
    {
        MoverCamara();

        if (controller == null) return;

        // RESET DE CONTADOR AL TOCAR EL SUELO
        if (controller.isGrounded)
        {
            if (contadorSaltos > 0)
            {
                contadorSaltos = 0; // Volvemos a empezar desde cero al pisar tierra
                Debug.Log("Reset de saltos acumulados.");
            }
        }

        // DETECTAR LA TECLA ESPACIO PARA EL SÚPER SALTO
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaltarAcumulativo();
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 direccionMovimiento = transform.right * x + transform.forward * z;

        if (direccionMovimiento.magnitude > 1f)
        {
            direccionMovimiento.Normalize();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && puedeDash)
        {
            StartCoroutine(HacerDash(direccionMovimiento));
        }

        if (!haciendoDash)
        {
            MoverJugador(direccionMovimiento);
        }
    }

    void SaltarAcumulativo()
    {
        // Sin este límite se podía "volar" pulsando espacio infinitamente
        if (contadorSaltos >= saltosMaximos) return;

        contadorSaltos++;

        // Calculamos la nueva fuerza usando la fórmula matemática que acumula los saltos
        float fuerzaFinal = fuerzaSaltoBase + (contadorSaltos * multiplicadorPorSalto);
        
        // Reemplazamos la velocidad vertical directamente con el nuevo impulso hacia arriba
        velocidadY = fuerzaFinal;

        Debug.Log("¡Salto consecutivo #" + contadorSaltos + "! Fuerza: " + fuerzaFinal);
    }

    void MoverJugador(Vector3 direccionMovimiento)
    {
        if (controller.isGrounded && velocidadY < 0)
        {
            velocidadY = -2f;
        }

        velocidadY += gravedad * Time.deltaTime;

        Vector3 velocidadFinal = direccionMovimiento * velocidad;
        velocidadFinal.y = velocidadY;

        controller.Move(velocidadFinal * Time.deltaTime);
    }

    void MoverCamara()
    {
        if (camara == null) return;

        float ratonX = Input.GetAxis("Mouse X") * sensibilidadRaton;
        float ratonY = Input.GetAxis("Mouse Y") * sensibilidadRaton;

        transform.Rotate(Vector3.up * ratonX);

        rotacionX -= ratonY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        camara.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
    }

    IEnumerator HacerDash(Vector3 direccion)
    {
        puedeDash = false;
        haciendoDash = true;

        // Golpe de FOV: la cámara "abre" el campo de visión durante el dash
        EfectosCamara.AvisarDash(duracionDash + 0.15f);

        if (direccion.sqrMagnitude <= 0.01f)
        {
            direccion = transform.forward;
        }

        direccion.y = 0f;
        direccion.Normalize();

        float tiempoInicio = Time.time;

        while (Time.time < tiempoInicio + duracionDash)
        {
            controller.Move(direccion * velocidadDash * Time.deltaTime);
            yield return null;
        }

        haciendoDash = false;

        yield return new WaitForSeconds(cooldownDash);

        puedeDash = true;
    }
}