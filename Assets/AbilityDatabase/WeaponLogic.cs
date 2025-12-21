using UnityEngine;

public abstract class WeaponLogic : ScriptableObject
{
    public abstract void Use(WeaponManager owner);
}
