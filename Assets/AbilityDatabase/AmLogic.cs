using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Am")]
public class AmLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var overdose = owner.GetComponentInParent<Overdose>();
        overdose.RequestAm(); // Åö CommandÇÕBehaviourë§Ç…âBÇ∑
    }
}
