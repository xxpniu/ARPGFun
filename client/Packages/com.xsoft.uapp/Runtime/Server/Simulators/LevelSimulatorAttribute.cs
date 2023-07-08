using System;
using Proto;

namespace Server
{
    public class LevelSimulatorAttribute : Attribute
    {
        public MapType MType { set; get; }
    }
}