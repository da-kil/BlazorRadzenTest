// Animate toast notifications when Blazor inserts them into the DOM.
// CSS animations can be skipped when Blazor batches multiple render operations
// in a single frame, so we use a class-based CSS transition instead.
(function () {
    const observer = new MutationObserver(mutations => {
        for (const m of mutations) {
            for (const node of m.addedNodes) {
                if (node.nodeType !== 1) continue;

                // Direct match or nested match (Blazor may insert a wrapper)
                const wrappers = node.classList?.contains('rz-notification-item-wrapper')
                    ? [node]
                    : [...(node.querySelectorAll?.('.rz-notification-item-wrapper') ?? [])];

                for (const wrapper of wrappers) {
                    // Use double-rAF to guarantee the browser has painted the
                    // initial state before we add the visible class.
                    requestAnimationFrame(() => requestAnimationFrame(() => {
                        wrapper.classList.add('toast-visible');
                    }));
                }
            }
        }
    });

    observer.observe(document.body, { childList: true, subtree: true });
})();

// Downloads a file from a base64-encoded byte array (PDF, ZIP, etc.)
function downloadBase64File(base64Data, fileName, mimeType) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Uint8Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const blob = new Blob([byteNumbers], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

// Legacy data-URI download for CSV exports
function downloadFile(dataUri, fileName) {
    const link = document.createElement('a');
    link.href = dataUri;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
