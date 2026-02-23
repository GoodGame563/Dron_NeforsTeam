import { create } from "zustand";

import type { Pillar, DronStation } from "../types/type.ts";
import type {
  WsMessage,
  AllDataMessage,
  ChangeStateDrone,
  ChangeStatePillar,
} from "../types/ws.ts";

type State = {
  socket: WebSocket | null;
  isConnected: boolean;
  pillars: Pillar[];
  stations: DronStation[];
  selectedPillar: Pillar | null;
  selectedStation: DronStation | null;
  isLoad: boolean;
};

type Actions = {
  connect: () => void;
  disconnect: () => void;
  send: (message: WsMessage) => void;
  setSelectedPillar: (pillar: Pillar) => void;
  setSelectedStation: (selectedStation: DronStation) => void;
  unselectPillar: () => void;
  unselectStation: () => void;
};

export const useSocketStore = create<State & Actions>((set, get) => ({
  socket: null,
  isLoad: true,
  isConnected: false,
  pillars: [],
  stations: [],
  selectedPillar: null,
  selectedStation: null,

  connect: () => {
    if (get().socket) return;

    const wsUrl = import.meta.env.VITE_WS_URL;
    if (!wsUrl) throw new Error("Invalid .env");

    const ws = new WebSocket(wsUrl);

    ws.onopen = () => set({ isConnected: true });
    ws.onmessage = (event) => {
      const message = JSON.parse(event.data) as WsMessage;
      if (message.event == "all_data") {
        set({
          isLoad: false,
          pillars: (message as AllDataMessage).data.pillars,
          stations: (message as AllDataMessage).data.dron_stations,
        });
        return;
      }

      if (message.event == "change_state_drone") {
        const { id, state, last_coordinates } = (message as ChangeStateDrone)
          .data;

        set((prev) => {
          const updatedStations = prev.stations.map((station) => {
            const updatedDrones = station.drons.map((drone) =>
              drone.id === id
                ? { ...drone, status: state, last_coordinates }
                : drone,
            );

            return {
              ...station,
              drons: updatedDrones,
            };
          });

          let updatedSelectedStation = prev.selectedStation;

          if (prev.selectedStation) {
            const updatedDrones = prev.selectedStation.drons.map((drone) =>
              drone.id === id
                ? { ...drone, status: state, last_coordinates }
                : drone,
            );

            updatedSelectedStation = {
              ...prev.selectedStation,
              drons: updatedDrones,
            };
          }

          return {
            stations: updatedStations,
            selectedStation: updatedSelectedStation,
          };
        });

        return;
      }

      if (message.event == "change_state_pillar") {
        const { id, state } = (message as ChangeStatePillar).data;

        set((prev) => {
          const updatedPillars = prev.pillars.map((pillar) =>
            pillar.id === id
              ? { ...pillar, state, last_update: new Date() }
              : pillar,
          );

          const updatedSelected =
            prev.selectedPillar?.id === id
              ? { ...prev.selectedPillar, state, last_update: new Date() }
              : prev.selectedPillar;

          return {
            pillars: updatedPillars,
            selectedPillar: updatedSelected,
          };
        });

        return;
      }
    };
    ws.onclose = () => set({ isConnected: false, socket: null });
    ws.onerror = (err) => console.error("WebSocket error:", err);

    set({ socket: ws });
  },

  disconnect: () => {
    const { socket } = get();
    if (socket) socket.close();
  },

  send: (message: WsMessage) => {
    const { socket } = get();
    if (socket && socket.readyState === WebSocket.OPEN) {
      socket.send(JSON.stringify(message));
    }
  },

  setSelectedPillar: (pillar: Pillar) => set({ selectedPillar: pillar }),
  setSelectedStation: (station: DronStation) =>
    set({ selectedStation: station }),

  unselectPillar: () => set({ selectedPillar: null }),
  unselectStation: () => set({ selectedStation: null }),
}));

export default useSocketStore;
