using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Efecto de texto escribiéndose sobre un TMP_Text que acumula la conversación: revela solo lo nuevo.
/// Un toque sobre el texto completa la escritura al momento.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class Typewriter : MonoBehaviour, IPointerClickHandler
{
    public const float BaseCharactersPerSecond = 45f;

    private TMP_Text text;
    private Coroutine routine;
    private bool skipRequested;

    public bool IsTyping => routine != null;
    public event Action OnFinished;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Número de caracteres visibles ahora mismo (antes de añadir texto nuevo).
    /// </summary>
    public int VisibleCount()
    {
        text.ForceMeshUpdate();
        return text.textInfo.characterCount;
    }

    public void Reveal(int fromCharacter, float speedMultiplier)
    {
        Complete();
        skipRequested = false;
        routine = StartCoroutine(Type(fromCharacter, BaseCharactersPerSecond * Mathf.Max(0.1f, speedMultiplier)));
    }

    /// <summary>
    /// Muestra todo el texto al momento (cambio de sospechoso, toque, etc.).
    /// </summary>
    public void Complete()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
            OnFinished?.Invoke();
        }

        if (text != null)
            text.maxVisibleCharacters = 99999;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        skipRequested = true;
    }

    private void OnDisable()
    {
        Complete();
    }

    private IEnumerator Type(int from, float charactersPerSecond)
    {
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        float shown = from;

        while (shown < total && !skipRequested)
        {
            text.maxVisibleCharacters = (int)shown;
            shown += charactersPerSecond * Time.unscaledDeltaTime;
            yield return null;
        }

        text.maxVisibleCharacters = 99999;
        routine = null;
        OnFinished?.Invoke();
    }
}
