using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private Text playerNameText;
    [Header("Stats Bars")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Text hungerText;
    [SerializeField] private Text thirstText;

    [Header("Resources")]
    [SerializeField] private Text woodText;
    [SerializeField] private Text stoneText;
    [SerializeField] private Text berriesText;

    [Header("Time")]
    [SerializeField] private Text timeText;

    void Start()
    {
        // Инициализация полосок
        if (healthSlider != null)
        {
            healthSlider.maxValue = 100;
            healthSlider.value = 100;
            if (healthText != null) healthText.text = "100 / 100";
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = 100;
            hungerSlider.value = 100;
            if (hungerText != null) hungerText.text = "100 / 100";
        }

        if (thirstSlider != null)
        {
            thirstSlider.maxValue = 100;
            thirstSlider.value = 100;
            if (thirstText != null) thirstText.text = "100 / 100";
        }
        GameObject localPlayer = GetLocalPlayer();
        if (localPlayer != null)
        {
            PlayerController1 player = localPlayer.GetComponent<PlayerController1>();
            if (player != null && playerNameText != null)
            {
                playerNameText.text = player.playerName;
            }
        }
        GameObject GetLocalPlayer()
        {
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                NetworkIdentity identity = obj.GetComponent<NetworkIdentity>();
                if (identity != null && identity.isLocalPlayer)
                {
                    return obj;
                }
            }
            return null;
        }
    }

    // ========== ЗДОРОВЬЕ ==========
    public void UpdateHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }

    // ========== ГОЛОД ==========
    public void UpdateHunger(float current, float max)
    {
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = max;
            hungerSlider.value = current;
        }
        if (hungerText != null)
            hungerText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }

    // ========== ЖАЖДА ==========
    public void UpdateThirst(float current, float max)
    {
        if (thirstSlider != null)
        {
            thirstSlider.maxValue = max;
            thirstSlider.value = current;
        }
        if (thirstText != null)
            thirstText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
    }

    // ========== РЕСУРСЫ ==========
    public void UpdateWood(int amount)
    {
        if (woodText != null)
            woodText.text = $"🌲 {amount}";
    }

    public void UpdateStone(int amount)
    {
        if (stoneText != null)
            stoneText.text = $"🪨 {amount}";
    }

    public void UpdateBerries(int amount)
    {
        if (berriesText != null)
            berriesText.text = $"🍓 {amount}";
    }

    // ========== ВРЕМЯ ==========
    public void UpdateTime(int day, bool isNight)
    {
        if (timeText != null)
            timeText.text = isNight ? $"Ночь {day}" : $"День {day}";
    }
}