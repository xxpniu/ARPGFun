using System.Collections;
using System.Collections.Generic;
using EngineCore.Simulater;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TickTimeTesting
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TickTimeTestingSimplePasses()
        {
            
           
            for (var d = 0; d < 100; d++)
            {
                Tick(d);
            }

            return;

            void Tick(int day)
            {
                var time = new GTime(24 * 60 * 60 * day, .3f);
                var start = time;
                for (var i = 0; i < 300; i++)
                {
                    time.TickTime(0.03f);
                }
                var total = time - start;
                Debug.Log($"{total}");
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TickTimeTestingWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
