import React from "react";
import { createRoot, Root } from "react-dom/client";
import MainModal from "./MainModal";
import { Modal } from "obsidian";

export default class ModalMounted extends Modal {
  private root: Root | null = null;
  private mountEl: HTMLElement | null = null;
  private options: { initialConnection?: "online" | "offline" } = {};

  constructor(app: any, options: { initialConnection?: "online" | "offline" } = {}) {
    super(app);
    this.options = options;
  }

  onOpen() {
    // Render into the modal's contentEl so Obsidian's modal chrome/backdrop/close works
    const target = this.contentEl as HTMLElement;

    // clear any previous content (Obsidian helper if present)
    if ((target as any).empty) {
      (target as any).empty();
    } else {
      target.innerHTML = "";
    }

    // create wrapper and mount React into the content area
    this.mountEl = document.createElement("div");
    this.mountEl.className = "obsidian-react-modal-wrapper";
    target.appendChild(this.mountEl);

    this.root = createRoot(this.mountEl);
    this.root.render(
      <MainModal initialConnection={this.options.initialConnection ?? "offline"} />
    );
  }

  onClose() {
    // unmount react tree
    if (this.root) {
      this.root.unmount();
      this.root = null;
    }

    // remove wrapper element
    if (this.mountEl && this.mountEl.parentElement) {
      this.mountEl.remove();
      this.mountEl = null;
    }

    // ensure content cleared
    if ((this.contentEl as any).empty) {
      (this.contentEl as any).empty();
    } else {
      (this.contentEl as HTMLElement).innerHTML = "";
    }
  }
}
