"use client";
import { useRouter } from "next/navigation";
import { items } from "../../../data/items";
import { characters } from "../../../data/characters";

interface Props {
  params: { name: string };
}

export default function DetailPage({ params }: Props) {
  const router = useRouter();
  const { name } = params;

  // アイテムを探す
  const item = items.find((i) => i.name === name);
  // キャラを探す
  const character = characters.find((c) => c.name === name);

  // どちらもなければエラーメッセージ
  if (!item && !character) {
    return <p>このデータの情報がありません。</p>;
  }

  // 共通化のため、対象データをまとめる
  const target = item || character;
  const typeLabel = item ? "アイテム" : "キャラクター";

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      <h2 style={{ fontSize: "1.5rem" }}>
        {target?.name}（ランク: {target?.rank} / {typeLabel}）
      </h2>
      <p>{target?.description}</p>

      <h3>調整履歴</h3>
      <ul>
        {target?.history.map((h, idx) => (
          <li
            key={idx}
            style={{
              color:
                h.type === "buff"
                  ? "limegreen"
                  : h.type === "debuff"
                  ? "red"
                  : "white",
              marginBottom: "6px",
            }}
          >
            {h.version} - {h.change}
          </li>
        ))}
      </ul>

      <button
        style={{
          marginTop: "20px",
          padding: "8px 12px",
          background: "#2a2a40",
          border: "none",
          borderRadius: "6px",
          cursor: "pointer",
          color: "white",
        }}
        onClick={() => router.back()}
      >
        戻る
      </button>
    </div>
  );
}
