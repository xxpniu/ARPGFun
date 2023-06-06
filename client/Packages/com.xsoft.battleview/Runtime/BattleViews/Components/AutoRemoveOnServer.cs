using UnityEngine;

namespace BattleViews.Components
{
    public class AutoRemoveOnServer : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
#if UNITY_SERVER
        Destroy(this.gameObject);
#endif
        }

    }
}
