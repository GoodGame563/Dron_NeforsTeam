import { useMemo } from "react";
import type { LampStatus } from "../types/type.ts";

interface StatusConfig {
  label: string;
  color: "success" | "error" | "default";
}

const STATUS_CONFIG: Record<LampStatus, StatusConfig> = {
  alive: { label: "Исправен", color: "success" },
  death: { label: "Требует замены", color: "error" },
  empty: { label: "Нет лампы", color: "default" },
};

export function useLampStatus(state: LampStatus) {
  return useMemo(() => STATUS_CONFIG[state], [state]);
}
