import { Button } from "../ui/Button";
import { Chip } from "../ui/Chip";
import type { Lamp, Station, LampStatus } from "../../types/type.ts";

interface LampDetailPanelProps {
  lamp: Lamp;
  station: Station | null;
  isDispatching: boolean;
  onDispatch: () => void;
  onClose: () => void;
}

const STATUS_CONFIG: Record<
  LampStatus,
  { label: string; variant: "success" | "danger" | "neutral" }
> = {
  alive: { label: "Исправен", variant: "success" },
  death: { label: "Требует замены", variant: "danger" },
  empty: { label: "Нет лампы", variant: "neutral" },
};

export function LampDetailPanel({
  lamp,
  station,
  isDispatching,
  onDispatch,
  onClose,
}: LampDetailPanelProps) {
  const { label, variant } = STATUS_CONFIG[lamp.status];
  const canDispatch = lamp.status === "death" || lamp.status === "empty";
  const hasAvailableDrone = station ? station.availableDrones > 0 : false;

  return (
    <div className="border-t border-white/10 bg-[#0f1117] px-5 py-4 flex items-center gap-6">
      {/* Иконка фонаря */}
      <div className="w-10 h-10 rounded-xl bg-white/5 border border-white/10 flex items-center justify-center flex-shrink-0">
        <LampIcon status={lamp.status} />
      </div>

      {/* Основная информация */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <p className="text-sm font-medium text-white">
            Фонарь #{lamp.id.replace("lamp-", "")}
          </p>
          <Chip label={label} variant={variant} dot />
        </div>
        <div className="flex items-center gap-3 text-xs text-white/40">
          {station && <span>{station.name}</span>}
          <span>
            x: {Math.round(lamp.position.x)}, y: {Math.round(lamp.position.y)}
          </span>
        </div>
      </div>

      {/* Предупреждение если нет свободных дронов */}
      {canDispatch && !hasAvailableDrone && (
        <p className="text-xs text-amber-400/80 max-w-[140px] text-right leading-tight">
          Нет свободных дронов на станции
        </p>
      )}

      {/* Кнопка отправки */}
      {canDispatch && (
        <Button
          variant="primary"
          onClick={onDispatch}
          loading={isDispatching}
          disabled={!hasAvailableDrone}
        >
          {isDispatching ? "Отправляем..." : "Отправить дрона"}
        </Button>
      )}

      {/* Закрыть */}
      <button
        onClick={onClose}
        className="w-8 h-8 rounded-lg flex items-center justify-center text-white/40 hover:text-white hover:bg-white/10 transition-colors flex-shrink-0"
        aria-label="Закрыть"
      >
        <svg
          className="w-4 h-4"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      </button>
    </div>
  );
}

function LampIcon({ status }: { status: LampStatus }) {
  const colorClass =
    status === "alive"
      ? "text-emerald-400"
      : status === "death"
        ? "text-red-400"
        : "text-white/30";

  return (
    <svg
      className={["w-5 h-5", colorClass].join(" ")}
      viewBox="0 0 24 24"
      fill="currentColor"
    >
      <path d="M12 2a7 7 0 0 1 5.292 11.585l-.143.172-.047.06A4.992 4.992 0 0 0 16 17v1a1 1 0 0 1-1 1H9a1 1 0 0 1-1-1v-1a4.993 4.993 0 0 0-1.102-3.183l-.047-.06-.143-.172A7 7 0 0 1 12 2zm2 18v1a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1v-1h4z" />
    </svg>
  );
}
