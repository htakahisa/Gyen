// data/characters.ts
export interface Character {
  name: string;
  rank: "S" | "A" | "B" | "C";
  description: string;
}

export const characters: Character[] = [
  { name: "Trident", rank: "S", description: "カナダから風に吹かれてトライデントは、自然と共存したスタイルで広大な自然を駆け回る。" },

];
