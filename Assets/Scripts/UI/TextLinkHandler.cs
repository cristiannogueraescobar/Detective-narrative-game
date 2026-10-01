using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enlaces tocables dentro de un texto TMP (&lt;link="id"&gt;…&lt;/link&gt;): avisa con el id del enlace tocado.
/// Un enlace es una línea de texto (mucho menos de 48 dp de alto): un toque que cae cerca, a menos de media zona
/// táctil, cuenta como el enlace más próximo.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public Action<string> onLink;

    /// <summary>
    /// Cómo lo nombra el lector de pantalla (id del enlace, texto del enlace) → rótulo; sin él, el texto tal cual.
    /// Para dar contexto: "añadir nota" solo no dice de quién.
    /// </summary>
    public Func<string, string, string> describe;

    public string Describe(string id, string text) => describe?.Invoke(id, text) ?? text;

    public void OnPointerClick(PointerEventData eventData)
    {
        var text = GetComponent<TMP_Text>();
        int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (index < 0 && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                text.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 local))
            index = NearestLink(text, local, Theme.MinTouchSize * 0.5f);
        if (index < 0 || index >= text.textInfo.linkCount)
            return;
        onLink?.Invoke(text.textInfo.linkInfo[index].GetLinkID());
    }

    /// <summary>
    /// Índice del enlace más cercano a 'point' (coordenadas locales del texto) a menos de 'tolerance', o -1.
    /// </summary>
    public static int NearestLink(TMP_Text text, Vector2 point, float tolerance)
    {
        TMP_TextInfo info = text.textInfo;
        int best = -1;
        float bestDistance = tolerance;
        for (int i = 0; i < info.linkCount; i++)
        {
            TMP_LinkInfo link = info.linkInfo[i];
            for (int c = link.linkTextfirstCharacterIndex; c < link.linkTextfirstCharacterIndex + link.linkTextLength && c < info.characterCount; c++)
            {
                TMP_CharacterInfo ch = info.characterInfo[c];
                if (!ch.isVisible)
                    continue;
                // Distancia del punto a la caja del carácter (0 si está dentro)
                Rect box = Rect.MinMaxRect(ch.bottomLeft.x, ch.descender, ch.topRight.x, ch.ascender);
                float dx = Mathf.Max(box.xMin - point.x, 0f, point.x - box.xMax);
                float dy = Mathf.Max(box.yMin - point.y, 0f, point.y - box.yMax);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
        }
        return best;
    }
}
