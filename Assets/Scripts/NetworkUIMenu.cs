using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkUIMenu : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private NetworkManagerCustom networkManager;

    [Header("UI Элементы")]
    [SerializeField] private InputField playerNameInput;
    [SerializeField] private InputField ipInputField;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject gamePanel;

    private string playerName;
    private bool isConnecting = false;

    void Start()
    {
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManagerCustom>();

        // Загружаем сохранённые данные
        LoadSavedData();

        ShowMainMenu();
    }

    void LoadSavedData()
    {
        // Загружаем ник
        playerName = PlayerPrefsManager.LoadPlayerName();
        if (playerNameInput != null)
            playerNameInput.text = playerName;

        // Загружаем IP
        string lastIP = PlayerPrefsManager.LoadLastIP();
        if (ipInputField != null)
            ipInputField.text = lastIP;

        Debug.Log($"Загружены данные: ник = {playerName}, IP = {lastIP}");
    }

    void Update()
    {
        if (isConnecting && statusText != null)
        {
            if (NetworkClient.isConnected)
            {
                statusText.text = "Подключено!";
                isConnecting = false;
            }
            else if (NetworkClient.active)
            {
                int dotCount = (int)(Time.time * 2) % 4;
                string dots = new string('.', dotCount);
                statusText.text = $"Подключение{dots}";
            }
        }

        if (NetworkServer.active || NetworkClient.isConnected)
        {
            if (gamePanel != null && !gamePanel.activeSelf)
                ShowGamePanel();
        }
    }

    public void OnHostButton()
    {
        // Сохраняем ник перед подключением
        SavePlayerName();

        networkManager.StartHostGame();
        ShowConnecting();
    }

    public void OnServerButton()
    {
        SavePlayerName();
        networkManager.StartServerGame();
        ShowConnecting();
    }

    public void OnClientButton()
    {
        SavePlayerName();
        SaveIP();

        string ip = ipInputField != null ? ipInputField.text : "localhost";
        networkManager.StartClientGame(ip);
        isConnecting = true;
        ShowConnecting();
    }

    void SavePlayerName()
    {
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            playerName = playerNameInput.text;
            PlayerPrefsManager.SavePlayerName(playerName);
        }
    }

    void SaveIP()
    {
        if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
        {
            PlayerPrefsManager.SaveLastIP(ipInputField.text);
        }
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            gamePanel?.SetActive(false);
            connectingPanel?.SetActive(false);
        }
        isConnecting = false;
    }

    void ShowConnecting()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            connectingPanel?.SetActive(true);
            gamePanel?.SetActive(false);
        }
        isConnecting = true;
    }

    void ShowGamePanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            connectingPanel?.SetActive(false);
            gamePanel?.SetActive(true);
        }
        isConnecting = false;
    }

    public void OnDisconnectButton()
    {
        networkManager.DisconnectGame();
        ShowMainMenu();
    }
}