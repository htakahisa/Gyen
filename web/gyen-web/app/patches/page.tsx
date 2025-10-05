
"use client";
import Link from "next/link";
import { useState } from "react";
import { items } from "../../data/items";
import { characters } from "../../data/characters";
import { systems } from "../../data/systems";
import { CardButton } from "../components/UI";

interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

export default function Patches() {
  const [openItems, setOpenItems] = useState<string[]>([]);

  // バージョンごとにまとめるマップ
  const versionMap: Record<
    string,
    {
      items: Record<string, HistoryEntry[]>;
      characters: Record<string, HistoryEntry[]>;
      systems: Record<string, HistoryEntry[]>;
    }
  > = {};

  // items
  items.forEach((item) => {
    item.history.forEach((h) => {
      if (!versionMap[h.version])
        versionMap[h.version] = { items: {}, characters: {}, systems: {} };
      if (!versionMap[h.version].items[item.name])
        versionMap[h.version].items[item.name] = [];
      versionMap[h.version].items[item.name].push(h);
    });
  });

  // characters
  characters.forEach((char) => {
    char.history.forEach((h) => {
      if (!versionMap[h.version])
        versionMap[h.version] = { items: {}, characters: {}, systems: {} };
      if (!versionMap[h.version].characters[char.name])
        versionMap[h.version].characters[char.name] = [];
      versionMap[h.version].characters[char.name].push(h);
    });
  });

  // systems
  systems.forEach((sys) => {
    sys.history.forEach((h) => {
      if (!versionMap[h.version])
        versionMap[h.version] = { items: {}, characters: {}, systems: {} };
      if (!versionMap[h.version].systems[sys.name])
        versionMap[h.version].systems[sys.name] = [];
      versionMap[h.version].systems[sys.name].push(h);
    });
  });

  const toggleItem = (name: string) => {
    setOpenItems((prev) =>
      prev.includes(name) ? prev.filter((n) => n !== name) : [...prev, name]
    );
  };

  // 展開枠の共通UI（items / characters / systems共通）
const renderHistoryBlock = (
  version: string,
  category: "items" | "characters" | "systems",
  name: string,
  histories: HistoryEntry[]
) => {
  const key = `${version}-${category}-${name}`;
  const isOpen = openItems.includes(key);

  const linkBase = "/item"; // ← ここを修正！

  return (
    <div
      key={name}
      style={{
        marginBottom: "16px",
        borderRadius: "12px",
        background: "#1f1f2e",
        overflow: "hidden",
        boxShadow: "0 4px 8px rgba(0,0,0,0.3)",
        transition: "all 0.3s ease",
      }}
    >
      <div
        onClick={() => toggleItem(key)}
        style={{
          padding: "12px 16px",
          cursor: "pointer",
          background: "#2a2a40",
          userSelect: "none",
        }}
      >
        <h3 style={{ margin: 0 }}>
          {isOpen ? "▼" : "▶"} {name}
        </h3>
      </div>
      <div
        style={{
          maxHeight: isOpen ? "1000px" : "0px",
          overflow: "hidden",
          transition: "max-height 0.3s ease",
          padding: isOpen ? "12px 16px" : "0 16px",
        }}
      >
        {isOpen && (
          <>
            {histories.map((h, idx) => (
              <div
                key={idx}
                style={{
                  padding: "4px 0",
                  color:
                    h.type === "buff"
                      ? "limegreen"
                      : h.type === "debuff"
                      ? "red"
                      : "white",
                }}
              >
                {h.change}
              </div>
            ))}
            <Link href={`${linkBase}/${encodeURIComponent(name)}`}>
              <CardButton style={{ marginTop: "8px" }}>
                この
                {category === "items"
                  ? "アイテム"
                  : category === "characters"
                  ? "キャラクター"
                  : "システム"}
                の過去のパッチも見る
              </CardButton>
            </Link>
          </>
        )}
      </div>
    </div>
  );
};

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      {/* ホームに戻るボタン */}
      <div style={{ marginBottom: "20px" }}>
        <Link href="/">
          <CardButton style={{ width: "fit-content" }}>ホームに戻る</CardButton>
        </Link>
      </div>

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>パッチノート</h1>

      {Object.entries(versionMap)
        .sort((a, b) => (a[0] < b[0] ? 1 : -1)) // 新しいバージョンを上に
        .map(([version, entry]) => (
          <div key={version} style={{ marginBottom: "32px" }}>
            <h2 style={{ fontSize: "1.5rem", marginBottom: "12px" }}>v{version}</h2>

            {/* アイテム */}
            {Object.entries(entry.items).map(([name, histories]) =>
              renderHistoryBlock(version, "items", name, histories)
            )}

            {/* キャラクター */}
            {Object.entries(entry.characters).map(([name, histories]) =>
              renderHistoryBlock(version, "characters", name, histories)
            )}

            {/* システム */}
            {Object.entries(entry.systems).map(([name, histories]) =>
              renderHistoryBlock(version, "systems", name, histories)
            )}
          </div>
        ))}
    </div>
  );
}
