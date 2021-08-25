window["metrics"] = ((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , eTPS = d.getElementById('tps')
        , eEmojiPerc = d.getElementById('emojiPerc')
        , eUrlPerc = d.getElementById('urlPerc')
        , eHtPerc = d.getElementById('htPerc')
        , ePicPerc = d.getElementById('picPerc')
        , eMentionPerc = d.getElementById('mentionPerc')
        , eTopDomains = d.getElementById('topDomains')
        , eTopPicDomains = d.getElementById('topPicDomains')
        , eTopEmojis = d.getElementById('topEmojis')
        , eTopHashtags = d.getElementById('topHashTags')
        , eTopMentions = d.getElementById('topMentions');
    
    let worker;

    function renderMetrics(e) {
        if (!e || !e.data) return;

        eTPS.innerText = e.data.tweetsPerSec;
        eTweetsReceived.innerText = e.data.tweetsReceived;
        eEmojiPerc.innerText = e.data.emojiPerc;
        eUrlPerc.innerText = e.data.urlPerc;
        ePicPerc.innerText = e.data.picPerc;
        eHtPerc.innerText = e.data.hashTagPerc;
        eMentionPerc.innerText = e.data.mentionPerc;

        const topDomains = e.data.topDomains;

        let domainHtml = '';
        for (const td of topDomains) {
            domainHtml += `<li>${td}</li>`;
        }
        eTopDomains.innerHTML = domainHtml;

        const topPicDomains = e.data.topPicDomains;
        let picHtml = '';
        for (const tpd of topPicDomains) {
            picHtml += `<li>${tpd}</li>`;
        }
        eTopPicDomains.innerHTML = picHtml;

        const topEmojis = e.data.topEmojis;
        let emojiHtml = '';
        for (const te of topEmojis) {
            emojiHtml += `<li>${te}</li>`;
        }
        eTopEmojis.innerHTML = emojiHtml;

        const topHashTags = e.data.topHashTags;
        let htHtml = '';
        for (const th of topHashTags) {
            const parts = th.split('(');
            const u = new URL(parts[0].trim());
            htHtml += `<li><a href="${u}" target="_blank">${u.pathname.split('/')[2]}</a> (${parts[1]}</li>`;
        }
        eTopHashtags.innerHTML = htHtml;

        const topMentions = e.data.topMentions;
        let mentionHtml = '';
        for (const tm of topMentions) {
            mentionHtml += `<li>${tm}</li>`;
        }
        eTopMentions.innerHTML = mentionHtml;
    }

    function stopWorker() {
        if (!worker) return;

        //  Tell our worker to stop it's event stream
        worker.postMessage('stop');
        //  Terminate our worker (which should also kill any stream)
        worker.terminate();
        //  Clean up the worker
        worker = null;
    }

    function startWorker() {
        //  Setup a new worker
        worker = new Worker('js/worker.js');
        //  Rig up our message handler
        worker.onmessage = renderMetrics;
        //  Start the firehose
        worker.postMessage('start');
    }

    startWorker();

    return {
        toggle: function () {
            if (worker)
                stopWorker();
            else
                startWorker();
        },
        dropAddUrl: function () {
            const t = event.target;

            //  Push our link update to the bottom of the event stack so that we don't update our link before we use it
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