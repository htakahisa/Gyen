using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections;
using Debug = UnityEngine.Debug;

public class MatchRecorder : MonoBehaviour
{
    public Camera targetCamera;
    public int frameRate = 30;
    private string outputPath;

    private Process ffmpegProcess;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private bool isRecording = false;
    private bool hasLoaded = false;

    void Start()
    {
        renderTexture = new RenderTexture(1920, 1080, 24);
        texture2D = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        outputPath = Path.Combine(Application.persistentDataPath, "output.mp4");
    }

    public void StartSetCamera()
    {
        var player = RoundManager.rm?.GetMyPlayer();
        if (player == null)
        {
            Debug.LogError("Player not found");
            return;
        }

        targetCamera = player.GetComponentInChildren<Camera>().transform.Find("RecordCamera").GetComponent<Camera>();
        if (targetCamera == null)
        {
            Debug.LogError("Camera not found");
            return;
        }

        targetCamera.targetTexture = renderTexture;
        StartRecording();
    }

    public void StartRecording()
    {
        // ビルド後はStreamingAssetsのパスが変わるため、特別な処理が必要
#if UNITY_EDITOR
        string ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
#else
    string ffmpegPath = Path.Combine(Application.dataPath, "StreamingAssets", "ffmpeg.exe");
#endif

        // 開発中はこのデバッグを有効に
        Debug.Log($"Full FFmpeg path: {ffmpegPath}");
        Debug.Log($"File exists: {File.Exists(ffmpegPath)}");

        if (!File.Exists(ffmpegPath))
        {
            // ディレクトリ内容を詳細にログ出力
            string[] files = Directory.GetFiles(Application.streamingAssetsPath);
            Debug.LogError($"FFmpeg not found. Directory contains: {string.Join(", ", files)}");
            return;
        }

        string arguments = $"-y -f rawvideo -pixel_format rgb24 -video_size 1920x1080 -framerate {frameRate} -i - -c:v libx264 -preset fast -pix_fmt yuv420p \"{outputPath}\"";

        ffmpegProcess = new Process();
        ffmpegProcess.StartInfo.FileName = ffmpegPath;
        ffmpegProcess.StartInfo.Arguments = arguments;
        ffmpegProcess.StartInfo.UseShellExecute = false;
        ffmpegProcess.StartInfo.CreateNoWindow = true;
        ffmpegProcess.StartInfo.RedirectStandardInput = true;

        try
        {
            ffmpegProcess.Start();
            isRecording = true;
            StartCoroutine(RecordingCoroutine());
            Debug.Log($"Recording started. Output: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start FFmpeg: {e.Message}");
            isRecording = false;
        }
    }

    IEnumerator RecordingCoroutine()
    {
        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            // ダブルバッファリングでパフォーマンス改善
            RenderTexture tempRT = RenderTexture.GetTemporary(1920, 1080, 24);
            Graphics.Blit(renderTexture, tempRT);

            Texture2D tempTex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture.active = tempRT;
            tempTex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tempTex.Apply();

            try
            {
                byte[] frameData = tempTex.GetRawTextureData();
                ffmpegProcess.StandardInput.BaseStream.Write(frameData, 0, frameData.Length);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"フレーム書き込みエラー: {e.Message}");
                StopRecording();
            }
            finally
            {
                RenderTexture.ReleaseTemporary(tempRT);
                Destroy(tempTex);
            }
        }
    }

    public void StopRecording()
    {
        if (isRecording && ffmpegProcess != null)
        {
            isRecording = false;
            StopAllCoroutines();

            try
            {
                if (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.StandardInput.Close();
                    ffmpegProcess.WaitForExit(1000);
                    if (!ffmpegProcess.HasExited)
                        ffmpegProcess.Kill();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error stopping FFmpeg: {e.Message}");
            }
            finally
            {
                ffmpegProcess.Close();
                ffmpegProcess.Dispose();
                Debug.Log("Recording stopped");
            }
        }
    }

    void Update()
    {
        if (!hasLoaded && RoundManager.rm != null && RoundManager.rm.hasLoaded)
        {
            StartSetCamera();
            hasLoaded = true;
        }
    }

    void OnDestroy() => StopRecording();
    void OnApplicationQuit() => StopRecording();
}