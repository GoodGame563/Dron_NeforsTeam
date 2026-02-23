export type LampStatus = "alive" | "death" | "empty";
export type DronStatus = "broken" | "in_station" | "fly";

export type Coordinates = {
  x: number;
  y: number;
};

export type Pillar = {
  id: string;
  coordinates: Coordinates;
  state: LampStatus;
  pillar_station_id: string | null;
  dron_station_id: string | null;
  last_update: Date;
};

export type PillarStation = {
  id: string;
  coordinates: Coordinates;
};

export type DronStation = {
  id: string;
  coordinates: Coordinates;
  total_drone_count: number;
  total_lamps_count: number;
  drons: Dron[];
};

export type Dron = {
  id: string;
  last_coordinates: Coordinates;
  status: DronStatus;
};
