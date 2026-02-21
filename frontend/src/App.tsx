import { ThemeProvider, CssBaseline } from "@mui/material";
import { theme } from "./theme";
import { Dashboard } from "./pages/Dashboard";

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Dashboard />
    </ThemeProvider>
  );
}
