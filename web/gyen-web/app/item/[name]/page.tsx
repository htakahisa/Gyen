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

  const item = items.find((i) => i.name === name);
  const character = characters.find((c) => c.name === name);

  if (!item && !character) {
    return <p>このデータの情報がありません。</p>;
  }

  const target = item || character;
  const typeLabel = item ? "アイテム" : "キャラクター";

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      <h2 style={{ fontSize: "1.5rem" }}>
        {target?.name}（ランク: {target?.rank} / {typeLabel}）
      </h2>
      <p>{target?.description}</p>

      {/* キャラクターの場合のみスキル表示 */}
      {character && character.skills && (
        <div style={{ marginTop: "20px" }}>
          <h3>スキル</h3>
          <ul>
            {character.skills.map((skill, idx) => (
              <li
                key={idx}
                style={{
                  marginBottom: "10px",
                  background: "#2a2a40",
                  padding: "8px 12px",
                  borderRadius: "8px",
                }}
              >
                <strong>{skill.name}</strong>
                <p style={{ margin: "4px 0 0" }}>{skill.description}</p>
              </li>
            ))}
          </ul>
        </div>
      )}

      <h3 style={{ marginTop: "20px" }}>調整履歴</h3>
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
