using System;
using UnityEngine;

public interface ISettingsStore
{
    float Get(string key, float fallback);
    void Set(string key, float value);
}

/// <summary>
/// Ajustes del jugador (persisten entre partidas): volumen, velocidad del texto y reducir animaciones.
/// </summary>
public static class GameSettings
{
    public const string VolumeKey = "ajustes.volumen";
    public const string TextSpeedKey = "ajustes.velocidadTexto";
    public const string ReduceMotionKey = "ajustes.reducirAnimaciones";

    public const float MinTextSpeed = 0.5f;
    public const float MaxTextSpeed = 3f;

    private class PlayerPrefsStore : ISettingsStore
    {
        public float Get(string key, float fallback) => PlayerPrefs.GetFloat(key, fallback);

        public void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
    }

    private static ISettingsStore store = new PlayerPrefsStore();

    public static event Action Changed;

    /// <summary>
    /// Sustituye el almacenamiento (tests). null vuelve a PlayerPrefs.
    /// </summary>
    public static void UseStore(ISettingsStore replacement)
    {
        store = replacement ?? new PlayerPrefsStore();
        Changed = null;
    }

    public static float Volume
    {
        get => store.Get(VolumeKey, 1f);
        set
        {
            store.Set(VolumeKey, Mathf.Clamp01(value));
            AudioListener.volume = Volume;
            Changed?.Invoke();
        }
    }

    public static float TextSpeed
    {
        get => store.Get(TextSpeedKey, 1f);
        set
        {
            store.Set(TextSpeedKey, Mathf.Clamp(value, MinTextSpeed, MaxTextSpeed));
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Desactiva temblores, respiración, parallax y giros; los fundidos se vuelven instantáneos.
    /// </summary>
    public static bool ReduceMotion
    {
        get => store.Get(ReduceMotionKey, 0f) > 0.5f;
        set
        {
            store.Set(ReduceMotionKey, value ? 1f : 0f);
            Changed?.Invoke();
        }
    }
}
