declare module 'base64url-arraybuffer' {
    export function encode(arraybuffer: ArrayBuffer): string;
    export function decode(base64: string, dontValidate?: boolean): ArrayBuffer;
}