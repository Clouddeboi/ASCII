#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

//SaveManager only has an active slot after going through MainMenu's save-slot flow (NewGame/LoadGame).
//Pressing Play directly on a scene like Game skips that flow, which is why "No active save slot" warnings
//show up while testing. These menu items force Play to always start from MainMenu regardless of which
//scene is open in the editor.
public static class PlayModeStartSceneSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("ASCII/Play Mode/Always Start From Main Menu")]
    private static void SetMainMenuAsStartScene()
    {
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
        if (sceneAsset == null)
        {
            Debug.LogError($"[PlayModeStartSceneSetup] Could not find scene at '{MainMenuScenePath}'.");
            return;
        }

        EditorSceneManager.playModeStartScene = sceneAsset;
        Debug.Log("[PlayModeStartSceneSetup] Play will now always start from MainMenu, regardless of which scene is open.");
    }

    [MenuItem("ASCII/Play Mode/Clear Forced Start Scene")]
    private static void ClearStartScene()
    {
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("[PlayModeStartSceneSetup] Play will now start from whichever scene is currently open.");
    }
}
#endif
