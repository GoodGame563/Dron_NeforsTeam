import type { ReactNode } from "react";

type ButtonVariant = "primary" | "secondary" | "danger" | "ghost";

interface ButtonProps {
  children: ReactNode;
  variant?: ButtonVariant;
  onClick?: () => void;
  disabled?: boolean;
  loading?: boolean;
  className?: string;
  fullWidth?: boolean;
}

const VARIANT_STYLES: Record<ButtonVariant, string> = {
  primary: "bg-blue-600 hover:bg-blue-500 text-white border-transparent",
  secondary: "bg-white/10 hover:bg-white/15 text-white border-white/20",
  danger: "bg-red-600/80 hover:bg-red-500 text-white border-transparent",
  ghost:
    "bg-transparent hover:bg-white/5 text-white/60 hover:text-white border-transparent",
};

export function Button({
  children,
  variant = "primary",
  onClick,
  disabled = false,
  loading = false,
  className = "",
  fullWidth = false,
}: ButtonProps) {
  const isDisabled = disabled || loading;

  return (
    <button
      onClick={onClick}
      disabled={isDisabled}
      className={[
        "inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg",
        "text-sm font-medium border transition-all duration-150",
        "active:scale-[0.98] focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500",
        VARIANT_STYLES[variant],
        isDisabled
          ? "opacity-50 cursor-not-allowed pointer-events-none"
          : "cursor-pointer",
        fullWidth ? "w-full" : "",
        className,
      ].join(" ")}
    >
      {loading && (
        <span className="w-3.5 h-3.5 border-2 border-current border-t-transparent rounded-full animate-spin" />
      )}
      {children}
    </button>
  );
}
