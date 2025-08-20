import { App, Modal } from "obsidian";
import { createRoot, Root } from "react-dom/client";
import MainModal from "./MainModal";
import Grid from '@mui/joy/Grid';

export default class ModalMounted extends Modal {
  private root: Root | null = null;

  constructor(app: App) {
    super(app);
    this.setTitle("Google cloud sync");
  }

  onOpen() {
    const mount = this.contentEl.createDiv();
    this.root = createRoot(mount);
    this.root.render(<MainModal />);
  }

  onClose() {
    if (this.root) {
      this.root.unmount();
      this.root = null;
    }
    this.contentEl.empty();
  }
}
