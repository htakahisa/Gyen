using Mirror;
using UnityEngine;

public class CharacterTransfromNetwork : NetworkBehaviour
{
    [Header("Interpolation Settings")]
    [SerializeField, Range(0.01f, 1f)] private float positionLerpFactor = 0.2f;
    [SerializeField, Range(0.01f, 1f)] private float rotationLerpFactor = 0.2f;
    [SerializeField, Range(0.01f, 1f)] private float pitchLerpFactor = 0.2f;

    public CharacterController controller;
    public Transform cameraRoot;

    // 受信用
    private Vector3 targetPos;
    private float targetYaw;
    private float targetPitch;

    // 送信用
    private Vector3 lastSentPos;
    private float lastSentYaw;
    private float lastSentPitch;

    [SerializeField] private float posThreshold = 0.005f;
    [SerializeField] private float rotThreshold = 0.2f;
    [SerializeField] private float pitchThreshold = 0.2f;

    private double lastRecvTime;
    public bool isSynchronize;

    public float pitch;   // カメラの上下角度
    public float yaw;     // プレイヤーの左右角度
    public bool initializedRotation = false;

    //--------------------------------------------------------------------
    // ここが超重要：プレイヤーもボットも同じ扱いで、所有者(isOwned)だけ送信
    //--------------------------------------------------------------------
    private bool isOwned =>
        GetComponent<SpawnOwner>().IsMine() ||
        GetComponent<SpawnOwner>().ownerNetId == 12345; // ボット所有者

    void Start()
    {
        targetPos = transform.position;
        targetYaw = transform.eulerAngles.y;

        if (cameraRoot)
            targetPitch = cameraRoot.localEulerAngles.x;

        lastSentPos = targetPos;
    }

    void Update()
    {
        //これがないと、スポーン時の強制セットが失敗する可能性がある
        if (!isSynchronize) return;

        //----------------------------------------------------------
        // 自分が所有しているキャラ（プレイヤー or ボット）
        //----------------------------------------------------------
        if (isOwned)
        {
            TrySendTransform();
            return; // 自分は補間不要
        }

        //----------------------------------------------------------
        // 所有していない → 補間表示
        //----------------------------------------------------------
        ApplyInterpolation();
    }

    //--------------------------------------------------------------------
    // 送信処理（プレイヤーでもボットでも共通）
    //--------------------------------------------------------------------
    private void TrySendTransform()
    {
        Vector3 pos = transform.position;
        float yaw = transform.eulerAngles.y;
        float pitch = cameraRoot ? cameraRoot.localEulerAngles.x : 0f;

        if (Vector3.Distance(pos, lastSentPos) > posThreshold ||
            Mathf.Abs(Mathf.DeltaAngle(yaw, lastSentYaw)) > rotThreshold ||
            Mathf.Abs(Mathf.DeltaAngle(pitch, lastSentPitch)) > pitchThreshold)
        {
            lastSentPos = pos;
            lastSentYaw = yaw;
            lastSentPitch = pitch;

            double t = NetworkTime.time;
            CmdSendTransform(pos, yaw, pitch, t);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSendTransform(Vector3 pos, float yaw, float pitch, double timestamp)
    {
        if (!isSynchronize) return;
        RpcSyncTransform(pos, yaw, pitch, timestamp);
    }

    //--------------------------------------------------------------------
    // 受信処理（全クライアント）
    //--------------------------------------------------------------------
    [ClientRpc]
    private void RpcSyncTransform(Vector3 pos, float yaw, float pitch, double timestamp)
    {
        if (timestamp + 0.001 <= lastRecvTime) return;
        lastRecvTime = timestamp;

        targetPos = pos;
        targetYaw = yaw;
        targetPitch = pitch;
    }

    //--------------------------------------------------------------------
    // 補間処理（所有者以外のクライアントのみ）
    //--------------------------------------------------------------------
    private void ApplyInterpolation()
    {
        if (controller) controller.enabled = false;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            positionLerpFactor
        );

        float newYaw = Mathf.LerpAngle(
            transform.eulerAngles.y,
            targetYaw,
            rotationLerpFactor
        );

        transform.rotation = Quaternion.Euler(0, newYaw, 0);

        if (cameraRoot)
        {
            float newPitch = Mathf.LerpAngle(
                cameraRoot.localEulerAngles.x,
                targetPitch,
                pitchLerpFactor
            );

            cameraRoot.localEulerAngles = new Vector3(newPitch, 0, 0);
        }

        if (controller) controller.enabled = true;
    }

    // ---- 強制ワープ用（瞬間移動）----
    [Server]
    public void ForceSetPosition(Vector3 pos, float yaw)
    {
        RpcForceTeleport(pos, yaw);
    }

    [ClientRpc]
    private void RpcForceTeleport(Vector3 pos, float yaw)
    {
        if (controller)
            controller.enabled = false;

        transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
        SetRotation(yaw);

        if (controller)
            controller.enabled = true;

        targetPos = pos;
        targetYaw = yaw;
    }

    public void SetRotation(float newYaw)
    {
        // 外側の Transform を更新
        transform.rotation = Quaternion.Euler(0, newYaw, 0);

        // 内部変数を更新（次のフレームで戻らないように）
        this.yaw = newYaw;

    }

    [Server]
    public void SetSynchronize(bool synchronize)
    {
        isSynchronize = synchronize;
    }
}
