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
    // ▼ 自分だけ映さない機能用（Renderer.enabled方式で安全）
    // =====================================================
    private Dictionary<Camera, List<Renderer>> hiddenRenderers = new Dictionary<Camera, List<Renderer>>();

    public void AddHiddenObject(Camera cam, GameObject obj)
    {
        if (!hiddenRenderers.ContainsKey(cam))
            hiddenRenderers[cam] = new List<Renderer>();

        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            if (!hiddenRenderers[cam].Contains(r))
                hiddenRenderers[cam].Add(r);
    }

    void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!hiddenRenderers.ContainsKey(cam)) return;
        foreach (var r in hiddenRenderers[cam])
            if (r != null)
                r.enabled = false;
    }

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!hiddenRenderers.ContainsKey(cam)) return;
        foreach (var r in hiddenRenderers[cam])
            if (r != null)
                r.enabled = true;
    }

    // =====================================================
    // ▼ MatchRecorder 元の機能
    // =====================================================
    public class CameraRecord
    {
        public Camera camera;
        public RenderTexture rt;
        public ConcurrentQueue<byte[]> frameQueue = new ConcurrentQueue<byte[]>();
        public string filePath;
        public bool recording = false;
        public bool writing = false;
    }

    [Header("Recording Settings")]
    public int width = 1280;
    public int height = 720;
    public int fps = 30;

    private List<CameraRecord> records = new List<CameraRecord>();
    private bool isRecording = false;
    private float nextFrameTime;
    private string ffmpegRuntimePath;

    private struct PendingCapture { public CameraRecord record; public double captureTime; }
    private List<PendingCapture> pendingCaptures = new List<PendingCapture>();

    void Awake()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        PrepareFFmpeg();

        RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void PrepareFFmpeg()
    {
#if UNITY_STANDALONE_WIN
        string fileName = "ffmpeg.exe";
#else
        string fileName = "ffmpeg";
#endif
        string src = Path.Combine(Application.streamingAssetsPath, fileName);
        string dst = Path.Combine(Application.persistentDataPath, fileName);
        ffmpegRuntimePath = dst;

        if (File.Exists(dst)) return;
        try { File.Copy(src, dst, true); } catch (Exception e) { Debug.LogError($"ffmpeg copy failed: {e}"); }

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        try
        {
            var chmod = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/chmod",
                    Arguments = $"+x \"{dst}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            chmod.Start(); chmod.WaitForExit();
        }
        catch (Exception e) { Debug.LogError($"chmod failed: {e}"); }
#endif
    }

    private string GetFFmpegPath() => ffmpegRuntimePath;

    public void AddCamera(Camera cam)
    {
        if (cam == null || records.Exists(r => r.camera == cam)) return;

        var record = new CameraRecord();
        record.camera = cam;
        record.rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        record.rt.Create();
        cam.targetTexture = record.rt;

        string name = cam.gameObject.name.Replace(" ", "_");
        string date = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        record.filePath = Path.Combine(Application.persistentDataPath, $"{name}_{date}_{records.Count}.mp4");

        records.Add(record);
    }

    public void ClearCamera()
    {
        foreach (var r in records)
        {
            if (r.camera != null)
                r.camera.targetTexture = null;
            if (r.rt != null)
                r.rt.Release();
        }
        records.Clear();
    }

    public void StartRecording()
    {
        if (isRecording) return;
        isRecording = true;
        nextFrameTime = Time.unscaledTime;
        foreach (var r in records) r.recording = true;
    }

    public async Task StopRecordingAndWait()
    {
        if (!isRecording) return;
        isRecording = false;
        foreach (var r in records) r.recording = false;
        await Task.Delay(50);

        foreach (var r in records)
        {
            if (!r.writing && !r.frameQueue.IsEmpty)
                await WriteToVideoAsync(r);
        }
    }

    void LateUpdate()
    {
        if (!isRecording) return;

        while (Time.unscaledTime >= nextFrameTime)
        {
            nextFrameTime += 1f / fps;
            foreach (var r in records)
                if (r.camera != null && r.recording)
                    pendingCaptures.Add(new PendingCapture { record = r, captureTime = Time.unscaledTime });
        }
    }

    private async Task WriteToVideoAsync(CameraRecord record)
    {
        if (record.frameQueue.IsEmpty) return;
        record.writing = true;

        string ffmpegPath = GetFFmpegPath();
        if (!File.Exists(ffmpegPath)) { record.writing = false; return; }

        await Task.Run(async () =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments =
                        $"-y -f rawvideo -pixel_format rgba -video_size {width}x{height} -framerate {fps} -i - " +
                        $"-vf vflip -pix_fmt yuv420p -c:v libx264 -preset ultrafast \"{record.filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                using (var proc = new Process { StartInfo = psi })
                {
                    proc.Start();
                    using (var stdin = proc.StandardInput.BaseStream)
                    {
                        while (record.frameQueue.TryDequeue(out var frame))
                            await stdin.WriteAsync(frame, 0, frame.Length);
                        await stdin.FlushAsync();
                    }
                    proc.WaitForExit();
                }
            }
            catch (Exception e) { Debug.LogError($"Write failed: {e}"); }
            finally { record.writing = false; }
        });
    }

    private void OnEndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
    {
        if (pendingCaptures.Count == 0) return;

        var camSet = new HashSet<Camera>(cameras);

        for (int i = pendingCaptures.Count - 1; i >= 0; --i)
        {
            var pc = pendingCaptures[i];
            var r = pc.record;

            if (r == null || r.camera == null || !r.recording || r.rt == null)
            {
                pendingCaptures.RemoveAt(i);
                continue;
            }

            if (!camSet.Contains(r.camera)) continue;

            AsyncGPUReadback.Request(r.rt, 0, TextureFormat.RGBA32, (req) =>
            {
                if (req.hasError || !r.recording) return;

                var data = req.GetData<byte>();
                byte[] frame = new byte[data.Length];
                data.CopyTo(frame);
                r.frameQueue.Enqueue(frame);
            });

            pendingCaptures.RemoveAt(i);
        }
    }
}
