let evtSource;

function startSse() {
    evtSource = new EventSource("/datafeed");

    evtSource.onmessage = function (evt) {
        const data = JSON.parse(evt.data);

        postMessage(data);
    };
}

self.onmessage = function (e) {
    switch (e.data) {
        case 'stop':
            evtSource.close();
            evtSource = null;
            return;

        case 'start':
            startSse();
            return;

        default:
            console.warn(`Command unrecognized: ${e.data}`);
            return;
    }
}