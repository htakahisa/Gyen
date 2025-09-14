using Mirror;
using System;
using UnityEngine;

// キャラクターのスキル定義
public class CharacterSkills : NetworkBehaviour
{
    // スキル1, スキル2のデリゲート（メソッド参照）
    public Action Skill1;
    public Action Skill2;
    public Action Skill3;

    public int skill1Energy;
    public int skill2Energy;
    public int skill3Energy;

    public int ultimateEnergy;

    private void Start()
    {
        //// スキルをデフォルトでセット（例）
        //Skill1 = DefaultSkill1;
        //Skill2 = DefaultSkill2;
    }

    // デフォルトスキル
    //void DefaultSkill1()
    //{
    //    Debug.Log("Default Skill1 executed");
    //}

    //void DefaultSkill2()
    //{
    //    Debug.Log("Default Skill2 executed");
    //}
}



