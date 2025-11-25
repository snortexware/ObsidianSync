let ws = null;
let taskCounter = 1;

const pending = new Map();
const listeners = [];

let wsReadyResolve = null;
let wsReady = new Promise((resolve) => {
  wsReadyResolve = resolve;
});

let vaultsFetched = false;
let cachedVaults = null;

function broadcast(msg) {
  for (const cb of listeners) {
    try { cb(msg); } catch (e) { console.warn("listener error", e); }
  }
}

export function startWS(port = 5000) {
  if (ws && (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING)) return ws;

  wsReady = new Promise((resolve) => { wsReadyResolve = resolve; });

  ws = new WebSocket(`ws://localhost:${port}/gsync`);

  ws.onopen = async () => {
    console.log("WS connected");
    // resolve waiters
    if (wsReadyResolve) wsReadyResolve();
    broadcast({ type: "connection", ok: true });

    // fetch vaults once per app run
    if (!vaultsFetched) {
      try {
        await requestVaults();
      } catch (e) {
        console.warn("initial vaults request failed", e);
      }
    }
  };

  ws.onmessage = (event) => {
    let data;
    try { data = JSON.parse(event.data); } catch (e) { console.warn("invalid ws message", e); return; }

    // if response to a task
    if (data && data.taskId && pending.has(data.taskId)) {
      const resolver = pending.get(data.taskId);
      pending.delete(data.taskId);
      try { resolver(data); } catch (e) { console.warn(e); }
      return;
    }

    // broadcast general messages
    broadcast(data);
  };

  ws.onclose = () => {
    console.warn("WS closed");
    ws = null;
    broadcast({ type: "connection", ok: false });
    // keep vaultsFetched true to avoid auto refetch until you explicitly refresh
  };

  ws.onerror = (ev) => {
    console.warn("WS error", ev);
    // let onclose handle status
  };

  return ws;
}

export function onWSMessage(cb) {
  listeners.push(cb);

  // immediately inform new listener of current connection state and cached vaults
  try {
    const ok = !!(ws && ws.readyState === WebSocket.OPEN);
    cb({ type: "connection", ok });
    if (cachedVaults) cb({ type: "vaultsUpdated", data: cachedVaults });
  } catch (e) {
    // swallow
  }

  return () => {
    const idx = listeners.indexOf(cb);
    if (idx !== -1) listeners.splice(idx, 1);
  };
}

export async function sendTask(taskType, payload = {}) {
  await wsReady;

  return new Promise((resolve, reject) => {
    if (!ws) return reject(new Error("No WS connection"));
    const taskId = taskCounter++;
    const msg = { taskId, taskType, payload };
    pending.set(taskId, resolve);
    try {
      ws.send(JSON.stringify(msg));
    } catch (e) {
      pending.delete(taskId);
      reject(e);
    }
  });
}

export async function requestVaults() {
  // adapt taskType to whatever your backend expects (string 'vaults' used here)
  try {
    const res = await sendTask("vaults", {});
    // backend response shape assumed: { taskId, ok: true, data: [...] }
    if (res && res.ok) {
      vaultsFetched = true;
      cachedVaults = res.data || [];
      broadcast({ type: "vaultsUpdated", data: cachedVaults });
      return cachedVaults;
    } else {
      throw new Error("vaults request failed");
    }
  } catch (e) {
    throw e;
  }
}

export function getCachedVaults() {
  return cachedVaults;
}
