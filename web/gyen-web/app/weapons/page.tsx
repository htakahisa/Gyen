"use client";
import Link from "next/link";
import { weapons } from "../../data/items";
import { CardButton } from "../components/UI";

export default function WeaponRank() {
  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      {/* ホームに戻るボタン */}
      <div style={{ marginBottom: "20px" }}>
        <Link href="/" className="no-underline">
          <CardButton>ホームに戻る</CardButton>
        </Link>
      </div>

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>武器Tier</h1>

      {/* ランキングの表示 */}
      {["1", "2", "3", "4"].map((rank) => (
        <div key={rank} style={{ marginBottom: "16px" }}>
          <h2 style={{ fontSize: "1.5rem", marginBottom: "8px" }}>Tier{rank}</h2>
          <div style={{ display: "flex", gap: "12px", flexWrap: "wrap" }}>
            {weapons
              .filter((w) => w.rank === rank)
              .map((w) => (
                <Link
                  key={w.name}
                  href={`/item/${w.name}`}
                  className="no-underline"
                >
                  <CardButton>{w.name}</CardButton>
                </Link>
              ))}
          </div>
        </div>
      ))}

      {/* ↓ ランク基準を下部に追加 */}
      <WeaponRankDescriptionSection />
    </div>
  );
}

// --- 武器ランク基準データ ---
const weaponRankDescriptions = [
  { rank: "Tier1", text: "値段と性能がいい意味で釣り合っていないか、何かにおいて圧倒的な性能を持つ武器。使えないと困る。" },
  { rank: "Tier2", text: "一部のキャラ、使い方においてはTier1と渡り会える性能を持つ武器。使えたほうがいい。" },
  { rank: "Tier3", text: "安い時に渋々買う武器や、効果的に使える場面が限られている武器。" },
  { rank: "Tier4", text: "値段と火力が悪い意味で釣り合っていない。この武器を使って勝てるラウンドはラッキー。" },
];

// --- 武器ランク基準セクション ---
const WeaponRankDescriptionSection: React.FC = () => {
  return (
    <div
      style={{
        marginTop: "40px",
        padding: "16px",
        background: "#1e1e2f",
        borderRadius: "8px",
      }}
    >
      <h2
        style={{
          fontSize: "1.4rem",
          fontWeight: "bold",
          marginBottom: "12px",
        }}
      >
        武器ランク基準
      </h2>
      <ul style={{ lineHeight: "1.8" }}>
        {weaponRankDescriptions.map((item) => (
          <li key={item.rank}>
            <strong>{item.rank}：</strong>
            {item.text}
          </li>
        ))}
      </ul>
    </div>
  );
};
