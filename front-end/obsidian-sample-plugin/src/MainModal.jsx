import React, { useEffect, useState } from "react";
import { onWSMessage, getCachedVaults, requestVaults } from "../LogicCore/webSocketCommunication";

import IconButton from "@mui/joy/IconButton";
import Stack from "@mui/joy/Stack";
import Grid from "@mui/joy/Grid";
import Typography from "@mui/joy/Typography";
import Tooltip from "@mui/joy/Tooltip";
import SyncIcon from "@mui/icons-material/Sync";
import PowerSettingsNewIcon from "@mui/icons-material/PowerSettingsNew";
import AddToDriveIcon from "@mui/icons-material/AddToDrive";
import SettingsIcon from "@mui/icons-material/Settings";
import CloudOffIcon from "@mui/icons-material/CloudOff";
import CloudDoneIcon from "@mui/icons-material/CloudDone";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import Box from "@mui/joy/Box";
import SettingsModal from "./SettingsModal";

// small inline icon component — variant: "normal" (multi-color) or "red" (single red)
function GoogleDriveIcon({ variant = "normal", size = 18 }) {
  const normal = (
    <svg width={size} height={size} viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
      <path fill="#0F9D58" d="M2.5 19.2 12 3l6.6 11.4z"/>
      <path fill="#FBBC05" d="M21.5 19.2 12 3 6.7 12.7 16.8 12.7z"/>
      <path fill="#4285F4" d="M2.5 19.2 6.7 12.7 16.8 12.7 12 21z"/>
    </svg>
  );
  const red = (
    <svg width={size} height={size} viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
      <path fill="#D32F2F" d="M2.5 19.2 12 3l6.6 11.4z"/>
      <path fill="#D32F2F" d="M21.5 19.2 12 3 6.7 12.7 16.8 12.7z"/>
      <path fill="#D32F2F" d="M2.5 19.2 6.7 12.7 16.8 12.7 12 21z"/>
    </svg>
  );
  return variant === "red" ? red : normal;
}

export default function MainModal({ initialConnection = "offline" }) {
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [connection, setConnection] = useState(initialConnection);
  const [files, setFiles] = useState([]);

  useEffect(() => {
    // populate from cached vaults if available
    const cached = getCachedVaults();
    if (cached) setFiles(cached);

    // subscribe to live updates
    const unsub = onWSMessage((msg) => {
      if (!msg) return;
      if (msg.type === "connection") {
        setConnection(msg.ok ? "online" : "offline");
        return;
      }
      if (msg.type === "vaultsUpdated") {
        setFiles(msg.data || []);
        return;
      }
    });

    // If no cached vaults and connection is already open, request them.
    // (requestVaults is idempotent via vaultsFetched guard in WS module)
    if (!cached) {
      requestVaults().catch((e) => console.warn("requestVaults failed:", e));
    }

    return () => unsub();
  }, []);

  async function loadVaults() {
    const res = await sendTask(0); // HandlerType.Vaults

    if (res.ok && res.data) {
      setFiles(res.data);
    }
  }

  async function forceSync() {
    const res = await sendTask(2);
    if (res.ok) loadVaults();
  }

  const getConnectionIcon = () => {
    // return Drive icon for online, grey/outline for offline
    if (connection === "online") return <GoogleDriveIcon variant="normal" size={18} />;
    return <GoogleDriveIcon variant="red" size={18} />; // use red to indicate offline/disconnected if you prefer
  };

  return (
    <>
      <Stack sx={{ width: "100%", p: 2, position: "relative", maxHeight: "calc(100vh - 200px)" }}>
        <Stack spacing={2} sx={{ flexGrow: 1, overflowY: "auto", pr: 2, pb: 2 }}>
          {files.map((file, index) => (
            <Grid
              key={index}
              container
              alignItems="center"
              spacing={2}
              sx={{
                borderBottom: index !== files.length - 1 ? "1px solid" : "none",
                borderColor: "neutral.outlinedBorder",
                pb: 1,
              }}
            >
              <Grid xs={2}>
                <IconButton size="sm" variant="plain">
                  <AddToDriveIcon />
                </IconButton>
              </Grid>

              <Grid xs={10}>
                <Typography level="body-sm">{file.name}</Typography>
                <Typography level="body-xs" color="neutral">
                  Loaded in {file.path} — {file.timestamp}
                </Typography>
              </Grid>
            </Grid>
          ))}
        </Stack>

        <Box
          sx={{
            position: "sticky",
            bottom: 0,
            left: 0,
            width: "100%",
            pb: 2,
            pt: 2,
            backgroundColor: "var(--joy-palette-background-level1)",
            zIndex: 1,
            boxSizing: "border-box",
          }}
        >
          <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
            <Stack direction="row" alignItems="center" spacing={2}>
              <Typography fontSize={"sm"} sx={{ color: "#FFFFFF" }}>
                Your vaults are {connection}
              </Typography>
              {getConnectionIcon()}
            </Stack>

            <Stack direction="row" spacing={2}>
              <Tooltip title="Settings">
                <IconButton color="primary" variant="solid" onClick={() => setSettingsOpen(true)}>
                  <SettingsIcon />
                </IconButton>
              </Tooltip>

              <Tooltip title="Restart the service">
                <IconButton color="warning" variant="solid">
                  <PowerSettingsNewIcon />
                </IconButton>
              </Tooltip>

              <Tooltip title="Resync all files">
                <IconButton onClick={forceSync} color="success" variant="solid">
                  <SyncIcon />
                </IconButton>
              </Tooltip>
            </Stack>
          </Stack>
        </Box>
      </Stack>

      <SettingsModal open={settingsOpen} onClose={() => setSettingsOpen(false)} />
    </>
  );
}
