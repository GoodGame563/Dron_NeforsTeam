import { Lightbulb, LightbulbOff, CircleOff } from "lucide-react";

export function LampIcon({ status }: { status: LampStatus }) {
  if (status === "alive") return <Lightbulb size={20} color="#34d399" />;
  if (status === "death") return <LightbulbOff size={20} color="#f87171" />;
  return <CircleOff size={20} color="rgba(255,255,255,0.3)" />;
}
