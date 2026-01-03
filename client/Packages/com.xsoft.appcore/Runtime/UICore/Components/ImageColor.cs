using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageColor : MonoBehaviour
{
    private CancellationTokenSource _cancellation;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void Show()
    {
        gameObject.SetActive(true);
        _cancellation?.Cancel();

        _cancellation = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token, destroyCancellationToken);
        ColorRun(0, 1, token.Token);
    }

    public void Hide()
    {
        if (!gameObject.activeSelf) return;
        _cancellation?.Cancel();
        _cancellation = new CancellationTokenSource();
        var token = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token, destroyCancellationToken);
        ColorRun(1, 0, token.Token);
    }

    private async void ColorRun(float f, float t, CancellationToken token = default)
    {
        var start = Time.time;
        var image = GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.g, f);
        await UniTask.Yield(token);
        while (Time.time - start < 0.3f)
        {
            var a = Mathf.Lerp(f, t, (Time.time - start) / 0.3f);
            image.color = new Color(image.color.r, image.color.g, image.color.g, a);
            await UniTask.Yield(token);
        }

        image.color = new Color(image.color.r, image.color.g, image.color.g, t);

        if (t == 0) gameObject.SetActive(false);
    }
}