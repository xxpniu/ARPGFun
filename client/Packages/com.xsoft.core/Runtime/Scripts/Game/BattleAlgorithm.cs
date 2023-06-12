using System;
using GameLogic.Game.Elements;
using Proto;
using UnityEngine;
using P = Proto.HeroPropertyType;

namespace GameLogic.Game
{
    public struct DamageResult
    {
        public DamageResult(DamageType t, bool isMissed, int da,int crtm)
        {
            DType = t;
            IsMissed = isMissed;
            Damage = da;
            CrtMult = crtm;
        }
        public DamageType DType { get; }
        public bool IsMissed { get; }
        public int Damage { get; }
        public int CrtMult { get; }
    }
	/// <summary>
	/// 战斗中的算法
	/// </summary>
	public static class BattleAlgorithm
	{
        /// <summary>
        /// 最快的移动速度
        /// </summary>
        public const float MaxSpeed = 6.5f; //最大速度

        /// <summary>
        /// 计算普通攻击
        /// </summary>
        /// <param name="attack"></param>
        /// <returns></returns>
        public static int CalNormalDamage(BattleCharacter attack)
        {
            return attack[P.Damage];
        }

        private static readonly float[][] DamageRate = new float[][]
		{
			new float[]{0f,0f,0f},//混乱
			new float[]{0f,0.5f,0f},
			new float[]{.5f,-0.5f,0f}
		};

        //处理伤害类型加成
        public static int CalFinalDamage(int damage, DamageType dType, DefanceType dfType)
        {
            var rate = 1 + DamageRate[(int)dType][(int)dfType];
            var result = damage * rate;
            return (int)result;
        }

        public static DamageResult GetDamageResult(BattleCharacter sources, int damage,DamageType dType, BattleCharacter defencer)
        {
            var crtMult = 1f; 
            if (GRandomer.Probability10000(sources[P.Crt]))
            {
                crtMult = 1 + sources[P.CrtDamageRate] / 10000f;
            }
            bool isMissed;
            switch (dType)
            {
                case DamageType.Physical:
                case DamageType.Confusion:
                case DamageType.Magic:
                default:
                    {
                        var d = defencer[P.Defance];
                        damage = (int)(damage * crtMult);
                        var result = Mathf.Max(1, damage - d);
                        isMissed = GRandomer.Probability10000(defencer[P.Dodge]);
                        damage = (int)result;
                    }
                    break;
            }
            return new DamageResult(dType, isMissed, damage, (int)crtMult);
        }
	}
}

