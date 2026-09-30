using System;
using UnityEngine;

public interface ISettingsStore
{
    float Get(string key, float fallback);
    void Set(string key, float value);
}

/// <summary>
/// Ajustes del jugador (persisten entre partidas y se aplican en caliente mediante Changed): volúmenes,
/// velocidad y tamaño del texto, reducir animaciones, filtro noir y alto contraste.
/// </summary>
public static class GameSettings
{
    public const string VolumeKey = "ajustes.volumen";
    public const string TextSpeedKey = "ajustes.velocidadTexto";
    public const string ReduceMotionKey = "ajustes.reducirAnimaciones";
    public const string NoirFilterKey = "ajustes.filtroNoir";
    public const string MusicVolumeKey = "ajustes.volumenMusica";
    public const string SfxVolumeKey = "ajustes.volumenEfectos";
    public const string TextSizeKey = "ajustes.tamanoTexto";
    public const string HighContrastKey = "ajustes.altoContraste";
    public const string VibrationKey = "ajustes.vibracion";

    /// <summary>
    /// Escala del texto por nivel (normal, grande, muy grande).
    /// </summary>
    public static readonly float[] TextScales = { 1f, 1.15f, 1.3f };

    public const float MinTextSpeed = 0.5f;
    public const float MaxTextSpeed = 3f;

    private class PlayerPrefsStore : ISettingsStore
    {
        public float Get(string key, float fallback) => PlayerPrefs.GetFloat(key, fallback);

        public void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value); // Se escribe a disco al pausar o salir (Flush), no en cada arrastre
        }
    }

    private static ISettingsStore backing = new PlayerPrefsStore();
    private static readonly CachedStore store = new CachedStore();

    // Los ajustes se leen del almacén una vez y luego de memoria (se consultan en cada fotograma)
    private class CachedStore
    {
        private readonly System.Collections.Generic.Dictionary<string, float> values = new System.Collections.Generic.Dictionary<string, float>();

        public float Get(string key, float fallback)
        {
            if (!values.TryGetValue(key, out float v))
            {
                v = backing.Get(key, fallback);
                values[key] = v;
            }
            return v;
        }

        public void Set(string key, float value)
        {
            values[key] = value;
            backing.Set(key, value);
        }

        public void Clear() => values.Clear();
    }

    /// <summary>
    /// Escribe los ajustes a disco (al pausar o cerrar la aplicación).
    /// </summary>
    public static void Flush()
    {
        if (backing is PlayerPrefsStore)
            PlayerPrefs.Save();
    }

    public static event Action Changed;

    /// <summary>
    /// Sustituye el almacenamiento (tests). null vuelve a PlayerPrefs.
    /// </summary>
    public static void UseStore(ISettingsStore replacement)
    {
        backing = replacement ?? new PlayerPrefsStore();
        store.Clear();
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

    /// <summary>
    /// Grano de película y viñeteado sobre toda la pantalla.
    /// </summary>
    public static bool NoirFilter
    {
        get => store.Get(NoirFilterKey, 1f) > 0.5f;
        set
        {
            store.Set(NoirFilterKey, value ? 1f : 0f);
            Changed?.Invoke();
        }
    }

    public static float MusicVolume
    {
        get => store.Get(MusicVolumeKey, 0.7f);
        set
        {
            store.Set(MusicVolumeKey, Mathf.Clamp01(value));
            Changed?.Invoke();
        }
    }

    public static float SfxVolume
    {
        get => store.Get(SfxVolumeKey, 1f);
        set
        {
            store.Set(SfxVolumeKey, Mathf.Clamp01(value));
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Nivel de tamaño del texto: 0 normal, 1 grande, 2 muy grande.
    /// </summary>
    public static int TextSizeLevel
    {
        get => Mathf.Clamp(Mathf.RoundToInt(store.Get(TextSizeKey, 0f)), 0, TextScales.Length - 1);
        set
        {
            store.Set(TextSizeKey, Mathf.Clamp(value, 0, TextScales.Length - 1));
            Changed?.Invoke();
        }
    }

    public static float TextScale => TextScales[TextSizeLevel];

    /// <summary>
    /// Texto más blanco, fondos más negros y bordes marcados.
    /// </summary>
    public static bool HighContrast
    {
        get => store.Get(HighContrastKey, 0f) > 0.5f;
        set
        {
            store.Set(HighContrastKey, value ? 1f : 0f);
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Marcas sueltas que persisten (el tutorial, por ejemplo). No avisan a Changed.
    /// </summary>
    public static bool GetFlag(string key) => store.Get(key, 0f) > 0.5f;

    public static void SetFlag(string key, bool value) => store.Set(key, value ? 1f : 0f);

    public static float GetValue(string key, float fallback) => store.Get(key, fallback);

    public static void SetValue(string key, float value) => store.Set(key, value);

    /// <summary>
    /// Vibración del móvil en los momentos fuertes (contradicción, pista).
    /// </summary>
    public static bool Vibration
    {
        get => store.Get(VibrationKey, 1f) > 0.5f;
        set
        {
            store.Set(VibrationKey, value ? 1f : 0f);
            Changed?.Invoke();
        }
    }
}
