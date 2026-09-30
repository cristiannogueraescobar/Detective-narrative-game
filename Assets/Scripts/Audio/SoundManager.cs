using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum Sfx
{
    Click,
    Send,
    Answer,
    Typing,
    Clue,
    Contradiction,
    Stamp,
    DayChange,
    Accusation,
    Heartbeat,
    EndingGood,
    EndingBittersweet,
    EndingInsufficient,
    EndingBad
}

public enum Music
{
    None,
    Menu,
    Story1,
    Story2,
    Story3,
    Tension
}

/// <summary>
/// Qué archivo corresponde a cada sonido (Assets/Resources/Audio/...). La lista completa, con duraciones y
/// descripciones, está en AUDIO-NEEDED.md. Si un archivo no existe, ese sonido no suena y el juego sigue.
/// </summary>
public static class SoundCatalog
{
    public const string Folder = "Audio";

    public static string PathOf(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Click: return "Audio/sfx/clic";
            case Sfx.Send: return "Audio/sfx/enviar";
            case Sfx.Answer: return "Audio/sfx/respuesta";
            case Sfx.Typing: return "Audio/sfx/maquina";
            case Sfx.Clue: return "Audio/sfx/pista";
            case Sfx.Contradiction: return "Audio/sfx/contradiccion";
            case Sfx.Stamp: return "Audio/sfx/sello";
            case Sfx.DayChange: return "Audio/sfx/nuevo_dia";
            case Sfx.Accusation: return "Audio/sfx/acusacion";
            case Sfx.Heartbeat: return "Audio/sfx/latido";
            case Sfx.EndingGood: return "Audio/sfx/final_bueno";
            case Sfx.EndingBittersweet: return "Audio/sfx/final_agridulce";
            case Sfx.EndingInsufficient: return "Audio/sfx/final_insuficiente";
            default: return "Audio/sfx/final_malo";
        }
    }

    public static string PathOf(Music music)
    {
        switch (music)
        {
            case Music.Menu: return "Audio/musica/menu";
            case Music.Story1: return "Audio/musica/historia1";
            case Music.Story2: return "Audio/musica/historia2";
            case Music.Story3: return "Audio/musica/historia3";
            case Music.Tension: return "Audio/musica/acusacion";
            default: return null;
        }
    }

    public static Music ForStory(string storyId)
    {
        switch (storyId)
        {
            case "1": return Music.Story1;
            case "2": return Music.Story2;
            case "3": return Music.Story3;
            default: return Music.Menu;
        }
    }

    public static Sfx ForEnding(Ending ending)
    {
        switch (ending)
        {
            case Ending.Good: return Sfx.EndingGood;
            case Ending.Bittersweet: return Sfx.EndingBittersweet;
            case Ending.Insufficient: return Sfx.EndingInsufficient;
            default: return Sfx.EndingBad;
        }
    }

    /// <summary>
    /// Volumen final de un canal: general × canal.
    /// </summary>
    public static float Effective(float master, float channel)
    {
        return Mathf.Clamp01(master) * Mathf.Clamp01(channel);
    }
}

/// <summary>
/// SONIDO: efectos (varias fuentes para que se solapen) y música con fundido cruzado. Se crea solo la primera
/// vez que se usa y sobrevive a los cambios de escena. Los volúmenes salen de Ajustes y se aplican en caliente.
/// Sin archivos de audio, todo funciona en silencio.
/// </summary>
public class SoundManager : MonoBehaviour
{
    private const int SfxVoices = 6;
    private const float CrossfadeSeconds = 1.2f;

    private static SoundManager instance;
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private readonly HashSet<string> missing = new HashSet<string>();
    private AudioSource[] voices;
    private int nextVoice;
    private AudioSource musicA;
    private AudioSource musicB;
    private Music currentMusic = Music.None;
    private Coroutine fade;

    /// <summary>
    /// Sonidos pedidos desde que arrancó (para tests: comprueban que un evento suena aunque no haya archivo).
    /// </summary>
    public static readonly List<Sfx> Played = new List<Sfx>();

    public static Music CurrentMusic => instance != null ? instance.currentMusic : Music.None;

