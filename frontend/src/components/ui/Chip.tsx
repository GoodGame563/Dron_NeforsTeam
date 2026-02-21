type ChipVariant = "success" | "danger" | "warning" | "neutral" | "info";

interface ChipProps {
  label: string;
  variant: ChipVariant;
  dot?: boolean;
  className?: string;
}

const VARIANT_STYLES: Record<ChipVariant, string> = {
  success: "bg-emerald-500/15 text-emerald-400 border-emerald-500/30",
  danger: "bg-red-500/15    text-red-400    border-red-500/30",
  warning: "bg-amber-500/15  text-amber-400  border-amber-500/30",
  neutral: "bg-white/10      text-white/50   border-white/20",
  info: "bg-blue-500/15   text-blue-400   border-blue-500/30",
};

const DOT_STYLES: Record<ChipVariant, string> = {
  success: "bg-emerald-400",
  danger: "bg-red-400",
  warning: "bg-amber-400",
  neutral: "bg-white/40",
  info: "bg-blue-400",
};

export function Chip({
  label,
  variant,
  dot = false,
  className = "",
}: ChipProps) {
  return (
    <span
      className={[
        "inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-xs font-medium border",
        VARIANT_STYLES[variant],
        className,
      ].join(" ")}
    >
      {dot && (
        <span
          className={[
            "w-1.5 h-1.5 rounded-full flex-shrink-0",
            DOT_STYLES[variant],
          ].join(" ")}
        />
      )}
      {label}
    </span>
  );
}
