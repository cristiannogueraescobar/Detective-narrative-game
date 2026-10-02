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
        public float[] cellWidths;   // A la medida de cada figura; con startX y endX suman el ancho de la fila
        public float startX;         // Con solape, sitio a los lados para lo que asoma de la primera y la última figura
        public float MarkY(int cm) => labelBand + cm * pxPerCm;
    }

    public const int WallTopCm = 200;
    private const float Padding = 0.92f; // Hueco entre figuras
    private const float TopLabelRoom = 24f;

    /// <summary>
    /// Escala y alturas de las figuras para un área (ancho × alto en px), la franja de nombres de abajo y, por
    /// sospechoso, su altura (cm) y la proporción ancho/alto de su figura.
    /// </summary>
    /// <param name="overlap">Parte del ancho de cada figura que puede quedar delante o detrás de la vecina (0 = cada
    /// una en su hueco). Los huecos (botón y anillo) no se solapan: es la figura la que asoma.</param>
    public static Layout Plan(float width, float height, float labelBand, IList<(int cm, float aspect)> people, float overlap = 0f)
    {
        int n = Mathf.Max(1, people.Count);
        // Huecos a la medida de cada figura: la limita el ancho de todas juntas, no la más ancha
        float widthPerScale = 0f;
        foreach (var (cm, aspect) in people)
            widthPerScale += Mathf.Max(0.05f, aspect) * (1f - overlap) * Mathf.Max(1, cm);
        // Lo que asoma por los extremos no puede salirse de la fila (taparía las cifras de la pared)
        float edgeStart = 0f, edgeEnd = 0f;
        if (people.Count > 0)
        {
            edgeStart = Mathf.Max(0.05f, people[0].aspect) * Mathf.Max(1, people[0].cm) * overlap * 0.5f;
            edgeEnd = Mathf.Max(0.05f, people[people.Count - 1].aspect) * Mathf.Max(1, people[people.Count - 1].cm) * overlap * 0.5f;
        }
        widthPerScale += edgeStart + edgeEnd;
        float scale = (height - labelBand - TopLabelRoom) / WallTopCm; // La cifra de 200 tiene que caber encima de su raya
        if (widthPerScale > 0f)
            scale = Mathf.Min(scale, width * (overlap > 0f ? 1f : Padding) / widthPerScale); // Con solape no hace falta hueco
        var heights = new float[people.Count];
        var cells = new float[people.Count];
        float used = 0f;
        for (int i = 0; i < people.Count; i++)
        {
            heights[i] = people[i].cm * scale;
            cells[i] = heights[i] * Mathf.Max(0.05f, people[i].aspect) * (1f - overlap);
            used += cells[i];
        }
        float start = edgeStart * scale, end = edgeEnd * scale;
        float extra = people.Count > 0 ? (width - start - end - used) / n : 0f; // El sobrante, repartido
        for (int i = 0; i < people.Count; i++)
            cells[i] += extra;
        return new Layout { pxPerCm = scale, labelBand = labelBand, figureHeights = heights, cellWidths = cells, startX = start };
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
        // Revisión de Cristian (sesión C): con tres o más, el ancho limitaba y media pantalla quedaba vacía
        Layout plan = Plan(rect.rect.width, rect.rect.height, labelBand, people, ThemeManager.Current.lineupOverlap);
        // Pero cada hueco se sigue tocando con el pulgar: si alguno baja de 48 dp, sin solape
        foreach (float cell in plan.cellWidths)
        {
            if (cell - 6f < Theme.MinTouchSize)
            {
                plan = Plan(rect.rect.width, rect.rect.height, labelBand, people);
                break;
            }
        }
        m_Tracker.Clear();
        float x = plan.startX;
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
