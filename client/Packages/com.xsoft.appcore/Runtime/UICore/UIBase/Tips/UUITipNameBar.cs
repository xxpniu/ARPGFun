using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using App.Core.UICore.Utility;

namespace Tips
{
    [UITipResources("UUITipNameBar")]
    public class UUITipNameBar : UUITip
    {

        private Transform SkillGuageGreen;
        private Transform SkillGuageRed;

        private Text Level;

        private Slider GreenSlider;
        private Slider RedSlider;
        protected override void OnCreate()
        {

            Name = FindChild<Text>("lb_Name"); 
            Level = FindChild<Text>("Level");
            SkillGuageGreen = FindChild<Transform>("SkillGuageGreen");
            GreenSlider = FindChild<Slider>("GreenSlider");
            SkillGuageRed = FindChild<Transform>("SkillGuageRed");
            RedSlider = FindChild<Slider>("RedSlider");
        }

        private Text Name;
        internal void SetInfo(string name, int level, int hp, int hpMax,  bool OwnerTeam)
        {
            float v = hp /(float) hpMax;
            Name.SetKey ( name);
            Level.text = $"{level}";
            SkillGuageGreen.ActiveSelfObject(OwnerTeam);
            SkillGuageRed.ActiveSelfObject(!OwnerTeam);
            Name.color = OwnerTeam ? Color.white : Color.red;
            RedSlider.value  = v;
            GreenSlider.value =v;
        }
    }

}