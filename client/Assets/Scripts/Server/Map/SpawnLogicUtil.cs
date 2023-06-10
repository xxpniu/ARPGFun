using System.Collections;
using System.Collections.Generic;
using BattleViews.Utility;
using Layout.AITree;
using UnityEngine;
using GameLogic;
using Utility;

namespace Server.Map
{
    public static class SpawnLogicUtil
    {
        public static TreeNode CreateTransportAI(Vector3 target,string transportMagicKey)
        {
            var root = new TreeNodeTick()
            {
                childs = {
                    new TreeNodeSequence() {
                        childs =
                        {
                            new TreeNodeCompareTargets
                            {
                                compareType= CompareType.Greater,
                                teamType= Proto.TargetTeamType.OwnTeam,
                                valueOf = DistanceValueOf.Value,
                                compareValue =1,
                                Distance = 10000
                            }.Initialization(),
                            new TreeNodeTransport{ linkPos = target.ToLVer3() }.Initialization(),
                            new TreeNodeReleaseMagic{  magicKey = transportMagicKey, valueOf= MagicValueOf.MagicKey, ReleaseATPos = true }.Initialization()

                        }
                }.Initialization()
                }
            }.Initialization();
            return root;
        }

        public static TreeNode CreateChestBoxAI()
        {
            return null;
        }
    }

    
}