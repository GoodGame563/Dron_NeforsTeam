import { useRef, useEffect, useCallback } from "react";
import type { Lamp, Station, LampStatus } from "../../types/type.ts";

interface MapCanvasProps {
  lamps: Lamp[];
  stations: Station[];
  selectedLamp: Lamp | null;
  highlightedStationId: string | null;
  onLampClick: (lamp: Lamp) => void;
}

// ─── Константы отрисовки ─────────────────────────────────────────────────────

const LAMP_RADIUS = 8;
const STATION_RADIUS = 14;
const HIT_RADIUS = 16; // зона клика чуть больше визуальной

const LAMP_COLORS: Record<LampStatus, { fill: string; glow: string }> = {
  alive: { fill: "#34d399", glow: "rgba(52, 211, 153, 0.35)" },
  death: { fill: "#f87171", glow: "rgba(248, 113, 113, 0.35)" },
  empty: { fill: "#6b7280", glow: "rgba(107, 114, 128, 0.20)" },
};

const GRID_COLOR = "rgba(255,255,255,0.04)";
const GRID_STEP = 40;
const BG_COLOR = "#0f1117";
const STATION_COLOR = "#3b82f6";
const STATION_GLOW = "rgba(59,130,246,0.30)";
const DIM_ALPHA = 0.25; // прозрачность для незадействованных элементов

// ─── Компонент ───────────────────────────────────────────────────────────────

export function MapCanvas({
  lamps,
  stations,
  selectedLamp,
  highlightedStationId,
  onLampClick,
}: MapCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  // ─── Отрисовка ─────────────────────────────────────────────────────────────

  const draw = useCallback(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const { width, height } = canvas;

    ctx.clearRect(0, 0, width, height);

    drawBackground(ctx, width, height);
    drawGrid(ctx, width, height);

    // Линии от станции к её фонарям (рисуем под остальным)
    stations.forEach((station) => {
      const isHighlighted =
        highlightedStationId === null || highlightedStationId === station.id;
      const alpha = isHighlighted ? 0.12 : 0.04;
      drawStationLines(ctx, station, lamps, alpha);
    });

    // Фонари
    lamps.forEach((lamp) => {
      const isDimmed =
        highlightedStationId !== null &&
        lamp.stationId !== highlightedStationId;
      const isSelected = selectedLamp?.id === lamp.id;
      drawLamp(ctx, lamp, isDimmed, isSelected);
    });

    // Станции (поверх линий и фонарей)
    stations.forEach((station) => {
      const isDimmed =
        highlightedStationId !== null && station.id !== highlightedStationId;
      drawStation(ctx, station, isDimmed);
    });
  }, [lamps, stations, selectedLamp, highlightedStationId]);

  // ─── Resize observer ───────────────────────────────────────────────────────

  useEffect(() => {
    const container = containerRef.current;
    const canvas = canvasRef.current;
    if (!container || !canvas) return;

    const observer = new ResizeObserver((entries) => {
      const entry = entries[0];
      const { width, height } = entry.contentRect;
      canvas.width = width;
      canvas.height = height;
      draw();
    });

    observer.observe(container);
    return () => observer.disconnect();
  }, [draw]);

  useEffect(() => {
    draw();
  }, [draw]);

  // ─── Клик по canvas ────────────────────────────────────────────────────────

  function handleClick(e: React.MouseEvent<HTMLCanvasElement>) {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    // Ищем ближайшую лампу в зоне клика
    const hit = lamps.find((lamp) => {
      const dx = lamp.position.x - x;
      const dy = lamp.position.y - y;
      return Math.sqrt(dx * dx + dy * dy) <= HIT_RADIUS;
    });

    if (hit) onLampClick(hit);
  }

  return (
    <div ref={containerRef} className="flex-1 relative overflow-hidden">
      <canvas
        ref={canvasRef}
        onClick={handleClick}
        className="absolute inset-0 cursor-crosshair"
        style={{ width: "100%", height: "100%" }}
      />

      {/* Легенда */}
      <Legend />
    </div>
  );
}

// ─── Функции отрисовки (pure) ─────────────────────────────────────────────────

function drawBackground(ctx: CanvasRenderingContext2D, w: number, h: number) {
  ctx.fillStyle = BG_COLOR;
  ctx.fillRect(0, 0, w, h);
}

function drawGrid(ctx: CanvasRenderingContext2D, w: number, h: number) {
  ctx.strokeStyle = GRID_COLOR;
  ctx.lineWidth = 1;

  for (let x = 0; x < w; x += GRID_STEP) {
    ctx.beginPath();
    ctx.moveTo(x, 0);
    ctx.lineTo(x, h);
    ctx.stroke();
  }
  for (let y = 0; y < h; y += GRID_STEP) {
    ctx.beginPath();
    ctx.moveTo(0, y);
    ctx.lineTo(w, y);
    ctx.stroke();
  }
}

