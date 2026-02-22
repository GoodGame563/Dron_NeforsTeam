import { useState } from "react";

import { ToggleButtonGroup, ToggleButton, Typography } from "@mui/material";

import { StationList } from "../components/Station/StationList";
import { MapCanvas } from "../components/Map/MapCanvas";
import { LampDetailPanel } from "../components/Lamp/Lampdetailpanel.tsx";
import { LoadingScreen } from "../components/LoadingScreen.tsx";
import { Header } from "../components/Header.tsx";
import { LampList } from "../components/Lamp/LampList.tsx";

import useSocketStore from "../stores/socketStore.ts";

export function Dashboard() {
  const isLoading = useSocketStore((state) => state.isLoad);
  const selectedLamp = useSocketStore((state) => state.selectedLampId);

  const [whatToSee, setWhatToSee] = useState<string>("drone");

  function handleWhatToSee(
    _: React.MouseEvent<HTMLElement>,
    whatToSee: string,
  ) {
    setWhatToSee(whatToSee);
  }

  if (isLoading) {
    return <LoadingScreen />;
  }

  return (
    <div className="flex h-screen bg-[#0f1117] text-white overflow-hidden">
      <aside className="w-80 shrink-0 flex flex-col border-r border-white/10">
        <Header />
        <ToggleButtonGroup
          value={whatToSee}
          exclusive
          onChange={handleWhatToSee}
          aria-label="what to see"
          sx={{
            mx: 2,
            my: 2,
            p: 0.5,
            backgroundColor: "#1a1d25",
            borderRadius: "12px",
            gap: "4px",
            "& .MuiToggleButton-root": {
              flex: 1,
              border: "none",
              borderRadius: "10px !important",
              textTransform: "none",
              color: "rgba(255,255,255,0.6)",
              transition: "all 0.2s ease",
              "&:hover": {
                backgroundColor: "rgba(255,255,255,0.05)",
              },
              "&.Mui-selected": {
                backgroundColor: "#2c3140",
                color: "#fff",
                boxShadow: "0 0 0 1px rgba(255,255,255,0.08)",
              },
              "&.Mui-selected:hover": {
                backgroundColor: "#353b4d",
              },
            },
          }}
        >
          <ToggleButton value="drone">
            <Typography variant="body2" fontWeight={600}>
              Дроны
            </Typography>
          </ToggleButton>

          <ToggleButton value="lamp">
            <Typography variant="body2" fontWeight={600}>
              Столбы
            </Typography>
          </ToggleButton>
        </ToggleButtonGroup>

        {(whatToSee == "drone" && <StationList />) || <LampList />}
      </aside>

      <main className="flex-1 flex flex-col min-w-0">
        <MapCanvas />

        {selectedLamp && <LampDetailPanel />}
      </main>
    </div>
  );
}
