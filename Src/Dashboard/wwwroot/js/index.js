window["metrics"] = ((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , eTPS = d.getElementById('tps')
        , eEmojiPerc = d.getElementById('emojiPerc')
        , eUrlPerc = d.getElementById('urlPerc')
        , eHtPerc = d.getElementById('htPerc')
        , ePicPerc = d.getElementById('picPerc')
        , eTopDomains = d.getElementById('topDomains')
        , eTopPicDomains = d.getElementById('topPicDomains')
        , eTopEmojis = d.getElementById('topEmojis')
        , eTopHashtags = d.getElementById('topHashTags')
        , worker = new Worker('js/worker.js');

    worker.onmessage = function (e) {
        if (!e || !e.data) return;

        eTPS.innerText = e.data.tweetsPerSec;
        eTweetsReceived.innerText = e.data.tweetsReceived;
        eEmojiPerc.innerText = e.data.emojiPerc;
        eUrlPerc.innerText = e.data.urlPerc;
        ePicPerc.innerText = e.data.picPerc;
        eHtPerc.innerText = e.data.hashTagPerc;

        const topDomains = e.data.topDomains;
        eTopDomains.innerHTML = `<li>${topDomains[0]}</li><li>${topDomains[1]}</li><li>${topDomains[2]}</li><li>${topDomains[3]}</li><li>${topDomains[4]}</li>`;

        const topPicDomains = e.data.topPicDomains;
        eTopPicDomains.innerHTML = `<li>${topPicDomains[0]}</li><li>${topPicDomains[1]}</li><li>${topPicDomains[2]}</li><li>${topPicDomains[3]}</li><li>${topPicDomains[4]}</li>`;

        const topEmojis = e.data.topEmojis;
        eTopEmojis.innerHTML = `<li>${topEmojis[0]}</li><li>${topEmojis[1]}</li><li>${topEmojis[2]}</li><li>${topEmojis[3]}</li><li>${topEmojis[4]}</li>`;

        const topHashTags = e.data.topHashTags;
        eTopHashtags.innerHTML = `<li>${topHashTags[0]}</li><li>${topHashTags[1]}</li><li>${topHashTags[2]}</li><li>${topHashTags[3]}</li><li>${topHashTags[4]}</li>`;
    }

    return {
        dropAddUrl: function () {
            const t = event.target;

            setTimeout(() => {
                if (!t) return;

                const href = t.href;

                if (!href) return;

                const loc = new URL(href);

                t.href = loc.origin;
            }, 0);
        }
    }
})(document);