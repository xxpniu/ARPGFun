using TMPro;
using UnityEngine.UI;

namespace Tips
{
    [UITipResourcesAttribute("UUINotify")]
    public class UUINotify : UUITip
    {
        private TextMeshProUGUI t_text;

        protected override void OnCreate()
        {
            t_text = FindChild<TextMeshProUGUI>("Text");
        }

        public void SetNotify(string text)
        {
            t_text.text = text;
        }
    }
}