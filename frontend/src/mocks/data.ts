import type { Station, Lamp, Drone } from "../types/type.ts";

export const MOCK_STATIONS: Station[] = [
  {
    id: "station-1",
    name: "Станция Север",
    position: { x: 150, y: 120 },
    totalDrones: 5,
    availableDrones: 3,
    brokenDrones: 1,
    lampCount: 8,
  },
  {
    id: "station-2",
    name: "Станция Центр",
    position: { x: 420, y: 300 },
    totalDrones: 8,
    availableDrones: 6,
    brokenDrones: 0,
    lampCount: 14,
  },
  {
    id: "station-3",
    name: "Станция Юг",
    position: { x: 200, y: 480 },
    totalDrones: 4,
    availableDrones: 1,
    brokenDrones: 2,
    lampCount: 6,
  },
  {
    id: "station-4",
    name: "Станция Восток",
    position: { x: 650, y: 200 },
    totalDrones: 6,
    availableDrones: 5,
    brokenDrones: 0,
    lampCount: 10,
  },
];

export const MOCK_LAMPS: Lamp[] = [
  // Станция Север
  {
    id: "lamp-1",
    stationId: "station-1",
    position: { x: 100, y: 80 },
    status: "alive",
  },
  {
    id: "lamp-2",
    stationId: "station-1",
    position: { x: 180, y: 80 },
    status: "death",
  },
  {
    id: "lamp-3",
    stationId: "station-1",
    position: { x: 100, y: 155 },
    status: "alive",
  },
  {
    id: "lamp-4",
    stationId: "station-1",
    position: { x: 220, y: 155 },
    status: "empty",
  },
  {
    id: "lamp-5",
    stationId: "station-1",
    position: { x: 80, y: 200 },
    status: "alive",
  },
  {
    id: "lamp-6",
    stationId: "station-1",
    position: { x: 200, y: 200 },
    status: "death",
  },
  {
    id: "lamp-7",
    stationId: "station-1",
    position: { x: 130, y: 60 },
    status: "alive",
  },
  {
    id: "lamp-8",
    stationId: "station-1",
    position: { x: 240, y: 100 },
    status: "empty",
  },

  // Станция Центр
  {
    id: "lamp-9",
    stationId: "station-2",
    position: { x: 340, y: 250 },
    status: "alive",
  },
  {
    id: "lamp-10",
    stationId: "station-2",
    position: { x: 400, y: 250 },
    status: "alive",
  },
  {
    id: "lamp-11",
    stationId: "station-2",
    position: { x: 460, y: 250 },
    status: "death",
  },
  {
    id: "lamp-12",
    stationId: "station-2",
    position: { x: 340, y: 310 },
    status: "alive",
  },
  {
    id: "lamp-13",
    stationId: "station-2",
    position: { x: 400, y: 310 },
    status: "empty",
  },
  {
    id: "lamp-14",
    stationId: "station-2",
    position: { x: 460, y: 310 },
    status: "alive",
  },
  {
    id: "lamp-15",
    stationId: "station-2",
    position: { x: 500, y: 270 },
    status: "death",
  },
  {
    id: "lamp-16",
    stationId: "station-2",
    position: { x: 360, y: 370 },
    status: "alive",
  },
  {
    id: "lamp-17",
    stationId: "station-2",
    position: { x: 420, y: 370 },
    status: "alive",
  },
  {
    id: "lamp-18",
    stationId: "station-2",
    position: { x: 480, y: 370 },
    status: "empty",
  },
  {
    id: "lamp-19",
    stationId: "station-2",
    position: { x: 340, y: 220 },
    status: "alive",
  },
  {
    id: "lamp-20",
    stationId: "station-2",
    position: { x: 500, y: 340 },
    status: "death",
  },
  {
    id: "lamp-21",
    stationId: "station-2",
    position: { x: 380, y: 400 },
    status: "alive",
  },
  {
    id: "lamp-22",
    stationId: "station-2",
    position: { x: 440, y: 220 },
    status: "empty",
  },

  // Станция Юг
  {
    id: "lamp-23",
    stationId: "station-3",
    position: { x: 140, y: 420 },
    status: "alive",
  },
  {
    id: "lamp-24",
    stationId: "station-3",
    position: { x: 210, y: 420 },
    status: "death",
  },
  {
    id: "lamp-25",
    stationId: "station-3",
    position: { x: 270, y: 420 },
    status: "alive",
  },
  {
    id: "lamp-26",
    stationId: "station-3",
    position: { x: 140, y: 490 },
    status: "empty",
  },
  {
    id: "lamp-27",
    stationId: "station-3",
    position: { x: 210, y: 490 },
    status: "alive",
  },
  {
    id: "lamp-28",
    stationId: "station-3",
    position: { x: 270, y: 490 },
    status: "death",
  },

  // Станция Восток
  {
    id: "lamp-29",
    stationId: "station-4",
    position: { x: 580, y: 150 },
    status: "alive",
  },
  {
    id: "lamp-30",
    stationId: "station-4",
    position: { x: 640, y: 150 },
    status: "alive",
  },
  {
    id: "lamp-31",
    stationId: "station-4",
    position: { x: 700, y: 150 },
    status: "death",
  },
  {
    id: "lamp-32",
    stationId: "station-4",
    position: { x: 580, y: 220 },
    status: "alive",
  },
  {
    id: "lamp-33",
    stationId: "station-4",
    position: { x: 640, y: 220 },
    status: "empty",
  },
  {
    id: "lamp-34",
    stationId: "station-4",
    position: { x: 700, y: 220 },
    status: "alive",
  },
  {
    id: "lamp-35",
    stationId: "station-4",
    position: { x: 580, y: 290 },
    status: "alive",
  },
  {
    id: "lamp-36",
    stationId: "station-4",
    position: { x: 640, y: 290 },
    status: "death",
  },
  {
    id: "lamp-37",
    stationId: "station-4",
    position: { x: 700, y: 290 },
    status: "alive",
  },
  {
    id: "lamp-38",
    stationId: "station-4",
    position: { x: 620, y: 110 },
    status: "empty",
  },
];

