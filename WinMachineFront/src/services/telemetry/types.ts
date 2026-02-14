export type EventType = "FlowStarted" | "FlowStopped" | "Error" | "Attach" | "Detach" | "Spawn" | "MaterialSpawn" | "MaterialTransform" | "MaterialConsume";

export interface TelemetryEvent {
    type: EventType;
    msg: string;      // Human readable message
    payload?: any;    // Depend on type
}

export interface MaterialEntity {
    id: string;
    class: string; // "New", "Old", etc.
}

export interface TelemetryPacket {
    /** 
     * [Required] Server Timestamp (Unix Milliseconds)
     * Monotonically increasing. used for interpolation.
     */
    t: number;

    /**
     * [Required] Current Business Step Name
     * e.g. "Rotate_180_CW"
     */
    step: string;

    /** 
     * [Optional] Motion Targets (Visual Position Delta)
     * Key: DeviceID
     * Value: Physical Value (mm / degrees / width)
     */
    m?: Record<string, number>;

    /**
     * [Optional] [v2.3] Material State Map
     * Key: StationID
     * Value: Material Entity or null/undefined
     */
    mat?: Record<string, MaterialEntity | null>;

    /** 
     * [Optional] IO / Sensor State
     * Key: SignalName
     * Value: boolean (Digital) or number (Analog)
     */
    io?: Record<string, boolean | number>;

    /** 
     * [Optional] Discrete Events
     * Processed in order.
     */
    e?: TelemetryEvent[];
}

export interface ITelemetryClient {
    connect(url: string): void;
    disconnect(): void;

    startFlow(scenario: string): void;
    stopFlow(): void;

    // Observable pattern for data
    subscribe(callback: (packet: TelemetryPacket) => void): () => void;

    // Status
    isConnected: boolean;
}
