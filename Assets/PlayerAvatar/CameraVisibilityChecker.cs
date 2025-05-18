using UnityEngine;

public static class CameraVisibilityChecker
{
    /// <summary>
    /// 指定したカメラの視野に、指定したオブジェクトが入っているかを判定します。
    /// </summary>
    /// <param name="camera">判定に使用するカメラ</param>
    /// <param name="target">視野に入っているかをチェックする対象オブジェクト</param>
    /// <returns>カメラの視野にオブジェクトが入っていれば true</returns>
    public static bool IsObjectVisibleFromCamera(Camera camera, GameObject target)
    {
        if (camera == null || target == null) return false;

        // カメラの視錐台を取得
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // 対象の Renderer を取得
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) return false;

        // Renderer のバウンディングボックスが視錐台に含まれているかをチェック
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}

public static class AimChecker
{
    /// <summary>
    /// カメラの中心（エイム）が対象のオブジェクトを見ているかを判定します。
    /// </summary>
    /// <param name="camera">視点カメラ</param>
    /// <param name="target">注視対象オブジェクト</param>
    /// <param name="maxDistance">レイの最大距離（省略可、デフォルト1000）</param>
    /// <returns>カメラの中心が対象を見ていれば true</returns>
    public static bool IsObjectInCameraCenter(Camera camera, GameObject target, float maxDistance = 1000f)
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // 画面中央からレイを飛ばす
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // レイが対象オブジェクトに当たっているかどうかをチェック
            return hit.transform == target.transform;
        }

        return false;
    }
}

