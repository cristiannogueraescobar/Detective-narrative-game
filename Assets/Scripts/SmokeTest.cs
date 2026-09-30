using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Prueba de humo de una build: con el argumento -smoketest, espera a que arranque la escena, comprueba que
/// están el juego, la UI y el tema, escribe "SMOKE OK" (o el fallo) en el log y cierra.
///   Detectives.exe -batchmode -nographics -smoketest -logFile smoke.log
/// </summary>
public class SmokeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-smoketest") < 0)
            return;
        var go = new GameObject("Prueba de humo");
        DontDestroyOnLoad(go);
        go.AddComponent<SmokeTest>();
    }

    private IEnumerator Start()
    {
        int errors = 0;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (type == LogType.Exception || type == LogType.Error)
                errors++;
        };

        yield return new WaitForSecondsRealtime(3f);

        bool ok = FindFirstObjectByType<GameManager>() != null
                  && FindFirstObjectByType<InterrogationUI>() != null
                  && FindFirstObjectByType<MenuManager>() != null
                  && ThemeManager.Current != null
                  && ArtLibrary.Load(ArtSlots.IconNotebook) != null
                  && ArtLibrary.Load(ArtSlots.StoryIntro("1")) != null   // Arte de las historias (C5) en el catálogo
                  && Shader.Find(ArtGrading.LitShaderName) != null;     // Relieve de los retratos (C3) en la build

        Debug.Log(ok && errors == 0 ? "SMOKE OK" : $"SMOKE FAIL (escena completa: {ok}, errores: {errors})");
        Application.Quit(ok && errors == 0 ? 0 : 1);
    }
}
