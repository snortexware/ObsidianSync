import { useState, useRef } from 'react';
import Modal from '@mui/joy/Modal';
import ModalDialog from '@mui/joy/ModalDialog';
import ModalClose from '@mui/joy/ModalClose';
import Typography from '@mui/joy/Typography';
import Button from '@mui/joy/Button';
import Input from '@mui/joy/Input';
import Stack from '@mui/joy/Stack';

export default function SettingsModal({ open, onClose, onSave }) {
  const [port, setPort] = useState(5000);
  const [folder, setFolder] = useState('');
  const dialogRef = useRef(null);

  const handleSave = () => {
    onSave({ port, folder });
    onClose();
  };

  const handlePortChange = (e) => {
  const value = e.target.value;
  const numberValue = Number(value);

  if (Number.isNaN(numberValue)) {
    setPort(0);
  }else{
    setPort(value);
  }
};

  return (
    <Modal
  open={open}
  onClose={onClose}
  disablePortal
  closeOnEsc
  closeOnBackdropClick={true}
  hideBackdrop
  slotProps={{
    backdrop: {
      sx: {
      },
    },
  }}
>
  <ModalDialog
    ref={dialogRef}
    variant="outlined"
    sx={{
      overflow: 'visible',
      p: 3,
      minWidth: 300,
      backgroundColor: 'var(--background-primary)', 
      color: 'var(--text-normal)',
      border: '1px solid var(--background-modifier-border)',
    }}
  >
      <ModalClose />
      <Typography level="h6" mb={2}>Settings</Typography>

    <Stack spacing={2}>
      <Input
        value={port}
        onChange={handlePortChange}
        placeholder="Port"
        sx={{ backgroundColor: 'var(--background-modifier-card)' }}
      />
      
      <Input
        value={folder}
        onChange={(e) => setFolder(e.target.value)}
        placeholder="Vault's folder"
        sx={{ backgroundColor: 'var(--background-modifier-card)' }}
      />
      <Button onClick={handleSave} variant="solid">Salvar</Button>
    </Stack>
  </ModalDialog>
</Modal>
  );
}
