// Kods ģenerēts ar AI rīku
(() => {
    window.audioController = {
        playSound(soundName) {
            if (!soundName) {
                return;
            }

            const sanitized = String(soundName)
                .trim()
                .replace(/[^a-zA-Z0-9-_.]/g, '');

            if (!sanitized) {
                return;
            }

            const audio = new Audio(`/sounds/${sanitized}.mp3`);
            audio.volume = 0.65;
            audio.play().catch(error => console.debug("Audio playback failed:", error));
        }
    };
})();