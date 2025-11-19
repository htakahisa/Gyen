using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public CharacterSkills currentCharacter;



    // スキル1を実行
    public void UseSkill1()
    {

        if (currentCharacter != null && currentCharacter.Skill1 != null)
        {
            currentCharacter.Skill1.Invoke();
        }
    }

    // スキル2を実行
    public void UseSkill2()
    {

        if (currentCharacter != null && currentCharacter.Skill2 != null)
        {
            currentCharacter.Skill2.Invoke();
        }
    }

    // スキル3を実行
    public void UseSkill3()
    {

        if (currentCharacter != null && currentCharacter.Skill3 != null)
        {
            currentCharacter.Skill3.Invoke();
        }
    }
}