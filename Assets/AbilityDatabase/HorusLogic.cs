using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Horus")]
public class HorusLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var ejah = owner.GetComponentInParent<Ejah>();
        ejah.RequestHorus(); // š Command‚ÍBehaviour‘¤‚É‰B‚·
    }
}
