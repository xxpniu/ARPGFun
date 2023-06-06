using UnityEngine;

namespace BattleViews.Components
{
    public class LookAtTargetTransfrom : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            if (target)
                this.transform.LookAt(target);
        }

        public Transform target;
    }
}
