using App.Core.UICore.Utility;
using Proto;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class MonsterGroupPosition : MonoBehaviour
{
    public MapElementType EType;

    public int ConfigID;

    [FormerlySerializedAs("Linktraget")] public Transform linkTarget;

    public int GroupID = 1;


    private void OnDrawGizmos()
    {
        var defaultColor = Gizmos.color;

        Gizmos.color = Color.red;
#if UNITY_EDITOR
        Handles.BeginGUI();
        Handles.Label(transform.position, $"{ConfigID}");
        Handles.EndGUI();
#endif

        GExtends.DrawSphere(transform.position, 2, transform.forward);
        if (linkTarget != null)
        {
            Gizmos.color = Color.green;
            GExtends.DrawSphere(linkTarget.position, 1, linkTarget.forward);
        }

        Gizmos.color = defaultColor;
    }
}