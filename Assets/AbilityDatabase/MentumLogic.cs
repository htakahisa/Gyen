using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Weapon Logic/Mentum")]
public class MentumLogic : WeaponLogic
{
    public override void Use(WeaponManager owner)
    {
        var ejah = owner.GetComponentInParent<Ejah>();
        ejah.RequestMentum(); // š Command‚ÍBehaviour‘¤‚É‰B‚·
    }
}
