// ─── Google GSI type declarations ────────────────────────────────────────────

declare namespace google.accounts.id {
    interface CredentialResponse { 
        credential: string;
    }

    interface IdConfiguration {
        client_id: string;
        callback: (response: CredentialResponse) => void;
    }

    interface GsiButtonConfiguration {
        theme?: "outline" | "filled_blue" | "filled_black";
        size?: "large" | "medium" | "small";
        width?: string | number;
        text?: "signin_with" | "signup_with" | "continue_with" | "signin";
        locale?: string;
    }

    function initialize(config: IdConfiguration): void;
    function renderButton(
        element: HTMLElement | null,
        options: GsiButtonConfiguration
    ): void;
}

// ─── Blazor DotNet reference type ────────────────────────────────────────────

interface DotNetObjectReference {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

// ─── Window augmentation ──────────────────────────────────────────────────────

interface Window {
    handleGoogleCredential: (response: google.accounts.id.CredentialResponse) => void;
    initGoogleSignIn: (clientId: string, dotNetRef: DotNetObjectReference) => void;
    blazorGoogleCallback: ((credential: string) => void) | undefined;
}

// ─── Google GSI callback (called by Google's script) ─────────────────────────

window.handleGoogleCredential = (response: google.accounts.id.CredentialResponse): void => {
    if (window.blazorGoogleCallback) {
        window.blazorGoogleCallback(response.credential);
    }
};

// ─── Initialize Google Sign-In button (called from Blazor via JSInterop) ─────

window.initGoogleSignIn = (clientId: string, dotNetRef: DotNetObjectReference): void => {
    if (typeof google === "undefined" || !google.accounts) {
        console.warn("Google GSI script not loaded yet.");
        return;
    }

    window.blazorGoogleCallback = (credential: string): void => {
        dotNetRef.invokeMethodAsync("OnGoogleCredentialReceived", credential);
    };

    google.accounts.id.initialize({
        client_id: clientId,
        callback: window.handleGoogleCredential,
    });

    google.accounts.id.renderButton(
        document.getElementById("google-signin-btn"),
        { theme: "outline", size: "large", width: "100%", text: "continue_with", locale: "es" }
    );
};
