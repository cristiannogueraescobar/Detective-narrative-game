using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PROTOTIPO C3 (no es un test: [Explicit]): el mismo retrato en plano 2D, 2.5D (relieve + lámpara) y vóxel,
/// para elegir con capturas. Salida: docs/screenshots/&lt;fecha&gt;/c3-prototipo/&lt;personaje&gt;_&lt;estilo&gt;.png
/// </summary>
[Explicit, Category("Capturas")]
public class CharacterStyleCapture
{
    private const int Width = 540;
    private const int Height = 720;
    private static readonly string[] Characters = { "padre", "madre", "vecina", "cartero" };
    private static readonly string[] Keys = { "Padre", "Madre", "Vecina", "Cartero" };

    private string folder;

    [SetUp]
    public void SetUp()
    {
        folder = Path.Combine("docs", "screenshots", DateTime.Now.ToString("yyyy-MM-dd"), "c3-prototipo");
        Directory.CreateDirectory(folder);
    }

    private static Texture2D Load(string name)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(File.ReadAllBytes(Path.Combine("Assets", "Images", "Suspects", name + ".gif.png")));
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    private void Save(RenderTexture target, string name)
    {
        RenderTexture.active = target;
        var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        image.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
        UnityEngine.Object.Destroy(image);
    }

    private static Camera MakeCamera(RenderTexture target, Color background)
    {
        Camera camera = new GameObject("Cámara prototipo", typeof(Camera)).GetComponent<Camera>();
        camera.targetTexture = target;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        return camera;
    }

    // Plano o 2.5D: RawImage con el material pedido sobre un lienzo de cámara
    private IEnumerator ShootUI(Texture2D texture, string key, Material material, string name)
    {
        var target = new RenderTexture(Width, Height, 24);
        Camera camera = MakeCamera(target, ThemeManager.Current.background);
        var canvasGo = new GameObject("Lienzo prototipo", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10f;
        var image = new GameObject("Retrato", typeof(RawImage)).GetComponent<RawImage>();
        image.transform.SetParent(canvas.transform, false);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
        image.texture = texture;
        image.uvRect = PortraitCrops.Bust(key);
        if (material != null)
            image.material = material;
        else
            ArtGrading.Apply(image, ArtGrading.Kind.LegacyPortrait); // Relieve con los valores del tema
        yield return null;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        Save(target, name);
        UnityEngine.Object.Destroy(canvasGo);
        UnityEngine.Object.Destroy(camera.gameObject);
        target.Release();
    }

    private IEnumerator ShootVoxel(Texture2D texture, string key, int columns, string name)
    {
        Theme theme = ThemeManager.Current;
        var target = new RenderTexture(Width, Height, 24);
        Camera camera = MakeCamera(target, theme.background);
        camera.fieldOfView = 24f;

        Color32[,] grid = VoxelPortrait.Sample(texture, PortraitCrops.Bust(key), columns);
        int[,] depth = VoxelPortrait.Depths(grid, 6);
        Mesh mesh = VoxelPortrait.Build(grid, depth, 1f);
        var model = new GameObject("Vóxel", typeof(MeshFilter), typeof(MeshRenderer));
        model.transform.position = new Vector3(0, -2000, 0);
        model.GetComponent<MeshFilter>().sharedMesh = mesh;
        var material = new Material(Shader.Find(VoxelPortrait.ShaderName));
        material.SetFloat("_Saturation", theme.legacyPortraitSaturation);
        material.SetColor("_Grade", theme.legacyPortraitGrade);
        material.SetFloat("_Brightness", theme.legacyPortraitBrightness * 1.1f);
        model.GetComponent<MeshRenderer>().sharedMaterial = material;

        // Tres cuartos: un poco desde la izquierda y desde arriba, como la lámpara
        float size = Mathf.Max(grid.GetLength(0), grid.GetLength(1) * Width / (float)Height);
        float distance = size / (2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad)) * 1.05f;
        camera.transform.position = model.transform.position + Quaternion.Euler(8f, 18f, 0) * new Vector3(0, 0, -distance);
        camera.transform.LookAt(model.transform.position + new Vector3(0, 0, 3f));
        camera.nearClipPlane = 1f;
        camera.farClipPlane = distance * 3f;
        yield return null;
        camera.Render();
        Save(target, name);
        UnityEngine.Object.Destroy(model);
        UnityEngine.Object.Destroy(camera.gameObject);
        target.Release();
        Debug.Log($"[C3] {name}: {mesh.vertexCount} vértices");
    }

    [UnityTest]
    public IEnumerator Estilos()
    {
        Theme theme = ThemeManager.Current;
        var flat = new Material(Shader.Find(ArtGrading.ShaderName));
        flat.SetFloat("_Saturation", theme.legacyPortraitSaturation);
        flat.SetColor("_Grade", theme.legacyPortraitGrade);
        flat.SetFloat("_Brightness", theme.legacyPortraitBrightness);
        Material lit = null; // null = ArtGrading con el tema

        for (int i = 0; i < Characters.Length; i++)
        {
            Texture2D texture = Load(Characters[i]);
            yield return ShootUI(texture, Keys[i], flat, Characters[i] + "_1_plano");
            yield return ShootUI(texture, Keys[i], lit, Characters[i] + "_2_relieve");
            if (Environment.GetCommandLineArgs().Contains("-voxel"))
            {
                yield return ShootVoxel(texture, Keys[i], 40, Characters[i] + "_3_voxel40");
                yield return ShootVoxel(texture, Keys[i], 64, Characters[i] + "_4_voxel64");
            }
        }
        Assert.Pass();
    }
}
