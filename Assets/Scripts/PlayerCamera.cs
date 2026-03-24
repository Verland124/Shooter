using UnityEngine;
using Mirror;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 1.5f, -3);
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0);

    [Header("Настройки мыши")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Ограничения")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private KeyCode switchKey = KeyCode.V;

    private bool isFirstPerson = false;
    private float yaw = 0f;
    private float pitch = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera: нет компонента Camera на этом объекте!");
            enabled = false;
            return;
        }
        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = 0f;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(switchKey))
        {
            isFirstPerson = !isFirstPerson;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        if (isFirstPerson)
        {
            transform.position = transform.parent.position + firstPersonOffset;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
        else
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = transform.parent.position + rotation * thirdPersonOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );
            transform.LookAt(transform.parent.position + Vector3.up * 1.5f);
        }
    }
}