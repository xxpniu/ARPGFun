using System.Collections;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Utility;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Layout.LayoutElements;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BattleViews.Views
{
    public class UBattleMissileView : UElementView, IBattleMissile
    {
        public string res;
        public float speed;

        public string fromBone;
        public string toBone;
        public int releaserIndex;
        public Vector3 offset;
        public int TargetIndex;
        internal float MaxDis;
        internal float MaxLifeTime;
        internal MovementType MType;


        private IEnumerator Start()
        {
            var viewRelease = PerView.GetViewByIndex<UMagicReleaserView>(releaserIndex);
            var viewTarget = viewRelease.CharacterTarget;
            var characterView = viewRelease.CharacterReleaser;
            var rotation = ((IBattleCharacter)characterView).Rotation;
            var target = PerView.GetViewByIndex<UCharacterView>(TargetIndex);

            transform.position = characterView.GetBoneByName(fromBone).position + rotation * offset;
            transform.rotation = Quaternion.identity;
#if !UNITY_SERVER


            var forward = characterView.GetBoneByName(UCharacterView.RootBone).forward;

            yield return ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(res, obj =>
            {
                if (obj == null) return;
                if (!this) return;
                var ins = Instantiate(obj, transform);

                var path = ins.GetComponent<MissileFollowPath>();
                if (!path)
                {
                    var go = new GameObject("MISSILE");
                    go.transform.SetParent(transform);
                    go.transform.RestRTS();
                    path = go.TryAdd<MissileFollowPath>();
                    ins.transform.SetParent(go.transform, false);
                    path.Moveing = ins.transform;
                }

                ins.transform.RestRTS();

                if (path && viewTarget)
                    switch (MType)
                    {
                        case MovementType.Follow:
                            path.BeginMove(viewTarget.GetBoneByName(toBone), speed);
                            break;
                        case MovementType.Line:
                            path.BeginMove(forward, speed, MaxDis / speed);
                            break;
                        case MovementType.AutoTarget:
                            path.BeginAutoTarget(target.GetBoneByName(toBone), speed);
                            break;
                    }
            });
#endif
        }

        Transform IBattleMissile.Transform => transform;

        public override IMessage ToInitNotify()
        {
            var createNotify = new Notify_CreateMissile
            {
                Index = Index,
                ResourcesPath = res,
                Speed = speed,
                ReleaserIndex = releaserIndex,
                FromBone = fromBone,
                ToBone = toBone,
                Offset = offset.ToPVer3(),
                MaxDis = MaxDis,
                MaxLifeTime = MaxLifeTime,
                MType = (int)MType,
                TargetIndex = TargetIndex
            };
            return createNotify;
        }
    }
}