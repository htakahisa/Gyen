using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Collections;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

public class DualCameraRecorder : MonoBehaviour
{
    // 録画設定
    public int frameRate = 30;
    private string outputPath1;
    private string outputPath2;

    // FFmpegプロセス
    private Process ffmpegProcess1;
    private Process ffmpegProcess2;

    // レンダーテクスチャ
    private RenderTexture renderTexture1;
    private RenderTexture renderTexture2;

    // 状態フラグ
    private bool isRecording = false;
    private bool hasLoaded = false;

    // カメラ参照
    private Camera targetCamera1;
    private Camera targetCamera2;

    // 定数
    const int WIDTH = 1280;
    const int HEIGHT = 720;
    const int BYTES_PER_PIXEL = 4; // RGBA
    const int BUFFER_COUNT = 3; // トリプルバッファリング

    // バッファシステム
    private byte[][] buffers1 = new byte[BUFFER_COUNT][];
    private byte[][] buffers2 = new byte[BUFFER_COUNT][];
    private volatile bool[] bufferReady1 = new bool[BUFFER_COUNT];
    private volatile bool[] bufferReady2 = new bool[BUFFER_COUNT];
    private int currentBufferIndex = 0;

    // スレッドプール
    private ConcurrentQueue<Action> threadPoolQueue = new ConcurrentQueue<Action>();
    private SemaphoreSlim threadPoolSemaphore = new SemaphoreSlim(0);
    private const int THREAD_POOL_SIZE = 4;




    void Start()
    {
        InitializeBuffers();
        InitializeRenderTextures();
        InitializeOutputPaths();
        StartThreadPool();
    }

    void InitializeBuffers()
    {
        for (int i = 0; i < BUFFER_COUNT; i++)
        {
            buffers1[i] = new byte[WIDTH * HEIGHT * BYTES_PER_PIXEL];
            buffers2[i] = new byte[WIDTH * HEIGHT * BYTES_PER_PIXEL];
            bufferReady1[i] = false;
            bufferReady2[i] = false;
        }
    }

    void InitializeRenderTextures()
    {
        renderTexture1 = new RenderTexture(WIDTH, HEIGHT, 24, RenderTextureFormat.ARGB32);
        renderTexture2 = new RenderTexture(WIDTH, HEIGHT, 24, RenderTextureFormat.ARGB32);
        renderTexture1.Create();
        renderTexture2.Create();
    }

