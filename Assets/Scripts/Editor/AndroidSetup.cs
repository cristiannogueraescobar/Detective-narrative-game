using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Ajustes de Android (Bloque E) en un solo sitio, repetibles: menú Detective/Android/Aplicar ajustes o
///   Unity -batchmode -projectPath . -executeMethod AndroidSetup.Apply -quit
/// Guía completa (módulo, firma, compilación, pruebas): docs/ANDROID-BUILD.md.
/// </summary>
public static class AndroidSetup
{
    // Provisional: el nombre definitivo lo decide Cristian (una vez publicado no se puede cambiar)
    public const string PackageName = "com.cristiannoguera.casos";
    public const int MinSdk = 26;     // Android 8.0: lo exige el lector de pantalla (módulo de accesibilidad de Unity 6)
    public const int TargetSdk = 36;  // Google Play exige API 36 desde el 31-08-2026 (apps nuevas y actualizaciones)

    [MenuItem("Detective/Android/Aplicar ajustes")]
    public static void Apply()
    {
        NamedBuildTarget android = NamedBuildTarget.Android;

        PlayerSettings.SetApplicationIdentifier(android, PackageName);
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP); // Obligatorio para ARM64
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinSdk;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetSdk;
        PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low); // JsonUtility y reflexión a salvo

        // Vertical, sin girar al revés (con una mano, el móvil boca abajo no es un caso real)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // Los sospechosos hablan por red (Anthropic por HTTPS; Ollama en la red local por HTTP)
        PlayerSettings.Android.forceInternetPermission = true;
        // HTTP sin cifrar solo en compilaciones de desarrollo (Ollama en el PC de casa); la versión publicada, solo HTTPS
        PlayerSettings.insecureHttpOption = InsecureHttpOption.DevelopmentOnly;

        // Icono de la app (arte nuevo de Assets/Art/Icons)
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Icons/app_icon.png");
        if (icon != null)
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

        AssetDatabase.SaveAssets();
        Debug.Log($"[Android] Ajustes aplicados: {PackageName}, IL2CPP ARM64, API {MinSdk}-{TargetSdk}, vertical, " +
                  "Internet, HTTP solo en desarrollo.");
    }
}
