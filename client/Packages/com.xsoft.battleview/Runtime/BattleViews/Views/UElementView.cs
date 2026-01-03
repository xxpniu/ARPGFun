using EngineCore.Simulater;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using UnityEngine;

namespace BattleViews.Views
{
    public abstract class UElementView : MonoBehaviour, IBattleElement, ISerializationElement
    {
        public UPerceptionView PerView { private set; get; }

        public GObject GElement { private set; get; }

        public int Index { set; get; }

        public abstract IMessage ToInitNotify();

        public void SetPerception(UPerceptionView view)
        {
            PerView = view;
        }


        public void DestroySelf(float time = 0.3f)
        {
            if (!this) return;
            Destroy(gameObject, time);
        }

        public virtual void OnJoined()
        {
        }

        protected void CreateNotify(IMessage notify)
        {
            PerView.AddNotify(notify); //  AddNotify();
        }

        #region IBattleElement implementation

        void IBattleElement.JoinState(int index)
        {
            OnJoined();
            Index = index;
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(ToInitNotify());
#endif
            PerView.AttachView(this);
        }

        void IBattleElement.ExitState(int index)
        {
            PerView.DeAttachView(this);
#if UNITY_SERVER||UNITY_EDITOR
            CreateNotify(new Notify_ElementExitState { Index = Index });
#endif
            DestroySelf();
        }

        void IBattleElement.AttachElement(GObject el)
        {
            GElement = el;
        }

        #endregion
    }
}