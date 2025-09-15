using UnityEngine;

public class MirrorTunnelScript : MonoBehaviour
{

    // プレイヤーカメラの前方に表示させるオフセット（親基準のローカル位置に変換して使います）
    private Vector3 offset = new Vector3(0f, 0f, 20f);

    private bool isVisible = false;

    void Start()
    {
        GameObject myPlayer = RoundManager.rm.GetMyPlayer();
        if (myPlayer != null)
        {
            Transform camTransform = myPlayer.GetComponentInChildren<Camera>().transform;

            // ワールド空間で鏡を置きたい場所を計算
            Vector3 targetWorldPos = camTransform.position + camTransform.forward * offset.z
                                                        + camTransform.up * offset.y
                                                        + camTransform.right * offset.x;

            // 親がいる場合はローカル座標に変換してセット
            if (gameObject.transform.parent != null)
            {
                gameObject.transform.localPosition =
                    gameObject.transform.parent.InverseTransformPoint(targetWorldPos);
            }
            else
            {
                gameObject.transform.position = targetWorldPos;
            }

            // 鏡の向きはカメラの正面を向く（カメラの前方方向の逆を向く）
            gameObject.transform.rotation = Quaternion.LookRotation(-camTransform.forward);

            Debug.Log("Mirror Position: " + gameObject.transform.position);
        }
    }

    void Update()
    {

         
    }
}
