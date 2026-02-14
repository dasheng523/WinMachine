import type { ITelemetryClient, TelemetryPacket, MaterialEntity, TelemetryEvent } from './types';
import { TelemetryObservable } from './TelemetryObservable';

interface SimStep {
    name: string;
    targets: Record<string, number>;
    materials?: Record<string, MaterialEntity | null>;
    events?: TelemetryEvent[];
    duration: number; // ms
}

export class MockTelemetryClient implements ITelemetryClient {
    private observable = new TelemetryObservable();
    private intervalId: number | null = null;
    public isConnected = false;

    private currentStepIndex = 0;
    private stepStartTime = 0;
    private lastStepValues: Record<string, number> = {};

    // Steps updated for API v2.1 (Binary Cylinder States) & v2.3 (Material Flow)
    private readonly steps: SimStep[] = [
        // 0. 初始状态：右侧抓手有物料
        {
            name: "右侧夹爪闭合(夹取)",
            targets: { 'Cyl_Grip_Right_L1': 0, 'Cyl_Grip_Right_L2': 0, 'Cyl_Grip_Right_R1': 0, 'Cyl_Grip_Right_R2': 0 },
            materials: { 'Grip_R1': { id: 'M1', class: 'New' }, 'Grip_R2': { id: 'M2', class: 'New' } },
            duration: 500
        },
        // 1.
        {
            name: "右侧升起",
            targets: { 'Cyl_Lift_Right': 0 },
            materials: { 'Grip_R1': { id: 'M1', class: 'New' }, 'Grip_R2': { id: 'M2', class: 'New' } },
            duration: 800
        },
        // 2.
        {
            name: "右侧旋转180",
            targets: { 'Axis_Table_Right': 180 },
            materials: { 'Grip_R1': { id: 'M1', class: 'New' }, 'Grip_R2': { id: 'M2', class: 'New' } },
            duration: 1500
        },
        // 3.
        {
            name: "右侧降下",
            targets: { 'Cyl_Lift_Right': 1 },
            materials: { 'Grip_R1': { id: 'M1', class: 'New' }, 'Grip_R2': { id: 'M2', class: 'New' } },
            duration: 800
        },
        // 4. 放料 -> 物料转移到中间滑台 (Vac3, Vac4)
        {
            name: "右侧夹爪松开(放料)",
            targets: { 'Cyl_Grip_Right_L1': 1, 'Cyl_Grip_Right_L2': 1, 'Cyl_Grip_Right_R1': 1, 'Cyl_Grip_Right_R2': 1 },
            materials: {
                'Grip_R1': null, 'Grip_R2': null,
                'Vac3': { id: 'M1', class: 'New' }, 'Vac4': { id: 'M2', class: 'New' }
            },
            duration: 500
        },

        // 5.
        {
            name: "中间滑台向左",
            targets: { 'Cyl_Middle_Slide': 1 },
            materials: {
                'Vac3': { id: 'M1', class: 'New' }, 'Vac4': { id: 'M2', class: 'New' }
            },
            duration: 1000
        },

        // 6. 左侧抓取 (假设此时左侧本来是空的，然后抓取 M1, M2?? 逻辑上有点奇怪，但这只是 Mock)
        // 假设中间滑台向左后，M1, M2 到了左侧抓手下方
        {
            name: "左侧夹爪闭合",
            targets: { 'Cyl_Grip_L1': 0, 'Cyl_Grip_L2': 0, 'Cyl_Grip_R1': 0, 'Cyl_Grip_R2': 0 },
            materials: {
                'Vac3': { id: 'M1', class: 'New' }, 'Vac4': { id: 'M2', class: 'New' },
                // 暂时还没抓稳
            },
            duration: 500
        },
        // 7. 左侧升起 -> 物料从 Vac 这里的逻辑位置转移到 Grip
        {
            name: "左侧升起",
            targets: { 'Cyl_R_Lift': 0 },
            materials: {
                'Vac3': null, 'Vac4': null,
                'Grip_L1': { id: 'M1', class: 'Processing' }, 'Grip_L2': { id: 'M2', class: 'Processing' }
            },
            duration: 800
        },
        // 8.
        {
            name: "左侧旋转180",
            targets: { 'Axis_R_Table': 180 },
            materials: { 'Grip_L1': { id: 'M1', class: 'Processing' }, 'Grip_L2': { id: 'M2', class: 'Processing' } },
            duration: 1500
        },
        // 9.
        {
            name: "左侧降下",
            targets: { 'Cyl_R_Lift': 1 },
            materials: { 'Grip_L1': { id: 'M1', class: 'Processing' }, 'Grip_L2': { id: 'M2', class: 'Processing' } },
            duration: 800
        },
        // 10. 放料 -> 销毁 (模拟进入下一流程)
        {
            name: "左侧夹爪松开(放料)",
            targets: { 'Cyl_Grip_L1': 1, 'Cyl_Grip_L2': 1, 'Cyl_Grip_R1': 1, 'Cyl_Grip_R2': 1 },
            materials: { 'Grip_L1': null, 'Grip_L2': null },
            events: [{ type: 'MaterialConsume', msg: 'Material Processed', payload: { id: 'M1' } }], // Just emit one for demo
            duration: 500
        },

        // 11.
        { name: "中间滑台回原位", targets: { 'Cyl_Middle_Slide': 0 }, duration: 1000 },

        // Loop Reset
        {
            name: "重置中...", targets: {
                'Axis_Table_Right': 0, 'Axis_R_Table': 0,
                'Cyl_Grip_L1': 1, 'Cyl_Grip_L2': 1, 'Cyl_Grip_R1': 1, 'Cyl_Grip_R2': 1,
                'Cyl_Grip_Right_L1': 1, 'Cyl_Grip_Right_L2': 1, 'Cyl_Grip_Right_R1': 1, 'Cyl_Grip_Right_R2': 1
            }, duration: 200
        }
    ];

