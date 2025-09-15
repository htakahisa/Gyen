// components/UI.tsx
"use client";

import React from "react";

export const CardButton = ({
  children,
  onClick,
}: {
  children: React.ReactNode;
  onClick?: () => void;
}) => {
  return (
    <div
      onClick={onClick}
      style={{
        display: "inline-block", // ←デフォルト修正
        background: "#31005fff",
        borderRadius: "12px",
        padding: "16px 20px",
        fontWeight: "bold",
        color: "white",
        cursor: "pointer",
        boxShadow: "0 2px 6px rgba(0,0,0,0.3)",
        transition: "transform 0.2s, box-shadow 0.2s",
        textAlign: "center",
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

export const NormalButton = ({
  children,
  href,
}: {
  children: React.ReactNode;
  href?: string;
}) => {
  return (
    <a
      href={href}
      style={{
        display: "inline-block",
        padding: "8px 16px",
        background: "#00ffb3ff",
        color: "white",
        fontWeight: "bold",
        borderRadius: "6px",
        textDecoration: "none",
        cursor: "pointer",
        transition: "transform 0.2s, box-shadow 0.2s",
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLAnchorElement).style.transform = "scale(1.05)";
        (e.currentTarget as HTMLAnchorElement).style.boxShadow =
          "0 4px 12px rgba(0,0,0,0.5)";
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLAnchorElement).style.transform = "scale(1)";
        (e.currentTarget as HTMLAnchorElement).style.boxShadow =
          "0 2px 6px rgba(0,0,0,0.3)";
      }}
    >
      {children}
    </a>
  );
};
