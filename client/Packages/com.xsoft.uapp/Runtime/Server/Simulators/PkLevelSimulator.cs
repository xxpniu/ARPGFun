﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [LevelSimulator(MType = Proto.MapType.Pk)]
    internal class PkLevelSimulator : BattleLevelSimulator
    {

        private int TeamIndex = 0;

        protected override int PlayerTeamIndex { get { return TeamIndex++; } }
    }
}
