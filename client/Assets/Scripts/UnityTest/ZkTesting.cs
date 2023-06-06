using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using org.apache.zookeeper;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ZkTesting
    {
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        [TestCase("129.211.9.75:2181", "/configs", ExpectedResult =null)]
        public IEnumerator ZookerWithEnumeratorPasses(string server,string root)
        {
            var zk = new ZooKeeper(server, 5000, null);
            // zk.closeAsync();
            yield break;
        }

    }
}
