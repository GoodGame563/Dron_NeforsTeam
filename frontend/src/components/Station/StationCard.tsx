import { Card } from "../ui/Card";
import type { Station } from "../../types/type.ts";

interface StationCardProps {
  station: Station;
  selected: boolean;
  onSelect: (station: Station) => void;
}

interface StatItemProps {
  label: string;
  value: number;
  valueClassName?: string;
}

function StatItem({
  label,
  value,
  valueClassName = "text-white",
}: StatItemProps) {
  return (
    <div className="flex flex-col items-center gap-0.5">
      <span
        className={[
          "text-base font-semibold tabular-nums",
          valueClassName,
        ].join(" ")}
      >
        {value}
      </span>
      <span className="text-[10px] text-white/40 text-center leading-tight">
        {label}
      </span>
    </div>
  );
}

export function StationCard({ station, selected, onSelect }: StationCardProps) {
  const busyDrones =
    station.totalDrones - station.availableDrones - station.brokenDrones;

  return (
    <Card
      selected={selected}
      onClick={() => onSelect(station)}
      className="p-3.5"
    >
      {/* Заголовок */}
      <div className="flex items-start justify-between gap-2 mb-3">
        <div className="flex items-center gap-2 min-w-0">
          <div
            className={[
              "w-2 h-2 rounded-full flex-shrink-0 mt-0.5",
              station.availableDrones > 0 ? "bg-emerald-400" : "bg-amber-400",
            ].join(" ")}
          />
          <span className="text-sm font-medium text-white truncate">
            {station.name}
          </span>
        </div>
        <span className="text-xs text-white/30 flex-shrink-0">
          {station.lampCount} фонарей
        </span>
      </div>

      {/* Статистика дронов */}
      <div className="grid grid-cols-3 gap-1 bg-white/5 rounded-lg px-2 py-2">
        <StatItem
          label="свободных"
          value={station.availableDrones}
          valueClassName={
            station.availableDrones > 0 ? "text-emerald-400" : "text-white/40"
          }
        />
        <StatItem
          label="в работе"
          value={busyDrones}
          valueClassName={busyDrones > 0 ? "text-blue-400" : "text-white/40"}
        />
        <StatItem
          label="сломано"
          value={station.brokenDrones}
          valueClassName={
            station.brokenDrones > 0 ? "text-red-400" : "text-white/40"
          }
        />
      </div>
    </Card>
  );
}
