import useSWR from "swr";
import type { Station } from "../types/type.ts";
import { fetchStations } from "../api/stations";

interface UseStationsReturn {
  stations: Station[];
  isLoading: boolean;
  isError: boolean;
  refresh: () => void;
}

export function useStations(): UseStationsReturn {
  const { data, error, isLoading, mutate } = useSWR<Station[]>(
    "/api/stations",
    fetchStations,
    {
      refreshInterval: 30_000,
      revalidateOnFocus: false,
    },
  );

  return {
    stations: data ?? [],
    isLoading,
    isError: !!error,
    refresh: mutate,
  };
}
