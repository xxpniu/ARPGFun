using System.Xml.Serialization;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
    public enum DisposeType
    {
        NONE = 0,
        SKILL = 1,
        MOVE = 2,
        HURT = 4,
        NormarlAttack = 8
    }


    [EditorEffect("释放技能buf")]
    [EffectId(5)]
    public class AddBufEffect : EffectBase
    {
        [Label("配置KEY")] public string buffMagicKey;

        [Label("复制魔法参数")] public bool CopyParams = false;

        [HideInEditor] public int DiType;

        [Label("持续时间")] public ValueSourceOf durationTime = 1000;

        [Label("消失方式")]
        [EnumMasker]
        [XmlIgnore]
        public DisposeType DType
        {
            set => DiType = (int)value;
            get => (DisposeType)DiType;
        }

        public override string ToString()
        {
            return $"效果 {buffMagicKey} 持续 {durationTime}s 消失方式{DType}";
        }
    }
}