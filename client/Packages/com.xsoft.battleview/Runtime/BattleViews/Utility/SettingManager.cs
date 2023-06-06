using Core;
using UnityEngine;

namespace BattleViews.Utility
{
    public class SettingManager : XSingleton<SettingManager>
    {

        private bool save_power = false;
        public bool SavePower
        {
            set
            {
                if (value)
                {
                    Application.targetFrameRate = 30;
                    Screen.sleepTimeout = SleepTimeout.SystemSetting;
                }
                save_power = value;
                PlayerPrefs.SetInt("__SETTING_Save_Power", 1);
            }
            get
            {
                return save_power;
            }
        }
        private float _BgmValue = 1f;
        public float BgmValue
        {
            set
            {
                _BgmValue = value;
                PlayerPrefs.SetFloat("__SETTING_Bgm_Value", value);
            }
            get
            {
                return _BgmValue;
            }
        }

        private bool _notice;
        public bool Notice
        {
            set
            {
                _notice = value;
                PlayerPrefs.SetInt("__SETTING_notice", 1);// == 1;
            }
            get
            {
                return _notice;
            }
        }

        public string Language { set; get; }


        private float music = 1;
        public float MusicValue
        {
            set
            {
                music = value;
                PlayerPrefs.SetFloat("__SETTING_music_Value", value);
            }
            get
            {
                return music;
            }
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
