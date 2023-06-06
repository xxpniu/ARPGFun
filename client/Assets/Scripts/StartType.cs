using System.Collections;
using BattleViews.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using XNet.Libs.Utility;


public class StartType : MonoBehaviour
{

    public enum SceneType
    {
        Server,
        Application,
        LocalGame
    }

    private IEnumerator Start()
    {
        yield return Addressables.InitializeAsync();
        Debuger.Loger = new UnityLoger();

        yield return SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);

        yield return new WaitForEndOfFrame();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_SERVER
        scene =  SceneType.Server;
        Application.targetFrameRate = 30;
#else
#if !UNITY_EDITOR
       scene =  SceneType.Application;
#endif
#endif
        yield return SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
        Destroy(this);
    }

    [Header("Type:Server/Application")]
    public SceneType scene = SceneType.Application;
}
