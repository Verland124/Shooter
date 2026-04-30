using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f; // Скорость в приседе
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Height Settings")]
    public float normalHeight = 2f;  // Обычный рост
    public float crouchHeight = 1f;  // Рост при нажатом Ctrl

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;

    [Header("Camera Modes")]
    public GameObject firstPersonCam;
    public GameObject thirdPersonCam;
    private bool isFirstPerson = true;

    [Header("References")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!isLocalPlayer)
        {
            if (cameraHolder != null) cameraHolder.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        UpdateCameraMode();
    }

    void Update()
    {
        HandleRotation();
        HandleCrouch(); // Добавили вызов метода приседания
        HandleJump();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            UpdateCameraMode();
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Выбираем скорость: если присел — crouchSpeed, если Shift — runSpeed, иначе walkSpeed
        float currentSpeed = walkSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        else if (Input.GetKey(KeyCode.LeftShift)) currentSpeed = runSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat("Speed", move.magnitude * (currentSpeed / runSpeed), 0.1f, Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // НОВЫЙ МЕТОД ДЛЯ ПРИСЕДАНИЯ
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            controller.height = crouchHeight; // Уменьшаем колайдер
            if (animator != null) animator.SetBool("isCrouching", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            controller.height = normalHeight; // Возвращаем рост
            if (animator != null) animator.SetBool("isCrouching", false);
        }
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (!isFirstPerson && Input.GetMouseButton(1))
        {
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -40f, 40f);
            cameraHolder.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            transform.Rotate(Vector3.up * mouseX);
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            yRotation = Mathf.Lerp(yRotation, 0, Time.deltaTime * 5f);
            cameraHolder.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }

    void HandleJump()
    {
        // Не даем прыгать, если игрок присел
        if (Input.GetButtonDown("Jump") && controller.isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger("Jump");
        }
    }

    void UpdateCameraMode()
    {
        if (firstPersonCam != null) firstPersonCam.SetActive(isFirstPerson);
        if (thirdPersonCam != null) thirdPersonCam.SetActive(!isFirstPerson);
        yRotation = 0;
    }
}