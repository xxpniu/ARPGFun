using Windows;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace UApp
{
    public class GameGMTools : MonoBehaviour
    {
        public bool ShowGM;
        private readonly GUIStyle green = new();
        private readonly GUIStyle red = new();

        private string level = "level 1";

        // Use this for initialization
        private void Start()
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
        private void Update()
        {
#if UNITY_EDITOR


            //Send Update

#endif
        }

        public void OnGUI()
        {
            GUI.Label(
                new Rect(Screen.width - 220, 5, 200, 40),
                $"FPS:{1 / Time.deltaTime:0}P:{UApplication.Singleton.pingDelay:0}\nS:{UApplication.Singleton.SendTotal / 1024.0f / Mathf.Max(1, Time.time - UApplication.Singleton.ConnectTime):0.00}kb/s R:{UApplication.Singleton.ReceiveTotal / 1024.0f / Mathf.Max(1, Time.time - UApplication.Singleton.ConnectTime):0.00}kb/s(AVG)",
                1 / Time.deltaTime > 28 ? green : red);

            if (!ShowGM) return;
            GUI.BeginGroup(new Rect(Screen.width - 185, 105, 180, 25));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("GM", GUILayout.Width(100), GUILayout.Height(40))) StartUI();

            GUILayout.EndHorizontal();
            GUI.EndGroup();
        }

        private async void StartUI()
        {
            var ui = await UUIManager.S.CreateWindowAsync<UUIGMPanel>();
            ui.ShowWindow();
        }
    }
}