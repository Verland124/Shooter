using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Камеры")]
    [SerializeField] private Transform player;           // Ссылка на персонажа
    [SerializeField] private Transform cameraThird;      // Точка для камеры от 3-го лица
    [SerializeField] private Transform cameraFirst;      // Точка для камеры от 1-го лица
    [SerializeField] private Camera playerCamera;       // Сама камера

    [Header("Настройки 3-го лица")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -3); // Отступ от персонажа
    [SerializeField] private float distance = 3f;        // Расстояние от персонажа
    [SerializeField] private float height = 1.5f;        // Высота над персонажем

    [Header("Настройки мыши")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Ограничения")]
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Переключение")]
    [SerializeField] private KeyCode switchKey = KeyCode.V;

    // Переменные для управления
    private bool isFirstPerson = false;
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 currentVelocity;

    // Для плавного переключения
    private Transform activeCameraPoint;

    void Start()
    {
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Устанавливаем начальную позицию камеры
        activeCameraPoint = cameraThird;

        // Инициализируем углы
        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;

        // Проверяем наличие компонентов
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // Переключение режимов
        if (Input.GetKeyDown(switchKey))
        {
            isFirstPerson = !isFirstPerson;
            OnSwitchCamera();
        }

        // Управление камерой мышью
        HandleMouseInput();

        // Обновление позиции и поворота
        UpdateCameraPosition();
    }

    void HandleMouseInput()
    {
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Обновляем углы
        currentX += mouseX;
        currentY -= mouseY;
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        // Поворачиваем камеру или персонажа в зависимости от режима
        if (isFirstPerson)
        {
            // В первом лице: поворачиваем персонажа по горизонтали
            player.rotation = Quaternion.Euler(0, currentX, 0);

            // Поворачиваем камеру по вертикали
            playerCamera.transform.localRotation = Quaternion.Euler(currentY, 0, 0);
        }
        else
        {
            // В третьем лице: камера вращается вокруг персонажа
            // Персонаж не поворачивается
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            playerCamera.transform.rotation = rotation;
        }
    }

    void UpdateCameraPosition()
    {
        if (isFirstPerson)
        {
            // Первое лицо: камера внутри головы персонажа
            if (cameraFirst != null)
            {
                playerCamera.transform.position = cameraFirst.position;
                // Поворот уже применён в HandleMouseInput
            }
            else
            {
                // Если нет точки привязки, ставим камеру на уровне глаз
                Vector3 headPosition = player.position + Vector3.up * 1.6f;
                playerCamera.transform.position = headPosition;
            }
        }
        else
        {
            // Третье лицо: камера за спиной персонажа
            // Вычисляем желаемую позицию
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = player.position + rotation * thirdPersonOffset;

            // Плавно двигаем камеру
            playerCamera.transform.position = Vector3.SmoothDamp(
                playerCamera.transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );

            // Камера всегда смотрит на персонажа
            playerCamera.transform.LookAt(player.position + Vector3.up * 1.5f);
        }
    }

    void OnSwitchCamera()
    {
        // При переключении сбрасываем плавность
        currentVelocity = Vector3.zero;

        // Сбрасываем углы, чтобы не было резкого скачка
        if (isFirstPerson)
        {
            // В первый раз переключаемся в 1-е лицо: выравниваем взгляд
            currentY = 0;
            player.rotation = Quaternion.Euler(0, currentX, 0);
        }
        else
        {
            // Переключаемся обратно в 3-е лицо: сохраняем текущий угол
            currentX = playerCamera.transform.eulerAngles.y;
            currentY = playerCamera.transform.eulerAngles.x;
        }
    }
}
