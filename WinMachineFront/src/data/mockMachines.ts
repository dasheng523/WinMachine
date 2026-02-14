import type { MachineSchema } from '../types';

// Import JSONs directly
import fullConfig from './machines/full_config.json';
import simpleConfig from './machines/simple_config.json';
import complexRotaryLift from './machines/complex_rotary_lift.json';

// Cast them to MachineSchema (assuming JSON is valid)
const machine1 = fullConfig as unknown as MachineSchema;
const machine2 = simpleConfig as unknown as MachineSchema;
const machine3 = complexRotaryLift as unknown as MachineSchema;

export const machines: MachineSchema[] = [
    machine1,
    machine2,
    machine3
];
