((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , eTPS = d.getElementById('tps')
        , eEmojiPerc = d.getElementById('emojiPerc')
        , eUrlPerc = d.getElementById('urlPerc')
        , eTopDomains = d.getElementById('topDomains')
        , worker = new Worker('js/worker.js');

    worker.onmessage = function (e) {
        if (!e || !e.data) return;

        eTPS.innerText = e.data.tweetsPerSec;
        eTweetsReceived.innerText = e.data.tweetsReceived;
        eEmojiPerc.innerText = e.data.emojiPerc;
        eUrlPerc.innerText = e.data.urlPerc;

        const topDomains = e.data.topDomains;

        eTopDomains.innerHTML = `<li>${topDomains[0]}</li><li>${topDomains[1]}</li><li>${topDomains[2]}</li><li>${topDomains[3]}</li><li>${topDomains[4]}</li>`;
    }
})(document);