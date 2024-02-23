using System;
using System.Collections.Generic;
using EConfig;
using UnityEngine;
using XNet.Libs.Utility;

namespace App.Core.Core
{
    public class LanguageManager : XSingleton<LanguageManager>
    {
        public enum LanguageType
        { 
            Zh,
            En
        }

        public LanguageType LType = LanguageType.Zh;

        public readonly Dictionary<string, string> Keys = new();

        protected override void Awake()
        {
            base.Awake();
            var xml = ResourcesManager.S.ReadStreamingFile("Language.xml");

            var ls = XmlParser.DeSerialize<LanguageSetting>(xml);
            foreach (var i in ls.Keys)
            {
                AddKey(i.Key, i.Value);
            }

        }

        private void AddKey(string key, string word)
        {
            try
            {
           
                if (Keys.ContainsKey(key))
                {
                    Debug.LogError($"{key} exists!");
                    return; ;
                }
                Keys.Add(key, word);
            }
            catch(Exception ex) {
                Debuger.LogError($"{key}->{word} \n{ex}");
            }

        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                if (Keys.TryGetValue(key, out string v)) return v;
                return key;
            }
        }

        public string Format(string key, params object[] pars)
        {
            if (pars.Length > 0) return string.Format(this[key], pars);
            return this[key];
        }

        public void AddLanguage(LanguageData[] la)
        {
            foreach (var i in la)
            {
                switch (LType)
                {
                    case LanguageType.En:
                        AddKey(i.Key, i.EN);
                        break;
                    default:
                        AddKey(i.Key, i.ZH);
                        break;
                }
            
            }
        }
    }
}
