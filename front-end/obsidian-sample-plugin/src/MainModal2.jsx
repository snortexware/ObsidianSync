import { useState, useEffect } from "react";
import Button from "@mui/joy/Button";
import IconButton from "@mui/joy/IconButton";
import Stack from "@mui/joy/Stack";
import Grid from "@mui/joy/Grid";
import Typography from "@mui/joy/Typography";
import Tooltip from "@mui/joy/Tooltip";
import { Refresh, Replay } from "@mui/icons-material";
import SyncIcon from "@mui/icons-material/Sync";
import PowerSettingsNewIcon from "@mui/icons-material/PowerSettingsNew";
import AddToDriveIcon from "@mui/icons-material/AddToDrive";
import SettingsIcon from "@mui/icons-material/Settings";
import { sign, startWS, sendNewConfig } from "../LogicCore/webSocketCommunication";
import SettingsModal from "./SettingsModal";
import CloudOffIcon from "@mui/icons-material/CloudOff";
import CloudDoneIcon from "@mui/icons-material/CloudDone";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import Box from "@mui/joy/Box";

export default function MainModal() {
	const [valor, SetValor] = useState(false);
	const [settingsOpen, setSettingsOpen] = useState(false);
	const [config, setConfig] = useState({ port: 5000, folder: "" });
	const [connection, setConnection] = useState("offline");
    const[files, setFiles] = useState([]);

    useEffect(() => {
    startWS(); 

    const unsign = sign((cache) => {
      setFiles([...cache]); 
    });

    return () => unsign();
  }, []);
  
	const getConnectionIcon = () => {
		if (valor) {
			return <CloudDoneIcon color="success" />;
		} else {
			return (
				<Stack direction="row" alignItems="center" spacing={1}>
					<CloudOffIcon color="error" />
					<ErrorOutlineIcon color="warning" />
				</Stack>
			);
		}
	};

	return (
		<>
			<Stack
				sx={{
					width: "100%",
					p: 2,
					position: "relative",
					maxHeight: "calc(100vh - 200px)",
				}}
			>
				<Stack
					spacing={2}
					sx={{
						flexGrow: 1,
						overflowY: "auto",
						pr: 2,
						pb: 2,
					}}
				>
					{files.map((file, index) => (
						<Grid
							key={index}
							container
							alignItems="center"
							spacing={2}
							sx={{
								borderBottom:
									index !== files.length - 1
										? "1px solid"
										: "none",
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
								<Typography level="body-sm">
									{file.name}
								</Typography>
								<Typography level="body-xs" color="neutral">
									Loaded in {file.location} — {file.time}
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
					<Stack
						direction="row"
						justifyContent="space-between"
						alignItems="center"
						spacing={2}
					>
						<Stack direction="row" alignItems="center" spacing={2}>
							<Typography
								fontSize={"sm"}
								sx={{ color: "#FFFFFF" }}
							>
								Your vault's are {connection}
							</Typography>
							{getConnectionIcon()}
						</Stack>

						<Stack direction="row" spacing={2}>
							<Tooltip title="Settings">
								<IconButton
									color="primary"
									variant="solid"
									onClick={() => setSettingsOpen(true)}
								>
									<SettingsIcon />
								</IconButton>
							</Tooltip>

							<Tooltip title="Restart the service">
								<IconButton color="warning" variant="solid">
									<PowerSettingsNewIcon />
								</IconButton>
							</Tooltip>

							<Tooltip title="Resync all files">
								<IconButton
									onClick={() => {
										var wc = startWS(config.port);
											

									}}
									color="success"
									variant="solid"
								>
									<SyncIcon />
								</IconButton>
							</Tooltip>
						</Stack>
					</Stack>
				</Box>
			</Stack>

			<SettingsModal
				open={settingsOpen}
				onClose={() => setSettingsOpen(false)}
				onSave={sendNewConfig(config)}
			/>
		</>
	);
}
