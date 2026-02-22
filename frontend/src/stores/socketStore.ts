import { create } from "zustand";

import type { Lamp, Station } from "../types/type.ts";
import type { WsMessage, StartMessage } from "../types/ws.ts";

type State = {
  socket: WebSocket | null;
  isConnected: boolean;
  lamps: Lamp[];
  stations: Station[];
  selectedLampId: string | null;
  selectedStationId: string | null;
  isLoad: boolean;
};

type Actions = {
  connect: () => void;
  disconnect: () => void;
  send: (message: WsMessage) => void;
  updateLamp: (lamp: Lamp) => void;
  updateStation: (station: Station) => void;
  setSelectedLampId: (id: string) => void;
  setSelectedStationId: (id: string) => void;
  unselectLamp: () => void;
  unselectStation: () => void;
};

export const useSocketStore = create<State & Actions>((set, get) => ({
  socket: null,
  isLoad: true,
  isConnected: false,
  lamps: [],
  stations: [],
  selectedLampId: null,
  selectedStationId: null,

  connect: () => {
    if (get().socket) return;

    const wsUrl = import.meta.env.VITE_WS_URL;
    if (!wsUrl) throw new Error("Invalid .env");

    const ws = new WebSocket(wsUrl);

    ws.onopen = () => set({ isConnected: true });
    ws.onmessage = (event) => {
      const message = JSON.parse(event.data) as WsMessage;
      if (message.event == "start") {
        set({
          isLoad: false,
          lamps: (message as StartMessage).data.lamps,
          stations: (message as StartMessage).data.stations,
        });
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

  updateLamp: (lamp: Lamp) =>
    set((state) => ({
      lamps: { ...state.lamps, [lamp.id]: lamp },
    })),

  updateStation: (station: Station) =>
    set((state) => ({
      stations: { ...state.stations, [station.id]: station },
    })),
  setSelectedLampId: (id: string) => set({ selectedLampId: id }),
  setSelectedStationId: (id: string) => set({ selectedStationId: id }),
  unselectLamp: () => set({ selectedLampId: null }),
  unselectStation: () => set({ selectedStationId: null }),
}));

export default useSocketStore;
