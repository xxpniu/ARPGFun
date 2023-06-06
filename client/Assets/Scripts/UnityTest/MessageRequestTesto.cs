using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    public class MessageRequestTesto
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MessageRequestTestoSimplePasses()
        {

            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MessageRequestTestoWithEnumeratorPasses()
        {
            yield return null;
        }
    }
}
