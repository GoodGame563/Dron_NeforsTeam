import { Box, Typography, Chip, Button, IconButton } from "@mui/material";
import { X, Send, Clock } from "lucide-react";

import { useTimePrefix } from "../../hooks/useTimePrefix";
import { useLampStatus } from "../../hooks/useLampStatus";
import { useSocketStore } from "../../stores/socketStore";
import { LampIcon } from "./LampIcon";
import { formatRelativeTime } from "../../utils/formatRelativeTime";

export function LampDetailPanel() {
  const { pillar, unsetSelectedPillar } = useSocketStore((state) => ({
    pillar: state.selectedPillar,
    unsetSelectedPillar: state.unselectPillar,
  }));
  const { label, color } = useLampStatus(pillar?.state ?? "empty");
  const timePrefix = useTimePrefix(pillar?.state ?? "empty");

  if (!pillar) return null;

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
        <LampIcon status={pillar.state} />
      </Box>

      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 0.5 }}>
          <Typography variant="body2" fontWeight={500}>
            Фонарь #{pillar.id.replace("lamp-", "")}
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
          <Typography variant="caption" color="text.disabled">
            x: {Math.round(pillar.coordinates.x)}, y:{" "}
            {Math.round(pillar.coordinates.y)}
          </Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
            <Clock size={11} color="rgba(255,255,255,0.3)" />
            <Typography variant="caption" color="text.disabled">
              {timePrefix} {formatRelativeTime(pillar.last_update)}
            </Typography>
          </Box>
        </Box>
      </Box>

      {pillar.dron_station_id && pillar.state != "death" && (
        <span>
          <Button
            variant="contained"
            size="small"
            startIcon={<Send size={14} />}
            sx={{ whiteSpace: "nowrap" }}
          >
            {"Отправить дрона"}
          </Button>
        </span>
      )}

      <IconButton
        size="small"
        onClick={unsetSelectedPillar}
        sx={{ color: "text.secondary" }}
      >
        <X size={16} />
      </IconButton>
    </Box>
  );
}
