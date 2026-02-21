import { simulateDelay } from "../mocks/data";

/**
 * Отправить дрона на фонарный столб.
 * Бекенд сам выбирает ближайший свободный дрон.
 *
 * TODO: заменить на реальный запрос:
 *   const res = await fetch(`/api/drones/dispatch`, {
 *     method: 'POST',
 *     headers: { 'Content-Type': 'application/json' },
 *     body: JSON.stringify({ lampId }),
 *   });
 *   if (!res.ok) throw new Error('Ошибка отправки дрона');
 */
export async function dispatchDrone(lampId: string): Promise<void> {
  await simulateDelay();
  console.info(`[MOCK] dispatchDrone → lampId: ${lampId}`);
}
