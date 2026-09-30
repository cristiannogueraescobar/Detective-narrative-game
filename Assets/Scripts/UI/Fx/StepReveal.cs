using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Descubre un texto línea a línea, con una pausa más larga en las líneas en blanco (informe final, línea
/// temporal). Un toque lo muestra todo. Con "reducir animaciones", todo a la vez.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class StepReveal : MonoBehaviour, IPointerClickHandler
{
    public float lineDelay = 0.18f;
    public float paragraphDelay = 0.6f;
    public float charactersPerSecond = 700f;

    private TMP_Text text;
    private Coroutine routine;

    public bool IsRevealing => routine != null;

    public void Play(float startDelay = 0f)
    {
        text = GetComponent<TMP_Text>();
        Stop();
        if (!Application.isPlaying || GameSettings.ReduceMotion || !isActiveAndEnabled)
        {
            text.maxVisibleCharacters = 99999;
            return;
        }
        text.maxVisibleCharacters = 0;
        routine = StartCoroutine(Reveal(startDelay));
    }

    public void Stop()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = null;
        if (text != null)
            text.maxVisibleCharacters = 99999;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Stop();
    }

    private void OnDisable()
    {
        Stop();
    }

    private IEnumerator Reveal(float startDelay)
    {
        yield return new WaitForSecondsRealtime(startDelay);
        text.ForceMeshUpdate();
        TMP_TextInfo info = text.textInfo;

        for (int line = 0; line < info.lineCount; line++)
        {
            TMP_LineInfo li = info.lineInfo[line];
            int end = li.lastCharacterIndex + 1;
            bool blank = li.visibleCharacterCount == 0;

            float shown = text.maxVisibleCharacters;
            while (shown < end)
            {
                shown += charactersPerSecond * Time.unscaledDeltaTime;
                text.maxVisibleCharacters = Mathf.Min((int)shown, end);
                yield return null;
            }
            text.maxVisibleCharacters = end;

            // Las líneas partidas por el ajuste no esperan; solo los saltos de línea reales
            bool hardBreak = end >= info.characterCount || info.characterInfo[Mathf.Max(0, end - 1)].character == '\n';
            if (hardBreak)
                yield return new WaitForSecondsRealtime(blank ? paragraphDelay : lineDelay);
        }

        text.maxVisibleCharacters = 99999;
        routine = null;
    }
}
