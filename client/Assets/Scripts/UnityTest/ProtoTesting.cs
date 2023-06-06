using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Proto;
using UnityEngine;
using UnityEngine.TestTools;
using Google.Protobuf.Reflection;
using Google.Protobuf;

namespace Tests
{
    public class ProtoTesting
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ProtoTestingSimplePasses()
        {
            //ExtensionSet<EnumValueOptions>()
            //ConstExtensions.LangugageKey.g
           // ExtensionSet.Get<EnumValueOptions>(ref sbyte,)
           //var options = ConstReflection.Descriptor.GetOptions();
           // options.GetExtension<>
           //var e = EnumDescriptor.FindValueByName   HeroPropertyType.DamageMax
           // HeroPropertyType
           //ConstReflection.Descriptor.FindTypeByName<lan>()
           //ConstExtensions.LangugageKey
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ProtoTestingWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
