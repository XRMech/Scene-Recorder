// using UnityEditor;
// using UnityEngine;
//
// public class BuildScript
// {
//     [MenuItem("Build/Build WebGL")]
//     public static void BuildWebGL()
//     {
//         // Remove or disable Meta components
//         var metaPackagePath = "Packages/com.meta.xr.sdk.voice";
//         if (AssetDatabase.IsValidFolder(metaPackagePath))
//         {
//             AssetDatabase.DeleteAsset(metaPackagePath);
//         }
//
//         // Set the build options
//         BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
//         buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
//         buildPlayerOptions.locationPathName = "Build/WebGL";
//         buildPlayerOptions.target = BuildTarget.WebGL;
//         buildPlayerOptions.options = BuildOptions.None;
//
//         // Build the project
//         BuildPipeline.BuildPlayer(buildPlayerOptions);
//
//         // Reimport the Meta package after the build
//         AssetDatabase.ImportPackage(metaPackagePath, true);
//     }
// }
