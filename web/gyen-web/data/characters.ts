// data/characters.ts

// 武器の履歴の型
export interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

export interface Character {
  name: string;
  rank: "S" | "A" | "B" | "C";
  description: string;
  history: HistoryEntry[];
}

export const characters: Character[] = [
    {
    name: "Trident",
        rank: "S",
    description: "カナダから風に吹かれてトライデントは、自然と共存したスタイルで広大な自然を駆け回る。",
    history: [
      { version: "1.0.2", change: "Yellow中の上昇、下降速度を3から1に減少" , type: "debuff" as const },
    ],
  },

];
