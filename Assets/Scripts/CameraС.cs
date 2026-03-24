using UnityEngine;

public class CameraC : MonoBehaviour
{
    [Header("Настройки мыши")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Ограничения")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private KeyCode switchKey = KeyCode.V;

    [Header("Позиция")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -3);
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0);

    private Transform player;
    private bool isFirstPerson = false;
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;

        if (player != null)
        {
            yaw = player.eulerAngles.y;
            pitch = 0f;
            Debug.Log($"CameraC: Привязана к игроку {player.name}");
        }
    }

    void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(switchKey))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log($"Режим камеры: {(isFirstPerson ? "1 лицо" : "3 лицо")}");
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        if (isFirstPerson)
        {
            transform.position = player.position + firstPersonOffset;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
        else
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = player.position + rotation * thirdPersonOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );
            transform.LookAt(player.position + Vector3.up * 1.5f);
        }
    }
}