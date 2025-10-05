import React from "react";

interface CardButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  style?: React.CSSProperties; // ←追加
}

export const CardButton: React.FC<CardButtonProps> = ({ children, onClick, style }) => {
  return (
    <div
      onClick={onClick}
      style={{
        display: "inline-block",
        background: "#31005fff",
        borderRadius: "12px",
        padding: "16px 20px",
        fontWeight: "bold",
        color: "white",
        cursor: "pointer",
        boxShadow: "0 2px 6px rgba(0,0,0,0.3)",
        transition: "transform 0.2s, box-shadow 0.2s",
        textAlign: "center",
        ...style, // ←ここで上書き可能に
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLDivElement).style.transform = "scale(1.05)";
        (e.currentTarget as HTMLDivElement).style.boxShadow =
          "0 4px 12px rgba(0,0,0,0.5)";
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLDivElement).style.transform = "scale(1)";
        (e.currentTarget as HTMLDivElement).style.boxShadow =
          "0 2px 6px rgba(0,0,0,0.3)";
      }}
    >
      {children}
    </div>
  );
};
