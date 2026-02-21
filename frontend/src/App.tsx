import { useState } from "react";
import { StationList } from "./components/Station/StationList";
import { MapCanvas } from "./components/Map/MapCanvas";
import { LampDetailPanel } from "./components/Lamp/Lampdetailpanel.tsx";
import { useStations } from "./hooks/useStations";
import { useLamps } from "./hooks/useLamps";
import { dispatchDrone } from "./api/drones";
import type { Lamp, Station } from "./types/type.ts";

export default function App() {
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
    <div className="px-5 py-4 border-b border-white/10">
      <div className="flex items-center gap-2.5">
        {/* Иконка дрона */}
        <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center flex-shrink-0">
          <svg
            className="w-5 h-5 text-white"
            viewBox="0 0 24 24"
            fill="currentColor"
          >
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 14H9V8h2v8zm4 0h-2V8h2v8z" />
          </svg>
        </div>
        <div>
          <h1 className="text-sm font-semibold text-white leading-tight">
            DroneLight
          </h1>
          <p className="text-xs text-white/40">Система управления дронами</p>
        </div>
      </div>
    </div>
  );
}

function LoadingScreen() {
  return (
    <div className="flex h-screen items-center justify-center bg-[#0f1117]">
      <div className="text-center">
        <div className="w-10 h-10 border-2 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
        <p className="text-white/40 text-sm">Загрузка данных...</p>
      </div>
    </div>
  );
}
