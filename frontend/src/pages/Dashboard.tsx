import { useState } from "react";

import { Box, Typography, CircularProgress } from "@mui/material";
import { Plane } from "lucide-react";

import { StationList } from "../components/Station/StationList";
import { MapCanvas } from "../components/Map/MapCanvas";
import { LampDetailPanel } from "../components/Lamp/Lampdetailpanel.tsx";
import { useStations } from "../hooks/useStations";
import { useLamps } from "../hooks/useLamps";
import { dispatchDrone } from "../api/drones";
import type { Lamp, Station } from "../types/type.ts";

export function Dashboard() {
  const { stations, isLoading: stationsLoading } = useStations();
  const { lamps, isLoading: lampsLoading, mutateLamp } = useLamps();

  const [selectedLamp, setSelectedLamp] = useState<Lamp | null>(null);
  const [selectedStation, setSelectedStation] = useState<Station | null>(null);
  const [isDispatching, setIsDispatching] = useState(false);

  const isLoading = stationsLoading || lampsLoading;

  const highlightedStationId = selectedStation?.id ?? null;

  async function handleLampClick(lamp: Lamp) {
    setSelectedLamp(lamp);
  }

  async function handleDispatchDrone(lamp: Lamp) {
    setIsDispatching(true);
    try {
      await dispatchDrone(lamp.id);
      mutateLamp(lamp.id, { ...lamp, status: "alive" });
      setSelectedLamp(null);
    } catch (e) {
      console.error("Ошибка отправки дрона:", e);
    } finally {
      setIsDispatching(false);
    }
  }

  function handleStationSelect(station: Station | null) {
    setSelectedStation(station);
    setSelectedLamp(null);
  }

  function handleClosePanel() {
    setSelectedLamp(null);
  }

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <div className="flex h-screen bg-[#0f1117] text-white overflow-hidden">
      {/* Сайдбар со станциями */}
      <aside className="w-80 flex-shrink-0 flex flex-col border-r border-white/10">
        <Header />
        <StationList
          stations={stations}
          selectedStation={selectedStation}
          onSelectStation={handleStationSelect}
        />
      </aside>

      <main className="flex-1 flex flex-col min-w-0">
        <MapCanvas
          lamps={lamps}
          stations={stations}
          selectedLamp={selectedLamp}
          highlightedStationId={highlightedStationId}
          onLampClick={handleLampClick}
        />

        {selectedLamp && (
          <LampDetailPanel
            lamp={selectedLamp}
            station={
              stations.find((s) => s.id === selectedLamp.stationId) ?? null
            }
            isDispatching={isDispatching}
            onDispatch={() => handleDispatchDrone(selectedLamp)}
            onClose={handleClosePanel}
          />
        )}
      </main>
    </div>
  );
}

function Header() {
  return (
    <Box
      sx={{
        px: 2.5,
        py: 2,
        borderBottom: "1px solid",
        borderColor: "divider",
        display: "flex",
        alignItems: "center",
        gap: 1.5,
      }}
    >
      <Box
        sx={{
          width: 32,
          height: 32,
          borderRadius: 2,
          bgcolor: "primary.dark",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          flexShrink: 0,
        }}
      >
        <Plane size={16} color="#ffffff" />
      </Box>
      <Box>
        <Typography variant="body2" fontWeight={600} lineHeight={1.2}>
          DroneLight
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Система управления дронами
        </Typography>
      </Box>
    </Box>
  );
}

function LoadingScreen() {
  return (
    <Box
      sx={{
        display: "flex",
        height: "100vh",
        alignItems: "center",
        justifyContent: "center",
        flexDirection: "column",
        gap: 2,
        bgcolor: "background.default",
      }}
    >
      <CircularProgress size={36} />
      <Typography variant="body2" color="text.secondary">
        Загрузка данных...
      </Typography>
    </Box>
  );
}
