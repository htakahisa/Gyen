using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Sing")]
public class SingLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var trident = owner.GetComponentInParent<Trident>();
        trident.RequestSing(); // š Command‚ÍBehaviour‘¤‚É‰B‚·
    }
}
