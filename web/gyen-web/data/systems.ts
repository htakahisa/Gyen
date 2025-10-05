export interface SystemChange {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}
// 武器の型
export interface System {
  name: string;
  rank: "S" | "A" | "B" | "C";
  description: string;
    details?: string;      // ← 詳しい解説（オプション）
  history: SystemChange[];
}



export const systems: System[] = [
    {
    name: "System_CharacterControl",
    rank: "S",
    description: "一般的な、キャラクターに依存しない共通のキャラクターコントロールについて。",
    history: [
      { version: "1.0.5", change: "ジャンプ力の低下", type: "debuff" as const },
      { version: "1.0.5", change: "移動速度の上昇", type: "buff" as const },
    ],
  },
     {
    name: "System_ControlScheme",
    rank: "B",
    description: "入力デバイスとして、キーボードマウスとコントローラーの違いにおける変更。",
    history: [
      { version: "1.0.5", change: "コントローラーの入力の対応を可能にしました。", type: "buff" as const },
      { version: "1.0.5", change: "入力デバイスがコントローラーのときのみ、エイムアシストが入るように仕様を変更しました", type: "buff" as const },
    ],
  },
     {
    name: "System_Visuals",
    rank: "B",
    description: "ゲームプレイに直接的に関係なく、UIでもないオブジェクト。エフェクトも含む。",
    history: [
      { version: "1.0.5", change: "武器を持っているとき、右手部分に武器を持っている見た目を追加しました。", type: "buff" as const },
    ],
  },
       {
    name: "System_UI",
    rank: "S",
    description: "一般的に言うUI。",
    history: [
      { version: "1.0.5", change: "購入画面と、ラウンド表示パネルのUIを変更しました。", type: "buff" as const },
    ],
  },
];
// 名前付きエクスポートをまとめておく（必要なら items としても扱える）
export const items = systems;
