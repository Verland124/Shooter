using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : NetworkBehaviour
{
    [SerializeField] private GameObject pausePanel;  // ← теперь ссылка в инспекторе!
    private Button resumeButton;
    private Button menuButton;
    private Button quitButton;
    private bool isPaused = false;
    void Start()
    {
        if (!isLocalPlayer) return;

        // Проверяем, что панель назначена
        if (pausePanel == null)
        {
            Debug.LogError("PausePanel не назначен в инспекторе!");
            return;
        }

        // Находим кнопки на панели
        Button[] buttons = pausePanel.GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            if (btn.name == "Resume")
                btn.onClick.AddListener(ResumeGame);
            else if (btn.name == "Menu")
                btn.onClick.AddListener(ReturnToMenu);
            else if (btn.name == "Quit")
                btn.onClick.AddListener(QuitGame);
        }

        // Панель изначально скрыта
        pausePanel.SetActive(false);

        Debug.Log("PauseMenu готов");
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (pausePanel == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}