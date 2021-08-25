((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , eTPS = d.getElementById('tps')
        , eEmojiPerc = d.getElementById('emojiPerc')
        , eUrlPerc = d.getElementById('urlPerc')
        , worker = new Worker('js/worker.js');

    worker.onmessage = function (e) {
        if (!e || !e.data) return;

        eTPS.innerText = e.data.tweetsPerSec;
        eTweetsReceived.innerText = e.data.tweetsReceived;
        eEmojiPerc.innerText = e.data.emojiPerc;
        eUrlPerc.innerText = e.data.urlPerc;
    }
})(document);