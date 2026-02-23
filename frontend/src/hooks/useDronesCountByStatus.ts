import { useMemo } from "react";

import type { Dron, DronStatus } from "../types/type.ts";

export function useDronesCountByStatus(drones: Dron[], status: DronStatus) {
  const count = useMemo(() => {
    if (!drones) return 0;

    return drones.filter((drone) => drone.status === status).length;
  }, [drones, status]);

  return count;
}
