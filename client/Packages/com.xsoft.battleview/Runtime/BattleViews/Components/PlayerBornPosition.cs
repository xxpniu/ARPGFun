using UGameTools;
using UnityEngine;

namespace BattleViews.Components
{
    public class PlayerBornPosition : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            GExtends.DrawSphere(this.transform.position, 2,this.transform.forward);
        }
    }
}
