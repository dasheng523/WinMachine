import React, { useEffect, useState } from 'react';
import { TelemetryContext } from './TelemetryContext';
import { MockTelemetryClient } from './MockTelemetryClient';
import { WebSocketTelemetryClient } from './WebSocketTelemetryClient';

// Configuration - could be env var
const USE_MOCK = false;
const WS_URL = 'ws://localhost:5000/ws/telemetry';

export const TelemetryProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    // We only instantiate once.
    const [instance] = useState(() => {
        const client = USE_MOCK ? new MockTelemetryClient() : new WebSocketTelemetryClient();
        return client;
    });

    useEffect(() => {
        instance.connect(WS_URL);
        return () => instance.disconnect();
    }, [instance]);

    return (
        <TelemetryContext.Provider value={{ client: instance, observable: instance.observable }}>
            {children}
        </TelemetryContext.Provider>
    );
};
