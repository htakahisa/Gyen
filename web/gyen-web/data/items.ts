// data/items.js

// 武器の履歴の型
export interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

// 武器の型
export interface Item {
  name: string;
  rank: "S" | "A" | "B" | "C";
  description: string;
  history: HistoryEntry[];
}

export const weapons: Item[] = [
    {
    name: "Lover",
        rank: "C",
    description: "軽量ピストル。無料で使える武器であるため高望みは出来ないが、それを考慮しても使いたくない武器である。",
    history: [
      { version: "1.0.2", change: "胴体ダメージを30から20に低下", type: "debuff" as const },
      { version: "1.0.2", change: "頭ダメージを70から80に増加", type: "buff" as const },
    ],
  },
   {
    name: "Leo",
        rank: "A",
    description: "ハイスペックなマシンピストル。値段が300creditで済むにもかかわらず、性能はLoverよりもかなり良いため、せめてこの武器は買っておきたい。",
    history: [
      { version: "1.0.2", change: "胴体ダメージを35から30に低下", type: "debuff" as const },
      { version: "1.0.2", change: "頭ダメージを60から70に増加", type: "buff" as const },
      { version: "1.0.2", change: "リロード時間を1sから1.2sに増加", type: "debuff" as const },
    ],
  },
     {
    name: "Liet",
        rank: "B",
    description: "高火力なサイレンサーピストル。アーマーがない相手の頭に当てると一撃確殺なため、ファーストラウンドで言えば最強の武器と言える。",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.16sから1/0.15sに向上", type: "buff" as const },
    ],
  },
   {
    name: "Anti-REX",
        rank: "A",
    description: "重たいヘビーピストル。火力と値段の割合が圧倒的で、頭に当てれば必ず仕留めることができるが、連射速度がかなり低い。",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.8sから1/0.7sに向上", type: "buff" as const },
    ],
  },
     {
    name: "Kafka",
        rank: "A",
    description: "スタンダードなサブマシンガン。連射レートが高いため、ズーム時に5点バーストとなる。",
    history: [
      { version: "1.0.2", change: "射撃レートを1/0.07sから1/0.065sに向上", type: "buff" as const },
    ],
  },
  {
    name: "FALLEN",
        rank: "A",
    description: "一撃が強力なヘビーライフル。連射速度は低めなため、若干使いにくいのが玉に瑕。",
    history: [
      { version: "1.0.1", change: "胴体ダメージを65から70へ増加",  type: "buff" as const },
      { version: "1.0.1", change: "スコープ視野角を60から45へ減少", type: "neutral" as const },
      { version: "1.0.2", change: "値段を2000creditから1800creditへ減少", type: "buff" as const },
    ],
  },
    {
    name: "kasMi",
        rank: "S",
    description: "連射速度、火力、マガジンサイズ全てにおいてハイスペックなライフル。使いやすさはピカイチ。",
    history: [
      { version: "1.0.1", change: "胴体ダメージを40から45へ増加" , type: "buff" as const }
    ],
  },
    {
    name: "ReiNe",
        rank: "S",
    description: "ヘッドショットで必ず敵を仕留めることができる、最強のライフル。リコイルが激しいため、最初の5発以内に倒しておきたい。",
    history: [
      { version: "1.0.1", change: "胴体ダメージを50から60へ増加" , type: "buff" as const },
    ],
  },
      {
    name: "Hazard",
        rank: "S",
    description: "一撃必殺のライフル。貫通で胴体にあてた場合を除き必ず相手を仕留めることができる。ただし外した時の隙には目をつぶろう。",
    history: [
      { version: "1.0.1", change: "頭ダメージを300から400へ増加" , type: "buff" as const },
      { version: "1.0.2", change: "スコープ視野角を30から25へ減少" , type: "neutral" as const },
    ],
  },
    {
    name: "RapetPuppet",
        rank: "B",
    description: "高速ライトマシンガン。連射速度が速く、壁抜きをするときの選択肢として入る武器であるが、それ以外の使い方ではあまりメリットがない。",
    history: [
      { version: "1.0.2", change: "連射レートを1/0.07sから1/0.05sに向上" , type: "buff" as const },
      { version: "1.0.2", change: "マガジンサイズを100から70に減少" , type: "debuff" as const },
    ],
  },
    {
    name: "Violets",
        rank: "A",
    description: "超重量ライトマシンガン。連射速度、マガジンサイズ共に最強の武器であるが、意外と一発のダメージ自体は控えめなので通常の打ち合いになると負けることが多い。",
    history: [
      { version: "1.0.2", change: "連射レートを1/0.06sから1/0.045sに向上" , type: "buff" as const },
    ],
  },
   
];
// 名前付きエクスポートをまとめておく（必要なら items としても扱える）
export const items = weapons;
