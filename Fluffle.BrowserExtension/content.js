const renderCheckInterval = 500;

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

function handleReverseSearch(request) {
    // Fluffle uses React as its front-end framework. The entire page might not have
    // been rendered yet even though all the files have been loaded. Here we wait for
    // React to render everything on the page before submitting the image for reverse searching.
    (function reverseSearch() {
        let fileField = document.getElementById('image-data-url');
        if (fileField == null) {
            setTimeout(reverseSearch, renderCheckInterval);
            return;
        }
        fileField.value = request.data;

        let submitButton = document.getElementById('programmatic-submit');
        submitButton.click();
    })();
}

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        if (request.id === 'download-blob') {
            handleDownloadBlob(request);
        } else if (request.id === 'reverse-search') {
            handleReverseSearch(request);
        }
    }
);
