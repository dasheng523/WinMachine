import type { TelemetryPacket } from './types';

type Listener = (packet: TelemetryPacket) => void;

export class TelemetryObservable {
    private listeners: Set<Listener> = new Set();
    private lastPacket: TelemetryPacket | null = null;

    notify(packet: TelemetryPacket) {
        this.lastPacket = packet;
        this.listeners.forEach(listener => listener(packet));
    }

    subscribe(listener: Listener): () => void {
        this.listeners.add(listener);
        // Immediately send latest if available? Maybe not for high freq data
        // if (this.lastPacket) listener(this.lastPacket);

        return () => {
            this.listeners.delete(listener);
        };
    }

    getLastPacket(): TelemetryPacket | null {
        return this.lastPacket;
    }
}
