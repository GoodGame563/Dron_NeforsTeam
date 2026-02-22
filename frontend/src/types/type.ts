export type LampStatus = "alive" | "death" | "empty";

export type Coordinates = {
  x: number;
  y: number;
};

export type Lamp = {
  id: string;
  stationId: string | null;
  coordinates: Coordinates;
  status: LampStatus;
  updatedAt: Date;
};

export type Station = {
  id: string;
  name: string;
  coordinates: Coordinates;
  totalDrones: number;
  availableDrones: number;
  brokenDrones: number;
  lampCount: number;
};
