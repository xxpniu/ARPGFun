using System;
using GameLogic.Utility;
using Layout;
using Layout.LayoutElements;
using Proto;
using UnityEngine;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        UnityEngine.Vector3 Position { get; }
        Quaternion Rotation { get; }
        MagicData MagicData { set; get; }
        //for editor test 
        void ShowDamageRanger(DamageLayout layout, UnityEngine.Vector3 tar, UnityEngine.Quaternion rotation);
        void PlayTest(int pIndex, TimeLine line);
        //end

        [NeedNotify(typeof(Notify_PlayTimeLine),"PlayIndex", "PathIndex", "TargetIndex", "Type")]
        void PlayTimeLine(int pIndex,int pathIndex, int target, int type);
        [NeedNotify(typeof(Notify_CancelTimeLine), "PlayIndex")]
        void CancelTimeLine(int pIndex);
    }
}

