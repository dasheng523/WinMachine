import type { ITelemetryClient, TelemetryPacket } from './types';
import { TelemetryObservable } from './TelemetryObservable';

export class WebSocketTelemetryClient implements ITelemetryClient {
    public observable = new TelemetryObservable();
    private socket: WebSocket | null = null;
    public isConnected = false;
    private url: string = '';
    private reconnectTimer: number | null = null;
    private isManualClose = false;

    connect(url: string): void {
        this.isManualClose = false;
        this.url = url;
        this.doConnect();
    }

    private doConnect(): void {
        // Prepare to store the current socket we are about to create
        if (this.socket) {
            if (this.socket.readyState === WebSocket.CONNECTING || this.socket.readyState === WebSocket.OPEN) {
                return;
            }
            this.socket.close();
        }

        console.log(`[WS] Connecting to ${this.url}...`);
        const currentSocket = new WebSocket(this.url);
        this.socket = currentSocket;

        currentSocket.onopen = () => {
            // Guard: Only proceed if this is still the active socket
            if (this.socket !== currentSocket) return;

            console.log('[WS] Connection established successfully.');
            this.isConnected = true;
            if (this.reconnectTimer) {
                clearTimeout(this.reconnectTimer);
                this.reconnectTimer = null;
            }
        };

        currentSocket.onmessage = (event) => {
            if (this.socket !== currentSocket) return;
            try {
                const packet: TelemetryPacket = JSON.parse(event.data);
                this.observable.notify(packet);
            } catch (err) {
                console.error('[WS] Failed to parse packet:', err);
            }
        };

        currentSocket.onclose = (event) => {
            // Guard: If this closing socket isn't our active one, just ignore it
            if (this.socket === currentSocket) {
                this.isConnected = false;
                this.socket = null;

                if (!this.isManualClose) {
                    console.warn(`[WS] Active connection lost. Code: ${event.code}. Retrying in 3s...`);
                    if (!this.reconnectTimer) {
                        this.reconnectTimer = window.setTimeout(() => this.doConnect(), 3000);
                    }
                } else {
                    console.log('[WS] Connection closed manually.');
                }
            } else {
                console.log('[WS] Obsolete socket closed cleanly.');
            }
        };

        currentSocket.onerror = (err) => {
            if (this.socket === currentSocket && !this.isManualClose) {
                console.error('[WS] WebSocket error:', err);
            }
        };
    }

    disconnect(): void {
        this.isManualClose = true;
        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
            this.reconnectTimer = null;
        }
        if (this.socket) {
            if (this.socket.readyState === WebSocket.OPEN || this.socket.readyState === WebSocket.CONNECTING) {
                this.socket.close();
            }
            this.socket = null;
        }
    }

    startFlow = (scenario: string): void => {
        const stateMap: Record<number, string> = {
            [0]: 'CONNECTING',
            [1]: 'OPEN',
            [2]: 'CLOSING',
            [3]: 'CLOSED'
        };

        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            const state = this.socket ? stateMap[this.socket.readyState] : 'NULL';
            console.warn(`[WS] Start failed - Reason: Socket is ${state}`);
            return;
        }

        const cmd = { cmd: 'Start', scenario };
        this.socket.send(JSON.stringify(cmd));
        console.log('[WS] Sent Start command:', cmd);
    };

    stopFlow = (): void => {
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.warn('[WS] Stop failed - Not connected.');
            return;
        }
        const cmd = { cmd: 'Stop' };
        this.socket.send(JSON.stringify(cmd));
        console.log('[WS] Sent Stop command');
    };

    subscribe(callback: (packet: TelemetryPacket) => void): () => void {
        return this.observable.subscribe(callback);
    }
}
