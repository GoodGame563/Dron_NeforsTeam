import { Box, Typography, Divider } from "@mui/material";
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
    <Box
      sx={{
        flex: 1,
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      <Box sx={{ px: 2, py: 1.5 }}>
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ textTransform: "uppercase", letterSpacing: 1.5 }}
        >
          Общая статистика
        </Typography>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr 1fr",
            textAlign: "center",
            mt: 1,
          }}
        >
          <Box>
            <Typography variant="h6" fontWeight={600}>
              {totalDrones}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              всего
            </Typography>
          </Box>
          <Box>
            <Typography variant="h6" fontWeight={600} color="success.main">
              {availableDrones}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              свободно
            </Typography>
          </Box>
          <Box>
            <Typography variant="h6" fontWeight={600} color="error.main">
              {brokenDrones}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              сломано
            </Typography>
          </Box>
        </Box>
      </Box>

      <Divider />

      <Box
        sx={{
          flex: 1,
          overflowY: "auto",
          px: 1.5,
          py: 1.5,
          display: "flex",
          flexDirection: "column",
          gap: 1,
        }}
      >
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ textTransform: "uppercase", letterSpacing: 1.5, px: 0.5 }}
        >
          Станции ({stations.length})
        </Typography>

        {stations.length === 0 ? (
          <Typography
            variant="body2"
            color="text.disabled"
            textAlign="center"
            sx={{ py: 4 }}
          >
            Нет станций
          </Typography>
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
      </Box>
    </Box>
  );
}
