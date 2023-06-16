using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Core
{
    [XmlType(TypeName = "Setting")]
    public class LanguageSetting
    {
        [XmlType(TypeName = "Key")]
        public class LanguageKey
        {
            [XmlAttribute(AttributeName = "K")]
            public string Key { set; get; }
            [XmlText]
            public string Value { set; get; }
        }
        [XmlElement(ElementName = "Add")]
        public List<LanguageKey> Keys = new List<LanguageKey>();
    }
}