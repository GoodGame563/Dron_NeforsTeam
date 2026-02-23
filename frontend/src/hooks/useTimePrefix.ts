import { useMemo } from "react";
import type { LampStatus } from "../types/type.ts";

export function useTimePrefix(state: LampStatus) {
  return useMemo(() => {
    switch (state) {
      case "alive":
        return "Работает";
      case "death":
        return "Сломана";
      default:
        return "Пуста";
    }
  }, [state]);
}
