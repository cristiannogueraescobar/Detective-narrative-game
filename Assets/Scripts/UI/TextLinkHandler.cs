using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enlaces tocables dentro de un texto TMP (&lt;link="id"&gt;…&lt;/link&gt;): avisa con el id del enlace tocado.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextLinkHandler : MonoBehaviour, IPointerClickHandler
{
    public Action<string> onLink;

    public void OnPointerClick(PointerEventData eventData)
    {
        var text = GetComponent<TMP_Text>();
        int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (index < 0 || index >= text.textInfo.linkCount)
            return;
        onLink?.Invoke(text.textInfo.linkInfo[index].GetLinkID());
    }
}
