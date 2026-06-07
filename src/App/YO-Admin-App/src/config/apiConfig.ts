import {
    BaseQueryFn,
    FetchArgs,
    fetchBaseQuery,
    FetchBaseQueryError,
} from "@reduxjs/toolkit/query";
import { jwtDecode } from "jwt-decode";
import { ITokenInfo, ITokenResponse } from "../types";
import { BaseEndpoints } from "./BaseEndpoints";
import { unwrapApiResponse } from "../security/apiPayloadSecurity";

/**
 * Get a client-credentials token from IdentityServer
 */
const getAppToken = async (): Promise<ITokenResponse> => {
    const res = await fetch(`${BaseEndpoints.base}/connect/token`, {
        method: "POST",
        mode: "cors",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
        },
        body: new URLSearchParams({
            client_id: import.meta.env.VITE_API_CLIENTID,
            client_secret: import.meta.env.VITE_API_SECRET,
            grant_type: "client_credentials",
        }),
    });

    if (!res.ok) {
        throw new Error(`Failed to fetch token: ${res.status} ${res.statusText}`);
    }

    const data: ITokenResponse = await res.json();
    console.log("Fetched new token:", data);
    return data;
};

// Module-level pending promise used to serialize token fetches so multiple
// concurrent API requests don't trigger parallel token calls. When a token
// request is already in progress other callers will await the same promise.
let pendingTokenPromise: Promise<ITokenResponse> | null = null;

const fetchTokenIfNeeded = async (): Promise<string | null> => {
    // If a token fetch is already in progress, wait for it
    if (pendingTokenPromise) {
        try {
            const data = await pendingTokenPromise;
            return data?.access_token ?? null;
        } catch (err) {
            // ensure we clear pending on error so next caller can retry
            pendingTokenPromise = null;
            throw err;
        }
    }

    // Start a new token fetch and store the promise so others can await it
    pendingTokenPromise = getAppToken();
    try {
        const data = await pendingTokenPromise;
        pendingTokenPromise = null;
        return data?.access_token ?? null;
    } catch (err) {
        pendingTokenPromise = null;
        throw err;
    }
};

const setAccessToken = (token: string) => {
    try {
        localStorage.setItem("token", token);
    } catch (e) {
        console.warn("Unable to persist token to localStorage:", e);
    }
};

const getAccessToken = (): string | null => {
    try {
        const accessToken = localStorage.getItem("token");
        return accessToken || null;
    } catch {
        return null;
    }
};

const removeAccessToken = () => {
    try {
        localStorage.removeItem("token");
    } catch (e) {
        console.warn("Unable to remove token from localStorage:", e);
    }
};

/**
 * Plain base query — no auth headers, no token fetching.
 * Used only for endpoints that obtain tokens (login, token exchange).
 */
export const baseQueryWithoutAuth: BaseQueryFn<
    string | FetchArgs,
    unknown,
    FetchBaseQueryError
> = async (args, api, extraOptions) => {
    const baseQuery = fetchBaseQuery({
        baseUrl: BaseEndpoints.base,
    });
    const fetchArgs: FetchArgs =
        typeof args === "string" ? { url: args } : { ...args };
    const result = await baseQuery(fetchArgs, api, extraOptions);

    if (result.data) {
        result.data = await unwrapApiResponse(result.data);
    }

    return result;
};

export const baseQueryWithAuth: BaseQueryFn<
    string | FetchArgs,
    unknown,
    FetchBaseQueryError
> = async (args, api, extraOptions) => {
    const baseQuery = fetchBaseQuery({
        baseUrl: BaseEndpoints.base,
    });

    // ---- 1) Get current token (if any) ----
    let tokenToUse: string | null = getAccessToken();

    if (tokenToUse) {
        try {
            const currentTokenInfo = jwtDecode(tokenToUse) as ITokenInfo;
            const currentTime = Math.floor(Date.now() / 1000);

            if (
                currentTokenInfo.exp &&
                currentTokenInfo.exp < currentTime + 60
            ) {
                console.log("Token expired / expiring soon, refreshing...");
                removeAccessToken();
                tokenToUse = null;
            }
        } catch (error) {
            console.error("Error decoding existing token:", error);
            removeAccessToken();
            tokenToUse = null;
        }
    }

    // ---- 2) Fetch new token if needed ----
    if (!tokenToUse) {
        try {
            const accessToken = await fetchTokenIfNeeded();
            if (accessToken) {
                setAccessToken(accessToken);
                tokenToUse = accessToken;
            } else {
                throw new Error("No access_token in token response");
            }
        } catch (error) {
            console.error("Error getting new token:", error);
            return {
                error: {
                    status: 401,
                    data: { message: "Unable to obtain access token" },
                } as FetchBaseQueryError,
            };
        }
    }

    // ---- 3) Build request args & add Authorization header ----
    const fetchArgs: FetchArgs =
        typeof args === "string" ? { url: args } : { ...args };

    const existingHeaders: Record<string, string> =
        fetchArgs.headers instanceof Headers
            ? Object.fromEntries(fetchArgs.headers.entries())
            : (fetchArgs.headers as Record<string, string>) || {};

    fetchArgs.headers = {
        ...existingHeaders,
        Authorization: `Bearer ${tokenToUse}`,
    };

    // ---- 4) Call underlying baseQuery ----
    const result = await baseQuery(fetchArgs, api, extraOptions);

    // ---- 4.5) Unwrap protected API payload (obfuscation/encryption) ----
    if (result.data) {
        result.data = await unwrapApiResponse(result.data);
    }

    // ---- 5) Handle { Code, Message, Data } envelope ----
    if (result.data && typeof result.data === "object" && "Code" in result.data) {
        const code = (result.data as any).Code;
        if (typeof code === "number" && (code < 200 || code >= 300)) {
            return {
                error: {
                    status: code,
                    data: result.data,
                },
            };
        }
    }

    return result;
};
