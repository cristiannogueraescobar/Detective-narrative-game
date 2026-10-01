using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reparte una rejilla de tarjetas de retrato (3:4 + nombre) en el hueco que tenga, sin salirse: elige columnas y
/// tamaño cada vez que cambia el tamaño del contenedor (rotación, otra pantalla, más sospechosos).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class GridFit : MonoBehaviour
{
    public int count;
    public float labelHeight = 64f;
    public float minCell = Theme.MinTouchSize;

    /// <summary>
    /// Columnas y tamaño de celda para 'count' tarjetas en un área dada.
    /// </summary>
    public static (int columns, Vector2 cell) Compute(int count, Vector2 area, float spacing, float labelHeight, float minCell)
    {
        count = Mathf.Max(1, count);
        int columns = count <= 2 ? count : count <= 4 ? 2 : 3;
        int rows = Mathf.CeilToInt(count / (float)columns);
        float cellWidth = (area.x - spacing * (columns - 1)) / columns;
        float cellHeight = (area.y - spacing * (rows - 1)) / rows;
        float portrait = Mathf.Min(cellWidth / 0.75f, cellHeight - labelHeight);
        float width = Mathf.Max(minCell, portrait * 0.75f);
        float height = Mathf.Max(minCell, portrait + labelHeight);
        // Nunca más que el hueco (aunque no llegue al mínimo)
        width = Mathf.Min(width, cellWidth);
        height = Mathf.Min(height, cellHeight);
        return (columns, new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, height)));
    }

    public void Fit()
    {
        var grid = GetComponent<GridLayoutGroup>();
        var (columns, cell) = Compute(count, ((RectTransform)transform).rect.size, grid.spacing.x, labelHeight, minCell);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = cell;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
            Fit();
    }
}