    private static SoundManager Instance
    {
        get
        {
            if (instance == null && Application.isPlaying)
            {
                var go = new GameObject("Sonido (auto)");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<SoundManager>();
                instance.Setup();
            }
            return instance;
        }
    }

    private void Setup()
    {
        voices = new AudioSource[SfxVoices];
        for (int i = 0; i < SfxVoices; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
        musicA = NewMusicSource();
        musicB = NewMusicSource();
        GameSettings.Changed += ApplyVolumes;
    }

    private AudioSource NewMusicSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        return source;
    }

    private void OnDestroy()
    {
        GameSettings.Changed -= ApplyVolumes;
        if (instance == this)
            instance = null;
    }

    public static void Play(Sfx sfx, float volume = 1f, float pitchVariation = 0f)
    {
        Played.Add(sfx);
        if (Played.Count > 200)
            Played.RemoveAt(0);

        SoundManager manager = Instance;
        if (manager == null)
            return;
        AudioClip clip = manager.Load(SoundCatalog.PathOf(sfx));
        if (clip == null)
            return;

        AudioSource voice = manager.voices[manager.nextVoice];
        manager.nextVoice = (manager.nextVoice + 1) % manager.voices.Length;
        voice.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        voice.PlayOneShot(clip, Mathf.Clamp01(volume) * GameSettings.SfxVolume);
    }

    /// <summary>
    /// Cambia la música con un fundido cruzado. La misma música no se reinicia.
    /// </summary>
    public static void PlayMusic(Music music)
    {
        SoundManager manager = Instance;
        if (manager == null || music == manager.currentMusic)
            return;
        manager.currentMusic = music;

        string path = SoundCatalog.PathOf(music);
        AudioClip clip = path != null ? manager.Load(path) : null;
        if (manager.fade != null)
            manager.StopCoroutine(manager.fade);
        manager.fade = manager.StartCoroutine(manager.Crossfade(clip));
    }

    private IEnumerator Crossfade(AudioClip clip)
    {
        // A la fuente libre entra la nueva; la otra se apaga
        AudioSource from = musicA.isPlaying && musicA.volume >= musicB.volume ? musicA : musicB;
        AudioSource to = from == musicA ? musicB : musicA;

        if (clip != null)
        {
            to.clip = clip;
            to.volume = 0f;
            to.Play();
        }

        float startFrom = from.volume;
        AudioSource other = to;
        float startOther = clip == null ? other.volume : 0f;
        for (float e = 0f; e < CrossfadeSeconds; e += Time.unscaledDeltaTime)
        {
            float t = e / CrossfadeSeconds;
            from.volume = startFrom * (1f - t);
            if (clip == null)
                other.volume = startOther * (1f - t);
            if (clip != null)
                to.volume = GameSettings.MusicVolume * t;
            yield return null;
        }

        // Solo queda sonando la nueva (un fundido interrumpido podía dejar la anterior a medio volumen)
        foreach (AudioSource source in new[] { musicA, musicB })
        {
            if (clip != null && source == to)
                continue;
            source.Stop();
            source.volume = 0f;
        }
        if (clip != null)
            to.volume = GameSettings.MusicVolume;
        fade = null;
    }

    private void ApplyVolumes()
    {
        AudioListener.volume = GameSettings.Volume;
        if (fade != null)
            return;
        foreach (AudioSource source in new[] { musicA, musicB })
        {
            if (source.isPlaying)
                source.volume = GameSettings.MusicVolume;
        }
    }

    private AudioClip Load(string path)
    {
        if (cache.TryGetValue(path, out AudioClip clip))
            return clip;
        if (missing.Contains(path))
            return null;

        clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            missing.Add(path);
            if (Application.isEditor)
                Debug.Log($"[Sonido] Falta Assets/Resources/{path} (ver AUDIO-NEEDED.md): se omite.");
            return null;
        }
        cache[path] = clip;
        return clip;
    }
}

/// <summary>
/// Clic de un botón (se añade a todos los botones al aplicar el tema).
/// </summary>
[RequireComponent(typeof(Selectable))]
public class ClickSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TryGetComponent(out Selectable s) && s.interactable)
            SoundManager.Play(Sfx.Click, 0.6f, 0.05f);
    }
}
