using Mirror;
using UnityEngine;

public class DayNightCycle : NetworkBehaviour
{
    [Header("Солнце")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity;

    [Header("Время")]
    [SyncVar] public float timeOfDay = 0.25f;
    public float dayDuration = 6f; // 10 минут день

    [Header("UI")]
    [SerializeField] private GameUI gameUI;

    void Start()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        if (!isServer) return;

        gameUI = FindObjectOfType<GameUI>();
    }

    void Update()
    {
        if (!isServer) return;

        // Обновляем время
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay >= 1f) timeOfDay -= 1f;

        // Применяем эффекты
        RpcUpdateLight(timeOfDay);

        // Обновляем UI
        int day = Mathf.FloorToInt(Time.time / dayDuration) + 1;
        bool isNight = timeOfDay > 0.75f || timeOfDay < 0.25f;

        if (gameUI != null)
            gameUI.UpdateTime(day, isNight);
    }

    [ClientRpc]
    void RpcUpdateLight(float time)
    {
        if (sunLight == null) return;

        // Поворот солнца
        float sunAngle = time * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

        // Цвет и интенсивность
        sunLight.intensity = sunIntensity.Evaluate(time);
        sunLight.color = sunColor.Evaluate(time);

        // Настройка окружения (опционально)
        RenderSettings.ambientIntensity = Mathf.Clamp01(sunIntensity.Evaluate(time) * 0.5f);
    }

    public bool IsNight()
    {
        return timeOfDay > 0.75f || timeOfDay < 0.25f;
    }
}