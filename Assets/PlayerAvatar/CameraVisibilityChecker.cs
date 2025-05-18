using UnityEngine;

public static class CameraVisibilityChecker
{
    /// <summary>
    /// �w�肵���J�����̎���ɁA�w�肵���I�u�W�F�N�g�������Ă��邩�𔻒肵�܂��B
    /// </summary>
    /// <param name="camera">����Ɏg�p����J����</param>
    /// <param name="target">����ɓ����Ă��邩���`�F�b�N����ΏۃI�u�W�F�N�g</param>
    /// <returns>�J�����̎���ɃI�u�W�F�N�g�������Ă���� true</returns>
    public static bool IsObjectVisibleFromCamera(Camera camera, GameObject target)
    {
        if (camera == null || target == null) return false;

        // �J�����̎�������擾
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // �Ώۂ� Renderer ���擾
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) return false;

        // Renderer �̃o�E���f�B���O�{�b�N�X��������Ɋ܂܂�Ă��邩���`�F�b�N
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}

public static class AimChecker
{
    /// <summary>
    /// �J�����̒��S�i�G�C���j���Ώۂ̃I�u�W�F�N�g�����Ă��邩�𔻒肵�܂��B
    /// </summary>
    /// <param name="camera">���_�J����</param>
    /// <param name="target">�����ΏۃI�u�W�F�N�g</param>
    /// <param name="maxDistance">���C�̍ő勗���i�ȗ��A�f�t�H���g1000�j</param>
    /// <returns>�J�����̒��S���Ώۂ����Ă���� true</returns>
    public static bool IsObjectInCameraCenter(Camera camera, GameObject target, float maxDistance = 1000f)
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // ��ʒ������烌�C���΂�
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // ���C���ΏۃI�u�W�F�N�g�ɓ������Ă��邩�ǂ������`�F�b�N
            return hit.transform == target.transform;
        }

        return false;
    }
}

