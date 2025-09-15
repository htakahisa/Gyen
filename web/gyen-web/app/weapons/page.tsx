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

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>武器ランク</h1>

      {/* ランキングの表示 */}
      {["S", "A", "B", "C"].map((rank) => (
        <div key={rank} style={{ marginBottom: "16px" }}>
          <h2 style={{ fontSize: "1.5rem", marginBottom: "8px" }}>{rank}ランク</h2>
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
    </div>
  );
}
