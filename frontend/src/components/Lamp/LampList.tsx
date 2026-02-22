import { Typography, Box, Divider } from "@mui/material";

export function LampList() {
  return (
    <Box
      sx={{
        flex: 1,
        display: "flex",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      <Box sx={{ px: 2, py: 1.5 }}>
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ textTransform: "uppercase", letterSpacing: 1.5 }}
        >
          Общая статистика
        </Typography>
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr 1fr",
            textAlign: "center",
            mt: 1,
          }}
        >
          <Box>
            <Typography variant="h6" fontWeight={600}>
              0
            </Typography>
            <Typography variant="caption" color="text.secondary">
              всего
            </Typography>
          </Box>
          <Box>
            <Typography variant="h6" fontWeight={600} color="success.main">
              0
            </Typography>
            <Typography variant="caption" color="text.secondary">
              свободно
            </Typography>
          </Box>
          <Box>
            <Typography variant="h6" fontWeight={600} color="error.main">
              10
            </Typography>
            <Typography variant="caption" color="text.secondary">
              сломано
            </Typography>
          </Box>
        </Box>
      </Box>

      <Divider />

      <Box
        sx={{
          flex: 1,
          overflowY: "auto",
          px: 1.5,
          py: 1.5,
          display: "flex",
          flexDirection: "column",
          gap: 1,
        }}
      >
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ textTransform: "uppercase", letterSpacing: 1.5, px: 0.5 }}
        >
          Станции (0)
        </Typography>

        <Typography
          variant="body2"
          color="text.disabled"
          textAlign="center"
          sx={{ py: 4 }}
        >
          Нет станций
        </Typography>
      </Box>
    </Box>
  );
}
