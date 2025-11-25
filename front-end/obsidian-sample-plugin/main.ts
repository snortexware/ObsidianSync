import { App, Editor, MarkdownView, Modal, Notice, Plugin, PluginSettingTab, Setting } from 'obsidian';
import ModalMounted from './src/ModalMounted';
import { startWS, onWSMessage } from './LogicCore/webSocketCommunication';

export default class MyPlugin extends Plugin {
	private _unsubWS: (() => void) | null = null;
	private statusEl: HTMLElement | null = null;

	async onload() {
		const stored: any = await this.loadData();
		const initialConnectionOk = !!(stored && stored.connectionOk);

		// status bar item
		this.statusEl = this.addStatusBarItem();
		const setStatus = (ok: boolean) => {
			if (!this.statusEl) return;
			this.statusEl.innerText = ok ? 'Google sync: online' : 'Google sync: offline';
			this.statusEl.style.color = ok ? 'limegreen' : 'orange';
		};
		setStatus(initialConnectionOk);

		// Start background WS (idempotent)
		startWS();

		// Keep plugin state in sync with WS messages
		this._unsubWS = onWSMessage(async (msg: any) => {
			if (!msg) return;

			if (msg.type === 'connection') {
				const ok = !!msg.ok;
				await this.saveData({ connectionOk: ok });
				setStatus(ok);
				if (!ok) new Notice('Sync service disconnected');
			}

			// handle other messages if needed (vaultsUpdated, etc.)
		});

		// Remove any previously injected custom styles (cleanup)
		const prevStyle = document.getElementById('obsidian-drive-ribbon-style');
		if (prevStyle) prevStyle.remove();

		// ribbon / open modal — restore Obsidian's default cloud icon (no custom SVG)
		this.addRibbonIcon('cloudy', 'Google cloud sync', (evt: MouseEvent) => {
			new ModalMounted(this.app, { initialConnection: initialConnectionOk ? "online" : "offline" }).open();
		});
	}

	onunload() {
		if (this._unsubWS) {
			this._unsubWS();
			this._unsubWS = null;
		}
	}
}
