// data/items.js


export interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

export interface Item {
  name: string;
  rank: "1" | "2" | "3" | "4";
  description: string;
  history: HistoryEntry[];

  // ▼ 追加項目 ▼
  type: string;              // 例: "展開型" "汎用型" など
  difficulty: string;        // 例: "簡単" "普通" "難しい"
  cost: string;              // 例: "高" "中" "低"
  features: string[];        // 特徴リスト
  details: string;         // 詳しい解説（任意）
}


export const weapons: Item[] = [
    {
    name: "Lover",
        rank: "4",
    description: "とにかく安い軽量ピストル",
        history: [
      { version: "1.0.2", change: "胴体ダメージを30から20へ低下", type: "debuff" as const },
      { version: "1.0.2", change: "頭ダメージを70から80へ増加", type: "buff" as const },
    ],
        type: "ピストル",
    difficulty: "普通",
    cost: "0",
    features: ["頭の火力はそれなりに高い", "値段が無料なので出費を抑えられる", "連射速度と火力の低さがどうしても気になる"],
    details:
      "連射速度と火力共に弱い。ファーストラウンドかエコラウンド以外で使うことはない。この武器でライフルに勝てるなら相当なエイマーであると言える。",
  },
  
   {
    name: "Leo",
        rank: "2",
    description: "値段に対してハイスペックなマシンピストル",
    history: [
      { version: "1.0.2", change: "胴体ダメージを35から30へ低下", type: "debuff" as const },
      { version: "1.0.2", change: "頭ダメージを60から70へ増加", type: "buff" as const },
      { version: "1.0.2", change: "リロード時間を1sから1.2sへ増加", type: "debuff" as const },
      { version: "1.0.5", change: "射撃レートを1/0.11sから1/0.1sへ向上", type: "buff" as const },
      { version: "1.0.9", change: "射撃レートを1/0.1sから1/0.11sへ低下", type: "debuff" as const },
      { version: "1.0.9", change: "横リコイルを0.1から0.25へ増加", type: "debuff" as const },
      { version: "1.0.9", change: "マガジンサイズを12から9へ減少", type: "debuff" as const },
    ],
        type: "マシンピストル",
    difficulty: "簡単",
    cost: "300",
    features: ["初心者でも扱いやすい", "胴体射ちでも火力が出るため、安定感がある"],
    details:
      "特に序盤で役に立つ。同じレベルの装備相手に、先に撃ち始めればエイムに自信がなくても基本的に勝てるため、安定感があるが、逆にアーマーなど高い装備の相手に対するワンチャンは狙いにくい。",
  },
  
     {
    name: "Liet",
        rank: "3",
    description: "高火力なサイレンサーピストル",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.16sから1/0.15sへ向上", type: "buff" as const },
      { version: "1.0.4", change: "頭ダメージを120から125へ増加", type: "buff" as const },
      { version: "1.0.9", change: "マガジンサイズを9から10へ拡張", type: "buff" as const },
    ],
    type: "サイレンサーピストル",
    difficulty: "普通",
    cost: "800",
    features: ["連射速度、ヘッドショット火力共に平均より高い", "圧倒的な強みはないがコスパが高い"],
    details:
      "勿論できればヘッドショットを狙っていきたいが、胴体火力も悪くなく、連射速度も速いため、グダって全く当たらず負けるということは少ない。",
    
  },
   {
    name: "Anti-REX",
        rank: "2",
    description: "重たいリボルバー",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.8sから1/0.7sへ向上", type: "buff" as const },
      { version: "1.0.7", change: "値段を1200creditから1100creditへ減少", type: "buff" as const },
      { version: "1.0.9", change: "縦リコイルを0.75から0.6へ減少", type: "buff" as const },
      { version: "1.0.9", change: "射撃レートを1/0.7sから1/0.6sへ向上", type: "buff" as const },
    ],
    type: "リボルバー",
    difficulty: "とても難しい",
    cost: "1100",
    features: ["初心者にはオススメできない", "難しいが使いこなせれば破格の性能", "ヘッドショットの癖がないと使えない"],
    details:
      "ヘッドショットに自信がある人以外は使わないほうがいい。ヘッドショット前提で作られた性能といっていいほど、火力以外がお粗末な性能。",
  },
     {
    name: "Kafka",
        rank: "3",
    description: "スタンダードなサブマシンガン",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.07sから1/0.065sへ向上", type: "buff" as const },
    ],
    type: "サブマシンガン",
    difficulty: "普通",
    cost: "1600",
    features: ["連射速度がとにかく速い", "リコイルさえコントロール出来ればDPSはとても高い", "胴体でもいいのでとにかく当てたい"],
    details:
      "連射速度が高く、セミオートの武器よりは使いやすいが、反動が大きいため、リコイル制御の練習は必要になる。また、スコープ時に射撃すると5点バーストになり、射撃速度が上がる。",
  },
  {
    name: "FALLEN",
        rank: "2",
    description: "一撃が強力なヘビーライフル",
    history: [
      { version: "1.0.1", change: "胴体ダメージを65から70へ増加",  type: "buff" as const },
      { version: "1.0.1", change: "スコープ視野角を60から45へ減少", type: "neutral" as const },
      { version: "1.0.2", change: "値段を2000creditから1800creditへ減少", type: "buff" as const },
      { version: "1.0.9", change: "ロスターから保管庫へ移動", type: "buff" as const },
    ],
    type: "ヘビーライフル",
    difficulty: "難しい",
    cost: "1800",
    features: ["火力がとにかく高い", "置きエイムに向いている", "遠距離も得意"],
    details:
      "ヘッドショットに自信がある人が使うべき。一応ボディーショットで倒し切るのも可能。スコープ倍率が高いため、遠くの敵を倒すのが得意な武器。",
  },
    {
    name: "Share",
        rank: "2",
    description: "シェアー",
    history: [
      { version: "1.0.9", change: "保管庫からロスターへ移動", type: "buff" as const },
      { version: "1.0.9", change: "頭ダメージを135から155へ増加", type: "buff" as const },
      { version: "1.0.9", change: "値段を2000creditから2100creditへ減少", type: "debuff" as const },
    ],
    type: "アサルトライフル",
    difficulty: "普通",
    cost: "2100",
    features: ["3点バースト付き", "置きエイムに向いている", "安定感が高い"],
    details:
      "KasMiやReiNeが買えないときに妥協で買う武器。全体的にステータスが高いので、それらにも勝てる可能性がある。",
  },
    {
    name: "KasMi",
        rank: "1",
    description: "ライフル界のクイーン",
    history: [
      { version: "1.0.1", change: "胴体ダメージを40から45へ増加" , type: "buff" as const },
      { version: "1.0.3", change: "射撃レートを1/0.08から1/0.09へ低下" , type: "debuff" as const },
      { version: "1.0.9", change: "値段を2500creditから2700creditへ減少", type: "debuff" as const },
    ],
    type: "アサルトライフル",
    difficulty: "簡単",
    cost: "2700",
    features: ["初心者でも比較的扱いやすい", "安定感の獣", "使い手を選ばなく簡単"],
    details:
      "バランスが完璧で、買える時はとりあえず買っておけば損はしないだろう。殆どの場面で対して勝算がある、オールマイティ",
  },
    {
    name: "ReiNe",
        rank: "1",
    description: "ライフル界のビショップ",
    history: [
      { version: "1.0.1", change: "胴体ダメージを50から60へ増加" , type: "buff" as const },
      { version: "1.0.5", change: "横リコイルを0.1から0.2へ増加" , type: "debuff" as const },
      { version: "1.0.6", change: "胴体ダメージを60から50へ減少" , type: "debuff" as const },
      { version: "1.0.9", change: "横リコイルを0.2から0.3へ増加" , type: "debuff" as const },
      { version: "1.0.9", change: "値段を2500creditから2700creditへ減少", type: "debuff" as const },
    ],
    type: "アサルトライフル",
    difficulty: "普通",
    cost: "2700",
    features: ["全てを瞬殺する破壊力", "弱い装備の相手を軽々踏み潰す", "使いこなせないならKasMiを使おう"],
    details:
      "圧倒的破壊力で、この武器を使いこなすものが真のGyenと言える。",
  },
      {
    name: "Hazard",
        rank: "1",
    description: "一撃必殺のスナイパーライフル",
    history: [
      { version: "1.0.1", change: "頭ダメージを300から400へ増加" , type: "buff" as const },
      { version: "1.0.2", change: "スコープ視野角を30から25へ減少" , type: "neutral" as const },
    ],
    type: "スナイパーライフル",
    difficulty: "やや難しい",
    cost: "4000",
    features: ["直接ヒットすれば必殺", "覗いていないとまっすぐ飛ばない"],
    details:
      "相手がピークする場所にエイムをしておいて、来たら撃つというのが正しい使い方。値段が高いため、負けると次のラウンドは経済的にピンチになる。",
  },
    {
    name: "RapetPuppet",
        rank: "4",
    description: "高速ライトマシンガン",
    history: [
      { version: "1.0.2", change: "連射レートを1/0.07sから1/0.05sへ向上" , type: "buff" as const },
      { version: "1.0.2", change: "マガジンサイズを100から70へ減少" , type: "debuff" as const },
      { version: "1.0.3", change: "連射レートを1/0.05sから1/0.07sへ低下" , type: "debuff" as const },
      { version: "1.0.3", change: "縦リコイルを0.3から0.4へ増加" , type: "debuff" as const },
      { version: "1.0.6", change: "胴体ダメージを30から55へ増加" , type: "buff" as const },
      { version: "1.0.7", change: "横リコイルを0.3から0.4へ増加" , type: "debuff" as const },
    ],
    type: "ライトマシンガン",
    difficulty: "かなり難しい",
    cost: "2000",
    features: ["連射速度が高いため、壁抜きが簡単", "正面からの撃ち合いは苦手"],
    details:
      "使い手を選ぶというより、使い方がよくわからない武器。それなりに値段がかかる割に強いのは連射速度だけ。",
  },
    {
    name: "Violets",
        rank: "3",
    description: "超重量ライトマシンガン",
    history: [
      { version: "1.0.2", change: "連射レートを1/0.06sから1/0.045sへ向上" , type: "buff" as const },
      { version: "1.0.3", change: "連射レートを1/0.045sから1/0.06sへ低下" , type: "debuff" as const },
      { version: "1.0.3", change: "縦リコイルを0.55から0.03へ減少" , type: "buff" as const },
      { version: "1.0.6", change: "胴体ダメージを45から60へ増加" , type: "buff" as const },
    ],
    type: "ライトマシンガン",
    difficulty: "かなり難しい",
    cost: "4500",
    features: ["壁抜き用の採用なら圧倒的な性能", "お金にかなり余裕があるとき以外購入は非推奨"],
    details:
      "RapetPuppetの上位互換。値段が跳ね上がったため性能の向上も著しく、壁抜きの適正は勿論、ライフルに正面から撃ち勝てなくもない。",
  },
   
];
// 名前付きエクスポートをまとめておく（必要なら items としても扱える）
export const items = weapons;
