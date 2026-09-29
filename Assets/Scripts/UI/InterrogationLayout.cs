using UnityEngine;

/// <summary>
/// Distribución vertical para jugar con una mano, aplicada por código sobre el panel de interrogatorio:
/// HUD compacto arriba, retrato, chat protagonista en el centro y controles (sospechoso, prueba, pregunta
/// y enviar) abajo, al alcance del pulgar. Todas las medidas salen del tema (px a 1080 × 1920).
/// No modifica la escena: si algo no encaja, se desactiva "Aplicar distribución móvil" en InterrogationUI.
/// </summary>
public static class InterrogationLayout
{
    public struct Elements
    {
        public RectTransform panel;
        public RectTransform hud;
        public RectTransform endDay;
        public RectTransform accuseNow;
        public RectTransform notebook;
        public RectTransform portrait;
        public RectTransform chat;
        public RectTransform waiting;
        public RectTransform suspect;
        public RectTransform evidence;
        public RectTransform question;
        public RectTransform send;
        public RectTransform clueNotice;
    }

    private const float RowHeight = 110f;
    private const float InputHeight = 130f;

    public static void Apply(Elements e)
    {
        if (e.panel == null)
            return;

        Theme t = ThemeManager.Current;
        float pad = t.padding;
        float gap = t.spacing;

        // --- Arriba: HUD compacto (texto a la izquierda, tres botones a la derecha)
        TopBand(e.hud, e.panel, 0f, 0.43f, 0f, t.hudHeight, pad, gap);
        TopBand(e.endDay, e.panel, 0.44f, 0.62f, 0f, t.hudHeight, pad, gap);
        TopBand(e.accuseNow, e.panel, 0.63f, 0.81f, 0f, t.hudHeight, pad, gap);
        TopBand(e.notebook, e.panel, 0.82f, 1f, 0f, t.hudHeight, pad, gap);

        // --- Retrato bajo el HUD, centrado
        if (e.portrait != null)
        {
            Adopt(e.portrait, e.panel);
            e.portrait.anchorMin = e.portrait.anchorMax = new Vector2(0.5f, 1f);
            e.portrait.pivot = new Vector2(0.5f, 1f);
            e.portrait.sizeDelta = new Vector2(t.portraitHeight * 0.75f, t.portraitHeight);
            e.portrait.anchoredPosition = new Vector2(0f, -(t.hudHeight + gap));
        }

        // --- Abajo, de abajo arriba: pregunta + enviar, prueba, sospechoso
        float y = pad;
        BottomBand(e.question, e.panel, 0f, 0.76f, y, InputHeight, pad, gap);
        BottomBand(e.send, e.panel, 0.77f, 1f, y, InputHeight, pad, gap);
        y += InputHeight + gap;
        BottomBand(e.evidence, e.panel, 0f, 1f, y, RowHeight, pad, gap);
        y += RowHeight + gap;
        BottomBand(e.suspect, e.panel, 0f, 1f, y, RowHeight, pad, gap);
        y += RowHeight + gap;

        float bottomArea = Mathf.Max(y, t.bottomAreaHeight);

        // --- Centro: el chat ocupa todo lo que queda
        if (e.chat != null)
        {
            Adopt(e.chat, e.panel);
            e.chat.anchorMin = new Vector2(0f, 0f);
            e.chat.anchorMax = new Vector2(1f, 1f);
            e.chat.pivot = new Vector2(0.5f, 0.5f);
            float top = t.hudHeight + (e.portrait != null ? t.portraitHeight + gap : 0f) + gap;
            e.chat.offsetMin = new Vector2(pad, bottomArea);
            e.chat.offsetMax = new Vector2(-pad, -top);
        }

        // "Esperando respuesta..." justo encima de los controles; aviso de pista bajo el HUD
        BottomBand(e.waiting, e.panel, 0f, 1f, bottomArea, RowHeight * 0.6f, pad, gap);
        TopBand(e.clueNotice, e.panel, 0.1f, 0.9f, t.hudHeight + gap, RowHeight * 1.6f, pad, 0f);
    }

    // Franja anclada arriba: de 'fromTop' a 'fromTop + height' px, entre xMin y xMax del ancho
    private static void TopBand(RectTransform rect, RectTransform panel, float xMin, float xMax, float fromTop, float height, float pad, float gap)
    {
        if (rect == null)
            return;

        Adopt(rect, panel);
        rect.anchorMin = new Vector2(xMin, 1f);
        rect.anchorMax = new Vector2(xMax, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(xMin == 0f ? pad : gap * 0.5f, -(fromTop + height) + gap * 0.5f);
        rect.offsetMax = new Vector2(xMax == 1f ? -pad : -gap * 0.5f, -fromTop - gap * 0.5f);
    }

    // Franja anclada abajo: de 'fromBottom' a 'fromBottom + height' px
    private static void BottomBand(RectTransform rect, RectTransform panel, float xMin, float xMax, float fromBottom, float height, float pad, float gap)
    {
        if (rect == null)
            return;

        Adopt(rect, panel);
        rect.anchorMin = new Vector2(xMin, 0f);
        rect.anchorMax = new Vector2(xMax, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(xMin == 0f ? pad : gap * 0.5f, fromBottom);
        rect.offsetMax = new Vector2(xMax == 1f ? -pad : -gap * 0.5f, fromBottom + height);
    }

    // Los elementos pasan a colgar directamente del panel para que los anclajes sean relativos a él
    private static void Adopt(RectTransform rect, RectTransform panel)
    {
        if (rect.parent != panel)
            rect.SetParent(panel, false);
    }
}
