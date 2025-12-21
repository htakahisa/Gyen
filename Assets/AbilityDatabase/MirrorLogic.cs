using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Mirror")]
public class MirrorLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var overdose = owner.GetComponentInParent<Overdose>();
        overdose.RequestMirror(); // Åö CommandÇÕBehaviourë§Ç…âBÇ∑
    }
}
