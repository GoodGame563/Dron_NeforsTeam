import { createTheme } from "@mui/material/styles";

export const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#3b82f6",
      dark: "#2563eb",
      light: "#60a5fa",
      contrastText: "#ffffff",
    },
    success: {
      main: "#34d399",
      contrastText: "#000000",
    },
    error: {
      main: "#f87171",
      contrastText: "#000000",
    },
    warning: {
      main: "#fbbf24",
    },
    background: {
      default: "#0f1117",
      paper: "#161920",
    },
    divider: "rgba(255,255,255,0.08)",
    text: {
      primary: "#ffffff",
      secondary: "rgba(255,255,255,0.50)",
      disabled: "rgba(255,255,255,0.25)",
    },
  },
  shape: {
    borderRadius: 12,
  },
  typography: {
    fontFamily: '"Inter", "system-ui", sans-serif',
    fontSize: 14,
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundImage: "none",
          backgroundColor: "rgba(255,255,255,0.04)",
          border: "1px solid rgba(255,255,255,0.08)",
          transition: "border-color 0.2s, background-color 0.2s",
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: "none",
          fontWeight: 500,
          borderRadius: 8,
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 99,
          fontWeight: 500,
          fontSize: 11,
          height: 22,
        },
      },
    },
    MuiDivider: {
      styleOverrides: {
        root: {
          borderColor: "rgba(255,255,255,0.08)",
        },
      },
    },
    MuiTooltip: {
      styleOverrides: {
        tooltip: {
          backgroundColor: "#1e2330",
          border: "1px solid rgba(255,255,255,0.1)",
          fontSize: 12,
        },
      },
    },
    MuiCircularProgress: {
      styleOverrides: {
        root: {
          color: "#3b82f6",
        },
      },
    },
  },
});
