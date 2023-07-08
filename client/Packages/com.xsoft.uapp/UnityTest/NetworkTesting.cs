using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using XNet.Libs.Utility;

namespace Tests
{
    public class NetworkTesting
    {
        private class UnityLoger : Loger
        {
            #region implemented abstract members of Loger
            public override void WriteLog(DebugerLog log)
            {
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

            }
            #endregion
        }

        // A Test behaves as an ordinary method
        [Test]
        public void NetworkTestingSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        [Timeout(3000)]
        public IEnumerator NetworkTestingWithEnumeratorPasses()
        {
            yield return null;
        }
    }
}
