mergeInto(LibraryManager.library, {
    GoToURLInSameTab: function (url) {
        if (typeof window !== "undefined") {
            const urlParams = new URLSearchParams(window.location.search);
            const device_type = urlParams.get("device_type");

            if (device_type === "Android") {
                window.JSBridge.receivedFromJS("GameOver");
            } else if (device_type === "ios") {
                window.webkit.messageHandlers.jsHandler.postMessage("GameOver");
            } else {
                window.location.reload();
            }
        }
    },

  broadcastCustom: function (ptr) {
    const message = UTF8ToString(ptr);
    console.log("broadcastCustom");
    console.log("Message type: " + typeof message);
    console.log(message);
    if (typeof window !== "undefined") {
        if (window.JSBridge && window.JSBridge.receivedFromJS) {
            // Android
            window.JSBridge.receivedFromJS(message);
        } else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.jsHandler) {
            // iOS
            window.webkit.messageHandlers.jsHandler.postMessage({ type: 'custom', message });
        } else if (window.self !== window.top) {
            // Web
            window.parent.postMessage({ type: 'custom', message }, '*');
             console.log('Message sent1');
        } else if (typeof window !== "undefined" && typeof window.document !== "undefined") {
            window.parent.postMessage({ type: 'custom', message }, '*');
            console.log('Message sent');
        } else {
            console.log('Could not post message to parent');
        }
    }
},
    initializeMessageListener: function () {
        if (typeof window !== "undefined" && this.isBrowser()) {
            console.log("Running in a browser.");

            window.addEventListener('message', (event) => {
                if (event.origin !== runtime.globalVars.ParentOrigin) {
                    return;
                }
                if (!event.data.type) {
                    return;
                }
                switch (event.data.type) {
                    case 'adHidden':
                        runtime.globalVars.gamePaused = false;
                        break;
                    default:
                        console.warn(`Unhandled message type: ${event.data.type}`);
                        break;
                }
            });
        } else {
            console.log("Not running in a browser.");
        }
    }
});

// Assign the broadcastScoreChange function to the global window object, but only if in a browser
if (typeof window !== "undefined") {
    window.broadcastScoreChange = function (message) {
        if (LibraryManager.library && LibraryManager.library.postMessageToParent) {
            LibraryManager.library.postMessageToParent({ type: 'scoreChange', message });
        }
    };

    // Initialize the message listener
    LibraryManager.library.initializeMessageListener();
}
