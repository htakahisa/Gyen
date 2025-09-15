import "./globals.css";
import { ReactNode } from "react";
import Link from "next/link";

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
             <Link href="/" style={{ marginRight: "15px" }}>
              ホーム
            </Link>
            <Link href="/patches" style={{ marginRight: "15px" }}>
              パッチノート
            </Link>
            <Link href="/weapons">
              武器ランキング
            </Link>
          </nav>
          <hr style={{ margin: "10px 0" }} />
        </header>
        <main>{children}</main>
      </body>
    </html>
  );
}
