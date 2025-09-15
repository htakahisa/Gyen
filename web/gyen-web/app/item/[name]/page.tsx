"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { items } from "../../../data/items";

interface Props {
  params: { name: string };
}

export default function ItemDetail({ params }: Props) {
  const router = useRouter();
  const { name } = params;
  const item = items.find((i) => i.name === name);

  if (!item) {
    return <p>このアイテムの情報がありません。</p>;
  }

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      <h2 style={{ fontSize: "1.5rem" }}>
        {item.name}（ランク: {item.rank}）
      </h2>
      <p>{item.description}</p>

      <h3>調整履歴</h3>
      <ul>
        {item.history.map((h, idx) => (
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
