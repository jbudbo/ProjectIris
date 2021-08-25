((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , worker = new Worker('js/worker.js');

    worker.onmessage = function (e) {
        if (!e || !e.data) return;

        eTweetsReceived.innerText = e.data.tweetsReceived;
    }
})(document);