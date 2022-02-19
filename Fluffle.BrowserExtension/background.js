// Where Fluffle its web UI is hosted
const fluffleUrl = 'https://fluffle.xyz/browser-extension';

// Register the reverse search context menu item
const contextMenuId = 'reverse-search-using-fluffle';
chrome.contextMenus.create({
    id: contextMenuId,
    title: 'Reverse search using Fluffle',
    contexts: ['image']
});

// We need to check if the loaded page is Fluffle. If it is, then we 
// reverse search the queued reverse search request. 
chrome.tabs.onUpdated.addListener(function (tabId, info, tab) {
    // Check if the tab has finished loading
    if (info.status !== 'complete') {
        return;
    }

    // Check if the tab its URL equals that of Fluffle
    if (!tab.url.startsWith(fluffleUrl)) {
        return;
    }

    // Get the reverse search request from local storage
    chrome.storage.local.get('reverseSearchRequest', item => {
        // Check if the request exists
        if (!('reverseSearchRequest' in item)) {
            return;
        }

        const reverseSearchRequest = item.reverseSearchRequest;
        chrome.storage.local.remove('reverseSearchRequest', () => {
            // Fetch the image, read it as a data uri, and signal the content script to submit it to Fluffle
            if (reverseSearchRequest.type === 'url') {
                fetch(reverseSearchRequest.data).then(response => response.blob()).then(blob => {
                    let reader = new FileReader();

                    reader.onloadend = () => {
                        chrome.tabs.sendMessage(tabId, {
                            id: 'reverse-search',
                            data: reader.result
                        });
                    };

                    reader.readAsDataURL(blob);
                });
            } else if (reverseSearchRequest.type === 'dataUri') {
                // We already got the image to reverse search, signal the content script to submit it to Fluffle
                chrome.tabs.sendMessage(tabId, {
                    id: 'reverse-search',
                    data: reverseSearchRequest.data
                });
            }
        });
    });
});

// Open Fluffle in a tab on the right side of the currently opened tab
function openFluffle() {
    chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
        chrome.tabs.create({
            url: fluffleUrl,
            index: tabs[0].index + 1
        });
    });
}

// Handles clicks from the context menu
chrome.contextMenus.onClicked.addListener((info, tab) => {
    if (info.menuItemId !== contextMenuId) {
        return;
    }

    // We can't get the data of a blob on a different tab when the service worker is executing on Fluffle.
    // Therefore we signal the content script to get it for us.
    if (info.srcUrl.startsWith('blob:')) {
        chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
            chrome.tabs.sendMessage(tabs[0].id, {
                id: 'download-blob',
                url: info.srcUrl
            });
        });

        return;
    }

    // Open Fluffle to reverse search the image
    chrome.storage.local.set({ reverseSearchRequest: { type: info.srcUrl.startsWith('data:') ? 'dataUri' : 'url', data: info.srcUrl } }, () => {
        openFluffle();
    });
});

// Open Fluffle to reverse search the image after the blob has been downloaded
function blobDownloaded(request) {
    chrome.storage.local.set({ reverseSearchRequest: { type: 'dataUri', data: request.data } }, () => {
        openFluffle();
    });
}

chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    if (request.id === 'blob-downloaded') {
        blobDownloaded(request);
    }
});
