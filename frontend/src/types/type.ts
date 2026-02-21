export type LampStatus = "alive" | "death" | "empty";
export type DroneStatus = "available" | "busy" | "broken";

export interface Lamp {
  id: string;
  stationId: string;
  position: { x: number; y: number };
  status: LampStatus;
}

export interface Drone {
  id: string;
  stationId: string;
  status: DroneStatus;
}

export interface Station {
  id: string;
  name: string;
  position: { x: number; y: number };
  totalDrones: number;
  availableDrones: number;
  brokenDrones: number;
  lampCount: number;
}
