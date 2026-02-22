import {
  Card,
  CardActionArea,
  CardContent,
  Chip,
  Typography,
  Box,
  Tooltip,
} from "@mui/material";
import { Bolt, Build, FlightTakeoff } from "@mui/icons-material";
import type { Station } from "../../types/type.ts";

interface StationCardProps {
  station: Station;
  selected: boolean;
  onSelect: (station: Station) => void;
}

interface StatItemProps {
  icon: React.ReactNode;
  value: number;
  label: string;
  color: string;
}

function StatItem({ icon, value, label, color }: StatItemProps) {
  return (
    <Tooltip title={label} placement="top">
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          gap: 0.5,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color }}>
          {icon}
          <Typography
            variant="body2"
            fontWeight={600}
            color={color}
            lineHeight={1}
          >
            {value}
          </Typography>
        </Box>
        <Typography
          variant="caption"
          color="text.secondary"
          fontSize={10}
          lineHeight={1}
        >
          {label}
        </Typography>
      </Box>
    </Tooltip>
  );
}

export function StationCard({ station, selected, onSelect }: StationCardProps) {
  const busyDrones =
    station.totalDrones - station.availableDrones - station.brokenDrones;

  return (
    <Card
      sx={{
        borderColor: selected ? "primary.main" : "divider",
        bgcolor: selected ? "rgba(59,130,246,0.10)" : "rgba(255,255,255,0.04)",
      }}
    >
      <CardActionArea
        onClick={() => onSelect(station)}
        sx={{ borderRadius: "inherit" }}
      >
        <CardContent sx={{ p: 1.5, "&:last-child": { pb: 1.5 } }}>
          {/* Заголовок */}
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              mb: 1.5,
            }}
          >
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 1,
                minWidth: 0,
              }}
            >
              <Box
                sx={{
                  width: 8,
                  height: 8,
                  borderRadius: "50%",
                  flexShrink: 0,
                  bgcolor:
                    station.availableDrones > 0
                      ? "success.main"
                      : "warning.main",
                }}
              />
              <Typography variant="body2" fontWeight={500} noWrap>
                {station.name}
              </Typography>
            </Box>
            <Chip
              label={`${station.lampCount} фонарей`}
              size="small"
              sx={{ fontSize: 10, height: 18, color: "text.secondary" }}
            />
          </Box>

          {/* Статистика дронов */}
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr 1fr",
              gap: 1,
              bgcolor: "rgba(255,255,255,0.04)",
              borderRadius: 2,
              px: 1,
              py: 1,
            }}
          >
            <StatItem
              icon={<Bolt sx={{ fontSize: 13 }} />}
              value={station.availableDrones}
              label="свободных"
              color={
                station.availableDrones > 0
                  ? "#34d399"
                  : "rgba(255,255,255,0.3)"
              }
            />
            <StatItem
              icon={<FlightTakeoff sx={{ fontSize: 13 }} />}
              value={busyDrones}
              label="в работе"
              color={busyDrones > 0 ? "#60a5fa" : "rgba(255,255,255,0.3)"}
            />
            <StatItem
              icon={<Build sx={{ fontSize: 13 }} />}
              value={station.brokenDrones}
              label="сломано"
              color={
                station.brokenDrones > 0 ? "#f87171" : "rgba(255,255,255,0.3)"
              }
            />
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
