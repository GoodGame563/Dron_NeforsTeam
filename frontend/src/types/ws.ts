import type { Pillar, DronStation, PillarStation } from "./type.ts";

export type AllDataMessage = {
  event: "all_data";
  data: {
    pillars: Pillar[];
    dron_stations: DronStation[];
    pillar_stations: PillarStation[];
  };
};

export type SendDroneMessage = {
  event: "send_drone";
  data: {
    lampId: string;
  };
};

export type WsMessage = AllDataMessage | SendDroneMessage;
