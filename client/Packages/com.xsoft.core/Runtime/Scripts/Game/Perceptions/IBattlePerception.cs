using System.Collections.Generic;
using EngineCore.Simulater;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Utility;
using Layout;
using Layout.AITree;
using Layout.LayoutElements;
using Proto;
using Vector3 = Proto.Vector3;

namespace GameLogic.Game.Perceptions
{
    /// <summary>
    ///     I battle perception.
    /// </summary>
    public interface IBattlePerception : ITreeLoader
    {
        /// <summary>
        ///     当前的时间仿真
        /// </summary>
        /// <returns>The time simulator.</returns>
        ITimeSimulator GetTimeSimulator();

        /// <summary>
        ///     Gets the AIT ree.
        /// </summary>
        /// <returns>The AIT ree.</returns>
        /// <param name="pathTree">Path tree.</param>
        TreeNode GetAITree(string pathTree);

        /// <summary>
        ///     获取当前的layout
        /// </summary>
        /// <returns>The timeline by path.</returns>
        /// <param name="path">Path.</param>
        TimeLine GetTimeLineByPath(string path);

        /// <summary>
        ///     Gets the magic by key.
        /// </summary>
        /// <returns>The magic by key.</returns>
        /// <param name="key">Key.</param>
        MagicData GetMagicByKey(string key);

        /// <summary>
        ///     Exists the magic key.
        /// </summary>
        /// <returns>The magic key.</returns>
        /// <param name="key">Key.</param>
        bool ExistMagicKey(string key);


        [NeedNotify(typeof(Notify_CreateBattleCharacter),
            "AccountUuid", "ConfigID", "TeamIndex",
            "Position", "Forward", "Level", "Name", "Cds", "OwnerIndex", "Properties", "Hp", "Mp")]
        IBattleCharacter CreateBattleCharacterView
        (string accountID, int config, int teamId,
            Vector3 pos, Vector3 forward, int level, string name, IList<HeroMagicData> cds, int ownerIndex,
            IList<HeroProperty> properties, int hp, int mp);


        [NeedNotify(typeof(Notify_CreateReleaser), "OPosition", "ORotation", "ReleaserIndex", "TargetIndex", "MagicKey",
            "Position", "RMType")]
        IMagicReleaser CreateReleaserView(Vector3 pos, Vector3 rotation, int releaser, int target, string magicKey,
            Vector3 targetPos,  MagicReleaseType rmt); 

        /// <summary>
        ///     Creates the missile.
        /// </summary>
        /// <param name="releaseIndex"></param>
        /// <param name="targetIndex"></param>
        /// <param name="res"></param>
        /// <param name="offset"></param>
        /// <param name="fromBone"></param>
        /// <param name="toBone"></param>
        /// <param name="speed"></param>
        /// <param name="mType"></param>
        /// <param name="maxDis"></param>
        /// <param name="maxLiftTime"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_CreateMissile), "ReleaserIndex", "TargetIndex",
            "ResourcesPath", "Offset", "FromBone", "ToBone", "Speed", "MType", "MaxDis", "MaxLifeTime")]
        IBattleMissile CreateMissile(int releaseIndex, int targetIndex,
            string res, Vector3 offset, string fromBone, string toBone, float speed, int mType, float maxDis,
            float maxLiftTime);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="item"></param>
        /// <param name="teamIndex"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_Drop), "Pos", "Item", "TeamIndex", "GroupIndex")]
        IBattleItem CreateDropItem(Vector3 pos, PlayerItem item, int teamIndex, int groupId);


        /// <summary>
        ///     Process damage
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        /// <param name="isMissed"></param>
        /// <param name="crtMult"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_DamageResult), "Index", "TargetIndex", "Damage", "IsMissed", "CrtMult")]
        bool ProcessDamage(int owner, int target, int damage, bool isMissed, int crtMult);
    }
}