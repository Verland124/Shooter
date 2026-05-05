using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Только если это наш персонаж
        if (!IsOwner) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (x != 0 || z != 0)
        {
            Vector3 move = new Vector3(x, 0, z).normalized * speed * Time.deltaTime;
            transform.Translate(move);
        }
    }
}