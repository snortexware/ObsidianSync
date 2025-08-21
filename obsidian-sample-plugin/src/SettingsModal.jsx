import { useState, useRef } from 'react';
import Modal from '@mui/joy/Modal';
import ModalDialog from '@mui/joy/ModalDialog';
import ModalClose from '@mui/joy/ModalClose';
import Typography from '@mui/joy/Typography';
import Button from '@mui/joy/Button';
import Input from '@mui/joy/Input';
import Stack from '@mui/joy/Stack';
import { NumbersRounded } from '@mui/icons-material';

export default function SettingsModal({ open, onClose, onSave }) {
  const [port, setPort] = useState(5000);
  const [folder, setFolder] = useState('');
  const dialogRef = useRef(null);
  const posRef = useRef({ x: 0, y: 0, offsetX: 0, offsetY: 0, dragging: false });

  const handleSave = () => {
    onSave({ port, folder });
    onClose();
  };

  const onMouseDown = (e) => {
    // arrastar apenas se clicar no cabeçalho (por exemplo, título)
    if (!e.target.closest('.draggable-header')) return;
    posRef.current.dragging = true;
    posRef.current.offsetX = e.clientX - posRef.current.x;
    posRef.current.offsetY = e.clientY - posRef.current.y;
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  };

  const onMouseMove = (e) => {
    if (!posRef.current.dragging) return;
    posRef.current.x = e.clientX - posRef.current.offsetX;
    posRef.current.y = e.clientY - posRef.current.offsetY;
    if (dialogRef.current) {
      dialogRef.current.style.transform = `translate(${posRef.current.x}px, ${posRef.current.y}px)`;
    }
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

  const onMouseUp = () => {
    posRef.current.dragging = false;
    window.removeEventListener('mousemove', onMouseMove);
    window.removeEventListener('mouseup', onMouseUp);
  };

  return (
    <div className="draggable-header">
    <Modal
  open={open}
  onClose={onClose}
  disablePortal
  closeOnEsc
  closeOnBackdropClick={false}
  slotProps={{
    backdrop: {
      sx: {
        backgroundColor: 'transparent', 
        backdropFilter: 'none',        
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
    onMouseDown={onMouseDown}
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
    </div>
  );
}
