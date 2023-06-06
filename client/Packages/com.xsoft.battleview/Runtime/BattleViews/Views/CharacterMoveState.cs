using System;
using EngineCore.Simulater;
using UnityEngine;

namespace BattleViews.Views
{
    public enum MoveCategory
    {
        NONE,
        Destination,
        Forward,
        Push
    }

    [Serializable]
    public abstract class CharacterMoveState
    {
        protected CharacterMoveState(UCharacterView view)
        {
            this.View = view;
            Velocity = Vector3.zero;
        }

        protected UCharacterView View { get; }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual bool Tick(GTime gTime) { return true; }

        public virtual Vector3 Velocity { get; }
    }
}