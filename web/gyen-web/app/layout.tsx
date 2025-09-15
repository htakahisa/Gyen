import "./globals.css";
import { ReactNode } from "react";

export const metadata = {
  title: "ゲーム情報ポータル",
  description: "パッチノートと武器ランキングを確認できます",
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="ja">
      <body style={{ fontFamily: "sans-serif", padding: "20px" }}>
        <header>
          <h1>ゲーム情報ポータル</h1>
          <nav>
            <a href="/" style={{ marginRight: "15px" }}>ホーム</a>
            <a href="/patches" style={{ marginRight: "15px" }}>パッチノート</a>
            <a href="/weapons">武器ランキング</a>
          </nav>
          <hr style={{ margin: "10px 0" }} />
        </header>
        <main>{children}</main>
      </body>
    </html>
  );
}
