using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraMovement : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public float velocidad = 10f;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = 10f;
    public float maxY = 50f;
    public float minZ = -10f;
    public float maxZ = 10f;

    private float inputX = 0f;
    private float inputY = 0f;
    private float inputZ = 0f;

    void Update()
    {
        inputX = 0f;
        inputY = 0f;
        inputZ = 0f;

        // Lee el valor de teclado usando el nuevo Input System
        if (Keyboard.current != null)
        {
            // Movimiento horizontal (A/D o flechas izquierda/derecha)
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                inputX = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                inputX = 1f;

            // Q / E para cambiar altura (eje Y): Q baja, E sube
            if (Keyboard.current.qKey.isPressed)
                inputY = -1f;
            else if (Keyboard.current.eKey.isPressed)
                inputY = 1f;

            // Flechas arriba/abajo para mover en Z: arriba aumenta Z, abajo disminuye Z
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                inputZ = 1f;
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                inputZ = -1f;
        }

        Vector3 delta = new Vector3(inputX, inputY, inputZ) * velocidad * Time.deltaTime;

        float nuevaX = Mathf.Clamp(transform.position.x + delta.x, minX, maxX);
        float nuevaY = Mathf.Clamp(transform.position.y + delta.y, minY, maxY);
        float nuevaZ = Mathf.Clamp(transform.position.z + delta.z, minZ, maxZ);

        transform.position = new Vector3(nuevaX, nuevaY, nuevaZ);
    }
}
