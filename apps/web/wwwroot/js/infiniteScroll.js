window.infiniteScroll = window.infiniteScroll || (() => {
    const observers = new Map();

    function init(dotNetRef, sentinelId) {
        const sentinel = document.getElementById(sentinelId);
        if (!sentinel || observers.has(sentinelId)) {
            return;
        }

        const observer = new IntersectionObserver(
            (entries) => {
                for (const entry of entries) {
                    if (entry.isIntersecting) {
                        dotNetRef.invokeMethodAsync('OnInfiniteScrollTrigger');
                        break;
                    }
                }
            },
            {
                root: null,
                rootMargin: '280px',
                threshold: 0
            }
        );

        observer.observe(sentinel);
        observers.set(sentinelId, observer);
    }

    function dispose(sentinelId) {
        const observer = observers.get(sentinelId);
        if (observer) {
            observer.disconnect();
            observers.delete(sentinelId);
        }
    }

    return {
        init,
        dispose
    };
})();
