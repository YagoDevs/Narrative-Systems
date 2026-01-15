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

    [Tooltip("Multiplicador de velocidade ao segurar a tecla de corrida.")]
    public float sprintMultiplier = 1.6f;

    [Tooltip("Tecla usada para correr.")]
    public KeyCode sprintKey = KeyCode.LeftShift;

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

    [Header("Animação")]
    [Tooltip("Componente Animator do personagem. Deixe vazio para buscar automaticamente. Se o Animator estiver em um filho, ele será encontrado.")]
    public Animator animator;

    [Tooltip("Nome do parâmetro de velocidade no Animator (padrão: 'Speed').")]
    public string speedParameter = "Speed";

    [Tooltip("Nome do parâmetro de corrida no Animator (padrão: 'IsRunning').")]
    public string isRunningParameter = "IsRunning";

    private CharacterController controller;
    private float yaw;
    private float pitch;
    private float verticalSpeed;
    private bool isRunning;
    private float currentSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;

        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform;
        }

        // Tenta encontrar o Animator automaticamente se não foi atribuído
        if (animator == null)
        {
            // Primeiro tenta no mesmo GameObject
            animator = GetComponent<Animator>();
            
            // Se não encontrou, tenta nos filhos (onde geralmente está o modelo do personagem)
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true); // true = inclui objetos desabilitados
            }
            
            // Se encontrou, mostra uma mensagem
            if (animator != null)
            {
                Debug.Log($"MouseLookMovement: Animator encontrado automaticamente no GameObject '{animator.gameObject.name}'. " +
                         $"Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}, " +
                         $"Enabled: {animator.enabled}, Apply Root Motion: {animator.applyRootMotion}");
            }
            else
            {
                Debug.LogWarning($"MouseLookMovement: Animator não encontrado no GameObject '{gameObject.name}' nem em seus filhos! " +
                               "Certifique-se de que há um componente Animator em algum lugar da hierarquia do personagem.");
            }
        }
        else
        {
            // Se foi atribuído manualmente, verifica se está OK
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"MouseLookMovement: Animator foi atribuído mas não tem um Controller! " +
                               "Atribua um Animator Controller no Inspector do Animator.");
            }
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
        UpdateAnimations();
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

        // Calcula a magnitude do input (0 a 1)
        float inputMagnitude = Mathf.Clamp01(new Vector2(inputX, inputZ).magnitude);
        bool hasInput = inputMagnitude > 0.01f;

        // Calcula direção do movimento
        Vector3 move = Vector3.zero;
        if (hasInput)
        {
            move = (transform.forward * inputZ + transform.right * inputX).normalized;
        }

        // Calcula velocidade
        float speed = moveSpeed;
        isRunning = Input.GetKey(sprintKey) && hasInput;
        
        if (isRunning)
        {
            speed *= sprintMultiplier;
        }

        // Aplica gravidade
        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f; // mantém colado no chão
        }

        verticalSpeed += gravity * Time.deltaTime;

        // Move o personagem
        Vector3 velocity = move * speed;
        velocity.y = verticalSpeed;

        // IMPORTANTE: Se Apply Root Motion está ativado, o movimento pode vir das animações
        // Se está desativado, o CharacterController deve mover
        // Mas se o Animator está em um filho, precisamos garantir que o movimento funcione
        if (animator != null && animator.applyRootMotion)
        {
            // Com Root Motion, o movimento vem das animações
            // Mas ainda aplicamos o movimento do CharacterController para garantir
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            // Sem Root Motion, o CharacterController deve mover normalmente
            controller.Move(velocity * Time.deltaTime);
        }

        // Calcula velocidade para animação
        // IMPORTANTE: inputMagnitude já está normalizado (0 a 1), então multiplica pela velocidade
        // Garante que enquanto houver input, o Speed seja maior que 0
        if (hasInput)
        {
            // Multiplica pela velocidade para ter o valor real
            currentSpeed = inputMagnitude * speed;
        }
        else
        {
            // Só zera quando realmente não há input
            currentSpeed = 0f;
        }
    }

    private void UpdateAnimations()
    {
        // Se não tem Animator, não faz nada
        if (animator == null) return;
        
        // Verifica se Animator está habilitado
        if (!animator.enabled) return;

        // Calcula velocidade normalizada (0 a 1)
        // IMPORTANTE: Normaliza sempre baseado na velocidade máxima (corrida) para consistência
        float maxSpeed = moveSpeed * sprintMultiplier;
        float normalizedSpeed = 0f;
        
        if (currentSpeed > 0.01f && maxSpeed > 0f)
        {
            // Normaliza baseado na velocidade máxima
            normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
            
            // CRÍTICO: Garante que enquanto houver movimento, o Speed seja sempre >= 0.2
            // O Animator tem transição Walk → Idle quando Speed < 0.05
            // E Idle → Walk quando Speed > 0.15
            // Para evitar oscilações, garante que quando anda, o Speed seja sempre >= 0.2
            if (normalizedSpeed > 0f && normalizedSpeed < 0.2f)
            {
                normalizedSpeed = 0.2f; // Mínimo quando está andando
            }
        }
        else
        {
            normalizedSpeed = 0f;
        }

        // Atualiza os parâmetros do Animator a cada frame
        animator.SetFloat(speedParameter, normalizedSpeed);
        animator.SetBool(isRunningParameter, isRunning);
    }
}

