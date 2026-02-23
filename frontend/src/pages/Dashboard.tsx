import { StationList } from "../components/Station/StationList";
import { MapCanvas } from "../components/Map/MapCanvas";
import { LampDetailPanel } from "../components/Lamp/Lampdetailpanel.tsx";
import { LoadingScreen } from "../components/LoadingScreen.tsx";
import { Header } from "../components/Header.tsx";

import useSocketStore from "../stores/socketStore.ts";

export function Dashboard() {
  const isLoading = useSocketStore((state) => state.isLoad);
  const selectedPillar = useSocketStore((state) => state.selectedPillar);

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <div className="flex h-screen bg-[#0f1117] text-white overflow-hidden">
      <aside className="w-80 shrink-0 flex flex-col border-r border-white/10">
        <Header />
        <StationList />
      </aside>

      <main className="flex-1 flex flex-col min-w-0">
        <MapCanvas />

        {selectedPillar && <LampDetailPanel />}
      </main>
    </div>
  );
}
