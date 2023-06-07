using UnityEngine;
using UGameTools;
using Proto;
using UnityEngine.Serialization;

public class MonsterGroupPosition : MonoBehaviour
{
    //public MapElementType EType;

    public int ConfigID = 0;

    [FormerlySerializedAs("Linktraget")] 
    public Transform linkTarget; 

    public int GroupID = 1; 


    void OnDrawGizmos()
    {
        Color defaultColor = Gizmos.color;
        
        Gizmos.color =Color.red;
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
        UnityEditor.Handles.Label(this.transform.position, $"{ConfigID}");
        UnityEditor.Handles.EndGUI();
#endif

        GExtends.DrawSphere(this.transform.position, 2,this.transform.forward);
        if (this.linkTarget != null)
        {
            Gizmos.color = Color.green;
            GExtends.DrawSphere(this.linkTarget.position, 1, this.linkTarget.forward);
        }
        Gizmos.color = defaultColor;
    }

   
}
