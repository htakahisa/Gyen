"use client";
import Link from "next/link";
import { useState } from "react";
import { items } from "../../data/items";
import { characters } from "../../data/characters"; // キャラデータを追加
import { CardButton } from "../components/UI";

interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

export default function Patches() {
  const [openItems, setOpenItems] = useState<string[]>([]);

  // バージョンごとにアイテムとキャラをまとめる
  const versionMap: Record<
    string,
    { items: { [name: string]: HistoryEntry[] }; characters: { [name: string]: HistoryEntry[] } }
  > = {};

  // アイテムをまとめる
  items.forEach((item) => {
    item.history.forEach((h) => {
      if (!versionMap[h.version]) {
        versionMap[h.version] = { items: {}, characters: {} };
      }
      if (!versionMap[h.version].items[item.name]) {
        versionMap[h.version].items[item.name] = [];
      }
      versionMap[h.version].items[item.name].push(h);
    });
  });

  // キャラをまとめる
  characters.forEach((char) => {
    char.history.forEach((h) => {
      if (!versionMap[h.version]) {
        versionMap[h.version] = { items: {}, characters: {} };
      }
      if (!versionMap[h.version].characters[char.name]) {
        versionMap[h.version].characters[char.name] = [];
      }
      versionMap[h.version].characters[char.name].push(h);
    });
  });

  const toggleItem = (key: string) => {
    setOpenItems((prev) =>
      prev.includes(key) ? prev.filter((n) => n !== key) : [...prev, key]
    );
  };

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      {/* ホームに戻るボタン */}
      <div style={{ marginBottom: "20px" }}>
        <Link href={"/"}>
          <CardButton>ホームに戻る</CardButton>
        </Link>
      </div>

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>パッチノート</h1>

      {Object.entries(versionMap)
        .sort((a, b) => (a[0] < b[0] ? 1 : -1)) // 新しいバージョンを上に
        .map(([version, data]) => (
          <div key={version} style={{ marginBottom: "32px" }}>
            <h2 style={{ fontSize: "1.5rem", marginBottom: "12px" }}>v{version}</h2>

            {/* キャラクター変更点 */}
            {Object.entries(data.characters).map(([charName, histories]) => {
              const key = `${version}-char-${charName}`;
              const isOpen = openItems.includes(key);
              return (
                <div
                  key={charName}
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
                      {isOpen ? "▼" : "▶"} {charName}（キャラクター）
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
                        <Link href={`/item/${encodeURIComponent(charName)}`}>
                          <CardButton>このキャラクターの過去のパッチも見る</CardButton>
                        </Link>
                      </>
                    )}
                  </div>
                </div>
              );
            })}

            {/* アイテム変更点 */}
            {Object.entries(data.items).map(([itemName, histories]) => {
              const key = `${version}-item-${itemName}`;
              const isOpen = openItems.includes(key);
              return (
                <div
                  key={itemName}
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
                      {isOpen ? "▼" : "▶"} {itemName}（アイテム）
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
                        <Link href={`/item/${encodeURIComponent(itemName)}`}>
                          <CardButton>このアイテムの過去のパッチも見る</CardButton>
                        </Link>
                      </>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        ))}
    </div>
  );
}
