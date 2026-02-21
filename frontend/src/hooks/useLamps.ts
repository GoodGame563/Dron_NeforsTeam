import useSWR from "swr";
import type { Lamp } from "../types/type.ts";
import { fetchLamps } from "../api/lamps";

interface UseLampsReturn {
  lamps: Lamp[];
  isLoading: boolean;
  isError: boolean;
  mutateLamp: (lampId: string, updated: Lamp) => void;
  refresh: () => void;
}

export function useLamps(): UseLampsReturn {
  const { data, error, isLoading, mutate } = useSWR<Lamp[]>(
    "/api/lamps",
    fetchLamps,
    {
      refreshInterval: 15_000,
      revalidateOnFocus: false,
    },
  );

  function mutateLamp(lampId: string, updated: Lamp): void {
    if (!data) return;

    const next = data.map((lamp) => (lamp.id === lampId ? updated : lamp));

    mutate(next, false);
  }

  return {
    lamps: data ?? [],
    isLoading,
    isError: !!error,
    mutateLamp,
    refresh: mutate,
  };
}
