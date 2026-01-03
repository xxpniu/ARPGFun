using System;
using System.Collections.Generic;
using EConfig;
using UnityEngine;
using XNet.Libs.Utility;

namespace App.Core.Core
{
    [Name("LanguageManager")]
    public class LanguageManager : XSingleton<LanguageManager>
    {
        public enum LanguageType
        {
            Zh,
            En
        }

        public LanguageType LType = LanguageType.Zh;

        public readonly Dictionary<string, string> Keys = new();

        public string this[string key] => string.IsNullOrEmpty(key) ? string.Empty : Keys.GetValueOrDefault(key, key);

        protected override void Awake()
        {
            base.Awake();
        }

        private async void Start()
        {
            var xml = await ResourcesManager.S.ReadStreamingFile("Language.xml");

            var ls = XmlParser.DeSerialize<LanguageSetting>(xml);
            foreach (var i in ls.Keys) AddKey(i.Key, i.Value);
        }

        private void AddKey(string key, string word)
        {
            try
            {
                if (Keys.ContainsKey(key))
                {
                    Debug.LogError($"{key} exists!");
                    return;
                    ;
                }

                Keys.Add(key, word);
            }
            catch (Exception ex)
            {
                Debuger.LogError($"{key}->{word} \n{ex}");
            }
        }

        public string Format(string key, params object[] pars)
        {
            return pars.Length > 0 ? string.Format(this[key], pars) : this[key];
        }

        public void AddLanguage(LanguageData[] la)
        {
            foreach (var i in la)
                switch (LType)
                {
                    case LanguageType.En:
                        AddKey(i.Key, i.EN);
                        break;
                    case LanguageType.Zh:
                    default:
                        AddKey(i.Key, i.ZH);
                        break;
                }
        }
    }
}