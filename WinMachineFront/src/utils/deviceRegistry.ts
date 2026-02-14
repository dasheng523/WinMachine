import type { Device } from '../types';

export const getDeviceMeta = (registry: Device[], id?: string) => {
    if (!id) return null;
    return registry.find(d => d.id === id)?.meta;
};
