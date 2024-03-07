using System.Collections;
using BattleViews.Utility;
using Cysharp.Threading.Tasks;
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

    private async void Start()
    {
        Debuger.Loger = new UnityLogger();
#if DEVELOPMENT_BUILD        
        SRDebug.Init();
#endif  
        await Addressables.InitializeAsync();
       

        await SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);

        await UniTask.Yield();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_SERVER
        scene =  SceneType.Server;
        Application.targetFrameRate = 30;
#else
#if !UNITY_EDITOR
       scene =  SceneType.Application;
#endif
#endif
        await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
        Destroy(this);
    }

    [Header("Type:Server/Application")]
    public SceneType scene = SceneType.Application;
}
