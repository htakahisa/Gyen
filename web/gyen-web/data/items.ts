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
    name: "FALLEN",
        rank: "A",
    description: "一撃が強力なヘビーピストル。連射速度は低めなため、若干使いにくいのが玉に瑕。",
    history: [
      { version: "1.0.1", change: "胴体ダメージを65から70へ増加",  type: "buff" as const },
      { version: "1.0.1", change: "スコープ視野角を60から45へ減少", type: "neutral" as const },
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
    ],
  },
];
// 名前付きエクスポートをまとめておく（必要なら items としても扱える）
export const items = weapons;
