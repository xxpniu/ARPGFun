using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleViews.Components;
using UnityEngine.UI;
using UGameTools;
using EConfig;
using ExcelConfig;
using GameLogic.Game.AIBehaviorTree;
using UnityEngine;

namespace Windows
{
    partial class UUIBattleEditor
    {
        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            public GridTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            bt_add.onClick.AddListener(() =>
            {

                if (int.TryParse(input_skill.text, out int skillId))
                {
                    var magic = ExcelToJSONConfigManager.GetId<CharacterMagicData>(skillId);
                    if (magic == null) return;
                    EditorStarter.S.releaser.AddMagic(magic);
                }
            });

            bt_remove.onClick.AddListener(() =>
            {
                if (int.TryParse(input_skill.text, out int skillId))
                {
                    var magic = ExcelToJSONConfigManager.GetId<CharacterMagicData>(skillId);
                    if (magic == null) return;
                    EditorStarter.S.releaser.RemoveMaic(magic.ID);
                }
            });

            bt_releaser.onClick.AddListener(() =>
            {
                int.TryParse(input_Level.text, out int level);
                level = Mathf.Clamp(level,1, 100);

                if (int.TryParse(input_index.text, out int charId))
                {
                    var character = ExcelToJSONConfigManager.GetId<CharacterData>(charId);
                    if (character == null) return;
                    EditorStarter.S.ReplaceRelease(level,character, to_do_remove.isOn, to_enable_ai.isOn);
                }
            });

            bt_targe.onClick.AddListener(() =>
            {
                int.TryParse(input_Level.text, out int level);
                level = Mathf.Clamp(level,1, 100);
                if (int.TryParse(input_index.text, out int charId))
                {
                    var character = ExcelToJSONConfigManager.GetId<CharacterData>(charId);
                    if (character == null) return;
                    EditorStarter.S.ReplaceTarget(level,character, to_do_remove.isOn, to_enable_ai.isOn);
                }
            });

            Joystick_Left.GetComponent<zFrame.UI.Joystick>().OnValueChanged.AddListener((v) =>
            {
                //Debug.Log(v);
                var dir = ThirdPersonCameraContollor.Current.LookRotation* new Vector3(v.x, 0, v.y);
                //Debug.Log($"{v}->{dir}");
            });

            s_distance.onValueChanged.AddListener((v) =>
            {
                EditorStarter.S.distanceCharacter = Mathf.Lerp(15, 3, v);
                EditorStarter.S.isChanged = true;
            });

            bt_normal_att.onClick.AddListener(() =>
            {
                EditorStarter.S.DoAction(new Proto.Action_NormalAttack());
            });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            s_rot_y.value = 0.725f;
            s_distance_camera.value = 1;
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            EditorStarter.S.ry = Mathf.Lerp(-180, 180, s_rot_y.value);
            EditorStarter.S.slider_y = Mathf.Lerp(8,87, s_rot_x.value);
            EditorStarter.S.distance = Mathf.Lerp(2, 30, s_distance_camera.value);
           
            Time.timeScale =  s_time_scale.value;
        }
    }
}