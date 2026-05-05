using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine; // Именно так для версии 3.x

public class CameraFollow : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Сработает только у того, кто управляет этим персонажем
        if (IsOwner)
        {
            var vcam = GameObject.FindObjectOfType<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Follow = transform;
                vcam.LookAt = transform;
                Debug.Log("Камера нашла хозяина!");
            }
        }
    }
}