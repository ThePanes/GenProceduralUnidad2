using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraMovement : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public float velocidad = 5f;
    public float minX = -10f;
    public float maxX = 10f;

    private float inputX = 0f;

    void Update()
    {
        // Lee el valor del eje X usando el nuevo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                inputX = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                inputX = 1f;
            else
                inputX = 0f;
        }

        float movimiento = inputX * velocidad * Time.deltaTime;
        float nuevaX = Mathf.Clamp(transform.position.x + movimiento, minX, maxX);
        transform.position = new Vector3(nuevaX, transform.position.y, transform.position.z);
    }
}
