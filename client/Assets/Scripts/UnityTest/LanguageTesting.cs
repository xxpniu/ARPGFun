using System.Collections;
using System.Collections.Generic;
using Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class LanguageTesting
    {
        // A Test behaves as an ordinary method
        [Test]
        public void LanguageTestingSimplePasses()
        {
            var la = new LanguageSetting();
            la.Keys.Add(new LanguageSetting.LanguageKey { Key = "AppName", Value = "ARPG Fun" });
            la.Keys.Add(new LanguageSetting.LanguageKey { Key = "AppVersion", Value = "1.0.1" });
            var xml = XmlParser.Serialize(la);
            Debug.Log(xml);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator LanguageTestingWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
