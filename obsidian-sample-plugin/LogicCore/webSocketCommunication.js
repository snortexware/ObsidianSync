const ConnectionStarter = (port = 5000) =>
{
    return new Promise((accepted, rejected) => {
        try {
            let path = `ws://localhost:${port}/ws`
            let wc = new WebSocket(path);
        
            if(wc.readyState !== wc.OPEN && wc.readyState !== wc.CONNECTING){
                rejected(new Error("Connection not made. check if the application is running!"));
            }
        
        wc.onopen(() => {
                accepted(true)
            });

        wc.onerror(() => {
                rejected(new Error());
        })
        
        wc.onclose(() => {
            rejected(false);
        })
            
        return true;

        } catch(e)
        {
            rejected(new Error("Problem trying to connect: " + e.message));
        }
    })
    

}
export default ConnectionStarter;