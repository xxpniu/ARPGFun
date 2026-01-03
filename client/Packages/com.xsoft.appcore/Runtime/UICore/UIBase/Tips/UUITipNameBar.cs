using App.Core.UICore.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Tips
{
    [UITipResources("UUITipNameBar")]
    public class UUITipNameBar : UUITip
    {
        private Slider GreenSlider;

        private Text Level;

        private Text Name;
        private Slider RedSlider;

        private Transform SkillGuageGreen;
        private Transform SkillGuageRed;

        protected override void OnCreate()
        {
            Name = FindChild<Text>("lb_Name");
            Level = FindChild<Text>("Level");
            SkillGuageGreen = FindChild<Transform>("SkillGuageGreen");
            GreenSlider = FindChild<Slider>("GreenSlider");
            SkillGuageRed = FindChild<Transform>("SkillGuageRed");
            RedSlider = FindChild<Slider>("RedSlider");
        }

        internal void SetInfo(string name, int level, int hp, int hpMax, bool OwnerTeam)
        {
            var v = hp / (float)hpMax;
            Name.SetKey(name);
            Level.text = $"{level}";
            SkillGuageGreen.ActiveSelfObject(OwnerTeam);
            SkillGuageRed.ActiveSelfObject(!OwnerTeam);
            Name.color = OwnerTeam ? Color.white : Color.red;
            RedSlider.value = v;
            GreenSlider.value = v;
        }
    }
}