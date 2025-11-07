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
    name: "System_GameMode",
    rank: "A", 
    description: "ゲームモード自体やそれの処理について。",
    history: [
      { version: "1.0.7", change: "新しいゲームモード「デュエルランド」を追加しました。実際の1VS1でのマップを使い、ボットを殲滅するモードです。", type: "buff" as const },
      { version: "1.0.7", change: "プラクティスモードにて、ボットがプレイヤーに撃ち返してくるようになりました", type: "buff" as const },
      { version: "1.0.7", change: "デュエルランドモード中にボットをキルした場合に、キルバナーとフィニッシャーが表示されるように仕様を変更", type: "buff" as const },
    ],
  },
    {
    name: "System_Sound",
    rank: "B", 
    description: "効果音や、BGMについて。",
    history: [
      { version: "1.0.6", change: "効果音が聞こえる最大範囲を30?から15へ狭めました", type: "buff" as const },
      { version: "1.0.6", change: "足音の鳴るインターバルを0.4sから1sに変更しました", type: "neutral" as const },
    ],
  },
    {
    name: "System_Map",
    rank: "A", 
    description: "試合に使うマップ自体や、それについての処理について。",
    history: [
      { version: "1.0.6", change: "マップ「Discipline」を追加しました", type: "buff" as const },
      { version: "1.0.7", change: "1VS1モードを開始したときに、ランダムなマップが選択されるように変更", type: "buff" as const },
    ],
  },
    {
    name: "System_GameRule",
    rank: "S", 
    description: "勝敗判定や、基本的かつ全般的な試合中のシステムについて。",
    history: [
      { version: "1.0.6", change: "”スパイク”を追加しました。ラウンド開始から一定の時間で爆発し、爆発すればアタッカー、解除すればディフェンダーの勝利です", type: "buff" as const },
      { version: "1.0.6", change: "購入画面で、左クリックで購入に対して、右クリックで売却という対応を追加しました", type: "buff" as const },
    ],
  },
    {
    name: "System_CharacterControl",
    rank: "A", 
    description: "一般的な、キャラクターに依存しない共通のキャラクターコントロールについて。",
    history: [
      { version: "1.0.5", change: "ジャンプ力の低下", type: "debuff" as const },
      { version: "1.0.5", change: "移動速度の上昇", type: "buff" as const },
      { version: "1.0.7", change: "移動速度の上昇", type: "buff" as const },
      { version: "1.0.7", change: "滞空時に、ジャンプ時の慣性に直前の入力を考慮して角度を計算するように変更", type: "buff" as const },
      { version: "1.0.7", change: "しゃがむアニメーション速度を減速", type: "buff" as const },
      { version: "1.0.7", change: "キャラクターが視点を回転させるとき、頭も一緒に回転して見えるように変更しました", type: "buff" as const },
    ],
  },
    {
    name: "System_InputSystem",
    rank: "S",
    description: "入力デバイスからのインプット自体またはそれの処理について。",
    history: [
      { version: "1.0.5", change: "コントローラーの入力の対応を可能にしました", type: "buff" as const },
      { version: "1.0.5", change: "入力デバイスがコントローラーのときのみ、エイムアシストが入るよう仕様を追加しました", type: "buff" as const },
      { version: "1.0.5", change: "購入画面を開いたとき、入力デバイスにかかわらず専用のカーソルが表示されるように仕様を変更しました", type: "neutral" as const },
      { version: "1.0.6", change: "ロビーでキーコンフィグを変更できるようにしました", type: "neutral" as const },
    ],
  },
    {
    name: "System_Visuals",
    rank: "B",
    description: "ゲームプレイに直接的に関係なく、UIでもないオブジェクト。エフェクトも含む。",
    history: [
      { version: "1.0.5", change: "武器を持っているとき、右手部分に武器を持っている見た目を追加しました", type: "buff" as const },
      { version: "1.0.7", change: "一部の武器の右手に持つ見た目を調整しました", type: "buff" as const },
    ],
  },
    {
    name: "System_UI",
    rank: "S",
    description: "一般的に言うUI。",
    history: [
      { version: "1.0.5", change: "購入画面と、ラウンド表示パネルのUIを変更しました", type: "buff" as const },
      { version: "1.0.6", change: "購入画面と、ラウンド表示パネルのUIを変更しました", type: "buff" as const },
      { version: "1.0.6", change: "画面左上に、ミニマップを表示しました", type: "buff" as const },
    ],
  },
];
// 名前付きエクスポートをまとめておく（必要なら items としても扱える）
export const items = systems;
