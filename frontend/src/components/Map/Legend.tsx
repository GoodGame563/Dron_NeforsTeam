export function Legend() {
  const items = [
    { color: "#34d399", label: "Исправен" },
    { color: "#f87171", label: "Требует замены" },
    { color: "#6b7280", label: "Нет лампы" },
    { color: "#3b82f6", label: "Станция", square: true },
  ];

  return (
    <div className="absolute bottom-4 right-4 flex flex-col gap-1.5 bg-black/40 backdrop-blur-sm border border-white/10 rounded-xl px-3 py-2.5">
      {items.map(({ color, label, square }) => (
        <div key={label} className="flex items-center gap-2">
          <span
            className="shrink-0"
            style={{
              width: square ? 10 : 8,
              height: square ? 10 : 8,
              borderRadius: square ? 2 : "50%",
              background: color,
              display: "inline-block",
            }}
          />
          <span className="text-[11px] text-white/60">{label}</span>
        </div>
      ))}
    </div>
  );
}
