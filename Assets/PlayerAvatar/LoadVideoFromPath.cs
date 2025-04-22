using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(MeshRenderer))]
public class VideoSetup : MonoBehaviour
{
    public string videoFileName;
    public VideoPlayer videoPlayerClass;

    public bool isMine;


    void Start()
    {
        string videoPath = isMine ? ReplayCollector.replayingPath.Item1 : ReplayCollector.replayingPath.Item2;

        var renderer = GetComponent<Renderer>();

        var videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.url = videoPath;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        var renderTexture = new RenderTexture(1280, 720, 0);
        videoPlayer.targetTexture = renderTexture;

        var material = GetComponent<Renderer>().material; // 明示的にUnlitを使用
        material.mainTexture = renderTexture;
        renderer.material = material;

        var audioSource = gameObject.AddComponent<AudioSource>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        if (!System.IO.File.Exists(videoPath))
        {
            Debug.LogError("Video file not found: " + videoPath);
            return;
        }

        // デバッグ用イベント
        videoPlayer.prepareCompleted += (source) =>
        {
            Debug.Log("Video prepared.");
            Debug.Log("Current Frame: " + videoPlayerClass.frame);
        };
        videoPlayer.errorReceived += (vp, msg) => Debug.LogError("VideoPlayer error: " + msg);

        videoPlayer.Prepare();

        videoPlayerClass = videoPlayer;
    }

    void Update()
    {
        if (videoPlayerClass == null)
        {
            videoPlayerClass = GetComponent<VideoPlayer>();
        }
        else
        {
            LogFrame();

            videoPlayerClass.playbackSpeed = ReplayManager.playbackSpeed;
            if (ReplayManager.isPaused)
            {
                videoPlayerClass.Pause();
            }
            else
            {
                videoPlayerClass.Play();
            }
        }

    }


    void LogFrame()
    {
       //Debug.Log("Current Frame: " + videoPlayerClass.frame);       
    }



}
