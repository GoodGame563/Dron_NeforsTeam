import { useEffect } from "react";

import { ThemeProvider, CssBaseline } from "@mui/material";

import { useSocketStore } from "./stores/socketStore";
import { theme } from "./theme";
import { Dashboard } from "./pages/Dashboard";

export default function App() {
  const connect = useSocketStore((state) => state.connect);
  const disconnect = useSocketStore((state) => state.disconnect);

  useEffect(() => {
    connect();
    return () => disconnect();
  }, [connect, disconnect]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Dashboard />
    </ThemeProvider>
  );
}
