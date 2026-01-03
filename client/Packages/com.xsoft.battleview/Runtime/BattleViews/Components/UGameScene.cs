using UnityEngine;

namespace BattleViews.Components
{
    public class UGameScene : MonoBehaviour
    {
        public Transform startPoint;

        public Transform enemyStartPoint;

        // Use this for initialization
        private void Start()
        {
            startPoint.gameObject.SetActive(false);
            enemyStartPoint.gameObject.SetActive(false);
            //tower.gameObject.SetActive (false);
            //towerEnemy.gameObject.SetActive (false);
        }

        //public Transform tower;
        //public Transform towerEnemy;
    }
}