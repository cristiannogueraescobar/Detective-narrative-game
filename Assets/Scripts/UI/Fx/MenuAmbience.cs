using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AMBIENTE DEL MENÚ sobre la ilustración del despacho (posiciones en coordenadas de la imagen, así siguen
/// al dibujo aunque la pantalla recorte los lados):
///   - halo cálido de la lámpara con un parpadeo de bombilla vieja cada pocos segundos;
///   - motas de polvo flotando en la luz (más visibles cuanto más cerca de la lámpara);
///   - lluvia fina en el cristal de la ventana;
///   - vapor del café.
/// Todo muy tenue: tiene que notarse sin llamar la atención. Con "reducir animaciones" solo queda el halo fijo.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MenuAmbience : MonoBehaviour
{
    // Puntos del dibujo (0-1, origen arriba a la izquierda) medidos sobre Assets/Backgrounds/menu_fondo
    public static readonly Vector2 LampLight = new Vector2(0.21f, 0.37f);
    public static readonly Rect DustArea = new Rect(0.03f, 0.27f, 0.42f, 0.32f);
    public static readonly Rect WindowGlass = new Rect(0.0f, 0.165f, 0.205f, 0.09f);
    public static readonly Vector2 CoffeeCup = new Vector2(0.84f, 0.545f);

    private const int DustCount = 22;
    private const int RainCount = 16;
    private const int SteamCount = 5;

    private class Particle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 position;   // Normalizada dentro de su zona
        public Vector2 velocity;
        public float size;
        public float phase;
        public float baseAlpha;
    }

    private Image glow;
    private RectTransform dustArea;
    private RectTransform rainArea;
    private RectTransform steamOrigin;
    private readonly List<Particle> dust = new List<Particle>();
    private readonly List<Particle> rain = new List<Particle>();
    private readonly List<Particle> steam = new List<Particle>();
    private float seed;
    private bool built;

    private static Theme T => ThemeManager.Current;

    public static MenuAmbience AddTo(RawImage backdrop)
    {
        if (backdrop == null)
            return null;
        MenuAmbience ambience = UIComponents.GetOrAdd<MenuAmbience>(backdrop.gameObject);
        ambience.Build();
        return ambience;
    }

    public void Build()
    {
        if (built)
            return;
        built = true;
        seed = Random.Range(0f, 50f);
        var random = new System.Random(11);

        // Halo de la lámpara (cuadrado en píxeles: la imagen es 2:3)
        RectTransform glowRect = Region("Halo lampara", new Rect(LampLight.x - 0.3f, LampLight.y - 0.2f, 0.6f, 0.4f));
        glow = glowRect.gameObject.AddComponent<Image>();
        glow.sprite = UISprites.Radial();
        glow.raycastTarget = false;

        dustArea = Region("Polvo", DustArea);
        for (int i = 0; i < DustCount; i++)
        {
            Particle p = NewParticle(dustArea, "Mota", UISprites.Radial());
            p.position = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            p.velocity = new Vector2(((float)random.NextDouble() - 0.5f) * 0.01f, 0.004f + (float)random.NextDouble() * 0.008f);
            p.size = 6f + (float)random.NextDouble() * 9f;
            p.phase = (float)random.NextDouble() * 10f;
            p.baseAlpha = 0.35f + (float)random.NextDouble() * 0.4f;
            p.rect.sizeDelta = new Vector2(p.size, p.size);
            dust.Add(p);
        }

        rainArea = Region("Lluvia", WindowGlass);
        rainArea.gameObject.AddComponent<RectMask2D>();
        for (int i = 0; i < RainCount; i++)
        {
            Particle p = NewParticle(rainArea, "Gota", null);
            p.position = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            p.velocity = new Vector2(-0.05f, -(0.9f + (float)random.NextDouble() * 0.5f));
            p.size = 16f + (float)random.NextDouble() * 18f;
            p.baseAlpha = 0.10f + (float)random.NextDouble() * 0.12f;
            p.rect.sizeDelta = new Vector2(1.6f, p.size);
            p.rect.localRotation = Quaternion.Euler(0f, 0f, -4f);
            rain.Add(p);
        }

        steamOrigin = Region("Vapor", new Rect(CoffeeCup.x - 0.06f, CoffeeCup.y - 0.12f, 0.12f, 0.12f));
        for (int i = 0; i < SteamCount; i++)
        {
            Particle p = NewParticle(steamOrigin, "Vaho", UISprites.Radial());
            p.phase = i / (float)SteamCount;
            steam.Add(p);
        }

        Render(0f);
    }

    private RectTransform Region(string name, Rect imageRect)
    {
        var go = new GameObject(name + " (auto)", typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(transform, false);
        rect.anchorMin = Ambience.ImageToAnchor(new Vector2(imageRect.xMin, imageRect.yMax));
        rect.anchorMax = Ambience.ImageToAnchor(new Vector2(imageRect.xMax, imageRect.yMin));
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.AddComponent<ThemeRole>().role = UIRole.Ignore;
        return rect;
    }

    private static Particle NewParticle(RectTransform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return new Particle { rect = rect, image = image };
    }

    private void Update()
    {
        Render(Time.unscaledDeltaTime);
    }

    private void Render(float dt)
    {
        bool still = GameSettings.ReduceMotion || !Application.isPlaying;
        float time = Application.isPlaying ? Time.unscaledTime : 0f;
        float lamp = still ? 1f : Ambience.LampFlicker(time, seed);

        Color warm = T.lampGlow;
        glow.color = new Color(warm.r, warm.g, warm.b, warm.a * lamp);

        dustArea.gameObject.SetActive(!GameSettings.ReduceMotion);
        rainArea.gameObject.SetActive(!GameSettings.ReduceMotion);
        steamOrigin.gameObject.SetActive(!GameSettings.ReduceMotion);
        if (GameSettings.ReduceMotion)
            return;

        Vector2 dustSize = dustArea.rect.size;
        foreach (Particle p in dust)
        {
            if (!still)
            {
                // Deriva lenta con un vaivén propio; al salir por arriba vuelve a entrar por abajo
                p.position += (p.velocity + new Vector2(Mathf.Sin(time * 0.6f + p.phase) * 0.004f, 0f)) * dt;
                p.position = new Vector2(Mathf.Repeat(p.position.x, 1f), Mathf.Repeat(p.position.y, 1f));
            }
            p.rect.anchoredPosition = Vector2.Scale(p.position, dustSize);

            Vector2 inImage = new Vector2(DustArea.x + p.position.x * DustArea.width, DustArea.yMax - p.position.y * DustArea.height);
            float twinkle = 0.75f + 0.25f * Mathf.Sin(time * 1.3f + p.phase * 3f);
            float a = p.baseAlpha * twinkle * lamp * Ambience.DustVisibility(inImage, LampLight + new Vector2(0.02f, 0.06f), 0.24f);
            p.image.color = new Color(1f, 0.92f, 0.78f, a);
        }

        Vector2 rainSize = rainArea.rect.size;
        foreach (Particle p in rain)
        {
            if (!still)
            {
                p.position += p.velocity * dt;
                if (p.position.y < -0.3f)
                    p.position = new Vector2(Random.value * 1.1f, 1.2f);
            }
            p.rect.anchoredPosition = Vector2.Scale(p.position, rainSize);
            p.image.color = new Color(0.78f, 0.86f, 1f, p.baseAlpha);
        }

        Vector2 steamSize = steamOrigin.rect.size;
        const float life = 3.2f;
        foreach (Particle p in steam)
        {
            float age = Mathf.Repeat(time / life + p.phase, 1f);
            float sway = Mathf.Sin((time + p.phase * 7f) * 1.4f) * 0.12f * age;
            p.rect.anchoredPosition = new Vector2((0.5f + sway) * steamSize.x, age * steamSize.y * 1.1f);
            float size = Mathf.Lerp(0.18f, 0.55f, age) * steamSize.x;
            p.rect.sizeDelta = new Vector2(size, size * 1.3f);
            float a = 0.13f * Mathf.Sin(age * Mathf.PI);
            p.image.color = new Color(1f, 1f, 1f, a);
        }
    }
}
