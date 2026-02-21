import type { Lamp } from "../types/type.ts";
import { MOCK_LAMPS, simulateDelay } from "../mocks/data";

/**
 * Получить список всех фонарей.
 *
 * TODO: заменить на реальный запрос:
 *   const res = await fetch('/api/lamps');
 *   return res.json();
 */
export async function fetchLamps(_key: string): Promise<Lamp[]> {
  await simulateDelay();
  return structuredClone(MOCK_LAMPS);
}
