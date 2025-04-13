using UnityEngine;
using System.Collections;
using System.Net.Mail;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

[RequireComponent(typeof( Scrollbar))]
public class AutoValueScrollbar : MonoBehaviour {


    private Scrollbar bar;
	
    void Awake()
    {
        bar = GetComponent<Scrollbar>();
    }


    private CancellationTokenSource _cancellation;

    public async void ResetValue(float duration)
    {
        _cancellation?.Cancel();
        _cancellation = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token, this.destroyCancellationToken);
        RunBar(duration:duration, token.Token);
    }

    private async void RunBar(float duration, CancellationToken token = default)
    {
        var start = Time.time;
        bar.size = 0;
        await UniTask.NextFrame(token);
        while (Time.time - start < duration)
        {
            bar.size = (Time.time - start) / duration;
            await UniTask.NextFrame(token);
        }

        bar.size = 1;
        await UniTask.NextFrame(token);
    }
}
