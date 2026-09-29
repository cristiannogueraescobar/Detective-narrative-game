using NUnit.Framework;
using UnityEngine;

public class UIComponentsTests
{
    [Test]
    public void GetOrAddDevuelveUnComponenteRealAunqueFalte()
    {
        var go = new GameObject("GetOrAdd");
        try
        {
            CanvasGroup group = UIComponents.GetOrAdd<CanvasGroup>(go);

            Assert.IsTrue(group != null, "debe añadirse (GetComponent devuelve un null falso en el editor)");
            group.alpha = 0.5f; // Acceso nativo: lanzaría con un null falso
            Assert.AreSame(group, UIComponents.GetOrAdd<CanvasGroup>(go), "no debe añadirse dos veces");
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
