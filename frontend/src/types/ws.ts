import type {
  Pillar,
  DronStation,
  PillarStation,
  LampStatus,
  DronStatus,
  Coordinates,
} from "./type.ts";

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

export type ChangeStatePillar = {
  event: "change_state_pillar";
  data: {
    id: string;
    state: LampStatus;
  };
};

export type ChangeStateDrone = {
  event: "change_state_drone";
  data: {
    id: string;
    state: DronStatus;
    last_coordinates: Coordinates;
  };
};

export type WsMessage =
  | AllDataMessage
  | SendDroneMessage
  | ChangeStatePillar
  | ChangeStateDrone;
