const ConnectionStarter = (port = 5000) => {
  return new Promise((resolve, reject) => {
    try {
      let path = `ws://localhost:${port}/ws`;
      let wc = new WebSocket(path);

      wc.onopen = () => {
        resolve(true);
      };

      wc.onerror = (err) => {
        reject(new Error("WebSocket connection error: " + err.message));
      };

      wc.onclose = () => {
        reject(new Error("WebSocket closed unexpectedly"));
      };
    } catch (e) {
      reject(new Error("Problem trying to connect: " + e.message));
    }
  });
};

export default ConnectionStarter;
