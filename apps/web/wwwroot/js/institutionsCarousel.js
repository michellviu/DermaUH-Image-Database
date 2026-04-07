window.institutionsCarousel = (() => {
    const carousels = new Map();

    function getGapPx(track) {
        const styles = window.getComputedStyle(track);
        const value = styles.columnGap || styles.gap || "0px";
        const parsed = Number.parseFloat(value);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function getItemStepPx(item, track) {
        if (!item) {
            return 0;
        }

        const rect = item.getBoundingClientRect();
        return rect.width + getGapPx(track);
    }

    function init(regionId, trackId) {
        dispose(regionId);

        const region = document.getElementById(regionId);
        const track = document.getElementById(trackId);

        if (!region || !track || track.children.length < 2) {
            return;
        }

        const state = {
            region,
            track,
            frameId: 0,
            lastTs: 0,
            offset: 0,
            pointerX: null,
            pointerInRegion: false,
            baseSpeedPxPerSec: 22,
            maxBoostPxPerSec: 210,
            edgeZoneRatio: 0.18,
        };

        const onMouseMove = (event) => {
            state.pointerInRegion = true;
            state.pointerX = event.clientX;
        };

        const onMouseLeave = () => {
            state.pointerInRegion = false;
            state.pointerX = null;
        };

        const onResize = () => {
            state.offset = 0;
            state.track.style.transform = "translate3d(0px, 0, 0)";
        };

        function getCurrentSpeed() {
            const base = state.baseSpeedPxPerSec;
            if (!state.pointerInRegion || state.pointerX === null) {
                return -base;
            }

            const rect = state.region.getBoundingClientRect();
            if (rect.width <= 0) {
                return -base;
            }

            const edgeZone = rect.width * state.edgeZoneRatio;
            const x = state.pointerX - rect.left;

            if (x <= edgeZone) {
                const proximity = 1 - Math.max(0, x) / edgeZone;
                const boost = state.maxBoostPxPerSec * proximity * proximity;
                return base + boost;
            }

            if (x >= rect.width - edgeZone) {
                const distance = Math.max(0, rect.width - x);
                const proximity = 1 - distance / edgeZone;
                const boost = state.maxBoostPxPerSec * proximity * proximity;
                return -(base + boost);
            }

            return -base;
        }

        function reorderForInfiniteLoop(speed) {
            if (speed < 0) {
                let first = state.track.firstElementChild;
                let firstStep = getItemStepPx(first, state.track);

                while (first && firstStep > 0 && -state.offset >= firstStep) {
                    state.offset += firstStep;
                    state.track.appendChild(first);
                    first = state.track.firstElementChild;
                    firstStep = getItemStepPx(first, state.track);
                }
                return;
            }

            if (speed > 0) {
                let last = state.track.lastElementChild;
                let lastStep = getItemStepPx(last, state.track);

                while (last && lastStep > 0 && state.offset > 0) {
                    state.offset -= lastStep;
                    state.track.insertBefore(last, state.track.firstElementChild);
                    last = state.track.lastElementChild;
                    lastStep = getItemStepPx(last, state.track);
                }
            }
        }

        function animate(ts) {
            if (!state.lastTs) {
                state.lastTs = ts;
            }

            const deltaSec = Math.min((ts - state.lastTs) / 1000, 0.05);
            state.lastTs = ts;

            const speed = getCurrentSpeed();
            state.offset += speed * deltaSec;
            reorderForInfiniteLoop(speed);
            state.track.style.transform = `translate3d(${state.offset}px, 0, 0)`;

            state.frameId = window.requestAnimationFrame(animate);
        }

        region.addEventListener("mousemove", onMouseMove);
        region.addEventListener("mouseleave", onMouseLeave);
        window.addEventListener("resize", onResize);

        state.dispose = () => {
            if (state.frameId) {
                window.cancelAnimationFrame(state.frameId);
                state.frameId = 0;
            }

            region.removeEventListener("mousemove", onMouseMove);
            region.removeEventListener("mouseleave", onMouseLeave);
            window.removeEventListener("resize", onResize);
            state.track.style.transform = "";
        };

        state.frameId = window.requestAnimationFrame(animate);
        carousels.set(regionId, state);
    }

    function recalculate(regionId) {
        const state = carousels.get(regionId);
        if (!state) {
            return;
        }

        state.offset = 0;
        state.lastTs = 0;
        state.track.style.transform = "translate3d(0px, 0, 0)";
    }

    function dispose(regionId) {
        const existing = carousels.get(regionId);
        if (!existing) {
            return;
        }

        existing.dispose();
        carousels.delete(regionId);
    }

    return {
        init,
        recalculate,
        dispose,
    };
})();
