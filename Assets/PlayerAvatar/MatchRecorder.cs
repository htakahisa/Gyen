using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class MatchRecorder : MonoBehaviour
{
    // =====================================================
    // ▼ 自分だけ映さない機能（Renderer.enabled方式）
    // =====================================================
    private Dictionary<Camera, List<Renderer>> hiddenRenderers = new();

    public void AddHiddenObject(Camera cam, GameObject obj)
    {
        if (!hiddenRenderers.ContainsKey(cam))
            hiddenRenderers[cam] = new List<Renderer>();

        foreach (var r in obj.GetComponentsInChildren<Renderer>(true))
            if (!hiddenRenderers[cam].Contains(r))
                hiddenRenderers[cam].Add(r);
    }

    void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!hiddenRenderers.TryGetValue(cam, out var list)) return;
        foreach (var r in list)
            if (r != null) r.enabled = false;
    }

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!hiddenRenderers.TryGetValue(cam, out var list)) return;
        foreach (var r in list)
            if (r != null) r.enabled = true;
    }

    // =====================================================
    // ▼ Recorder 本体
    // =====================================================
    public class CameraRecord
    {
        public Camera camera;
        public RenderTexture rt;
        public ConcurrentQueue<byte[]> frameQueue = new();
        public string filePath;
        public bool recording;
        public bool writing;
        public int pendingReadbacks;
        public int pendingCaptures;

        public bool cameraDestroyed; // ★ 追加
    }

    [Header("Recording Settings")]
    public int width = 1280;
    public int height = 720;
    public int fps = 30;

    private List<CameraRecord> records = new();
    private bool isRecording;
    private float nextFrameTime;
    private string ffmpegRuntimePath;

    void Awake()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        PrepareFFmpeg();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering_Record; // ★ FIX
    }

    // =====================================================
    // ▼ fps制御（そのまま）
    // =====================================================
    void LateUpdate()
    {
        if (!isRecording) return;

        while (Time.unscaledTime >= nextFrameTime)
        {
            nextFrameTime += 1f / fps;

            foreach (var r in records)
            {
                // ★ Camera が消えていたら完了扱い
                if (r.camera == null)
                {
                    r.cameraDestroyed = true;
                    r.recording = false;
                    r.pendingCaptures = 0;
                    continue;
                }

                if (r.recording)
                    r.pendingCaptures++;
            }
        }
    }

    // =====================================================
    // ▼ Cameraごとに Readback（最重要修正）
    // =====================================================
    void OnEndCameraRendering_Record(ScriptableRenderContext ctx, Camera cam)
    {
        var r = records.Find(x => x.camera == cam);
        if (r == null || r.cameraDestroyed || r.rt == null) return;
        if (!r.recording || r.pendingCaptures <= 0) return;

        r.pendingCaptures--;
        r.pendingReadbacks++;

        AsyncGPUReadback.Request(r.rt, 0, TextureFormat.RGBA32, req =>
        {
            r.pendingReadbacks--;

            if (req.hasError || r.cameraDestroyed) return;

            var data = req.GetData<byte>();
            var frame = new byte[data.Length];
            data.CopyTo(frame);
            r.frameQueue.Enqueue(frame);
        });
    }


    // =====================================================
    // ▼ 録画制御
    // =====================================================
    public void AddCamera(Camera cam)
    {
        if (records.Exists(r => r.camera == cam)) return;

        var r = new CameraRecord
        {
            camera = cam,
            rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32),
            recording = isRecording
        };
        r.rt.Create();
        cam.targetTexture = r.rt;

        r.filePath = Path.Combine(
            Application.persistentDataPath,
            $"{cam.name}_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{(UnityEngine.Random.Range(0, 9999))}.mp4"
        );

        records.Add(r);
    }

    public async Task ClearCamera()
    {
        // ★ 録画は止める（セッション終了）
        isRecording = false;
        foreach (var r in records)
            r.recording = false;

        // ★ GPU Readback 完了待ち
        foreach (var r in records)
        {
            while (r.pendingReadbacks > 0)
                await Task.Delay(10);
        }

        // ★ Camera / RT を安全に解放
        foreach (var r in records)
        {
            // Camera は Destroy 済みの可能性がある
            if (r.camera != null)
            {
                // targetTexture を外す（重要）
                if (r.camera.targetTexture == r.rt)
                    r.camera.targetTexture = null;
            }

            if (r.rt != null)
            {
                if (r.rt.IsCreated())
                    r.rt.Release();

                Destroy(r.rt);
            }

            // 念のためキューもクリア
            while (r.frameQueue.TryDequeue(out _)) { }
        }

        records.Clear();
    }


    public void StartRecording()
    {
        isRecording = true;
        nextFrameTime = Time.unscaledTime;
        foreach (var r in records) r.recording = true;
    }

    public async Task StopRecordingAndWait()
    {
        isRecording = false;
        foreach (var r in records) r.recording = false;

        foreach (var r in records)
            while (r.pendingReadbacks > 0)
                await Task.Delay(10);
    }

    public async Task WriteRecordingAndWait()
    {
        isRecording = false;
        foreach (var r in records) r.recording = false;

        // ★ Destroy 済み Camera は pendingReadbacks を無視
        foreach (var r in records)
        {
            while (!r.cameraDestroyed && r.pendingReadbacks > 0)
                await Task.Delay(10);
        }

        foreach (var r in records)
        {
            if (!r.writing && !r.frameQueue.IsEmpty)
                await WriteToVideoAsync(r);
        }
    }


    // =====================================================
    // ▼ 書き込み（ほぼそのまま）
    // =====================================================
    async Task WriteToVideoAsync(CameraRecord r)
    {
        if (r.frameQueue.IsEmpty) return;
        r.writing = true;

        await Task.Run(async () =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegRuntimePath,
                Arguments =
                    $"-y -f rawvideo -pixel_format rgba -video_size {width}x{height} -framerate {fps} -i - " +
                    $"-vf vflip -pix_fmt yuv420p -preset ultrafast \"{r.filePath}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            using var stdin = proc.StandardInput.BaseStream;

            while (r.frameQueue.TryDequeue(out var frame))
                await stdin.WriteAsync(frame, 0, frame.Length);

            stdin.Close();
            proc.WaitForExit();
            r.writing = false;
            Debug.Log("write success");
        });
    }

    // =====================================================
    // ▼ FFmpeg準備（変更なし）
    // =====================================================
    void PrepareFFmpeg()
    {
#if UNITY_STANDALONE_WIN
        ffmpegRuntimePath = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
#else
        ffmpegRuntimePath = Path.Combine(Application.streamingAssetsPath, "ffmpeg");
#endif
    }
}
