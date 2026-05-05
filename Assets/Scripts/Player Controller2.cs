using UnityEngine;
using Fusion;

public class PlayerController2 : NetworkBehaviour
{
    private NetworkCharacterController _cc;

    [Tooltip("Скорость передвижения")]
    public float speed = 5f;

    public override void Spawned()
    {
        // Получаем ссылку на компонент при появлении игрока в сети
        _cc = GetComponent<NetworkCharacterController>();
    }
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 direction;
    }
    // В Fusion 2 для движения используется FixedUpdateNetwork вместо обычного Update
    public override void FixedUpdateNetwork()
    {
        // Проверяем, имеем ли мы право управлять этим конкретным персонажем
        if (GetInput(out NetworkInputData data))
        {
            Vector3 dir = data.direction;
            dir.Normalize();
            _cc.Move(speed * dir * Runner.DeltaTime);
        }
    }
}