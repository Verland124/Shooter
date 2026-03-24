using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkManagerCustom networkManager;
    [SerializeField] private InputField ipInputField;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject gamePanel;

    [Header("Status")]
    [SerializeField] private Text statusText;

    private bool isConnecting = false;

    void Start()
    {
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManagerCustom>();

        ShowMainMenu();
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
        networkManager.StartHostGame();
        ShowConnecting();
    }

    public void OnServerButton()
    {
        networkManager.StartServerGame();
        ShowConnecting();
    }

    public void OnClientButton()
    {
        string ip = ipInputField != null ? ipInputField.text : "localhost";
        networkManager.StartClientGame(ip);
        isConnecting = true;
        ShowConnecting();
    }

    public void OnDisconnectButton()
    {
        networkManager.DisconnectGame();
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            gamePanel?.SetActive(false);
            connectingPanel?.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }

        isConnecting = false;
    }

    private void ShowConnecting()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            connectingPanel?.SetActive(true);
            gamePanel?.SetActive(false);
        }

        isConnecting = true;
    }

    private void ShowGamePanel()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            connectingPanel?.SetActive(false);
            gamePanel?.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

        isConnecting = false;
    }

    public void SetStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}