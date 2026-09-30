using NUnit.Framework;

public class BuildScriptTests
{
    [Test]
    public void SinArgumentoUsaLaRutaDeSiempre()
    {
        Assert.AreEqual("Builds/Windows/Detectives.exe", BuildScript.OutputPath(new[] { "Unity.exe", "-batchmode" }, "Builds/Windows/Detectives.exe"));
    }

    [Test]
    public void ElArgumentoCambiaLaRuta()
    {
        Assert.AreEqual("Builds/Otra/Detectives.exe",
            BuildScript.OutputPath(new[] { "Unity.exe", "-buildPath", "Builds/Otra/Detectives.exe" }, "Builds/Windows/Detectives.exe"));
        Assert.AreEqual("Builds/Windows/Detectives.exe", BuildScript.OutputPath(new[] { "-buildPath" }, "Builds/Windows/Detectives.exe"), "sin valor, la de siempre");
    }
}
