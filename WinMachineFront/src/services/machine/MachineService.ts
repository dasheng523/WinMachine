import type { MachineSchema } from '../../types';

const API_BASE = 'http://localhost:5000/api/machine';

export const MachineService = {
    async getScenarios(): Promise<string[]> {
        // Mock fallback for now
        try {
            const res = await fetch(`${API_BASE}/scenarios`);
            if (!res.ok) return ['Complex_Rotary_Assembly'];
            return res.json();
        } catch {
            return ['Complex_Rotary_Assembly'];
        }
    },

    async getSchema(name: string): Promise<MachineSchema | null> {
        try {
            const res = await fetch(`${API_BASE}/schema?name=${name}`);
            if (!res.ok) return null;
            return res.json();
        } catch {
            // Fallback to local data if server is down during dev
            return null;
        }
    }
};
