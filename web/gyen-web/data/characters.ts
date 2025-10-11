// data/characters.ts

// 武器の履歴の型
export interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}
export interface SkillsEntry {
  name: string;
  description: string;
}

export interface Character {
  name: string;
  rank: "1" | "2" | "3" | "4";
  description: string;
  skills : SkillsEntry[];
  history: HistoryEntry[];
    // ▼ 追加項目 ▼
  type: string;              // 例: "展開型" "汎用型" など
  difficulty: string;        // 例: "簡単" "普通" "難しい"
  from: string;
  features: string[];        // 特徴リスト
  details: string;         // 詳しい解説（任意）
}


export const characters: Character[] = [
    {
    name: "Trident",
        rank: "1",
    description: "カナダから風に吹かれてトライデントは、自然と共存したスタイルで広大な自然を駆け回る。",
    history: [
      { version: "1.0.0", change: "トライデント参戦！" , type: "neutral" as const },
      { version: "1.0.2", change: "Yellowの上昇、下降速度を3から1に減少" , type: "debuff" as const },
      { version: "1.0.3", change: "Singingで回復中、専用のエフェクトの発生を追加" , type: "debuff" as const },
      { version: "1.0.3", change: "Yellowの前進スピードを3から5に上昇" , type: "buff" as const },
      { version: "1.0.3", change: "Yellow中にもダメージを受けるよう変更" , type: "debuff" as const },
      { version: "1.0.4", change: "Yellowには専用のHPがあり、100HP削れると人間アバターに強制的に切り替わる仕様に変更" , type: "neutral" as const },
      { version: "1.0.4", change: "Yellow後解除されたときに、慣性に関わらず必ず真下に落ちる仕様に変更" , type: "neutral" as const },
    ],
    skills: [
      { name: "Ability : Lime", description: "半透明なオーブを飛ばし、着地地点で爆発してライム色のスモークが生成される。" },
      { name: "Ability : Yellow", description: "鳥に変身し、空を自由に飛び回る。" },
      { name: "Ability : Singing", description: "自然の癒やしで、自身を回復させる。" },     
    ],
           type: "Hunter",
    difficulty: "簡単",
    from: "カナダ",
    features: ["初心者でも扱いやすい", "スキルYellowがチート性能", "モビリティ、スモーク、ヒールと純粋に出来ることが多い"],
    details:
      "現環境最強。他のアビリティをYellow中にも使えるため、取り敢えずYellowを打っておけば有利になるとさえ言える。",
  },
   {
    name: "Ejah",
        rank: "2",
    description: "エジプトから導かれるままにエリヤは、古代の超技術を使い空中に土を舞い上げ、レーザー付きナノマシンを操る。",
    history: [
      { version: "1.0.3", change: "エリヤ参戦！" , type: "neutral" as const },
      { version: "1.0.4", change: "Horusが検知する条件として、壁によって射線が遮られていないことを追加" , type: "debuff" as const },
      { version: "1.0.4", change: "Horusが検知する範囲を、10mから30mに拡大" , type: "buff" as const },
      { version: "1.0.6", change: "Horusを設置する際のプレイヤーからの最大距離が3mから5mに増加" , type: "buff" as const },
    ],
    skills: [
      { name: "Ability : Terra", description: "空中であっても、自由な位置に土元素由来のスモークを生成する。" },
      { name: "Ability : Horus", description: "自動で敵を検知しレーザーを射撃する古代のナノマシン、”ホルス”を生成する。" },
      { name: "Ability : Mentum", description: "地面を盛り上げ、土元素由来の土壁を生成する。" },  
    ],
           type: "Manipulator",
    difficulty: "普通",
    features: ["スモークが強い", "エリアマネージメントに優れる", "スキルを使い切るとシンプルに撃ち合うしかない"],
    from: "エジプト",
    details:
      "スモークの炊き方が独特で、エリアを保持する際にはTier1を超える適正を持つ。アタッカーだと難しいのが弱点。",
  },
     {
    name: "Lucifer",
        rank: "3",
    description: "地獄から現れたルシファーは、敵の血を何よりも欲し、相手が傷ついた分だけ強くなる。",
    history: [
      { version: "1.0.4", change: "ルシファー参戦！" , type: "neutral" as const },
    ],
    skills: [
      { name: "Ability : Rebelliousness", description: "ルシファー専用のリボルバー（のような武器。炎をまとっているためよく分からない）" },
      { name: "Ability : LighhtLoad", description: "ダークオーブを回収し、敵の位置を1秒間透視できる。" },
      { name: "Passive : Darkness", description: "相手に与えたダメージに応じてダークオーブが生成される。それを射撃で破壊すると与えたダメージに応じて回復する。" },  
    ],
           type: "Barbarian",
    difficulty: "難しい",
    from: "生まれたのは天国、育ったのは地獄",
    features: ["相手にダメージを与えてスキルが発動", "撃ち合うまではスキルが役に立たない"],
    details:
      "デフォルトで強力なリボルバーを持っているため、ファーストからサードくらいにかけては最強と言える。ただその後のラウンドでどうしてもスキルが役に立たなくなっていく",
  },
    {
    name: "Overdose",
        rank: "2",
    description: "アメリカ合衆国から来たオーバードーズは、科学が武力に勝ること、100%が存在すること証明する。",
    history: [
      { version: "1.0.4", change: "オーバードーズ参戦！" , type: "neutral" as const },
      { version: "1.0.6", change: "ITweaksを設置する際のプレイヤーからの最大距離が3mから5mに増加" , type: "buff" as const },
      { version: "1.0.6", change: "IMirrorをクライアントが発動したときに発射する向きが正しくない場合があった不具合を修正" , type: "buff" as const },
    ],
    skills: [
      { name: "Ability : ITweaks", description: "半透明のナノセンサーで、相手を検知しX線を発射する。それには弱い目眩の効果もある。" },
      { name: "Ability : IMirror", description: "「これは簡単に言えば小さなブラックホールだ。」視界が真っ黒になる、強烈な黒い何かを飛ばす。" },
      { name: "Ability : null", description: "null" },  
    ],
           type: "Warden",
    difficulty: "やや難しい",
    from: "アメリカ合衆国",
    features: ["主に", "特筆すべき強みはないが安定感がある"],
    details:
      "バランス型の武器で、特に序盤で役立つ。後半になると火力不足が目立ち始める。",
  },

];
