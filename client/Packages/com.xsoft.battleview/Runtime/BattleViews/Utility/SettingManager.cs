using App.Core.Core;
using UnityEngine;

namespace BattleViews.Utility
{
    [Name("SettingManager")]
    public class SettingManager : XSingleton<SettingManager>
    {

        private bool _savePower = false;
        public bool SavePower
        {
            set
            {
                if (value)
                {
                    Application.targetFrameRate = 30;
                    Screen.sleepTimeout = SleepTimeout.SystemSetting;
                }
                _savePower = value;
                PlayerPrefs.SetInt("__SETTING_Save_Power", 1);
            }
            get => _savePower;
        }
        private float _bgmValue = 1f;
        public float BgmValue
        {
            set
            {
                _bgmValue = value;
                PlayerPrefs.SetFloat("__SETTING_Bgm_Value", value);
            }
            get => _bgmValue;
        }

        private bool _notice;
        public bool Notice
        {
            set
            {
                _notice = value;
                PlayerPrefs.SetInt("__SETTING_notice", 1);// == 1;
            }
            get => _notice;
        }

        public string Language { set; get; }


        private float _music = 1;
        public float MusicValue
        {
            set
            {
                _music = value;
                PlayerPrefs.SetFloat("__SETTING_music_Value", value);
            }
            get => _music;
        }

        protected override void Awake()
        {
            base.Awake();
            if (PlayerPrefs.HasKey("__SETTING_Save_Power"))
            {

                SavePower = PlayerPrefs.GetInt("__SETTING_Save_Power") == 1;
                BgmValue = PlayerPrefs.GetFloat("__SETTING_Bgm_Value");
                Notice = PlayerPrefs.GetInt("__SETTING_notice") == 1;
                MusicValue = PlayerPrefs.GetFloat("__SETTING_music_Value");
            }
            else {
                SavePower =  false;
                BgmValue =  1;
                Notice = true;
                MusicValue =1;
            }
        }


    }
}
