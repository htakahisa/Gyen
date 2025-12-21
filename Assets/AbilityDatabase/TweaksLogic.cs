using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Tweaks")]
public class TweaksLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var overdose = owner.GetComponentInParent<Overdose>();
        overdose.RequestTweaks(); // Åö CommandÇÕBehaviourë§Ç…âBÇ∑
    }
}
