import { Box, Typography, CircularProgress } from "@mui/material";

export function LoadingScreen() {
  return (
    <Box
      sx={{
        display: "flex",
        height: "100vh",
        alignItems: "center",
        justifyContent: "center",
        flexDirection: "column",
        gap: 2,
        bgcolor: "background.default",
      }}
    >
      <CircularProgress size={36} />
      <Typography variant="body2" color="text.secondary">
        Загрузка данных...
      </Typography>
    </Box>
  );
}
