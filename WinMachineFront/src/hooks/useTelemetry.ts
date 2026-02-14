import { useEffect, useState, useRef } from 'react';
import { useTelemetryObservable } from '../services/telemetry/TelemetryContext';
import type { MaterialEntity } from '../services/telemetry/types';

/**
 * Hook to get the real-time value of a device from the telemetry stream.
 * @param deviceId The ID of the device to listen for.
 * @param defaultValue Initial value (usually 0).
 * @returns The current value (interpolated or latest).
 */
export const useDeviceTelemetry = (deviceId: string | undefined, defaultValue: number | undefined = undefined) => {
    const observable = useTelemetryObservable();
    const [value, setValue] = useState<number | undefined>(defaultValue);
    const valueRef = useRef<number | undefined>(defaultValue);

    useEffect(() => {
        if (!deviceId) return;

        const unsubscribe = observable.subscribe((packet) => {
            if (packet.m && packet.m[deviceId] !== undefined) {
                const newValue = packet.m[deviceId];
                // Simple optimization: only update if changed significantly
                // In a real app, we might do interpolation here using packet.t
                if (valueRef.current === undefined || Math.abs(newValue - valueRef.current) > 0.001) {
                    valueRef.current = newValue;
                    setValue(newValue);
                }
            }
        });

        return unsubscribe;
    }, [deviceId, observable]);

    return value;
};

/**
 * Hook to get the real-time material state of a station from the telemetry stream.
 * @param stationId The ID of the station (e.g., "Vac1").
 * @returns The current material entity on that station, or null if empty.
 */
export const useMaterialState = (stationId: string | undefined) => {
    const observable = useTelemetryObservable();
    const [material, setMaterial] = useState<MaterialEntity | null | undefined>(undefined);
    // Use a ref to avoid stale closure issues if we were doing more complex logic, 
    // but mainly here to reduce renders if the object content hasn't changed.
    const materialRef = useRef<MaterialEntity | null | undefined>(undefined);

    useEffect(() => {
        if (!stationId) return;

        const unsubscribe = observable.subscribe((packet) => {
            if (packet.mat && stationId in packet.mat) {
                const newMat = packet.mat[stationId];

                // Simple equality check to avoid re-rendering if the object reference or content is same
                // Note: In a high-frequency loop, constructing new objects every time will cause diffs.
                // We rely on the fact that if 'id' and 'class' are same, it is the same material.
                const current = materialRef.current;

                const isSame = (newMat === null && current === null) ||
                    (newMat && current && newMat.id === current.id && newMat.class === current.class);

                if (!isSame) {
                    materialRef.current = newMat;
                    setMaterial(newMat);
                }
            }
        });

        return unsubscribe;
    }, [stationId, observable]);

    return material;
};
