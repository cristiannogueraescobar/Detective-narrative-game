using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SELECCIÓN DE CASO (capa sobre el menú): un expediente de papel por historia con su lugar y el mejor final
/// conseguido, y "Caso al azar". La variante (quién lo hizo) siempre se sortea.
/// </summary>
public static class CaseSelect
{
    public const string PanelName = "Casos (auto)";

    public static RectTransform Build(RectTransform menuPanel, Action<string> onChoose, Action onBack)
    {
        Theme t = ThemeManager.Current;
        RectTransform panel = UIFactory.Container(menuPanel, PanelName, Vector2.zero, Vector2.one);
        UIComponents.GetOrAdd<LayoutElement>(panel.gameObject).ignoreLayout = true;
        var background = panel.gameObject.AddComponent<Image>();
        background.color = t.background; // Opaco: el título del menú no se transparenta
        panel.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

        RectTransform column = LayoutKit.Column(panel, out _);

        TMP_Text title = UIFactory.Label(column, "ELIGE UN CASO", t.titleSize, t.textPrimary);
        title.name = "Titulo casos";
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 10f;
        title.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        LayoutKit.Put(title, column, height: 140f);
        LayoutKit.OneLine(title, t.titleSize);

        TMP_Text hint = UIFactory.Label(column, "Cada historia tiene tres culpables posibles: nunca sabes cuál te tocará.", t.secondarySize, t.textSecondary);
        hint.name = "Pista casos";
        hint.alignment = TextAlignmentOptions.Center;
        hint.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        LayoutKit.Put(hint, column, height: 90f);
        LayoutKit.MultiLine(hint, t.secondarySize);

        for (int i = 0; i < CaseLibrary.Stories.Count; i++)
        {
            StoryData story = CaseLibrary.Stories[i];
            RectTransform card = UIFactory.Container(column, "Caso " + story.id, Vector2.zero, Vector2.one);
            LayoutKit.Size(card, height: 250f, flexibleHeight: 1f);
            var paper = card.gameObject.AddComponent<Image>();
            paper.sprite = UISprites.Rounded(14);
            paper.type = Image.Type.Sliced;
            paper.color = t.paper;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = paper;
            string id = story.id;
            button.onClick.AddListener(() => onChoose(id));
            card.gameObject.AddComponent<ClickSound>();
            card.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;

            TMP_Text number = Text(card, "Numero", $"EXPEDIENTE Nº {story.id.PadLeft(3, '0')}", t.secondarySize, t.paperInk, new Vector2(0f, 0.7f), new Vector2(0.6f, 0.93f));
            number.fontStyle = FontStyles.Bold;
            number.alignment = TextAlignmentOptions.MidlineLeft;
            number.characterSpacing = 8f;
            LayoutKit.OneLine(number, t.secondarySize);

            TMP_Text name = Text(card, "Titulo", story.title.ToUpperInvariant(), t.headingSize, t.paperText, new Vector2(0f, 0.4f), new Vector2(1f, 0.7f));
            name.fontStyle = FontStyles.Bold;
            LayoutKit.OneLine(name, t.headingSize);

            TMP_Text place = Text(card, "Lugar", story.place, t.secondarySize, t.paperText, new Vector2(0f, 0.05f), new Vector2(1f, 0.42f));
            LayoutKit.MultiLine(place, t.secondarySize);

            // Mejor final: un sello pequeño en la esquina
            TMP_Text best = Text(card, "Mejor final", "", t.secondarySize * 0.95f, t.paperInk, new Vector2(0.55f, 0.7f), new Vector2(1f, 0.93f));
            best.alignment = TextAlignmentOptions.MidlineRight;
            best.fontStyle = FontStyles.Bold;
            LayoutKit.OneLine(best, t.secondarySize);
        }

        Button random = UIFactory.Button(column, "Caso al azar", false, () => onChoose(null));
        random.name = "Caso al azar";
        LayoutKit.Put(random, column, height: Theme.MinTouchSize);
        Button back = UIFactory.Button(column, "Volver", false, onBack);
        back.name = "Volver de casos";
        LayoutKit.Put(back, column, height: Theme.MinTouchSize);

        Refresh(panel);
        panel.gameObject.SetActive(false);
        return panel;
    }

    /// <summary>
    /// Actualiza los sellos de mejor final de cada expediente.
    /// </summary>
    public static void Refresh(RectTransform panel)
    {
        Theme t = ThemeManager.Current;
        foreach (StoryData story in CaseLibrary.Stories)
        {
            Transform card = FindDeep(panel, "Caso " + story.id);
            Transform label = card != null ? card.Find("Mejor final") : null;
            if (label == null || !label.TryGetComponent(out TMP_Text text))
                continue;
            Ending? best = CaseRecords.Best(story.id);
            text.text = best == null ? "SIN RESOLVER" : EndingStyle.For(best.Value, t).stamp;
            text.color = best == Ending.Good ? new Color32(46, 100, 50, 255) : (Color)t.paperInk;
        }
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }

    private static TMP_Text Text(RectTransform card, string name, string text, float size, Color color, Vector2 min, Vector2 max)
    {
        TMP_Text label = UIFactory.Label(card, text, size, color);
        label.name = name;
        label.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
        label.raycastTarget = false;
        var rect = label.rectTransform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = new Vector2(28f, 0f);
        rect.offsetMax = new Vector2(-28f, 0f);
        if (label.TryGetComponent(out LayoutElement element))
            element.ignoreLayout = true;
        return label;
    }
}
