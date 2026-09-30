using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ficha policial del culpable al final del informe (ronda final 3): foto con el relieve 2.5D, sujeta sobre una
/// cartulina y con su nombre a máquina. Va dentro del informe, después del texto (se desplaza con él y nunca lo
/// tapa) y aparece cuando el informe termina de descubrirse. Se quita con Theme.endingMugshot = false.
/// </summary>
public static class Mugshot
{
    public const string Name = "Ficha policial (auto)";
    private const float CardWidth = 320f;
    private const float CardHeight = 420f;
    private const float Border = 16f;
    private const float Caption = 96f;

    /// <summary>
    /// Pone (o actualiza) la ficha detrás de 'report'. Sin retrato o con la ficha desactivada en el tema, la quita.
    /// </summary>
    public static GameObject Show(TMP_Text report, Texture portrait, Rect uv, bool legacyArt, string caption)
    {
        if (report == null)
            return null;
        Theme t = ThemeManager.Current;
        Transform parent = report.transform.parent;
        Transform existing = parent.Find(Name);
        if (!t.endingMugshot || portrait == null)
        {
            if (existing != null)
                existing.gameObject.SetActive(false);
            return null;
        }

        GameObject holder = existing != null ? existing.gameObject : Build(parent, t);
        holder.transform.SetSiblingIndex(report.transform.GetSiblingIndex() + 1);
        holder.SetActive(true);

        var photo = holder.GetComponentInChildren<RawImage>(true);
        photo.texture = portrait;
        photo.uvRect = uv;
        if (legacyArt)
            ArtGrading.Apply(photo, ArtGrading.Kind.LegacyPortrait);
        else
            ArtGrading.Clear(photo);
        holder.GetComponentInChildren<TMP_Text>(true).text = caption;

        holder.GetComponent<RevealAfter>().Arm(report.GetComponent<StepReveal>());
        return holder;
    }

    private static GameObject Build(Transform parent, Theme t)
    {
        var holder = new GameObject(Name, typeof(RectTransform));
        holder.transform.SetParent(parent, false);
        var element = holder.AddComponent<LayoutElement>();
        element.minHeight = element.preferredHeight = CardHeight + 2f * t.spacing;
        holder.AddComponent<CanvasGroup>();
        holder.AddComponent<RevealAfter>();

        // Cartulina ligeramente torcida, como sujeta con un clip al expediente
        RectTransform card = UIFactory.Container(holder.transform, "Cartulina", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        card.sizeDelta = new Vector2(CardWidth, CardHeight);
        card.localRotation = Quaternion.Euler(0f, 0f, -3f);
        var paper = card.gameObject.AddComponent<Image>();
        paper.sprite = UISprites.Rounded(t.RadiusTiny);
        paper.type = Image.Type.Sliced;
        paper.color = t.paper;
        paper.raycastTarget = false;
        card.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

        RectTransform photoRect = UIFactory.Container(card, "Foto", Vector2.zero, Vector2.one);
        photoRect.offsetMin = new Vector2(Border, Caption);
        photoRect.offsetMax = new Vector2(-Border, -Border);
        var photo = photoRect.gameObject.AddComponent<RawImage>();
        photo.raycastTarget = false;
        photo.color = Color.white;

        RectTransform captionRect = UIFactory.Container(card, "Pie", Vector2.zero, new Vector2(1f, 0f));
        captionRect.pivot = new Vector2(0.5f, 0f);
        captionRect.sizeDelta = new Vector2(-2f * Border, Caption);
        captionRect.anchoredPosition = Vector2.zero;
        TMP_Text label = UIFactory.Label(captionRect, "", t.secondarySize, t.paperInk);
        label.font = UIFactory.TitleFont();
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;  // "CULPABLE" y el nombre, en dos líneas: nada se corta
        label.enableAutoSizing = true;
        label.fontSizeMin = t.secondarySize * 0.6f;
        label.fontSizeMax = t.secondarySize;
        label.raycastTarget = false;
        var labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        UIComponents.GetOrAdd<LayoutElement>(label.gameObject).ignoreLayout = true;
        label.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        return holder;
    }

    /// <summary>
    /// Invisible mientras el informe se descubre línea a línea; después aparece (sin fundido con "Reducir animaciones").
    /// </summary>
    public class RevealAfter : MonoBehaviour
    {
        private StepReveal reveal;
        private CanvasGroup group;
        private bool started;
        private float alpha;

        public void Arm(StepReveal target)
        {
            reveal = target;
            group = GetComponent<CanvasGroup>();
            started = false;
            alpha = reveal != null && reveal.IsRevealing ? 0f : 1f;
            group.alpha = alpha;
        }

        private void Update()
        {
            if (group == null || alpha >= 1f)
                return;
            if (reveal != null && reveal.IsRevealing)
            {
                started = true;
                return;
            }
            if (!started && reveal != null && Time.frameCount < 2)
                return;
            alpha = GameSettings.ReduceMotion ? 1f : Mathf.MoveTowards(alpha, 1f, Time.unscaledDeltaTime / 0.5f);
            group.alpha = alpha;
        }
    }
}
