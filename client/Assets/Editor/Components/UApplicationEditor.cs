using System;
using Proto;
using Proto.ServerConfig;
using UApp;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UApplication))]
public class UApplicationEditor : Editor
{

    public override void OnInspectorGUI()
    {

        var target = this.target as UApplication;

       
        Draw(target.ChatServer,"Chat Server");
        Draw(target.GateServer, "Gate Server");
        Draw(target.LoginServer, "Login Server");

        base.OnInspectorGUI();

    }

    private void Draw(ServiceAddress address,string title)
    {
        if (address != null)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label(title);
            EditorGUILayout.BeginHorizontal();
            address.IpAddress = EditorGUILayout.TextField(address.IpAddress??string.Empty, GUILayout.MaxWidth(200));
            address.Port = EditorGUILayout.IntField(address.Port,GUILayout.MaxWidth(80));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

}
