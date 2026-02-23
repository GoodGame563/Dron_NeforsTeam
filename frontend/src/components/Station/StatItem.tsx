import { Typography, Box, Tooltip } from "@mui/material";

interface StatItemProps {
  icon: React.ReactNode;
  value: number;
  label: string;
  color: string;
}

export function StatItem({ icon, value, label, color }: StatItemProps) {
  return (
    <Tooltip title={label} placement="top">
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          gap: 0.5,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color }}>
          {icon}
          <Typography
            variant="body2"
            fontWeight={600}
            color={color}
            lineHeight={1}
          >
            {value}
          </Typography>
        </Box>
        <Typography
          variant="caption"
          color="text.secondary"
          fontSize={10}
          lineHeight={1}
        >
          {label}
        </Typography>
      </Box>
    </Tooltip>
  );
}
