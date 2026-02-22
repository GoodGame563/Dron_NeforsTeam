import type { Lamp, Station } from "./type.ts";

export type StartMessage = {
  event: "start";
  data: {
    lamps: Lamp[];
    stations: Station[];
  };
};

export type SendDroneMessage = {
  event: "send_drone";
  data: {
    lampId: string;
  };
};

export type WsMessage = StartMessage | SendDroneMessage;
