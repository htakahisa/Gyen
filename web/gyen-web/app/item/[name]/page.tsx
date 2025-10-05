"use client";
import React from "react";
import { useRouter } from "next/navigation";
import { items } from "../../../data/items";
import { characters } from "../../../data/characters";
import { systems } from "../../../data/systems";

interface Props {
  params: Promise<{ name: string }>;
}

export default function DetailPage({ params }: Props) {
  const router = useRouter();
  const { name } = React.use(params);

  const item = items.find((i) => i.name === name);
  const character = characters.find((c) => c.name === name);
  const system = systems.find((s) => s.name === name);

  if (!item && !character && !system) {
    return <p>このデータの情報がありません。</p>;
  }

  const target = item || character || system;
  const typeLabel = item ? "アイテム" : character ? "キャラクター" : "システム";

  const cellStyle: React.CSSProperties = {
    padding: "8px",
    border: "1px solid #3a3a55",
    minWidth: "100px",
    textAlign: "center",
    wordBreak: "break-word",
  };

  const data = item || character;
  const features = data?.features ?? [];
  const skills = character?.skills ?? [];

  return (
    <div style={{ padding: "20px", color: "white", fontFamily: "sans-serif" }}>
      {/* タイトル */}
      <h2 style={{ fontSize: "1.8rem", fontWeight: "bold", marginBottom: "10px" }}>
        {target?.name}
        {target?.rank && <>（{system ? "重要度" : "Tier"}: {target.rank} / {typeLabel}）</>}
      </h2>

      {/* 簡易説明 */}
      <p style={{ marginBottom: "24px", lineHeight: "1.6" }}>{target?.description}</p>

      {/* 評価と特徴 */}
      <div
        style={{
          background: "#1e1e2f",
          border: "1px solid #3a3a55",
          borderRadius: "8px",
          padding: "16px",
          marginBottom: "24px",
        }}
      >
        <h3 style={{ fontSize: "1.3rem", marginBottom: "12px", fontWeight: "bold" }}>
          評価と特徴
        </h3>

        {/* 評価テーブル */}
        <table
          style={{
            width: "100%",
            borderCollapse: "collapse",
            tableLayout: "fixed",
            textAlign: "center",
            marginBottom: "12px",
          }}
        >
          <thead>
            <tr style={{ background: "#2a2a40" }}>
              <th style={cellStyle}>Tier</th>
              <th style={cellStyle}>タイプ</th>
              <th style={cellStyle}>難易度</th>
              <th style={cellStyle}>{data === item ? "コスト" : "出身地"}</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td style={cellStyle}>
                <span style={{ fontWeight: "bold", color: "#ffd700" }}>
                  Tier{data?.rank}
                </span>
              </td>
              <td style={cellStyle}>{data?.type}</td>
              <td style={cellStyle}>{data?.difficulty}</td>
              {data === item && <td style={cellStyle}>{data?.cost}</td>}
              {data === character && <td style={cellStyle}>{data?.from}</td>}
            </tr>
          </tbody>
        </table>

        {/* 特徴 */}
        {features.length > 0 && (
          <div style={{ marginTop: "12px" }}>
            <ul style={{ listStyle: "disc", paddingLeft: "20px", lineHeight: "1.8" }}>
              {features.map((feature, idx) => (
                <li key={idx}>{feature}</li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {/* スキル（キャラクターのみ） */}
      {skills.length > 0 && (
        <div
          style={{
            background: "#1e1e2f",
            border: "1px solid #3a3a55",
            borderRadius: "8px",
            padding: "16px",
            marginBottom: "24px",
          }}
        >
          <h3 style={{ fontSize: "1.3rem", marginBottom: "12px", fontWeight: "bold" }}>
            スキル
          </h3>
          <ul style={{ listStyle: "circle", paddingLeft: "20px", lineHeight: "1.6" }}>
            {skills.map((skill, idx) => (
              <li key={idx}>
                <span style={{ fontWeight: "bold" }}>{skill.name}</span>: {skill.description}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* 詳しい解説 */}
      {target?.details && (
        <div
          style={{
            background: "#1e1e2f",
            border: "1px solid #3a3a55",
            borderRadius: "8px",
            padding: "16px",
            marginBottom: "24px",
            lineHeight: "1.7",
          }}
        >
          <h3 style={{ fontSize: "1.3rem", marginBottom: "12px", fontWeight: "bold" }}>
            詳しい解説
          </h3>
          <p>{target.details}</p>
        </div>
      )}

      {/* 調整履歴 */}
      {target?.history?.length ? (
        <div
          style={{
            background: "#1e1e2f",
            border: "1px solid #3a3a55",
            borderRadius: "8px",
            padding: "16px",
            marginBottom: "24px",
          }}
        >
          <h3 style={{ fontSize: "1.3rem", marginBottom: "12px", fontWeight: "bold" }}>
            調整履歴
          </h3>
          <ul>
            {target.history.map((h, idx) => (
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
        </div>
      ) : (
        <p>調整履歴はありません</p>
      )}

      {/* 戻るボタン */}
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