function drawStationLines(
  ctx: CanvasRenderingContext2D,
  station: Station,
  lamps: Lamp[],
  alpha: number,
) {
  const stationLamps = lamps.filter((l) => l.stationId === station.id);
  ctx.strokeStyle = `rgba(99,130,246,${alpha})`;
  ctx.lineWidth = 1;
  ctx.setLineDash([4, 4]);

  stationLamps.forEach((lamp) => {
    ctx.beginPath();
    ctx.moveTo(station.position.x, station.position.y);
    ctx.lineTo(lamp.position.x, lamp.position.y);
    ctx.stroke();
  });

  ctx.setLineDash([]);
}

function drawLamp(
  ctx: CanvasRenderingContext2D,
  lamp: Lamp,
  isDimmed: boolean,
  isSelected: boolean,
) {
  const { fill, glow } = LAMP_COLORS[lamp.status];
  const alpha = isDimmed ? DIM_ALPHA : 1;
  const { x, y } = lamp.position;

  ctx.globalAlpha = alpha;

  // Glow
  if (!isDimmed) {
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, LAMP_RADIUS * 2.5);
    gradient.addColorStop(0, glow);
    gradient.addColorStop(1, "transparent");
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(x, y, LAMP_RADIUS * 2.5, 0, Math.PI * 2);
    ctx.fill();
  }

  // Кольцо выделения
  if (isSelected) {
    ctx.globalAlpha = 1;
    ctx.strokeStyle = "#ffffff";
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(x, y, LAMP_RADIUS + 4, 0, Math.PI * 2);
    ctx.stroke();
  }

  // Заливка
  ctx.globalAlpha = alpha;
  ctx.fillStyle = fill;
  ctx.beginPath();
  ctx.arc(x, y, LAMP_RADIUS, 0, Math.PI * 2);
  ctx.fill();

  ctx.globalAlpha = 1;
}

function drawStation(
  ctx: CanvasRenderingContext2D,
  station: Station,
  isDimmed: boolean,
) {
  const alpha = isDimmed ? DIM_ALPHA : 1;
  const { x, y } = station.position;

  ctx.globalAlpha = alpha;

  // Glow
  if (!isDimmed) {
    const gradient = ctx.createRadialGradient(
      x,
      y,
      0,
      x,
      y,
      STATION_RADIUS * 2.5,
    );
    gradient.addColorStop(0, STATION_GLOW);
    gradient.addColorStop(1, "transparent");
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(x, y, STATION_RADIUS * 2.5, 0, Math.PI * 2);
    ctx.fill();
  }

  // Внешний круг
  ctx.strokeStyle = STATION_COLOR;
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.arc(x, y, STATION_RADIUS, 0, Math.PI * 2);
  ctx.stroke();

  // Внутренний круг (залитый)
  ctx.fillStyle = STATION_COLOR;
  ctx.beginPath();
  ctx.arc(x, y, STATION_RADIUS - 5, 0, Math.PI * 2);
  ctx.fill();

  // Иконка дрона (простой крест)
  ctx.strokeStyle = "#ffffff";
  ctx.lineWidth = 1.5;
  ctx.lineCap = "round";
  const arm = 4;
  ctx.beginPath();
  ctx.moveTo(x - arm, y);
  ctx.lineTo(x + arm, y);
  ctx.moveTo(x, y - arm);
  ctx.lineTo(x, y + arm);
  ctx.stroke();

  // Подпись
  ctx.globalAlpha = isDimmed ? DIM_ALPHA : 0.7;
  ctx.fillStyle = "#ffffff";
  ctx.font = "10px system-ui";
  ctx.textAlign = "center";
  ctx.textBaseline = "top";
  ctx.fillText(station.name, x, y + STATION_RADIUS + 4);

  ctx.globalAlpha = 1;
  ctx.textAlign = "left";
  ctx.textBaseline = "alphabetic";
}

// ─── Легенда ─────────────────────────────────────────────────────────────────

function Legend() {
  const items = [
    { color: "#34d399", label: "Исправен" },
    { color: "#f87171", label: "Требует замены" },
    { color: "#6b7280", label: "Нет лампы" },
    { color: "#3b82f6", label: "Станция", square: true },
  ];

  return (
    <div className="absolute bottom-4 right-4 flex flex-col gap-1.5 bg-black/40 backdrop-blur-sm border border-white/10 rounded-xl px-3 py-2.5">
      {items.map(({ color, label, square }) => (
        <div key={label} className="flex items-center gap-2">
          <span
            className="flex-shrink-0"
            style={{
              width: square ? 10 : 8,
              height: square ? 10 : 8,
              borderRadius: square ? 2 : "50%",
              background: color,
              display: "inline-block",
            }}
          />
          <span className="text-[11px] text-white/60">{label}</span>
        </div>
      ))}
    </div>
  );
}
