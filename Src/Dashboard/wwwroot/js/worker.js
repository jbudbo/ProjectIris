const evtSource = new EventSource("/datafeed");

evtSource.onmessage = function (evt) {
    const data = JSON.parse(evt.data);

    postMessage(data);
};
