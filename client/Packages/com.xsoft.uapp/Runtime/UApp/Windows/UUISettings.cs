using App.Core.UICore.Utility;
using BattleViews.Utility;
using UApp;
using UGameTools;

namespace Windows
{
    internal partial class UUISettings
    {
        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(HideWindow);
            ButtonExit.OnMouseClick(o =>
            {
                UApplication.S.GotoLoginGate();
                HideWindow();
            });

            Slider_bgm.onValueChanged.AddListener(v => { SettingManager.S.BgmValue = v; });
            sfx_Slider.onValueChanged.AddListener(v => { SettingManager.S.MusicValue = v; });
            SaveToggle.onValueChanged.AddListener(v =>
            {
                SettingManager.S.SavePower = v;
                SetKeyText();
            });
            NoticeToggle.onValueChanged.AddListener(v =>
            {
                SettingManager.S.Notice = v;
                SetKeyText();
            });
        }

        protected override void OnShow()
        {
            base.OnShow();


            Slider_bgm.value = SettingManager.S.BgmValue;
            sfx_Slider.value = SettingManager.S.MusicValue;
            SaveToggle.isOn = SettingManager.S.SavePower;
            NoticeToggle.isOn = SettingManager.S.Notice;
            SetKeyText();
        }


        private void SetKeyText()
        {
            lb_title.SetKey("UUISetting_Title");
            lb_save_Text.SetKey("UUISetting_SAVE");
            lb_notice_Text.SetKey("UUISetting_NOTICE");
            lb_sfx.SetKey("UUISetting_sfx");
            lb_bgm.SetKey("UUISetting_Bgm");

            //UUISetting_On //UUISetting_Off
            lb_notice_text_value.SetKey(NoticeToggle.isOn ? "UUISetting_On" : "UUISetting_Off");
            lb_save_Text_value.SetKey(SaveToggle.isOn ? "UUISetting_On" : "UUISetting_Off");
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}