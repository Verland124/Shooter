using UnityEngine;

public static class PlayerPrefsManager
{
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string LAST_IP_KEY = "LastIP";

    // Сохранить ник
    public static void SavePlayerName(string name)
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
        PlayerPrefs.Save();
        Debug.Log($"Ник сохранён: {name}");
    }

    // Загрузить ник
    public static string LoadPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY, "Игрок");
    }

    // Сохранить IP
    public static void SaveLastIP(string ip)
    {
        PlayerPrefs.SetString(LAST_IP_KEY, ip);
        PlayerPrefs.Save();
        Debug.Log($"IP сохранён: {ip}");
    }

    // Загрузить IP
    public static string LoadLastIP()
    {
        return PlayerPrefs.GetString(LAST_IP_KEY, "localhost");
    }

    // Очистить все сохранения (для отладки)
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Все сохранения очищены");
    }
}