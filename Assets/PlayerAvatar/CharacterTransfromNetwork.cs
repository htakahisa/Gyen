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
        // ローカルプレイヤーが自分の変化を送信
        if (isLocalPlayer && isSynchronize)
        {
            TrySendTransform();
            return;
        }

        // 他クライアント or サーバー上の補間
        if (!isLocalPlayer)
        {
            // 固定補間率でスムーズに追従（Editorでも安定）
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

    /// <summary>
    /// プレイヤーの位置や角度が変わったらサーバーへ送信
    /// </summary>
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

            CmdSendTransform(pos, rot, pitch);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSendTransform(Vector3 position, Quaternion rotation, float pitch)
    {
        if (!isSynchronize) return;

        // Move()を使わずに強制ワープ
        transform.position = position;
        transform.rotation = rotation;

        if (cameraRoot != null)
        {
            Vector3 euler = cameraRoot.localEulerAngles;
            euler.x = pitch;
            cameraRoot.localEulerAngles = euler;
        }

        // 所有者以外にのみ通知（自分は補正不要）
        RpcSyncTransform(position, rotation, pitch);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcSyncTransform(Vector3 position, Quaternion rotation, float pitch)
    {
        // 差分補間：いきなり更新せず、滑らかに追従
        targetPosition = Vector3.Lerp(targetPosition, position, 0.5f);
        targetRotation = Quaternion.Slerp(targetRotation, rotation, 0.5f);
        targetPitch = Mathf.LerpAngle(targetPitch, pitch, 0.5f);
    }

    /// <summary>
    /// サーバー側からの位置リセット（ワープ）
    /// </summary>
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

        RpcForceSetTransform(newPosition, newRotation, newPitch);

        // クライアント送信基準を更新（反応遅延防止）
        lastSentPosition = newPosition;
        lastSentRotation = newRotation;
        lastSentPitch = newPitch;
    }

    [ClientRpc]
    private void RpcForceSetTransform(Vector3 position, Quaternion rotation, float pitch)
    {
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
