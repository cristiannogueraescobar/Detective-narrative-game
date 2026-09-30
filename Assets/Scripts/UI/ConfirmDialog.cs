using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pregunta de confirmación a pantalla completa: fondo oscuro, caja centrada que mide lo que su contenido, botón
/// principal y botón de cancelar. Se crea una vez por nombre dentro de 'parent' y después se reutiliza.
/// Los botones se llaman como su texto (los tests y la navegación los encuentran por nombre).
/// </summary>
public static class ConfirmDialog
{
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
            UIFactory.Button(box, yes, true, () => { }).name = yes;
            Button cancel = UIFactory.Button(box, no, false, () => { });
            cancel.name = no;
            cancel.onClick.AddListener(() => dialog.gameObject.SetActive(false));
        }

        // El texto y la acción pueden cambiar entre usos (p. ej. cuántas preguntas quedan)
        dialog.Find("Caja/Pregunta").GetComponent<TMP_Text>().text = question;
        Button confirm = dialog.Find("Caja/" + yes).GetComponent<Button>();
        confirm.onClick.RemoveAllListeners();
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
    /// Cierra el aviso si está abierto (botón Atrás). Devuelve true si había algo que cerrar.
    /// </summary>
    public static bool Hide(Transform parent, string name)
    {
        Transform dialog = parent != null ? parent.Find(name) : null;
        if (dialog == null || !dialog.gameObject.activeSelf)
            return false;
        dialog.gameObject.SetActive(false);
        return true;
    }
}
