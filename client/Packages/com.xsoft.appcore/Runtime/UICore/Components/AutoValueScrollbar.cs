using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class AutoValueScrollbar : MonoBehaviour
{
    private CancellationTokenSource _cancellation;


    private Scrollbar bar;

    private void Awake()
    {
        bar = GetComponent<Scrollbar>();
    }

    public void ResetValue(float duration)
    {
        _cancellation?.Cancel();
        _cancellation = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token, destroyCancellationToken);
        RunBar(duration, token.Token);
    }

    private async void RunBar(float duration, CancellationToken token = default)
    {
        try
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
        catch
        {
            //ignore
        }
    }
}