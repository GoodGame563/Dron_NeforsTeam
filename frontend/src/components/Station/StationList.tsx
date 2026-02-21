import { StationCard } from "./StationCard";
import type { Station } from "../../types/type.ts";

interface StationListProps {
  stations: Station[];
  selectedStation: Station | null;
  onSelectStation: (station: Station | null) => void;
}

export function StationList({
  stations,
  selectedStation,
  onSelectStation,
}: StationListProps) {
  function handleSelect(station: Station) {
    // Повторный клик на выбранную станцию снимает выделение
    if (selectedStation?.id === station.id) {
      onSelectStation(null);
    } else {
      onSelectStation(station);
    }
  }

  const totalDrones = stations.reduce((sum, s) => sum + s.totalDrones, 0);
  const availableDrones = stations.reduce(
    (sum, s) => sum + s.availableDrones,
    0,
  );
  const brokenDrones = stations.reduce((sum, s) => sum + s.brokenDrones, 0);

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      {/* Суммарная статистика */}
      <div className="px-4 py-3 border-b border-white/10">
        <p className="text-[11px] text-white/40 uppercase tracking-widest mb-2">
          Общая статистика
        </p>
        <div className="grid grid-cols-3 gap-2 text-center">
          <div>
            <p className="text-lg font-semibold text-white tabular-nums">
              {totalDrones}
            </p>
            <p className="text-[10px] text-white/40">всего</p>
          </div>
          <div>
            <p className="text-lg font-semibold text-emerald-400 tabular-nums">
              {availableDrones}
            </p>
            <p className="text-[10px] text-white/40">свободно</p>
          </div>
          <div>
            <p className="text-lg font-semibold text-red-400 tabular-nums">
              {brokenDrones}
            </p>
            <p className="text-[10px] text-white/40">сломано</p>
          </div>
        </div>
      </div>

      {/* Список станций */}
      <div className="flex-1 overflow-y-auto px-3 py-3 space-y-2">
        <p className="text-[11px] text-white/40 uppercase tracking-widest px-1 mb-1">
          Станции ({stations.length})
        </p>
        {stations.length === 0 ? (
          <p className="text-sm text-white/30 text-center py-8">Нет станций</p>
        ) : (
          stations.map((station) => (
            <StationCard
              key={station.id}
              station={station}
              selected={selectedStation?.id === station.id}
              onSelect={handleSelect}
            />
          ))
        )}
      </div>
    </div>
  );
}
