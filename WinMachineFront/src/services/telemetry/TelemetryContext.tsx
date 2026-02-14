import { createContext, useContext } from 'react';
import type { ITelemetryClient } from './types';
import { TelemetryObservable } from './TelemetryObservable';

interface TelemetryContextType {
    client: ITelemetryClient;
    observable: TelemetryObservable; // Exposed for hooks
}

export const TelemetryContext = createContext<TelemetryContextType | null>(null);

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
