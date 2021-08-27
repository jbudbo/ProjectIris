let evtSource;

function startSse() {
    evtSource = new EventSource("/datafeed");

    function processMessage(msg) {
        const data = JSON.parse(msg.data);

        self.postMessage(data);
    }

    evtSource.onopen = function (evt) {
        this.onmessage = processMessage;
    }
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