import Link from "next/link";

export default function HomePage() {
  return (
    <div style={{ padding: "30px", background: "#1e1e2f", minHeight: "100vh", color: "#fff" }}>
      <h1 style={{ fontSize: "2rem", marginBottom: "20px" }}>Gyeend Database</h1>

      <div style={{ display: "flex", gap: "20px", flexWrap: "wrap" }}>
        <Link href="/weapons" style={{
          padding: "20px", background: "#2c2c3d", borderRadius: "10px",
          textDecoration: "none", color: "#fff", flex: "1 1 200px", textAlign: "center"
        }}>
          <h2>武器ランキング</h2>
          <p>全ての武器のランクと情報を見る</p>
        </Link>

        <Link href="/characters" style={{
          padding: "20px", background: "#2c2c3d", borderRadius: "10px",
          textDecoration: "none", color: "#fff", flex: "1 1 200px", textAlign: "center"
        }}>
          <h2>キャラクターランキング</h2>
          <p>全てのキャラクターのランクと情報を見る</p>
        </Link>

        <Link href="/patches" style={{
          padding: "20px", background: "#2c2c3d", borderRadius: "10px",
          textDecoration: "none", color: "#fff", flex: "1 1 200px", textAlign: "center"
        }}>
          <h2>パッチノート</h2>
          <p>最新パッチや過去の調整履歴を見る</p>
        </Link>
      </div>
    </div>
  );
}
