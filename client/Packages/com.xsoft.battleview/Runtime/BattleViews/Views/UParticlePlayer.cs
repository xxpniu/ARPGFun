using App.Core.Core;
using GameLogic.Game.LayoutLogics;
using UnityEngine;
using UnityEngine.Serialization;

namespace BattleViews.Views
{
    public class UParticlePlayer:MonoBehaviour, IParticlePlayer
    {
        public string path;

        private bool _isDestroy = false;

        private async void Start()
        {
            var instance = await ResourcesManager.Singleton.LoadResourcesWithExName<GameObject>(path);

            Instantiate(instance, transform);
        }

        #region IParticlePlayer implementation
        public void DestroyParticle()
        {
            _isDestroy = true;
            Destroy(this.gameObject);
        }

        public void AutoDestroy(float time)
        {
            _isDestroy = true;
            Destroy(gameObject, time); 
        }
        

        public bool CanDestroy => !_isDestroy;

        #endregion

    }
}
