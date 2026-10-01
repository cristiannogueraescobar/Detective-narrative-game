using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rueda de reconocimiento en una sola fila con alturas reales (Sesión A): todos pisan el mismo suelo (encima de la
/// franja de los nombres) y miden su altura en centímetros a la misma escala que las rayas de la pared de comisaría.
/// La escala es la mayor que deja ver 2 m de pared y que las figuras, una al lado de otra, quepan en la fila.
/// Es un LayoutGroup: Unity la recoloca sola al cambiar de tamaño (también en el editor y en la vista previa).
/// </summary>
public class HeightLineup : LayoutGroup
{
    public struct Layout
    {
        public float pxPerCm;
        public float labelBand;
        public float[] figureHeights;
        public float[] cellWidths;   // A la medida de cada figura; suman el ancho de la fila
        public float MarkY(int cm) => labelBand + cm * pxPerCm;
    }

    public const int WallTopCm = 200;
    private const float Padding = 0.92f; // Hueco entre figuras

    /// <summary>
    /// Escala y alturas de las figuras para un área (ancho × alto en px), la franja de nombres de abajo y, por
    /// sospechoso, su altura (cm) y la proporción ancho/alto de su figura.
    /// </summary>
    public static Layout Plan(float width, float height, float labelBand, IList<(int cm, float aspect)> people)
    {
        int n = Mathf.Max(1, people.Count);
        // Huecos a la medida de cada figura: la limita el ancho de todas juntas, no la más ancha
        float widthPerScale = 0f;
        foreach (var (cm, aspect) in people)
            widthPerScale += Mathf.Max(0.05f, aspect) * Mathf.Max(1, cm);
        float scale = (height - labelBand) / WallTopCm;
        if (widthPerScale > 0f)
            scale = Mathf.Min(scale, width * Padding / widthPerScale);
        var heights = new float[people.Count];
        var cells = new float[people.Count];
        float used = 0f;
        for (int i = 0; i < people.Count; i++)
        {
            heights[i] = people[i].cm * scale;
            cells[i] = heights[i] * Mathf.Max(0.05f, people[i].aspect);
            used += cells[i];
        }
        float extra = people.Count > 0 ? (width - used) / n : 0f; // El sobrante, repartido
        for (int i = 0; i < people.Count; i++)
            cells[i] += extra;
        return new Layout { pxPerCm = scale, labelBand = labelBand, figureHeights = heights, cellWidths = cells };
    }

    // ---------- En escena ----------

    public float labelBand = 64f;
    public RectTransform marks;  // Las rayas de la pared ("Alturas"): se recolocan a la misma escala

    public override void CalculateLayoutInputVertical() { }
    public override void SetLayoutHorizontal() => Relayout();
    public override void SetLayoutVertical() { }

    public void Relayout()
    {
        var rect = rectTransform;
        var items = new List<HeightLineupItem>();
        for (int i = 0; i < rect.childCount; i++)
        {
            var item = rect.GetChild(i).GetComponent<HeightLineupItem>();
            if (item != null && item.gameObject.activeSelf)
                items.Add(item);
        }
        var people = new List<(int, float)>();
        foreach (HeightLineupItem it in items)
            people.Add((it.heightCm, it.aspect));
        Layout plan = Plan(rect.rect.width, rect.rect.height, labelBand, people);
        m_Tracker.Clear();
        float x = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            var cellRect = (RectTransform)items[i].transform;
            m_Tracker.Add(this, cellRect, DrivenTransformProperties.All);
            float figureHeight = plan.figureHeights[i];
            float figureWidth = figureHeight * items[i].aspect;
            cellRect.anchorMin = cellRect.anchorMax = Vector2.zero;
            cellRect.pivot = new Vector2(0.5f, 0f);
            // La celda (botón y anillo de selección) envuelve figura y nombre, sin salirse de su hueco
            float cell = plan.cellWidths[i];
            cellRect.sizeDelta = new Vector2(cell - 6f, labelBand + figureHeight + 8f);
            cellRect.anchoredPosition = new Vector2(x + cell * 0.5f, 0f);
            x += cell;
            if (items[i].figure != null)
            {
                RectTransform f = items[i].figure;
                f.anchorMin = f.anchorMax = new Vector2(0.5f, 0f);
                f.pivot = new Vector2(0.5f, 0f);
                f.sizeDelta = new Vector2(figureWidth, figureHeight);
                f.anchoredPosition = new Vector2(0f, labelBand);
            }
        }
        PlaceMarks(plan);
    }

    // Las rayas "Linea N" / "Cifra N" de la pared, a N cm del suelo
    private void PlaceMarks(Layout plan)
    {
        if (marks == null)
            return;
        float wallHeight = marks.rect.height;
        foreach (RectTransform child in marks)
        {
            string[] parts = child.name.Split(' ');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int cm))
                continue;
            float y = plan.MarkY(cm);
            // Por encima de la pared, sin dibujar: escala 0 (SetActive no se puede dentro del cálculo del layout)
            child.localScale = y <= wallHeight - 2f ? Vector3.one : Vector3.zero;
            child.anchorMin = new Vector2(child.anchorMin.x, 0f);
            child.anchorMax = new Vector2(child.anchorMax.x, 0f);
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
        }
    }
}
