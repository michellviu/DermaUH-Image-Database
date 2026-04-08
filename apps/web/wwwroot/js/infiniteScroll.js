window.infiniteScroll = window.infiniteScroll || (() => {
    const observers = new Map();

    function findScrollRoot(element) {
        let current = element?.parentElement;

        while (current) {
            const style = window.getComputedStyle(current);
            const overflowY = style.overflowY;
            const isScrollable = (overflowY === 'auto' || overflowY === 'scroll') && current.scrollHeight > current.clientHeight;

            if (isScrollable) {
                return current;
            }

            current = current.parentElement;
        }

        return null;
    }

    function shouldTrigger(state) {
        const sentinel = document.getElementById(state.sentinelId);
        if (!sentinel) {
            return false;
        }

        const preloadOffset = 320;
        const sentinelRect = sentinel.getBoundingClientRect();

        if (state.root) {
            const rootRect = state.root.getBoundingClientRect();
            return sentinelRect.top <= rootRect.bottom + preloadOffset;
        }

        return sentinelRect.top <= window.innerHeight + preloadOffset;
    }

    function triggerLoad(state) {
        if (state.loading) {
            return;
        }

        state.loading = true;
        Promise.resolve(state.dotNetRef.invokeMethodAsync('OnInfiniteScrollTrigger'))
            .catch(() => {
                // Ignore callback errors on disconnected circuits.
            })
            .finally(() => {
                state.loading = false;
            });
    }

    function scheduleCheck(state) {
        if (!state || state.scheduled) {
            return;
        }

        state.scheduled = true;
        requestAnimationFrame(() => {
            state.scheduled = false;
            if (shouldTrigger(state)) {
                triggerLoad(state);
            }
        });
    }

    function init(dotNetRef, sentinelId) {
        const sentinel = document.getElementById(sentinelId);
        if (!sentinel || observers.has(sentinelId)) {
            return;
        }

        const root = findScrollRoot(sentinel);

        const state = {
            dotNetRef,
            sentinelId,
            root,
            loading: false,
            scheduled: false,
            observer: null,
            onScroll: null,
            scrollTarget: root || window
        };

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
                root,
                rootMargin: '320px',
                threshold: 0
            }
        );

        observer.observe(sentinel);
        state.observer = observer;

        state.onScroll = () => scheduleCheck(state);
        state.scrollTarget.addEventListener('scroll', state.onScroll, { passive: true });
        window.addEventListener('resize', state.onScroll, { passive: true });

        observers.set(sentinelId, state);
        scheduleCheck(state);
    }

    function check(sentinelId) {
        const state = observers.get(sentinelId);
        if (!state) {
            return;
        }

        scheduleCheck(state);
    }

    function dispose(sentinelId) {
        const state = observers.get(sentinelId);
        if (state) {
            state.observer?.disconnect();

            if (state.onScroll) {
                state.scrollTarget.removeEventListener('scroll', state.onScroll);
                window.removeEventListener('resize', state.onScroll);
            }

            observers.delete(sentinelId);
        }
    }

    return {
        init,
        check,
        dispose
    };
})();
