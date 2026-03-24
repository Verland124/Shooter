using Mirror;
using UnityEngine;

public class SurvivalSystem : NetworkBehaviour
{
    [Header("Статы")]
    [SyncVar] public float health = 100f;
    [SyncVar] public float hunger = 100f;
    [SyncVar] public float thirst = 100f;

    [Header("Настройки")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    [Header("Скорость траты")]
    public float hungerRate = 0.1f;     // трата голода в секунду
    public float thirstRate = 0.15f;    // трата жажды в секунду

    [Header("Урон от голода/жажды")]
    public float hungerDamage = 0.5f;   // урон в секунду
    public float thirstDamage = 0.7f;

    [Header("Ссылки")]
    [SerializeField] private GameUI gameUI;

    void Start()
    {
        if (!isServer) return;

        // Находим UI
        if (gameUI == null)
            gameUI = FindObjectOfType<GameUI>();
    }

    void Update()
    {
        if (!isServer) return;

        // Трата голода и жажды
        hunger -= hungerRate * Time.deltaTime;
        thirst -= thirstRate * Time.deltaTime;

        // Ограничиваем значения
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);

        // Урон от голода/жажды
        if (hunger <= 0)
        {
            health -= hungerDamage * Time.deltaTime;
        }
        if (thirst <= 0)
        {
            health -= thirstDamage * Time.deltaTime;
        }

        // Ограничиваем здоровье
        health = Mathf.Clamp(health, 0, maxHealth);

        // Смерть
        if (health <= 0)
        {
            Die();
        }

        // Обновляем UI на всех клиентах
        RpcUpdateUI(health, hunger, thirst);
    }

    [Server]
    public void AddFood(float amount)
    {
        hunger = Mathf.Min(hunger + amount, maxHunger);
    }

    [Server]
    public void AddWater(float amount)
    {
        thirst = Mathf.Min(thirst + amount, maxThirst);
    }

    [Server]
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    [Server]
    void Die()
    {
        health = maxHealth;
        hunger = maxHunger;
        thirst = maxThirst;

        // Телепорт на спавн
        Transform spawn = GetSpawnPoint();
        if (spawn != null)
            transform.position = spawn.position;

        RpcOnDeath();
    }

    Transform GetSpawnPoint()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawns.Length > 0)
            return spawns[Random.Range(0, spawns.Length)].transform;
        return null;
    }

    [ClientRpc]
    void RpcUpdateUI(float hp, float hng, float thst)
    {
        health = hp;
        hunger = hng;
        thirst = thst;

        if (gameUI != null)
        {
            gameUI.UpdateHealth(health, maxHealth);
            gameUI.UpdateHunger(hunger, maxHunger);
            gameUI.UpdateThirst(thirst, maxThirst);
        }
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        Debug.Log("Вы умерли!");
        // Эффект смерти
    }
}