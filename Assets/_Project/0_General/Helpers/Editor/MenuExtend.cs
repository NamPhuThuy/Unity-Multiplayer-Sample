using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MenuExtend : MonoBehaviour
{
    const string SWITCH_SCENCE_MENU_NAME = "Tools/Switch Scene";
    private const string PATH_TO_SCENES_FOLDER = "Assets/_NetcodeExample//";

    private const string ALT = "&";
    private const string SHIFT = "#";
    private const string CTRL = "%";
    
#if UNITY_EDITOR
    [MenuItem(SWITCH_SCENCE_MENU_NAME + "/Boostrap/" + ALT + "1")]
    // [MenuItem(SWITCH_SCENCE_MENU_NAME + "/Intro &1")]
    
    static void Boots()
    {
        LoadSceneByName("0_General/Scenes/Boostrap");
    }


    [MenuItem(SWITCH_SCENCE_MENU_NAME + "/MainMenu " + ALT + "2")]
    static void MainMenu()
    {
        LoadSceneByName("MainMenu");

    }
    
    [MenuItem(SWITCH_SCENCE_MENU_NAME + "/GamePlay " + ALT + "3")]
    static void Gameplay()
    {
        LoadSceneByName("GamePlay");

    }
    
    static void LoadSceneByName(string _nameScene)
    {
        // EditorApplication.SaveCurrentSceneIfUserWantsTo();
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene($"{PATH_TO_SCENES_FOLDER}{_nameScene}.unity");
    }
#endif
}
