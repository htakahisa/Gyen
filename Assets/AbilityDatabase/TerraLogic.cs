using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Terra")]
public class TerraLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var ejah = owner.GetComponentInParent<Ejah>();
        ejah.RequestTerra(); // š Command‚ÍBehaviour‘¤‚É‰B‚·
    }
}
