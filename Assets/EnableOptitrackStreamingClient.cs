using UnityEngine;

public class EnableOptitrackStreamingClient : MonoBehaviour
{
    public OptitrackStreamingClient optitrackClient;

    void Start()
    {
        // OptitrackStreamingClient.csスクリプトがアタッチされたゲームオブジェクトのOptitrackStreamingClientコンポーネントを取得
        if (optitrackClient == null)
        {
            optitrackClient = GetComponent<OptitrackStreamingClient>();
        }

        // OptitrackStreamingClientコンポーネントが取得できた場合、enabledをtrueに設定
        if (optitrackClient != null)
        {
            optitrackClient.enabled = true;
        }
        else
        {
            Debug.LogError("OptitrackStreamingClient not found!");
        }
    }
}
