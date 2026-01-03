using GameLogic.Utility;
using Layout;
using Layout.LayoutElements;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }

        MagicData MagicData { set; get; }

        //for editor test 
        void ShowDamageRanger(DamageLayout layout, Vector3 tar, Quaternion rotation);

        void PlayTest(int pIndex, TimeLine line);
        //end

        [NeedNotify(typeof(Notify_PlayTimeLine), "PlayIndex", "PathIndex", "TargetIndex", "Type")]
        void PlayTimeLine(int pIndex, int pathIndex, int target, int type);

        [NeedNotify(typeof(Notify_CancelTimeLine), "PlayIndex")]
        void CancelTimeLine(int pIndex);
    }
}