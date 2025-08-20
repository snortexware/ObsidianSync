import Button from '@mui/joy/Button';
import IconButton from '@mui/joy/IconButton';
import Stack from '@mui/joy/Stack';
import Grid from '@mui/joy/Grid';
import Typography from '@mui/joy/Typography';
import Tooltip from '@mui/joy/Tooltip';
import { Refresh, Replay } from '@mui/icons-material';
import SyncIcon from '@mui/icons-material/Sync';
import PowerSettingsNewIcon from '@mui/icons-material/PowerSettingsNew';
import AddToDriveIcon from '@mui/icons-material/AddToDrive';
import ConnectionStarter from '../LogicCore/webSocketCommunication'
import { useState } from 'react';

const files = [
  { name: "Vault1", location: "C:\\obsidian-sync", time: "há 1 hora" },
  { name: "Vault2", location: "C:\\obsidian-sync", time: "há 3 horas" },
  { name: "Vault3", location: "C:\\obsidian-sync", time: "há 7 horas" },
];

export default function MainModal() {

  const [valor, SetValor] = useState("Aguardando conexão");
  
  return <>
    <Stack spacing={2} sx={{ width: "100%", p: 2 }}>
      
      {files.map((file, index) => (
        <Grid
          key={index}
          container
          alignItems="center"
          spacing={2}
          sx={{ 
            borderBottom: index !== files.length - 1
             ? "1px solid" : "none", borderColor: "neutral.outlinedBorder",
              pb: 1 }}
        >
          <Grid xs={2}>
            <IconButton size="sm" variant="plain">
              <AddToDriveIcon />
            </IconButton>
          </Grid>
          <Grid xs={10}>
            <Typography level="body-sm">{file.name}</Typography>
            <Typography level="body-xs" color="neutral">
              {valor} Carregado para {file.location} — {file.time}
            </Typography>
          </Grid>
        </Grid>
      ))}

      <Stack
        direction="row"
        justifyContent="flex-end"
        spacing={2}
        sx={{ pt: 2, borderTop: "1px solid", borderColor: "neutral.outlinedBorder" }}
        >
        <Tooltip title="Restart the service">
          <IconButton color="warning" variant="solid">
            <PowerSettingsNewIcon />
          </IconButton>
        </Tooltip>
        <Tooltip title="Resync all files">
          <IconButton onClick={() => {  ConnectionStarter(5000)
                                      .then((ok) => ok ? SetValor("Conectado") : SetValor("Não conectado"))
                                      .catch(() => SetValor("Erro")) }} color="success" variant="solid" >
            <SyncIcon />
          </IconButton>
        </Tooltip>
      </Stack>
    </Stack>
    </>
}
