import { Injectable } from '@angular/core';
import {
  PasskeyRegistrationOptions,
  PasskeyAuthenticationOptions,
  PasskeyRegistrationResponse,
  PasskeyAuthenticationResponse,
} from '../../models';

/**
 * Service for handling WebAuthn/passkey browser operations.
 * Abstracts the WebAuthn API for registration and authentication.
 */
@Injectable({
  providedIn: 'root',
})
export class WebAuthnService {
  /**
   * Check if WebAuthn is supported in the current browser
   */
  isSupported(): boolean {
    return (
      typeof window !== 'undefined' &&
      window.PublicKeyCredential !== undefined &&
      typeof window.PublicKeyCredential === 'function'
    );
  }

  /**
   * Check if platform authenticator (Touch ID, Face ID, Windows Hello) is available
   */
  async isPlatformAuthenticatorAvailable(): Promise<boolean> {
    if (!this.isSupported()) {
      return false;
    }

    try {
      return await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
    } catch {
      return false;
    }
  }

  /**
   * Check if conditional mediation (autofill) is supported
   */
  async isConditionalMediationAvailable(): Promise<boolean> {
    if (!this.isSupported()) {
      return false;
    }

    try {
      // Check if the method exists (it's relatively new in browsers)
      if ('isConditionalMediationAvailable' in PublicKeyCredential) {
        return await (PublicKeyCredential as unknown as { isConditionalMediationAvailable: () => Promise<boolean> }).isConditionalMediationAvailable();
      }
      return false;
    } catch {
      return false;
    }
  }

  /**
   * Create a new passkey (registration)
   * @param options Registration options from server
   * @returns Credential response to send back to server
   */
  async createPasskey(options: PasskeyRegistrationOptions): Promise<PasskeyRegistrationResponse> {
    if (!this.isSupported()) {
      throw new Error('WebAuthn is not supported in this browser');
    }

    const publicKeyCredentialCreationOptions: PublicKeyCredentialCreationOptions = {
      challenge: this.base64ToArrayBuffer(options.challenge),
      rp: {
        id: options.rp.id,
        name: options.rp.name,
      },
      user: {
        id: this.base64ToArrayBuffer(options.user.id),
        name: options.user.name,
        displayName: options.user.displayName,
      },
      pubKeyCredParams: options.pubKeyCredParams,
      timeout: options.timeout || 60000,
      attestation: options.attestation || 'none',
      authenticatorSelection: options.authenticatorSelection || {
        authenticatorAttachment: 'platform',
        residentKey: 'preferred',
        userVerification: 'preferred',
      },
    };

    const credential = (await navigator.credentials.create({
      publicKey: publicKeyCredentialCreationOptions,
    })) as PublicKeyCredential;

    if (!credential) {
      throw new Error('Failed to create passkey');
    }

    const response = credential.response as AuthenticatorAttestationResponse;

    return {
      id: credential.id,
      rawId: this.arrayBufferToBase64(credential.rawId),
      type: 'public-key',
      response: {
        clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON),
        attestationObject: this.arrayBufferToBase64(response.attestationObject),
        transports: response.getTransports?.() as AuthenticatorTransport[] | undefined,
      },
    };
  }

  /**
   * Authenticate with a passkey
   * @param options Authentication options from server
   * @param conditional Whether to use conditional UI (autofill)
   * @returns Authentication response to send back to server
   */
  async authenticateWithPasskey(
    options: PasskeyAuthenticationOptions,
    conditional = false
  ): Promise<PasskeyAuthenticationResponse> {
    if (!this.isSupported()) {
      throw new Error('WebAuthn is not supported in this browser');
    }

    const publicKeyCredentialRequestOptions: PublicKeyCredentialRequestOptions = {
      challenge: this.base64ToArrayBuffer(options.challenge),
      rpId: options.rpId,
      timeout: options.timeout || 60000,
      userVerification: options.userVerification || 'preferred',
      allowCredentials: options.allowCredentials?.map((cred) => ({
        type: cred.type,
        id: this.base64ToArrayBuffer(cred.id as unknown as string),
        transports: cred.transports,
      })),
    };

    const credentialRequestOptions: CredentialRequestOptions = {
      publicKey: publicKeyCredentialRequestOptions,
    };

    // Use conditional mediation for autofill UI if requested
    if (conditional) {
      (credentialRequestOptions as CredentialRequestOptions & { mediation: string }).mediation = 'conditional';
    }

    const credential = (await navigator.credentials.get(credentialRequestOptions)) as PublicKeyCredential;

    if (!credential) {
      throw new Error('Failed to authenticate with passkey');
    }

    const response = credential.response as AuthenticatorAssertionResponse;

    return {
      id: credential.id,
      rawId: this.arrayBufferToBase64(credential.rawId),
      type: 'public-key',
      response: {
        clientDataJSON: this.arrayBufferToBase64(response.clientDataJSON),
        authenticatorData: this.arrayBufferToBase64(response.authenticatorData),
        signature: this.arrayBufferToBase64(response.signature),
        userHandle: response.userHandle ? this.arrayBufferToBase64(response.userHandle) : undefined,
      },
    };
  }

  /**
   * Cancel any ongoing WebAuthn operation
   */
  async abortOngoingOperation(controller: AbortController): Promise<void> {
    controller.abort();
  }

  // ============================================
  // Private Helper Methods
  // ============================================

  /**
   * Convert base64 string to ArrayBuffer
   */
  private base64ToArrayBuffer(base64: string): ArrayBuffer {
    // Handle URL-safe base64
    const base64Standard = base64.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64Standard.length % 4)) % 4);
    const binaryString = atob(base64Standard + padding);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  /**
   * Convert ArrayBuffer to URL-safe base64 string
   */
  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    // Use URL-safe base64
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }
}
