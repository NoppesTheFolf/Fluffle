const renderCheckInterval = 250;

// Blobs can only be read from the content script, not from within the service worker
function handleDownloadBlob(request) {
    fetch(request.url).then(response => response.blob()).then(blob => {
        let reader = new FileReader();

        reader.onloadend = () => {
            chrome.runtime.sendMessage({ id: 'blob-downloaded', data: reader.result });
        };

        reader.readAsDataURL(blob);
    })
}

function getElementById(id, cb) {
    // Fluffle uses React as its front-end framework. The entire page might not have
    // been rendered yet even though all the files have been loaded. Here we wait for
    // React to render everything on the page before submitting the image for reverse searching.
    (function _getElementById() {
        let element = document.getElementById(id);
        if (element == null) {
            setTimeout(_getElementById, renderCheckInterval);
            return;
        }

        cb(element);
    })();
}

function programmaticSubmit() {
    getElementById('programmatic-submit', submitButton => {
        submitButton.click();
    });
}

function handleReverseSearch(request) {
    getElementById('image-data-url', dataUrlField => {
        dataUrlField.value = request.data;

        programmaticSubmit();
    });
}

function handleNothingQueued(request) {
    programmaticSubmit();
}

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        if (request.id === 'download-blob') {
            handleDownloadBlob(request);
        } else if (request.id === 'reverse-search') {
            handleReverseSearch(request);
        } else if (request.id === 'nothing-queued') {
            handleNothingQueued(request);
        }
    }
);
