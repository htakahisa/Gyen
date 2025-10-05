"use client";
import Link from "next/link";
import { characters } from "../../data/characters"; // キャラデータ
import { CardButton } from "../components/UI";

export default function CharacterRank() {
  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      {/* ホームに戻るボタン */}
      <div style={{ marginBottom: "20px" }}>
        <Link href="/" className="no-underline">
          <CardButton>ホームに戻る</CardButton>
        </Link>
      </div>

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>キャラクターTier</h1>

      {/* ランキングの表示 */}
      {["1", "2", "3", "4"].map((rank) => (
        <div key={rank} style={{ marginBottom: "16px" }}>
          <h2 style={{ fontSize: "1.5rem", marginBottom: "8px" }}>Tier{rank}</h2>
          <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
            {characters
              .filter((c) => c.rank === rank)
              .map((c) => (
                <Link
                  key={c.name}
                  href={`/item/${c.name}`}
                  className="no-underline"
                >
                  <CardButton>{c.name}</CardButton>
                </Link>
              ))}
          </div>
        </div>
      ))}

      {/* ↓ ランク基準を下部に追加 */}
      <RankDescriptionSection />
    </div>
  );
}

// --- ランク基準データ ---
const characterRankDescriptions = [
  { rank: "Tier1", text: "環境トップ。圧倒的なパワーがある。使えなくてもせめてちゃんと対策しておきたい。" },
  { rank: "Tier2", text: "キャラパワーではTier1には届かないが、一部のTier1キャラと渡り合える性能がある。対策しておきたい。" },
  { rank: "Tier3", text: "一部の戦術では見ることがある。Tier1かTier2の一部のキャラに勝算があり、対策してなくてもそこまで問題ない。" },
  { rank: "Tier4", text: "かなり安定性に欠けるか、Tier1、Tier2のキャラに殆ど不利をとるキャラ。使うとしたら好みの問題。" },
];

// --- ランク基準セクション ---
const RankDescriptionSection: React.FC = () => {
  return (
    <div style={{ marginTop: "40px", padding: "16px", background: "#1e1e2f", borderRadius: "8px" }}>
      <h2 style={{ fontSize: "1.4rem", fontWeight: "bold", marginBottom: "12px" }}>
        ランク基準
      </h2>
      <ul style={{ lineHeight: "1.8" }}>
        {characterRankDescriptions.map((item) => (
          <li key={item.rank}>
            <strong>{item.rank}：</strong>
            {item.text}
          </li>
        ))}
      </ul>
    </div>
  );
};
