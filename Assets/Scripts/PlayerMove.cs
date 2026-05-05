using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public float speed = 5f;
    private Animator animator;

    void Start()
    {
        GetComponentInChildren<Animator>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        // Только если это наш персонаж
        if (!IsOwner) return;

        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        if (inputHorizontal != 0 || inputVertical != 0)
        {
            Vector3 move = new Vector3(inputHorizontal, 0, inputVertical).normalized * speed * Time.deltaTime;
            transform.Translate(move);
        }
        if (IsOwner)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = Camera.main.transform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * inputVertical + cameraRight * inputHorizontal).normalized;
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                animator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            }
        }

    }
}