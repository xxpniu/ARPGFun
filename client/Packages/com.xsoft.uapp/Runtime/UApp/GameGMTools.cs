using Windows;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace UApp
{
	public class GameGMTools : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
        Destroy(this);
        return;
#else
			var data = PlayerPrefs.GetString("GM");
			if (!string.IsNullOrEmpty(data)) level = data;
			green.alignment = TextAnchor.MiddleRight;
			green.normal.textColor = Color.green;
			red.alignment = TextAnchor.MiddleRight;
			red.normal.textColor = Color.red;
#endif
		}


		// Update is called once per frame
		void Update()
		{
#if UNITY_EDITOR


			//Send Update

#endif
		}

		private string level = "level 1";
		public bool ShowGM = false;
		readonly GUIStyle red = new GUIStyle();
		readonly GUIStyle green = new GUIStyle();

		public void OnGUI()
		{
			GUI.Label(
				new Rect(Screen.width - 220, 5, 200, 40),
				string.Format("FPS:{0:0}P:{1:0}\nS:{2:0.00}kb/s R:{3:0.00}kb/s(AVG)",
					1 / Time.deltaTime,
					UApplication.Singleton.pingDelay,
					(UApplication.Singleton.SendTotal / 1024.0f) /
					Mathf.Max(1, Time.time - UApplication.Singleton.ConnectTime),
					(UApplication.Singleton.ReceiveTotal / 1024.0f) /
					Mathf.Max(1, Time.time - UApplication.Singleton.ConnectTime)),
				1 / Time.deltaTime > 28 ? green : red);

			if (!ShowGM) return;
			GUI.BeginGroup(new Rect(Screen.width - 185, 105, 180, 25));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("GM", GUILayout.Width(100), GUILayout.Height(40)))
			{
				StartUI();
			}

			GUILayout.EndHorizontal();
			GUI.EndGroup();
		}

		private async void StartUI()
		{
			var ui =await UUIManager.S.CreateWindowAsync<UUIGMPanel>();
			ui.ShowWindow();
		}
	}

}
