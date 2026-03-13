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
