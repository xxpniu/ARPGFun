using App.Core.Core;
using GameLogic.Game.LayoutLogics;
using UnityEngine;

namespace BattleViews.Views
{
    public class UParticlePlayer:MonoBehaviour, IParticlePlayer
    {
        public string Path;

        private bool IsDestory = false;

        private async void Start()
        {
            var instance = await ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(Path);

            Instantiate(instance, transform);
        }

        #region IParticlePlayer implementation
        public void DestoryParticle()
        {
            IsDestory = true;
            Destroy(this.gameObject);
        }

        public void AutoDestory(float time)
        {
            IsDestory = true;
            Destroy(gameObject, time); 
        }
        

        public bool CanDestory
        {
            get
            {
                return !IsDestory;
            }
        }

    
        #endregion

    }
}
