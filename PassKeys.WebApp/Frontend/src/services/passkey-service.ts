import {
    create,
    get,
    parseCreationOptionsFromJSON,
    parseRequestOptionsFromJSON
} from "@github/webauthn-json/browser-ponyfill";
import {sessionStorageService} from "./session-storage-service.ts";
import {SessionConstants} from "../../constants.ts";

class PassKeyService {
    async createCredentialOptions(userName: string): Promise<{ options: CredentialCreationOptions, userId: string }> {
        const response = await fetch('/api/fido2/credential-options', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userName)
        });
        if (response.status == 400) {
            throw new Error(await response.text());
        }
        const credentialOptionsResponse = await response.json() as { options: any, userId: string }
        const options = credentialOptionsResponse.options
        const abortController = new AbortController();
        return {
            options: parseCreationOptionsFromJSON({publicKey: options, signal: abortController.signal}),
            userId: credentialOptionsResponse.userId
        }
    }

    async createCredentialOptionsForCurrentUser(): Promise<{ options: CredentialCreationOptions, userId: string }> {
        const token = sessionStorageService.get<string>(SessionConstants.TokenKey);
        if (token === null) {
            throw new Error("Token expired!");
        }
        const response = await fetch('/api/fido2/credential-options', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
        });
        if (response.status == 400) {
            throw new Error(await response.text());
        }
        const credentialOptionsResponse = await response.json() as { options: any, userId: string }
        const options = credentialOptionsResponse.options
        const abortController = new AbortController();
        return {
            options: parseCreationOptionsFromJSON({publicKey: options, signal: abortController.signal}),
            userId: credentialOptionsResponse.userId
        }
    }

    async createCredential(userId: string, options: CredentialCreationOptions) {
        const attestationResponse = await create(options);
        const credentialResponse = await fetch('/api/fido2/credential', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'User-Agent': navigator.userAgent,
            },
            body: JSON.stringify({
                attestationResponse: attestationResponse.toJSON(),
                userId: userId
            })
        });
        if (credentialResponse.status == 400) {
            throw new Error(await credentialResponse.text());
        }
        return await credentialResponse.json();
    }

    async createAssertionOptions(userName: string): Promise<{ options: CredentialRequestOptions, userId: string }> {
        const response = await fetch('/api/fido2/assertion-options', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userName)
        });
        if (response.status == 400) {
            throw new Error(await response.text());
        }
        const assertionOptionsResponse = await response.json() as { userId: string, assertionOptions: any }
        const abortController = new AbortController();
        return {
            options: parseRequestOptionsFromJSON({
                publicKey: assertionOptionsResponse.assertionOptions,
                signal: abortController.signal
            }),
            userId: assertionOptionsResponse.userId
        }
    }

    async verifyAssertion(userId: string, options: CredentialRequestOptions) {
        const isConditionalMediationAvailable = (PublicKeyCredential && await PublicKeyCredential.isConditionalMediationAvailable());
        if (!isConditionalMediationAvailable) {
            throw new Error('Mediation is not supported :(');
        }
        const assertionResponse = await get(options);
        const verificationResponse = await fetch('/api/fido2/assertion', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'User-Agent': navigator.userAgent,
            },
            body: JSON.stringify({
                assertionRawResponse: assertionResponse.toJSON(),
                userId: userId
            })
        });
        return await verificationResponse.json();
    }

    async revokeCredential(credentialId: string) {
        const token = sessionStorageService.get<string>(SessionConstants.TokenKey);
        if (token === null) {
            throw new Error("Token expired!");
        }
        const response = await fetch('/api/fido2/credential', {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(credentialId)
        });
        if (response.status != 204) {
            throw new Error(await response.text());
        }
    }
}

const passKeyService = new PassKeyService();
export {passKeyService}