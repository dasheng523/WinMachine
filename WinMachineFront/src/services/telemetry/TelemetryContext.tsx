import React, { createContext, useContext, useEffect, useState } from 'react';
import type { ITelemetryClient } from './types';
import { MockTelemetryClient } from './MockTelemetryClient';
import { WebSocketTelemetryClient } from './WebSocketTelemetryClient';
import { TelemetryObservable } from './TelemetryObservable';

interface TelemetryContextType {
    client: ITelemetryClient;
    observable: TelemetryObservable; // Exposed for hooks
}

export const TelemetryContext = createContext<TelemetryContextType | null>(null);

// Configuration - could be env var
const USE_MOCK = false;
const WS_URL = 'ws://localhost:5000/ws/telemetry';

export const TelemetryProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    // We only instantiate once.
    const [instance] = useState(() => {
        const client = USE_MOCK ? new MockTelemetryClient() : new WebSocketTelemetryClient();
        // The client HAS an observable inside, but the interface exposes `subscribe`. 
        // We need to access the internal observable OR wrap it. 
        // For simplicity, let's cast or adjust the interface. 
        // actually, let's just use the `subscribe` from client in our hooks.
        // But to make `useTelemetry` hook easy, we might want a raw way to get latest packet.

        // Let's attach the client.
        return client;
    });

    useEffect(() => {
        instance.connect(WS_URL);
        return () => instance.disconnect();
    }, [instance]);

    return (
        <TelemetryContext.Provider value={{ client: instance, observable: (instance as any).observable }}>
            {children}
        </TelemetryContext.Provider>
    );
};

export const useTelemetryClient = () => {
    const ctx = useContext(TelemetryContext);
    if (!ctx) throw new Error('useTelemetryClient must be used within TelemetryProvider');
    return ctx.client;
};

// Hook for components to get data
export const useTelemetryObservable = () => {
    const ctx = useContext(TelemetryContext);
    if (!ctx) throw new Error('useTelemetryObservable must be used within TelemetryProvider');
    return ctx.observable;
};
