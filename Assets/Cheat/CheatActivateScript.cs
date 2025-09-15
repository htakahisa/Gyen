using UnityEngine;

public class CheatActivateScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    [SerializeField]
    private GameObject mirrorTunnelPrefab; // プレハブをここにセットしてください
    private GameObject mirrorTunnelInstance; // 生成したインスタンス


    [SerializeField]
    private GameObject mirroMapPref; // プレハブをここにセットしてください
    private GameObject mirroMapInstance;


    private Vector3 offset = new Vector3(0f, 0f, 0.5f); // プレイヤーカメラの前方に表示

    private bool isVisible = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (mirrorTunnelInstance == null)
            {
                // プレハブから初回インスタンス化
                mirrorTunnelInstance = Instantiate(mirrorTunnelPrefab);
                mirrorTunnelInstance.SetActive(false);
                isVisible = true;
            }
            else
            {
                Destroy(mirrorTunnelInstance);  // インスタンスを破棄
                mirrorTunnelInstance = null;    // 参照をクリア
                isVisible = false;              // 状態も切る
                return;
            }

            
            mirrorTunnelInstance.SetActive(isVisible);

            if (isVisible)
            {
                GameObject myPlayer = RoundManager.rm.GetMyPlayer();
                if (myPlayer != null)
                {
                    Transform camTransform = myPlayer.GetComponentInChildren<Camera>().transform; // 必要ならカメラのTransformに置き換えてください

                    Vector3 newPosition = camTransform.position + camTransform.forward * offset.z +
                                          camTransform.up * offset.y +
                                          camTransform.right * offset.x;

                    mirrorTunnelInstance.transform.position = newPosition;
                    mirrorTunnelInstance.transform.rotation = Quaternion.LookRotation(-camTransform.forward);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            if (mirroMapInstance == null) {
                mirroMapInstance = Instantiate(mirroMapPref);
            } else {
                Destroy(mirroMapInstance);  // インスタンスを破棄
                mirroMapInstance = null;    // 参照をクリア
                return;
            }
        }

    }
}
