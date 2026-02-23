import { Box, Typography, Divider } from "@mui/material";

import { useSocketStore } from "../../stores/socketStore";
import { StationCard } from "./StationCard";
import type { DronStation, Dron } from "../../types/type.ts";

export function StationList() {
  const { stations, selectedStation, setSelectedStation, unselectStation } =
    useSocketStore((state) => ({
      stations: state.stations,
      setSelectedStation: state.setSelectedStation,
      selectedStation: state.selectedStation,
      unselectStation: state.unselectStation,
    }));

  function handleSelect(station: DronStation) {
    if (selectedStation?.id === station.id) {
      unselectStation();
    } else {
      setSelectedStation(station);
    }
  }

  const totalDrones = stations.reduce((sum, s) => sum + s.total_drone_count, 0);
  const availableDrones = stations.reduce(
    (sum, s) =>
      sum +
      s.drons.filter((drone: Dron) => drone.status == "in_station").length,
    0,
  );
  const brokenDrones = stations.reduce(
    (sum, s) =>
      sum + s.drons.filter((drone: Dron) => drone.status === "broken").length,
    0,
  );

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
