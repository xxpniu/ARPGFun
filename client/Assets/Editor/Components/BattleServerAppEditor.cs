using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleServerApp))]
public class BattleServerAppEditor : Editor
{
    public override void OnInspectorGUI()
    {

        var t = this.target as BattleServerApp;
        if (!t.BattleSimulator)
        {
            if (GUILayout.Button("StartTest"))
            {
                t.StartTest();
            }
        }
        base.OnInspectorGUI();
    }
}
