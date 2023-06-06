﻿using UnityEngine;

namespace BattleViews.Components
{
	public class UGameScene : MonoBehaviour {

		// Use this for initialization
		void Start ()
		{
			startPoint.gameObject.SetActive (false);
			enemyStartPoint.gameObject.SetActive (false);
			//tower.gameObject.SetActive (false);
			//towerEnemy.gameObject.SetActive (false);
		}
		

		public Transform startPoint;

		public Transform enemyStartPoint;

		//public Transform tower;
		//public Transform towerEnemy;
	}
}
