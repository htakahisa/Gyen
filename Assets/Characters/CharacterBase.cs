using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    public abstract void Init(CharacterDataBase data);
    public abstract void Use();
}
