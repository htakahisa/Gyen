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

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>キャラクターランク</h1>

      {/* ランキングの表示 */}
      {["S", "A", "B", "C"].map((rank) => (
        <div key={rank} style={{ marginBottom: "16px" }}>
          <h2 style={{ fontSize: "1.5rem", marginBottom: "8px" }}>{rank}ランク</h2>
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
    </div>
  );
}
