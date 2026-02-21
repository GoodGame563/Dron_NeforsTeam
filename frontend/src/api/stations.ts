import type { Station } from "../types/type.ts";
import { MOCK_STATIONS, simulateDelay } from "../mocks/data";

/**
 * Получить список всех станций.
 *
 * TODO: заменить на реальный запрос:
 *   const res = await fetch('/api/stations');
 *   return res.json();
 */
export async function fetchStations(_key: string): Promise<Station[]> {
  await simulateDelay();
  return structuredClone(MOCK_STATIONS);
}
