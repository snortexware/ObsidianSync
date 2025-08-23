let ws;
const listeners = [];

export const startWS = (port = 5000) => {
  if (ws && ws.readyState === WebSocket.OPEN) return ws; 

  ws = new WebSocket(`ws://localhost:${port}/ws`);

  ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    listeners.forEach((cb) => cb(data));
  };

  ws.onclose = () => {
    console.warn("WebSocket closed. Try reconnecting later.");
  };

  return ws;
};

export const sendNewConfig = (configObj) => 
  {
    if(ws.readyState == ws.OPEN){
      ws.send(configObj);
    }
    else{
      throw new Error("WebSocket is closed!")
    }
}

export const sign = (callback) => {
  listeners.push(callback);
  return () => {
    const idx = listeners.indexOf(callback);
    if (idx !== -1) listeners.splice(idx, 1);
  };
};
