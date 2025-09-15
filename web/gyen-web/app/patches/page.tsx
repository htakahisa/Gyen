"use client";
import Link from "next/link";
import { useState } from "react";
import { items } from "../../data/items";
import { CardButton } from "../components/UI";

interface HistoryEntry {
  version: string;
  change: string;
  type: "buff" | "debuff" | "neutral";
}

export default function Patches() {
  const [openItems, setOpenItems] = useState<string[]>([]);

  // バージョンごとにまとめる
  const versionMap: Record<string, { [itemName: string]: HistoryEntry[] }> = {};

  items.forEach((item) => {
    item.history.forEach((h) => {
      if (!versionMap[h.version]) versionMap[h.version] = {};
      if (!versionMap[h.version][item.name]) versionMap[h.version][item.name] = [];
      versionMap[h.version][item.name].push(h);
    });
  });

  const toggleItem = (name: string) => {
    setOpenItems((prev) =>
      prev.includes(name) ? prev.filter((n) => n !== name) : [...prev, name]
    );
  };

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", color: "white" }}>
      {/* ホームに戻るボタン */}
      <div style={{ marginBottom: "20px" }}>
        <Link href={"/"}>
          <CardButton>
            ホームに戻る
          </CardButton>
        </Link>
      </div>

      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>パッチノート</h1>

      {Object.entries(versionMap)
        .sort((a, b) => (a[0] < b[0] ? 1 : -1)) // 新しいバージョンを上に
        .map(([version, itemMap]) => (
          <div key={version} style={{ marginBottom: "32px" }}>
            <h2 style={{ fontSize: "1.5rem", marginBottom: "12px" }}>v{version}</h2>
            {Object.entries(itemMap).map(([itemName, histories]) => {
              const isOpen = openItems.includes(`${version}-${itemName}`);
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
                    onClick={() => toggleItem(`${version}-${itemName}`)}
                    style={{
                      padding: "12px 16px",
                      cursor: "pointer",
                      background: "#2a2a40",
                      userSelect: "none",
                    }}
                  >
                    <h3 style={{ margin: 0 }}>
                      {isOpen ? "▼" : "▶"} {itemName}
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
                          <CardButton>
                            この武器の過去のパッチも見る
                          </CardButton>
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
