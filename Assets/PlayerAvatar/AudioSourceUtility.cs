using UnityEngine;

public static class AudioSourceUtility
{
    /// <summary>
    /// PlayClipAtPointの拡張版。
    /// 3D音や距離減衰などの設定を自由に調整可能。
    /// </summary>
    public static void PlayClipAtPointCustom(
        AudioClip clip,
        Vector3 position,
        float volume = 1f,
        float minDistance = 1f,
        float maxDistance = 500f,
        AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic)
    {
        if (clip == null)
            return;

        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.position = position;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1.0f; // 3D音にする
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;

        audioSource.Play();
        Object.Destroy(tempGO, clip.length);
    }
}
