using Mirror;
using UnityEngine;

public class CharacterTransfromNetwork : NetworkBehaviour
{
    [Header("Interpolation Settings")]
    [SerializeField, Range(0.01f, 1f)] private float positionLerpFactor = 0.2f;
    [SerializeField, Range(0.01f, 1f)] private float rotationLerpFactor = 0.2f;
    [SerializeField, Range(0.01f, 1f)] private float pitchLerpFactor = 0.2f;

    [Header("References")]
    public CharacterController controller;
    public Transform cameraRoot; // FPSのカメラ親 (pitch制御用)

    [SyncVar] public bool isSynchronize = true;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetPitch;

    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;
    private float lastSentPitch;

    private const float positionSendThreshold = 0.02f;
    private const float rotationSendThreshold = 0.5f;
    private const float pitchSendThreshold = 0.5f;

    private double lastSentTime;   // クライアントが送信した時刻
    private double lastRecvTime;   // クライアントが受信した最新の時刻

    public float yaw;

    private void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        if (cameraRoot != null)
            targetPitch = cameraRoot.localEulerAngles.x;
    }

    private void Update()
    {
        if (isLocalPlayer && isSynchronize)
        {
            TrySendTransform();
            return;
        }

        // 他クライアント側：補間
        if (!isLocalPlayer && isSynchronize)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpFactor);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpFactor);

            if (cameraRoot != null)
            {
                Vector3 euler = cameraRoot.localEulerAngles;
                euler.x = Mathf.LerpAngle(euler.x, targetPitch, pitchLerpFactor);
                cameraRoot.localEulerAngles = euler;
            }
        }
    }

    private void TrySendTransform()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        float pitch = cameraRoot != null ? cameraRoot.localEulerAngles.x : 0f;

        if (Vector3.Distance(pos, lastSentPosition) > positionSendThreshold ||
            Quaternion.Angle(rot, lastSentRotation) > rotationSendThreshold ||
            Mathf.Abs(Mathf.DeltaAngle(pitch, lastSentPitch)) > pitchSendThreshold)
        {
            lastSentPosition = pos;
            lastSentRotation = rot;
            lastSentPitch = pitch;
            lastSentTime = NetworkTime.time; // Mirrorが提供するサーバー同期時間

            CmdSendTransform(pos, rot, pitch, lastSentTime);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSendTransform(Vector3 position, Quaternion rotation, float pitch, double timestamp)
    {
        if (!isSynchronize) return;

        // サーバー側でクライアントからのtimestampをそのまま転送
        RpcSyncTransform(position, rotation, pitch, timestamp);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSyncTransform(Vector3 position, Quaternion rotation, float pitch, double timestamp)
    {
        // 古いデータを無視
        if (timestamp <= lastRecvTime)
            return;

        lastRecvTime = timestamp;

        // 差分補間（巻き戻らない）
        targetPosition = Vector3.Lerp(targetPosition, position, 0.5f);
        targetRotation = Quaternion.Slerp(targetRotation, rotation, 0.5f);
        targetPitch = Mathf.LerpAngle(targetPitch, pitch, 0.5f);
    }

    [Server]
    public void ResetPosition(Vector3 newPosition, Quaternion newRotation, float newPitch = 0f)
    {
        transform.position = newPosition;
        transform.rotation = newRotation;

        if (cameraRoot != null)
        {
            Vector3 euler = cameraRoot.localEulerAngles;
            euler.x = newPitch;
            cameraRoot.localEulerAngles = euler;
        }

        double resetTime = NetworkTime.time;
        RpcForceSetTransform(newPosition, newRotation, newPitch, resetTime);

        lastSentPosition = newPosition;
        lastSentRotation = newRotation;
        lastSentPitch = newPitch;
        lastRecvTime = resetTime;
    }

    [ClientRpc]
    private void RpcForceSetTransform(Vector3 position, Quaternion rotation, float pitch, double timestamp)
    {
        if (timestamp <= lastRecvTime)
            return;

        lastRecvTime = timestamp;

        transform.position = position;
        transform.rotation = rotation;

        if (cameraRoot != null)
        {
            Vector3 euler = cameraRoot.localEulerAngles;
            euler.x = pitch;
            cameraRoot.localEulerAngles = euler;
        }

        targetPosition = position;
        targetRotation = rotation;
        targetPitch = pitch;
    }
}
