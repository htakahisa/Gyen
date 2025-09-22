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

    [SyncVar]
    public int skill1Energy;
    [SyncVar]
    public int skill2Energy;
    [SyncVar]
    public int skill3Energy;

    public int default1Energy;
    public int default2Energy;
    public int default3Energy;

    public int ultimateEnergy;

    public void ResetSkill()
    {
        skill1Energy = default1Energy;
        skill2Energy = default2Energy;
        skill3Energy = default3Energy;
    }


}



