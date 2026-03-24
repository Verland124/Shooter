using Mirror;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar] public float currentHealth = 100f;
    public float maxHealth = 100f;
    public float punchDamage = 15f;

    [Header("Эффекты")]
    public GameObject hitEffect;

    private bool isDead = false;

    void Update()
    {
        if (!isLocalPlayer) return;

        if (currentHealth <= 0 && !isDead)
        {
            CmdDie();
        }
    }

    [Server]
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        RpcUpdateHealth(currentHealth);

        if (currentHealth <= 0)
        {
            CmdDie();
        }
    }

    [Server]
    void CmdDie()
    {
        isDead = true;
        RpcOnDeath();

        // Респавн через 3 секунды
        Invoke(nameof(Respawn), 3f);
    }

    [Server]
    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        RpcUpdateHealth(currentHealth);

        // Телепорт на точку спавна
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawns.Length > 0)
        {
            Transform spawn = spawns[Random.Range(0, spawns.Length)].transform;
            transform.position = spawn.position;
        }

        RpcOnRespawn();
    }

    [ClientRpc]
    void RpcUpdateHealth(float health)
    {
        currentHealth = health;

        if (isLocalPlayer)
        {
            GameUI gameUI = FindObjectOfType<GameUI>();
            if (gameUI != null) gameUI.UpdateHealth(health, maxHealth);
        }
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Вы умерли!");
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Death");
    }

    [ClientRpc]
    void RpcOnRespawn()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Вы воскресли!");
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Respawn");
    }
}