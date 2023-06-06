using Layout.EditorAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Layout.LayoutEffects
{


    [EditorEffect("恢复魔法")]
    [EffectId(3)]
    public class CureMPEffect : EffectBase
    {

        [Label("取值来源")]
        public ValueOf valueType = ValueOf.NormalAttack;

        [Label("值")]
        public ValueSourceOf value = 0;
    }
}
