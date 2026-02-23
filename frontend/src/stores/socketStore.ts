import { create } from "zustand";

import type { Pillar, DronStation } from "../types/type.ts";
import type { WsMessage, AllDataMessage } from "../types/ws.ts";

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
