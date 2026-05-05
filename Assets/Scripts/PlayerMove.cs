using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine; // Если используешь Cinemachine 3.x

public class PlayerMove : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController controller;
    private Animator animator;
    private NetworkAnimator networkAnimator;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Ищем аниматор в дочерних объектах, так как модель обычно внутри
        animator = GetComponentInChildren<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();

        // Привязываем камеру только для владельца
        if (IsOwner)
        {
            var vcam = GameObject.FindObjectOfType<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Follow = transform;
                vcam.LookAt = transform;
            }
        }
    }

    void Update()
    {
        // Выполняем код только для своего персонажа
        if (!IsOwner) return;

        HandleMovement();
        HandleJumpAndGravity();
    }

    private void HandleMovement()
    {
        // 1. Считываем ввод
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.LeftControl);

        // 2. Рассчитываем направление относительно камеры
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 move = (cameraForward * z + cameraRight * x).normalized;

        // 3. Выбираем скорость
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        if (isCrouching) currentSpeed = walkSpeed * 0.5f;

        // 4. Двигаем персонажа
        if (move.magnitude > 0.1f)
        {
            controller.Move(move * currentSpeed * Time.deltaTime);

            // Плавный поворот лицом в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 5. Передаем данные в Аниматор
        // Если стоим - 0, идем - 1, бежим - 2
        float animSpeed = move.magnitude * (isRunning ? 2f : 1f);
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleJumpAndGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое прижатие к земле
        }

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // Используем NetworkAnimator для синхронизации триггера прыжка
            if (networkAnimator != null)
                networkAnimator.SetTrigger("Jump");
            else
                animator.SetTrigger("Jump");
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}