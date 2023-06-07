using System.IO;
using BattleViews.Utility;
using Core;
using UGameTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EditorToolsItemMenu
{
    public const string EL_ROOT = "__ROOT__ELEMENT__";

    private static bool ShowSave()
    {
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isDirty)
            {
                return EditorUtility.DisplayDialog("Notify", "Drop modify？", "Yes", "Cancel");
            }
        }
        return true;
    }

    [MenuItem("GAME/GoEditor &e")]
    public static void GoToEditorScene()
    {
        if (!ShowSave())
            return;

        if (EditorApplication.isPlaying)
        {
            EditorApplication.Beep();
            return;
        }

        var editor = "Assets/Scenes/EditorReleaseMagic.unity";
        EditorSceneManager.OpenScene(editor);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("GAME/Play Game &s")]
    public static void GoToStarScene()
    {

        if (!ShowSave()) return;
        if (EditorApplication.isPlaying)
        {
            EditorApplication.Beep();
            return;
        }
        var editor = "Assets/Scenes/Launch.unity";
        EditorSceneManager.OpenScene(editor);
        EditorApplication.isPlaying = true;

    }


    [MenuItem("GAME/Map/Import &I")]
    public static void GoImportSceneConfig()
    {

        var path = $"{Application.dataPath}/AssetRes/Level/";
        var file = EditorUtility.OpenFilePanelWithFilters("Import Map Config", path, new[] { "json config","json" });
        if (string.IsNullOrEmpty(file)) return;
        var config = File.ReadAllText(file).Parser<Proto.MapCongfig>();

        var obj = GameObject.Find(EL_ROOT);
        if (obj) if (!EditorUtility.DisplayDialog("Elements Root Exsited", "Do you want import again?", "Yes", "No")) return;

        Object.DestroyImmediate(obj);

        var go = new GameObject(EL_ROOT);
        go.transform.SetAsFirstSibling();
        foreach (var i in config.Elements)
        {
            var c = new GameObject($"[{i.GroupID}]{i.Type}", typeof(MonsterGroupPosition));
            c.transform.SetParent(go.transform, false);
            c.transform.RestRTS();
            c.transform.position = i.Position.ToVer3();
            c.transform.forward = i.Forward.ToVer3();
            var l = new GameObject("Linked");
            l.transform.SetParent(c.transform, false);
            l.transform.RestRTS();
            l.transform.position = i.LinkPos?.ToVer3() ?? c.transform.position;
            var group = c.GetComponent<MonsterGroupPosition>();
            group.EType = i.Type;
            group.linkTarget = l.transform;
            group.ConfigID = i.ConfigID;
            group.GroupID = i.GroupID;
        }
    }

    [MenuItem("GAME/Map/Export &e")]
    public static void GoExportSceneConfig()
    {

        var path = $"{Application.dataPath}/AssetRes/Level/";
        var file = EditorUtility.SaveFilePanel("Import Map Config", path, "level", "json");
        if (string.IsNullOrEmpty(file)) return;

        var groups = GameObject.FindObjectsOfType<MonsterGroupPosition>();
        var config = new Proto.MapCongfig();
        foreach (var i in groups)
        {
            var el = new Proto.MapElement
            {
                ConfigID = i.ConfigID,
                Forward = i.transform.forward.ToPVer3(),
                Position = i.transform.position.ToPVer3(),
                Type = i.EType,
                LinkPos = i.linkTarget.position.ToPVer3(),
                GroupID = i.GroupID
            };

            config.Elements.Add(el);
        }

        File.WriteAllText(file, config.ToJson());

        var go = GameObject.Find(EL_ROOT);
        if (go)
        {
            Object.DestroyImmediate(go);
        }
    }
}

