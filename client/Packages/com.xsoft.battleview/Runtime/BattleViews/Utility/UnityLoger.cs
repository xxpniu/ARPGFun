using UnityEngine;
using XNet.Libs.Utility;

namespace BattleViews.Utility
{
    public class UnityLoger : Loger
    {
        public override void WriteLog(DebugerLog log)
        {

#if UNITY_SERVER
        Console.WriteLine(log.ToString());
#else
            switch (log.Type)
            {
                case LogerType.Error:
                    Debug.LogError(log);
                    break;
                case LogerType.Log:
                    Debug.Log(log);
                    break;
                case LogerType.Waring:
                case LogerType.Debug:
                    Debug.LogWarning(log);
                    break;
            }
#endif

        }
    }
}