using Mirror;
using UnityEngine;

public class CharacterTransfromNetwork : NetworkBehaviour
{
    private Vector3 lastPosition;
    private Vector3 targetPosition;

    private Quaternion lastRotation;

    public float yaw;   // 左右回転（身体）
    private float t;

    public CharacterController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            t += Time.deltaTime * 10f; // 補間速度
            transform.position = Vector3.Lerp(lastPosition, targetPosition, t);
        }

    }

    public void SetTarget(Vector3 newPos)
    {
        lastPosition = transform.position;
        targetPosition = newPos;
        t = 0f;
    }

    [Command]
    public void CmdPos(Vector3 position)
    {
        controller.enabled = false;
        transform.transform.position = position;
        controller.enabled = true;

        RpcPositionNetwork(position);
    }

    [Command]
    public void CmdRotate(Quaternion rotation)
    {
        transform.transform.rotation = rotation;
        RpcRotationNetwork(rotation);
    }

    [Server]
    public void ServerPositionNetwork(Vector3 position)
    {
        RpcPositionNetwork(position);
        lastPosition = transform.position;
    }

    [Server]
    public void ServerRotationNetwork(Quaternion rotation)
    {
        RpcRotationNetwork(rotation);
        lastRotation = transform.rotation;
    }


    [ClientRpc]
    public void RpcPositionNetwork(Vector3 position)
    {
        if (!isLocalPlayer)
        {

            SetTarget(position);

            // 他プレイヤーはサーバーの位置に補正
            controller.enabled = false;
            transform.transform.position = position;
            controller.enabled = true;
        }
        else
        {

            // 自分自身はサーバーとの差分を補正
            float dist = Vector3.Distance(transform.position, position);
            if (dist > 0.1f) // 大きくズレていたら修正
            {
                transform.position = Vector3.Lerp(transform.position, position, 0.5f);
            }
        }
    }

    [ClientRpc]
    public void RpcRotationNetwork(Quaternion rotation)
    {
        if (!isLocalPlayer)
        {
            // 他プレイヤーはサーバーの位置に補正
            controller.enabled = false;
            transform.transform.rotation = rotation;
            controller.enabled = true;
        }
        else
        {

            float angleDiff = Quaternion.Angle(transform.rotation, rotation);
            if (angleDiff > 2f) // ある程度ズレが大きいときだけ補正
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.5f);
            }
        }
    }
}
