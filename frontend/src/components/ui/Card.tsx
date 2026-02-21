import type { ReactNode } from "react";

interface CardProps {
  children: ReactNode;
  className?: string;
  onClick?: () => void;
  selected?: boolean;
}

export function Card({
  children,
  className = "",
  onClick,
  selected = false,
}: CardProps) {
  const isClickable = !!onClick;

  return (
    <div
      role={isClickable ? "button" : undefined}
      tabIndex={isClickable ? 0 : undefined}
      onClick={onClick}
      onKeyDown={
        isClickable ? (e) => e.key === "Enter" && onClick?.() : undefined
      }
      className={[
        "rounded-xl border transition-all duration-200",
        selected
          ? "bg-blue-600/15 border-blue-500/60"
          : "bg-white/5 border-white/10",
        isClickable
          ? "cursor-pointer hover:bg-white/10 hover:border-white/20 active:scale-[0.99]"
          : "",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
