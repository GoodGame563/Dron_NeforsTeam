import {
  Box,
  Typography,
  Chip,
  Button,
  IconButton,
  Tooltip,
} from "@mui/material";
import {
  Lightbulb,
  LightbulbOff,
  CircleOff,
  X,
  Send,
  Clock,
} from "lucide-react";
import type { Lamp, Station, LampStatus } from "../../types/type";
import { formatRelativeTime } from "../../utils/formatRelativeTime";

interface LampDetailPanelProps {
  lamp: Lamp;
  station: Station | null;
  isDispatching: boolean;
  onDispatch: () => void;
  onClose: () => void;
}

const STATUS_CONFIG: Record<
  LampStatus,
  {
    label: string;
    color: "success" | "error" | "default";
  }
> = {
  alive: { label: "Исправен", color: "success" },
  death: { label: "Требует замены", color: "error" },
  empty: { label: "Нет лампы", color: "default" },
};

function LampIcon({ status }: { status: LampStatus }) {
  if (status === "alive") return <Lightbulb size={20} color="#34d399" />;
  if (status === "death") return <LightbulbOff size={20} color="#f87171" />;
  return <CircleOff size={20} color="rgba(255,255,255,0.3)" />;
}

export function LampDetailPanel({
  lamp,
  station,
  isDispatching,
  onDispatch,
  onClose,
}: LampDetailPanelProps) {
  const { label, color } = STATUS_CONFIG[lamp.status];
  const canDispatch = lamp.status === "death" || lamp.status === "empty";
  const hasAvailableDrone = station ? station.availableDrones > 0 : false;

  const timePrefix =
    lamp.status === "alive"
      ? "Работает"
      : lamp.status === "death"
        ? "Сломана"
        : "Пуста";

  return (
    <Box
      sx={{
        borderTop: "1px solid",
        borderColor: "divider",
        bgcolor: "background.default",
        px: 2.5,
        py: 1.5,
        display: "flex",
        alignItems: "center",
        gap: 2,
      }}
    >
      {/* Иконка фонаря */}
      <Box
        sx={{
          width: 40,
          height: 40,
          borderRadius: 2,
          bgcolor: "rgba(255,255,255,0.05)",
          border: "1px solid",
          borderColor: "divider",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          flexShrink: 0,
        }}
      >
        <LampIcon status={lamp.status} />
      </Box>

      {/* Основная информация */}
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 0.5 }}>
          <Typography variant="body2" fontWeight={500}>
            Фонарь #{lamp.id.replace("lamp-", "")}
          </Typography>
          <Chip label={label} color={color} size="small" />
        </Box>
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            gap: 2,
            flexWrap: "wrap",
          }}
        >
          {station && (
            <Typography variant="caption" color="text.secondary">
              {station.name}
            </Typography>
          )}
          <Typography variant="caption" color="text.disabled">
            x: {Math.round(lamp.position.x)}, y: {Math.round(lamp.position.y)}
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
            <Clock size={11} color="rgba(255,255,255,0.3)" />
            <Typography variant="caption" color="text.disabled">
              {timePrefix} {formatRelativeTime(lamp.updatedAt)}
            </Typography>
          </Box>
        </Box>
      </Box>

      {/* Предупреждение если нет свободных дронов */}
      {canDispatch && !hasAvailableDrone && (
        <Typography
          variant="caption"
          color="warning.main"
          sx={{ maxWidth: 140, textAlign: "right" }}
        >
          Нет свободных дронов на станции
        </Typography>
      )}

      {/* Кнопка отправки */}
      {canDispatch && (
        <Tooltip title={!hasAvailableDrone ? "Нет свободных дронов" : ""}>
          <span>
            <Button
              variant="contained"
              size="small"
              onClick={onDispatch}
              disabled={!hasAvailableDrone || isDispatching}
              startIcon={!isDispatching && <Send size={14} />}
              sx={{ whiteSpace: "nowrap" }}
            >
              {isDispatching ? "Отправляем..." : "Отправить дрона"}
            </Button>
          </span>
        </Tooltip>
      )}

      {/* Закрыть */}
      <IconButton
        size="small"
        onClick={onClose}
        sx={{ color: "text.secondary" }}
      >
        <X size={16} />
      </IconButton>
    </Box>
  );
}
