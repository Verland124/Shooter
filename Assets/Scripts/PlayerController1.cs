using Mirror;
using Mirror.Examples.Common;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController1 : NetworkBehaviour
{
    private AudioSource footstepAudioSource;

    [SyncVar] public string playerName;

    [Header("Движение")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Поворот")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Звуки")]
    [SerializeField] private AudioClip footstepSound;

    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    [Header("Приседание")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [Header("Рукопашный бой")]
    [SerializeField] private float punchRange = 2f;
    [SerializeField] private float punchDamage = 15f;
    [SerializeField] private float punchCooldown = 0.5f;
    private float lastPunchTime = 0f;
    private Camera playerCamera;
    private bool isAttacking = false;


    private bool isCrouching = false;
    private float currentHeight;



    private Vector3 velocity;
    private float currentSpeed;
    private bool isGrounded;
    private bool isRunning;
    private Vector3 moveDirection;
    private float footstepTimer;

    [SyncVar(hook = nameof(OnSyncPositionChanged))]
    private Vector3 syncPosition;

    [SyncVar(hook = nameof(OnSyncRotationChanged))]
    private Quaternion syncRotation;

    [SyncVar(hook = nameof(OnSyncAnimatorChanged))]
    private Vector2 syncAnimatorParams;

    [SyncVar(hook = nameof(OnSyncRunningChanged))]
    private bool syncRunning;

    [SyncVar(hook = nameof(OnSyncGroundedChanged))]
    private bool syncGrounded;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (footstepSound != null)
            audioSource.clip = footstepSound;

        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }
        currentHeight = standHeight;
        controller.height = standHeight;
        if (isLocalPlayer)
        {
            // Загружаем ник из сохранения
            playerName = PlayerPrefsManager.LoadPlayerName();
            CmdSetPlayerName(playerName);
        }
        footstepAudioSource = gameObject.AddComponent<AudioSource>();
        footstepAudioSource.clip = footstepSound;
        footstepAudioSource.loop = true; // не включаем loop
        footstepAudioSource.playOnAwake = false;
    }
    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;
        Debug.Log($"Игрок {name} подключился");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isAttacking)
        {
            isCrouching = !isCrouching;
            animator.SetBool("Crouch_b", isCrouching);
            Debug.Log($"Приседание: {isCrouching}"); // для проверки
        }
        if (!isLocalPlayer) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift) && isGrounded;
        currentSpeed = isRunning ? runSpeed : walkSpeed;
        moveDirection = transform.right * horizontal + transform.forward * vertical;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpForce;
            animator.SetTrigger("Jump_trig");
            CmdJump();

        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        controller.Move(finalMove * Time.deltaTime);

        CmdSendPosition(transform.position);
        CmdSendRotation(transform.rotation);

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        CmdSendAnimatorParams(moveX, moveZ, isRunning, isGrounded);

        UpdateAnimations(moveDirection.magnitude);
        UpdateFootsteps(moveDirection.magnitude);

        void UpdateFootsteps(float speedMagnitude)
        {
            bool isMoving = isGrounded && speedMagnitude > 0.1f;

            if (!isMoving)
            {
                if (footstepAudioSource.isPlaying)
                    footstepAudioSource.Stop();
                footstepTimer = 0f;
                return;
            }

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f && !footstepAudioSource.isPlaying)
            {
                float stepDelay = isRunning ? 0.35f : 0.55f;
                footstepTimer = stepDelay;
                footstepAudioSource.Play();
            }
        }
        if (Input.GetMouseButtonDown(0) && Time.time >= lastPunchTime + punchCooldown && !isAttacking)
        {
            lastPunchTime = Time.time;
            CmdPunch();
        }


    }
    [Command]
    void CmdPunch()
    {
        // Начинаем кулдаун на сервере
        StartCoroutine(AttackCooldown());

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, punchRange))
        {
            PlayerHealth target = hit.collider.GetComponent<PlayerHealth>();
            if (target != null && target != this)
            {
                target.TakeDamage(punchDamage);
                RpcPlayPunchEffect(hit.point);
            }
        }

        // Анимация удара на всех клиентах
        RpcPlayPunchAnimation();
    }

    IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f); // подбери время под твою анимацию
        isAttacking = false;
    }

    [ClientRpc]
    void RpcPlayPunchAnimation()
    {
        animator.SetTrigger("Punch");
        StartCoroutine(ResetAttack());
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    public void OnAttackStart()
    {
        isAttacking = true;
    }

    // Вызывается из анимации в конце удара
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
    [ClientRpc]
    void RpcPlayPunchEffect(Vector3 point)
    {
        // Эффект удара (частицы)
        Debug.Log($"Удар в {point}");
    }

    [Command]
    private void CmdSendPosition(Vector3 pos)
    {
        syncPosition = pos;
    }

    [Command]
    private void CmdSendRotation(Quaternion rot)
    {
        syncRotation = rot;
    }

    [Command]
    private void CmdSendAnimatorParams(float moveX, float moveZ, bool running, bool grounded)
    {
        syncAnimatorParams = new Vector2(moveX, moveZ);
        syncRunning = running;
        syncGrounded = grounded;
    }

    [Command]
    private void CmdJump()
    {
        RpcPlayJumpAnimation();
    }

    [ClientRpc]
    private void RpcPlayJumpAnimation()
    {
        if (!isLocalPlayer)
        {
            animator.SetTrigger("Jump_trig");
        }
    }

    private void OnSyncPositionChanged(Vector3 oldPos, Vector3 newPos)
    {
        if (!isLocalPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 15f);
        }
    }

    private void OnSyncRotationChanged(Quaternion oldRot, Quaternion newRot)
    {
        if (!isLocalPlayer)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * 15f);
        }
    }

    private void OnSyncAnimatorChanged(Vector2 oldParams, Vector2 newParams)
    {
        if (!isLocalPlayer)
        {
            animator.SetFloat("MoveX", newParams.x);
            animator.SetFloat("MoveZ", newParams.y);
        }
    }

    private void OnSyncRunningChanged(bool oldVal, bool newVal)
    {
        if (!isLocalPlayer)
        {
            animator.SetBool("Running", newVal);
        }
    }

    private void OnSyncGroundedChanged(bool oldVal, bool newVal)
    {
        if (!isLocalPlayer)
        {
            animator.SetBool("Grounded", newVal);
        }
    }

    void UpdateAnimations(float speedMagnitude)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isGrounded;

        // Для локального игрока устанавливаем параметры напрямую
        if (isLocalPlayer)
        {
            if (speedMagnitude > 0.1f)
            {
                animator.SetFloat("MoveX", horizontal);
                animator.SetFloat("MoveZ", vertical);
                animator.SetBool("Running", isRunning);
            }
            else
            {
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveZ", 0);
            }

            animator.SetBool("Grounded", isGrounded);
            animator.SetBool("Crouch_b", isCrouching);
        }
    }
    [TargetRpc]
    public void TargetPlayerDisconnected(NetworkConnection target, string playerName)
    {
        Debug.Log($"Игрок {playerName} отключился");
    }
    public override void OnStartLocalPlayer()
    {
        // Находим камеру на сцене
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Получаем скрипт камеры
            CameraC cameraController = mainCamera.GetComponent<CameraC>();

            if (cameraController != null)
            {
                // Передаём ссылку на себя
                cameraController.SetPlayer(transform);
                Debug.Log("PlayerController: Камера привязана к локальному игроку");
            }
            else
            {
                Debug.LogError("На MainCamera нет компонента CameraC!");
            }
        }
        else
        {
            Debug.LogError("Camera.main не найден! Убедись, что у камеры тег MainCamera");
        }
    }
}