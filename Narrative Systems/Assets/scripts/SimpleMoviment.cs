using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 700f;

    void Update()
    {
        // Get keyboard input (W/S or Up/Down Arrow)
        float moveForward = Input.GetAxis("Vertical");
        
        // Get rotation input (A/D or Left/Right Arrow)
        float turn = Input.GetAxis("Horizontal");

        // Move character forward/backward
        transform.Translate(0, 0, moveForward * speed * Time.deltaTime);

        // Rotate character left/right
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }
}