    void InitializeOutputPaths()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        outputPath1 = Path.Combine(Application.streamingAssetsPath, $"record1_{timestamp}.mp4");
        outputPath2 = Path.Combine(Application.streamingAssetsPath, $"record2_{timestamp}.mp4");
    }

    void StartThreadPool()
    {
        for (int i = 0; i < THREAD_POOL_SIZE; i++)
        {
            new Thread(ThreadPoolWorker)
            {
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.BelowNormal
            }.Start();
        }
    }

    void ThreadPoolWorker()
    {
        while (true)
        {
            threadPoolSemaphore.Wait();
            if (threadPoolQueue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"ThreadPool Error: {e.Message}");
                }
            }

            if (!isRecording && threadPoolQueue.IsEmpty) break;
        }
    }

    public void SetupCameras()
    {
        SetupMainCamera();
        SetupSecondCamera();

        if (targetCamera1 != null && targetCamera2 != null)
        {
            StartRecording();
        }
        else
        {
            Debug.LogError("Failed to setup cameras - one or both cameras are null");
        }
    }

    void SetupMainCamera()
    {
        try
        {
            var player1 = RoundManager.rm?.GetMyPlayer();
            if (player1 == null) return;

            Transform recordCameraTransform = player1.GetComponentInChildren<Camera>()?.transform.Find("RecordCamera");
            targetCamera1 = recordCameraTransform?.GetComponent<Camera>();
            if (targetCamera1 != null)
            {
                targetCamera1.targetTexture = renderTexture1;
                Debug.Log("Main camera setup successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Main camera setup error: {e.Message}");
        }
    }

    void SetupSecondCamera()
    {
        try
        {
            var player2 = RoundManager.rm?.GetOtherPlayer();
            if (player2 == null) return;

            Transform recordCameraTransform = player2.GetComponentInChildren<Camera>()?.transform.Find("RecordCamera");
            targetCamera2 = recordCameraTransform?.GetComponent<Camera>();
            if (targetCamera2 != null)
            {
                targetCamera2.targetTexture = renderTexture2;
                Debug.Log("Second camera setup successfully");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Second camera setup error: {e.Message}");
        }
    }

    public void StartRecording()
    {
        try
        {
            string ffmpegPath = GetFFmpegPath();
            if (!File.Exists(ffmpegPath))
            {
                Debug.LogError("FFmpeg not found at: " + ffmpegPath);
                return;
            }

            ffmpegProcess1 = StartFFmpeg(ffmpegPath, outputPath1);
            ffmpegProcess2 = StartFFmpeg(ffmpegPath, outputPath2);

            isRecording = true;
            StartCoroutine(RecordingCoroutine());

            Debug.Log($"Recording started to:\n{outputPath1}\n{outputPath2}");
        }
        catch (Exception e)
        {
            Debug.LogError($"StartRecording error: {e.Message}");
            StopRecording();
        }
    }

    string GetFFmpegPath()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
#else
        return Path.Combine(Application.dataPath, "StreamingAssets", "ffmpeg.exe");
#endif
    }

    Process StartFFmpeg(string ffmpegPath, string outputPath)
    {
        string args = $"-y -f rawvideo -pixel_format rgba -video_size {WIDTH}x{HEIGHT} -framerate {frameRate} -i - -vf \"vflip\" " +
                      $"-c:v libx264 -preset medium -crf 23 -pix_fmt yuv420p -vsync passthrough \"{outputPath}\"";

        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Debug.LogWarning($"FFmpeg Error: {e.Data}");
        };

        process.Start();
        process.BeginErrorReadLine();
        return process;
    }

    IEnumerator RecordingCoroutine()
    {
        yield return new WaitForEndOfFrame(); // 最初のフレームをスキップ

        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            int bufferIndex = currentBufferIndex;
            currentBufferIndex = (currentBufferIndex + 1) % BUFFER_COUNT;

            // 前のバッファがまだ処理中ならスキップ
            if (bufferReady1[bufferIndex] || bufferReady2[bufferIndex])
            {
                Debug.LogWarning($"Frame {Time.frameCount} skipped - buffer {bufferIndex} not ready");
                continue;
            }

            // 一時レンダーテクスチャを作成
            RenderTexture tempRT1 = RenderTexture.GetTemporary(WIDTH, HEIGHT, 0, RenderTextureFormat.ARGB32);
            RenderTexture tempRT2 = RenderTexture.GetTemporary(WIDTH, HEIGHT, 0, RenderTextureFormat.ARGB32);

            // メインカメラからコピー
            if (targetCamera1 != null)
            {
                Graphics.Blit(renderTexture1, tempRT1);
            }

            // サブカメラからコピー
            if (targetCamera2 != null)
            {
                Graphics.Blit(renderTexture2, tempRT2);
            }

            // 非同期読み取りリクエスト
            bool readback1Done = false;
            bool readback2Done = false;

            if (targetCamera1 != null)
            {
                AsyncGPUReadback.Request(tempRT1, 0, TextureFormat.RGBA32, request =>
                {
                    if (request.hasError)
                    {
                        Debug.LogError("Failed to read GPU texture 1");
                    }
                    else if (isRecording)
                    {
                        request.GetData<byte>().CopyTo(buffers1[bufferIndex]);
                        bufferReady1[bufferIndex] = true;
                    }
                    readback1Done = true;
                });
            }
            else
            {
                readback1Done = true;
            }

            if (targetCamera2 != null)
            {
                AsyncGPUReadback.Request(tempRT2, 0, TextureFormat.RGBA32, request =>
                {
                    if (request.hasError)
                    {
                        Debug.LogError("Failed to read GPU texture 2");
                    }
                    else if (isRecording)
                    {
                        request.GetData<byte>().CopyTo(buffers2[bufferIndex]);
                        bufferReady2[bufferIndex] = true;
                    }
                    readback2Done = true;
                });
            }
            else
            {
                readback2Done = true;
            }

            // 両方のReadbackが完了するまで待機
            yield return new WaitUntil(() => readback1Done && readback2Done);

            // 一時テクスチャを解放
            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);

            // 前のバッファを処理
            int prevBufferIndex = (bufferIndex - 1 + BUFFER_COUNT) % BUFFER_COUNT;
            ProcessReadyBuffers(prevBufferIndex);
        }
    }

    void ProcessReadyBuffers(int index)
    {
        if (bufferReady1[index] && bufferReady2[index])
        {
            var data1 = buffers1[index];
            var data2 = buffers2[index];

            threadPoolQueue.Enqueue(() => WriteToFFmpeg(data1, ffmpegProcess1));
            threadPoolQueue.Enqueue(() => WriteToFFmpeg(data2, ffmpegProcess2));

            bufferReady1[index] = false;
            bufferReady2[index] = false;
            threadPoolSemaphore.Release(2);
        }
    }

    void WriteToFFmpeg(byte[] data, Process process)
    {
        if (data == null || !isRecording || process == null || process.HasExited) return;

        try
        {
            process.StandardInput.BaseStream.Write(data, 0, data.Length);
            process.StandardInput.BaseStream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"FFmpeg Write Error: {e.Message}");
            StopRecording();
        }
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        StopAllCoroutines();
        StopFFmpegProcesses();
        Debug.Log("Recording stopped.");

    }

    void StopFFmpegProcesses()
    {
        StopProcess(ref ffmpegProcess1);
        StopProcess(ref ffmpegProcess2);
    }

    void StopProcess(ref Process process)
    {
        if (process == null) return;

        try
        {
            if (!process.HasExited)
            {
                process.StandardInput.Close();
                if (!process.WaitForExit(2000))
                {
                    process.Kill();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error stopping FFmpeg: {e.Message}");
        }
        finally
        {
            process.Close();
            process.Dispose();
            process = null;
        }
    }

    void Update()
    {
        if (!hasLoaded && RoundManager.rm != null && RoundManager.rm.hasLoaded)
        {
            SetupCameras();
            hasLoaded = true;
        }
    }

    void OnDestroy()
    {
        StopRecording();
        Cleanup();
    }

    void Cleanup()
    {
        if (renderTexture1 != null)
        {
            renderTexture1.Release();
            Destroy(renderTexture1);
            renderTexture1 = null;
        }

        if (renderTexture2 != null)
        {
            renderTexture2.Release();
            Destroy(renderTexture2);
            renderTexture2 = null;
        }
    }

    void OnApplicationQuit()
    {
        StopRecording();
    }
}