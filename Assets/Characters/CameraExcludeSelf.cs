using UnityEngine;

public class CameraExcludeSelfLight : MonoBehaviour
{
    public Camera cam;
    public int selfLayer = 6; // PlayerSelf のレイヤー番号

    int originalMask;

    void OnPreCull()
    {
        if (Camera.current == cam)
        {
            originalMask = cam.cullingMask;
            cam.cullingMask &= ~(1 << selfLayer);  // 自分のレイヤーだけ非表示
        }
    }

    void OnPostRender()
    {
        if (Camera.current == cam)
        {
            cam.cullingMask = originalMask; // 元に戻す
        }
    }
}
