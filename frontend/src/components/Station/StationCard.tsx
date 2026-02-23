import {
  Card,
  CardActionArea,
  CardContent,
  Chip,
  Typography,
  Box,
} from "@mui/material";
import { Bolt, Build, FlightTakeoff } from "@mui/icons-material";

import { StatItem } from "./StatItem.tsx";
import type { DronStation } from "../../types/type.ts";

interface StationCardProps {
  station: DronStation;
  selected: boolean;
  onSelect: (station: DronStation) => void;
}

export function StationCard({ station, selected, onSelect }: StationCardProps) {
  const brokenDrones = station.drons.filter(
    (drone) => drone.status == "broken",
  ).length;

  const availableDrones = station.drons.filter(
    (drone) => drone.status == "in_station",
  ).length;

  const busyDrones = station.drons.filter(
    (drone) => drone.status == "fly",
  ).length;

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
                    availableDrones > 0 ? "success.main" : "warning.main",
                }}
              />
              <Typography variant="body2" fontWeight={500} noWrap>
                {station.id}
              </Typography>
            </Box>
            <Chip
              label={`${station.total_lamps_count} фонарей`}
              size="small"
              sx={{ fontSize: 10, height: 18, color: "text.secondary" }}
            />
          </Box>

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
              value={availableDrones}
              label="свободных"
              color={availableDrones > 0 ? "#34d399" : "rgba(255,255,255,0.3)"}
            />
            <StatItem
              icon={<FlightTakeoff sx={{ fontSize: 13 }} />}
              value={busyDrones}
              label="в работе"
              color={busyDrones > 0 ? "#60a5fa" : "rgba(255,255,255,0.3)"}
            />
            <StatItem
              icon={<Build sx={{ fontSize: 13 }} />}
              value={brokenDrones}
              label="сломано"
              color={brokenDrones > 0 ? "#f87171" : "rgba(255,255,255,0.3)"}
            />
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
