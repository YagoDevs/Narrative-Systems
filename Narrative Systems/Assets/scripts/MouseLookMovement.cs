using UnityEngine;

/// <summary>
/// Move com WASD enquanto a rotação vem somente do mouse.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MouseLookMovement : MonoBehaviour
{
    [Header("Movimento")]
    [Tooltip("Velocidade de deslocamento em m/s.")]
    public float moveSpeed = 5f;

    [Tooltip("Força da gravidade usada para manter o personagem no chão.")]
    public float gravity = -20f;

    [Header("Mouse")]
    [Tooltip("Sensibilidade da rotação horizontal/vertical.")]
    public float mouseSensitivity = 180f;

    [Tooltip("Transform usado para inclinar a câmera no eixo X (pitch).")]
    public Transform cameraPivot;

    [Tooltip("Limite de ângulo para olhar para cima/baixo.")]
    public Vector2 pitchClamp = new Vector2(-80f, 80f);

    [Tooltip("Travará o cursor enquanto o script estiver habilitado.")]
    public bool lockCursor = true;

    private CharacterController controller;
    private float yaw;
    private float pitch;
    private float verticalSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;

        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnDisable()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        RotateWithMouse();
        MoveWithKeyboard();
    }

    private void RotateWithMouse()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch - mouseY, pitchClamp.x, pitchClamp.y);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void MoveWithKeyboard()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.forward * inputZ + transform.right * inputX).normalized;

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f; // mantém colado no chão
        }

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalSpeed;

        controller.Move(velocity * Time.deltaTime);
    }
}

