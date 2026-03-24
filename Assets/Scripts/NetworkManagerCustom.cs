using UnityEngine;
using Mirror;
using UnityEditor;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkManagerCustom : NetworkManager
{
    [Header("Настройки персонажа")]
    [SerializeField] private GameObject playerCameraPrefab;

    [Header("Настройки спавна")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool useRandomSpawn = true;

    private int currentSpawnIndex = 0;

    private static NetworkManagerCustom instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("[Клиент] Подключен к серверу");
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
        Debug.Log($"[Клиент] Загружена сцена: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        Debug.Log($"[Сервер] Загружена сцена: {sceneName}");
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Сервер запущен");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Клиент подключен");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);

        NetworkServer.AddPlayerForConnection(conn, player);

        if (playerCameraPrefab != null)
        {
            GameObject cameraObj = Instantiate(playerCameraPrefab);
            CameraC cameraController = cameraObj.GetComponent<CameraC>();

            if (cameraController != null)
            {
                // ВОТ ЭТА СТРОКА КРИТИЧЕСКИ ВАЖНА!
                Debug.Log("Вызываем SetPlayer");
                cameraController.SetPlayer(player.transform);
                Debug.Log($"Камера привязана к игроку: {player.name}");
            }
            else
            {
                Debug.LogError("CameraC компонент не найден на префабе камеры!");
            }

            //NetworkServer.Spawn(cameraObj, conn);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            GameObject player = conn.identity.gameObject;

            foreach (NetworkConnectionToClient client in NetworkServer.connections.Values)
            {
                if (client != conn && client.identity != null)
                {
                    client.identity.GetComponent<PlayerController1>()?.TargetPlayerDisconnected(client, player.name);
                }
            }
        }
        base.OnServerDisconnect(conn);
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            if (useRandomSpawn)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                return spawnPoints[randomIndex].position;
            }
            else
            {
                Vector3 pos = spawnPoints[currentSpawnIndex].position;
                currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;
                return pos;
            }
        }

        Vector3 defaultPosition = Vector3.zero;
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawns.Length > 0)
        {
            if (useRandomSpawn)
            {
                int randomIndex = Random.Range(0, spawns.Length);
                return spawns[randomIndex].transform.position;
            }
            else
            {
                return spawns[currentSpawnIndex % spawns.Length].transform.position;
            }
        }

        return defaultPosition;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Сервер остановлен");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Клиент отключен");
    }

    public void StartHostGame()
    {
        StartHost();
    }

    public void StartClientGame(string ipAddress)
    {
        networkAddress = ipAddress;
        StartClient();
    }

    public void StartServerGame()
    {
        StartServer();
    }

    public void DisconnectGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            StopClient();
        }
        else if (NetworkServer.active)
        {
            StopServer();
        }

        SceneManager.LoadScene(0);
    }

}