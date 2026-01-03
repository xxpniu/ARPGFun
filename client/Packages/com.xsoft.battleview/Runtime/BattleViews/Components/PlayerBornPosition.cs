using App.Core.UICore.Utility;
using UnityEngine;

namespace BattleViews.Components
{
    public class PlayerBornPosition : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            GExtends.DrawSphere(transform.position, 2, transform.forward);
        }
    }
}