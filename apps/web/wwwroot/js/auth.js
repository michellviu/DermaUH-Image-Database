"use strict";
// ─── Google GSI type declarations ────────────────────────────────────────────
// ─── Google GSI callback (called by Google's script) ─────────────────────────
window.handleGoogleCredential = (response) => {
    if (window.blazorGoogleCallback) {
        window.blazorGoogleCallback(response.credential);
    }
};
// ─── Initialize Google Sign-In button (called from Blazor via JSInterop) ─────
window.initGoogleSignIn = (clientId, dotNetRef) => {
    if (typeof google === "undefined" || !google.accounts) {
        console.warn("Google GSI script not loaded yet.");
        return;
    }
    window.blazorGoogleCallback = (credential) => {
        dotNetRef.invokeMethodAsync("OnGoogleCredentialReceived", credential);
    };
    google.accounts.id.initialize({
        client_id: clientId,
        callback: window.handleGoogleCredential,
    });
    google.accounts.id.renderButton(document.getElementById("google-signin-btn"), { theme: "outline", size: "large", width: "100%", text: "continue_with", locale: "es" });
};
