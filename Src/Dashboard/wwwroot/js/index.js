﻿window["metrics"] = ((d) => {
    const eTweetsReceived = d.getElementById('tweetsReceived')
        , eTPS = d.getElementById('tps')

        , eEmojiPerc = d.getElementById('emojiPerc')
        , eEPT = d.getElementById('ept')
        , eTopEmojis = d.getElementById('topEmojis')

        , eUrlPerc = d.getElementById('urlPerc')
        , eUPT = d.getElementById('upt')
        , eTopDomains = d.getElementById('topDomains')

        , eHtPerc = d.getElementById('htPerc')
        , eHTPT = d.getElementById('htpt')
        , eTopHashtags = d.getElementById('topHashTags')

        , ePicPerc = d.getElementById('picPerc')
        , ePPT = d.getElementById('ppt')
        , eTopPicDomains = d.getElementById('topPicDomains')

        , eMentionPerc = d.getElementById('mentionPerc')
        , eMPT = d.getElementById('mpt')
        , eTopMentions = d.getElementById('topMentions');
    
    let worker;

    function renderMetrics(e) {
        if (!e || !e.data) return;

        eTPS.innerText = e.data.tps;
        eTweetsReceived.innerText = e.data.tweetCount;
        eEmojiPerc.innerText = e.data.emojis.tweetsWithCount / e.data.tweetCount * 100.0;
        eEPT.innerText = e.data.emojis.overallCount / e.data.emojis.tweetsWithCount;
        eUrlPerc.innerText = e.data.urls.tweetsWithCount / e.data.tweetCount * 100.0;
        eUPT.innerText = e.data.urls.overallCount / e.data.urls.tweetsWithCount;
        ePicPerc.innerText = e.data.images.tweetsWithCount / e.data.tweetCount * 100.0;
        ePPT.innerText = e.data.images.overallCount / e.data.images.tweetsWithCount;
        eHtPerc.innerText = e.data.hashTags.tweetsWithCount / e.data.tweetCount * 100.0;
        eHTPT.innerText = e.data.hashTags.overallCount / e.data.hashTags.tweetsWithCount;
        eMentionPerc.innerText = e.data.mentions.tweetsWithCount / e.data.tweetCount * 100.0;
        eMPT.innerText = e.data.mentions.overallCount / e.data.mentions.tweetsWithCount;

        const topDomains = e.data.urls.entityTexts;
        let domainHtml = '';
        for (const td of topDomains) {
            domainHtml += `<li>${td}</li>`;
        }
        eTopDomains.innerHTML = domainHtml;

        const topPicDomains = e.data.images.entityTexts;
        let picHtml = '';
        for (const tpd of topPicDomains) {
            picHtml += `<li>${tpd}</li>`;
        }
        eTopPicDomains.innerHTML = picHtml;

        const topEmojis = e.data.emojis.entityTexts;
        let emojiHtml = '';
        for (const te of topEmojis) {
            emojiHtml += `<li>${te}</li>`;
        }
        eTopEmojis.innerHTML = emojiHtml;

        const topHashTags = e.data.hashTags.entityTexts;
        let htHtml = '';
        for (const th of topHashTags) {
            htHtml += `<li>${th}</li>`;
        }
        eTopHashtags.innerHTML = htHtml;

        const topMentions = e.data.mentions.entityTexts;
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