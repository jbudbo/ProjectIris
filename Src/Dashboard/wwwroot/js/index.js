(() => {
    const evtSource = new EventSource("/datafeed");

    evtSource.onmessage = function (evt) {

    };
})();