export const MOCK_DRONES: Drone[] = [
  { id: "drone-1", stationId: "station-1", status: "available" },
  { id: "drone-2", stationId: "station-1", status: "available" },
  { id: "drone-3", stationId: "station-1", status: "available" },
  { id: "drone-4", stationId: "station-1", status: "busy" },
  { id: "drone-5", stationId: "station-1", status: "broken" },

  { id: "drone-6", stationId: "station-2", status: "available" },
  { id: "drone-7", stationId: "station-2", status: "available" },
  { id: "drone-8", stationId: "station-2", status: "available" },
  { id: "drone-9", stationId: "station-2", status: "available" },
  { id: "drone-10", stationId: "station-2", status: "available" },
  { id: "drone-11", stationId: "station-2", status: "available" },
  { id: "drone-12", stationId: "station-2", status: "busy" },
  { id: "drone-13", stationId: "station-2", status: "busy" },

  { id: "drone-14", stationId: "station-3", status: "available" },
  { id: "drone-15", stationId: "station-3", status: "busy" },
  { id: "drone-16", stationId: "station-3", status: "broken" },
  { id: "drone-17", stationId: "station-3", status: "broken" },

  { id: "drone-18", stationId: "station-4", status: "available" },
  { id: "drone-19", stationId: "station-4", status: "available" },
  { id: "drone-20", stationId: "station-4", status: "available" },
  { id: "drone-21", stationId: "station-4", status: "available" },
  { id: "drone-22", stationId: "station-4", status: "available" },
  { id: "drone-23", stationId: "station-4", status: "busy" },
];

// Задержка для имитации сетевого запроса
export const MOCK_DELAY_MS = 600;

export async function simulateDelay(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, MOCK_DELAY_MS));
}
