import { Box, Typography } from "@mui/material";
import { Plane } from "lucide-react";

export function Header() {
  return (
    <Box
      sx={{
        px: 2.5,
        py: 2,
        borderBottom: "1px solid",
        borderColor: "divider",
        display: "flex",
        alignItems: "center",
        gap: 1.5,
      }}
    >
      <Box
        sx={{
          width: 32,
          height: 32,
          borderRadius: 2,
          bgcolor: "primary.dark",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          flexShrink: 0,
        }}
      >
        <Plane size={16} color="#ffffff" />
      </Box>
      <Box>
        <Typography variant="body2" fontWeight={600} lineHeight={1.2}>
          DroneLight
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Система управления дронами
        </Typography>
      </Box>
    </Box>
  );
}
