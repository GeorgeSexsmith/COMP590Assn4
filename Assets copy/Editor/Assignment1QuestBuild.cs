using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Assignment1QuestBuild
{
    static Assignment1QuestBuild()
    {
        EditorApplication.update += ProcessBuildRequest;
    }

    private static void ProcessBuildRequest()
    {
        const string request = "Temp/Assignment1Quest-build.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(request);
        try { Build(); }
        catch (Exception error)
        {
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/Assignment1Quest-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    public const string ScenePath = "Assets/Assignment1/Assignment1Quest.unity";

    [MenuItem("Assignment 1/Build Quest APK")]
    public static void Build()
    {
        GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Assignment1/Settings/Mobile_RPAsset.asset");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Validate();
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        Directory.CreateDirectory("Builds");
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Assignment1Quest.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        File.WriteAllText("Builds/Assignment1Quest-build.txt", report.summary.result +
            "\nErrors: " + report.summary.totalErrors + "\nSize: " + report.summary.totalSize);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Quest build failed. See Console and Builds/Assignment1Quest-build.txt.");
    }

    [MenuItem("Assignment 1/Validate Quest Scene")]
    public static void Validate()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        try
        {
            int cameras = 0, rigs = 0, shooters = 0, targets = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0)
                        throw new Exception("Missing script on " + t.name);
                foreach (Camera c in root.GetComponentsInChildren<Camera>(true))
                    if (c.enabled) cameras++;
                rigs += root.GetComponentsInChildren<OVRCameraRig>(true).Length;
                foreach (ButtonClick shooter in root.GetComponentsInChildren<ButtonClick>(true))
                {
                    shooters++;
                    if (!shooter.ballPreFab || !shooter.ballPreFab.GetComponent<Arrow>() ||
                        !shooter.ballPreFab.GetComponent<Rigidbody>())
                        throw new Exception("Arrow prefab is not wired correctly.");
                    if (!shooter.GetComponent<Camera>()) throw new Exception("Shooter must use head aiming.");
                }
                foreach (Hit target in root.GetComponentsInChildren<Hit>(true))
                {
                    targets++;
                    if (!target.ScoreBoard || !target.GetComponent<Collider>())
                        throw new Exception("Target needs its original collider and scoreboard.");
                    if (target.ScoreBoard.canvas.renderMode != RenderMode.WorldSpace)
                        throw new Exception("Quest scoreboard must be world space.");
                }
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                    foreach (Material material in renderer.sharedMaterials)
                        if (!material || !material.shader || material.shader.name == "Hidden/InternalErrorShader")
                            throw new Exception("Missing material/shader on " + renderer.name);
            }
            if (cameras != 1 || rigs != 1 || shooters != 1 || targets != 1)
                throw new Exception($"Unexpected scene wiring: cameras={cameras}, rigs={rigs}, shooters={shooters}, targets={targets}");
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/Assignment1Quest-validation.txt", "PASS: scene loads; no missing scripts; one active camera, Quest rig, head-directed shooter, arrow rigidbody, target collider and world-space scoreboard.\n");
            Debug.Log("Assignment 1 Quest scene validation passed.");
        }
        finally { EditorSceneManager.CloseScene(scene, true); }
    }
}
