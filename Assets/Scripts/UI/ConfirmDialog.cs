using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pregunta de confirmación a pantalla completa: fondo oscuro, caja centrada que mide lo que su contenido, botón
/// principal y botón de cancelar. Se crea una vez por nombre dentro de 'parent' y después se reutiliza; la
/// pregunta, los textos de los botones y la acción se renuevan en cada uso.
/// </summary>
public static class ConfirmDialog
{
    public const string ConfirmName = "Confirmar";
    public const string CancelName = "Cancelar";

    public static RectTransform Show(Transform parent, string name, string question, string yes, string no, Action onYes)
    {
        Theme theme = ThemeManager.Current;
        RectTransform dialog = parent.Find(name) as RectTransform;
        if (dialog == null)
        {
            dialog = UIFactory.Container(parent, name, Vector2.zero, Vector2.one);
            dialog.gameObject.AddComponent<Image>().color = theme.overlay;
            UIComponents.GetOrAdd<LayoutElement>(dialog.gameObject).ignoreLayout = true;

            // Caja centrada que mide lo que su contenido
            RectTransform box = UIFactory.Container(dialog, "Caja", new Vector2(0.06f, 0.5f), new Vector2(0.94f, 0.5f));
            box.pivot = new Vector2(0.5f, 0.5f);
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = theme.panel; // Más oscura que el botón secundario: "Cancelar" se ve como botón
            boxImage.sprite = UISprites.Rounded(20);
            boxImage.type = Image.Type.Sliced;
            boxImage.gameObject.AddComponent<ThemeRole>().role = UIRole.Ignore;
            UIFactory.VerticalLayout(box, theme.spacing, theme.padding);
            box.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TMP_Text label = UIFactory.Label(box, question, theme.bodySize, theme.textPrimary);
            label.name = "Pregunta";
            label.alignment = TextAlignmentOptions.Center;
            label.GetComponent<LayoutElement>().minHeight = theme.bodySize * 3.2f; // Dos líneas holgadas
            UIFactory.Button(box, yes, true, () => { }).name = ConfirmName;
            Button cancel = UIFactory.Button(box, no, false, () => { });
            cancel.name = CancelName;
            cancel.onClick.AddListener(() => dialog.gameObject.SetActive(false));
        }

        dialog.Find("Caja/Pregunta").GetComponent<TMP_Text>().text = question;
        Button confirm = dialog.Find("Caja/" + ConfirmName).GetComponent<Button>();
        confirm.GetComponentInChildren<TMP_Text>(true).text = yes;
        dialog.Find("Caja/" + CancelName).GetComponentInChildren<TMP_Text>(true).text = no;
        confirm.onClick.RemoveAllListeners(); // El sonido del clic no depende de onClick (ClickSound)
        confirm.onClick.AddListener(() =>
        {
            dialog.gameObject.SetActive(false);
            onYes?.Invoke();
        });

        dialog.gameObject.SetActive(true);
        dialog.SetAsLastSibling();
        return dialog;
    }

    /// <summary>
    /// ¿Está abierto el aviso con este nombre?
    /// </summary>
    public static bool IsOpen(Transform parent, string name)
    {
        Transform dialog = parent != null ? parent.Find(name) : null;
        return dialog != null && dialog.gameObject.activeSelf;
    }

    /// <summary>
    /// Cierra el aviso si está abierto (botón Atrás). Devuelve true si había algo que cerrar.
    /// </summary>
    public static bool Hide(Transform parent, string name)
    {
        if (!IsOpen(parent, name))
            return false;
        parent.Find(name).gameObject.SetActive(false);
        return true;
    }
}
