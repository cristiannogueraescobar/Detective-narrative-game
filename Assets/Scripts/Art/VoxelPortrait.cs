using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Retrato en vóxeles (prototipo C3): reduce el encuadre del retrato a una rejilla de columnas × filas, levanta
/// un cubo por celda opaca (más hondo cuanto más lejos del borde: volumen de cuerpo) y dibuja la malla con una
/// cámara propia sobre una textura. El archivo original no se toca; todo se crea en memoria.
/// </summary>
public static class VoxelPortrait
{
    public const string ShaderName = "Detective/VoxelLit";

    /// <summary>
    /// Rejilla de colores (alfa 0 = vacío) a partir del encuadre 'crop' de la textura, 'columns' de ancho.
    /// La textura debe ser legible (o se lee con una copia por GPU).
    /// </summary>
    public static Color32[,] Sample(Texture2D texture, Rect crop, int columns)
    {
        Texture2D readable = Readable(texture);
        int w = Mathf.RoundToInt(crop.width * texture.width);
        int h = Mathf.RoundToInt(crop.height * texture.height);
        int rows = Mathf.Max(1, Mathf.RoundToInt(columns * (float)h / w));
        var grid = new Color32[columns, rows];
        float cell = (float)w / columns;
        int x0 = Mathf.RoundToInt(crop.x * texture.width);
        int y0 = Mathf.RoundToInt(crop.y * texture.height);

        for (int cx = 0; cx < columns; cx++)
        for (int cy = 0; cy < rows; cy++)
        {
            // Color dominante de la celda (media de los píxeles opacos); vacía si es mayoría transparente
            float r = 0, g = 0, b = 0;
            int opaque = 0, total = 0;
            int step = Mathf.Max(1, Mathf.RoundToInt(cell / 4));
            for (float px = cx * cell; px < (cx + 1) * cell; px += step)
            for (float py = cy * cell; py < (cy + 1) * cell; py += step)
            {
                Color c = readable.GetPixel(x0 + (int)px, y0 + (int)py);
                total++;
                if (c.a < 0.5f)
                    continue;
                opaque++;
                r += c.r; g += c.g; b += c.b;
            }
            grid[cx, cy] = opaque * 2 < total ? new Color32(0, 0, 0, 0)
                : (Color32)new Color(r / opaque, g / opaque, b / opaque, 1f);
        }

        if (readable != texture)
            Object.Destroy(readable);
        return grid;
    }

    /// <summary>
    /// Profundidad de cada celda: 1 en el borde, hasta 'maxDepth' en el interior (distancia al vacío).
    /// </summary>
    public static int[,] Depths(Color32[,] grid, int maxDepth)
    {
        int cols = grid.GetLength(0), rows = grid.GetLength(1);
        var dist = new int[cols, rows];
        var queue = new Queue<Vector2Int>();
        for (int x = 0; x < cols; x++)
        for (int y = 0; y < rows; y++)
        {
            bool edge = grid[x, y].a == 0;
            dist[x, y] = edge ? 0 : int.MaxValue;
            if (edge)
                queue.Enqueue(new Vector2Int(x, y));
        }
        // Fuera de la rejilla también es vacío
        for (int x = 0; x < cols; x++)
        for (int y = 0; y < rows; y++)
            if (grid[x, y].a != 0 && (x == 0 || y == 0 || x == cols - 1 || y == rows - 1))
            {
                dist[x, y] = 1;
                queue.Enqueue(new Vector2Int(x, y));
            }

        var steps = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            foreach (Vector2Int s in steps)
            {
                Vector2Int q = p + s;
                if (q.x < 0 || q.y < 0 || q.x >= cols || q.y >= rows || dist[q.x, q.y] <= dist[p.x, p.y] + 1)
                    continue;
                dist[q.x, q.y] = dist[p.x, p.y] + 1;
                queue.Enqueue(q);
            }
        }

        var depth = new int[cols, rows];
        for (int x = 0; x < cols; x++)
        for (int y = 0; y < rows; y++)
            depth[x, y] = grid[x, y].a == 0 ? 0 : Mathf.Min(maxDepth, dist[x, y]);
        return depth;
    }

    /// <summary>
    /// Malla de cubos: solo las caras visibles (frente, y laterales donde el vecino es menos hondo).
    /// El frente queda en z = 0 y el volumen crece hacia +z (lejos de la cámara).
    /// </summary>
    public static Mesh Build(Color32[,] grid, int[,] depth, float voxel)
    {
        int cols = grid.GetLength(0), rows = grid.GetLength(1);
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colors = new List<Color32>();
        var triangles = new List<int>();
        Vector3 origin = new Vector3(-cols * voxel / 2f, -rows * voxel / 2f, 0);

        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 n, Color32 color)
        {
            int i = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            for (int k = 0; k < 4; k++) { normals.Add(n); colors.Add(color); }
            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
        }

        int D(int x, int y) => x < 0 || y < 0 || x >= cols || y >= rows ? 0 : depth[x, y];

        for (int x = 0; x < cols; x++)
        for (int y = 0; y < rows; y++)
        {
            int d = depth[x, y];
            if (d == 0)
                continue;
            Color32 c = grid[x, y];
            Vector3 p = origin + new Vector3(x * voxel, y * voxel, 0);
            float z = d * voxel;
            Vector3 v = new Vector3(voxel, 0, 0), u = new Vector3(0, voxel, 0);

            // Frente (mira a -z, hacia la cámara)
            Quad(p, p + u, p + u + v, p + v, Vector3.back, c);
            // Laterales: la parte que asoma sobre el vecino
            Color32 side = Darken(c, 0.85f);
            if (D(x - 1, y) < d)
                Quad(p + new Vector3(0, 0, z), p + u + new Vector3(0, 0, z), p + u, p, Vector3.left, side);
            if (D(x + 1, y) < d)
                Quad(p + v, p + v + u, p + v + u + new Vector3(0, 0, z), p + v + new Vector3(0, 0, z), Vector3.right, side);
            if (D(x, y + 1) < d)
                Quad(p + u, p + u + new Vector3(0, 0, z), p + u + v + new Vector3(0, 0, z), p + u + v, Vector3.up, side);
            if (D(x, y - 1) < d)
                Quad(p + new Vector3(0, 0, z), p, p + v, p + v + new Vector3(0, 0, z), Vector3.down, side);
        }

        var mesh = new Mesh { name = "Retrato vóxel", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    private static Color32 Darken(Color32 c, float k)
    {
        return new Color32((byte)(c.r * k), (byte)(c.g * k), (byte)(c.b * k), c.a);
    }

    private static Texture2D Readable(Texture2D texture)
    {
        if (texture.isReadable)
            return texture;
        RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(texture, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        copy.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }
}