    connect(url: string): void {
        console.log(`[Mock] Connecting to ${url}...`);
        setTimeout(() => {
            this.isConnected = true;
            console.log('[Mock] Connected.');
        }, 300);
    }

    disconnect(): void {
        this.stopFlow();
        this.isConnected = false;
    }

    startFlow(scenario: string): void {
        if (this.intervalId) return;

        // API v2.0: Error semantics for unknown scenario
        if (scenario !== 'Complex_Rotary_Assembly') {
            console.error(`[Mock] Error: Scenario ${scenario} not found.`);
            this.observable.notify({
                t: Date.now(),
                step: 'Error',
                e: [
                    {
                        type: 'Error',
                        msg: `Unknown scenario '${scenario}'.`,
                        payload: { code: 'ERR_SCENARIO_NOT_FOUND', source: scenario }
                    },
                    {
                        type: 'FlowStopped',
                        msg: 'Stopped due to error',
                        payload: { reason: 'Error' }
                    }
                ]
            });
            return;
        }

        console.log(`[Mock] Starting Flow: ${scenario}`);
        this.currentStepIndex = 0;
        this.stepStartTime = Date.now();
        this.lastStepValues = {
            'Axis_Table_Right': 0, 'Axis_R_Table': 0,
            'Cyl_Lift_Right': 1, 'Cyl_R_Lift': 1,
            'Cyl_Middle_Slide': 0,
            'Cyl_Grip_L1': 1, 'Cyl_Grip_L2': 1, 'Cyl_Grip_R1': 1, 'Cyl_Grip_R2': 1,
            'Cyl_Grip_Right_L1': 1, 'Cyl_Grip_Right_L2': 1, 'Cyl_Grip_Right_R1': 1, 'Cyl_Grip_Right_R2': 1
        };

        this.observable.notify({
            t: Date.now(),
            step: 'FlowStarted',
            e: [{ type: 'FlowStarted', msg: 'Actions flow initiated', payload: { scenario } }]
        });

        this.intervalId = window.setInterval(() => this.tick(), 33);
    }

    stopFlow(): void {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
            this.observable.notify({
                t: Date.now(),
                step: 'Stopped',
                e: [{ type: 'FlowStopped', msg: 'User stopped' }]
            });
        }
    }

    subscribe(callback: (packet: TelemetryPacket) => void): () => void {
        return this.observable.subscribe(callback);
    }

    private tick() {
        const now = Date.now();
        const step = this.steps[this.currentStepIndex];
        let elapsed = now - this.stepStartTime;

        if (elapsed >= step.duration) {
            this.lastStepValues = { ...this.lastStepValues, ...step.targets };
            this.currentStepIndex = (this.currentStepIndex + 1) % this.steps.length;
            this.stepStartTime = now;
            elapsed = 0;
        }

        const t = elapsed / this.steps[this.currentStepIndex].duration;
        const currentM: Record<string, number> = { ...this.lastStepValues };

        Object.entries(this.steps[this.currentStepIndex].targets).forEach(([id, target]) => {
            const startVal = this.lastStepValues[id] ?? 0;

            // API v2.1: Continuous interpolation only for Axis
            if (id.startsWith('Axis')) {
                currentM[id] = startVal + (target - startVal) * t;
            } else {
                // Binary mode for cylinders: Mock just sends the target state
                currentM[id] = target;
            }
        });

        this.observable.notify({
            t: now,
            step: step.name,
            m: currentM,
            mat: step.materials, // Inject material state
            io: { 'Busy': true },
            e: step.events // Inject events if any
        });
    }
}
