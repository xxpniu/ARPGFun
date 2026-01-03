using UnityEngine;

namespace BattleViews.Components
{
    public class LookAtTargetTransfrom : MonoBehaviour
    {
        public Transform target;

        // Update is called once per frame
        private void Update()
        {
            if (target)
                transform.LookAt(target);
        }
    }
}