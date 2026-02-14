export interface Transform {
    x: number;
    y: number;
    z: number;
}

export interface SceneNode {
    name: string;
    nodeType: 'Group' | 'RotaryTable' | 'Gripper' | 'SuctionPen' | 'LinearGuide' | 'SlideBlock' | 'MaterialSlot';
    linkedDeviceId?: string;
    offset?: Transform;
    rotation?: Transform; // [v2.2] Initial Euler Rotation (deg)
    stroke?: Transform;   // [v2.2] Action Stroke Vector (mm)
    children?: SceneNode[];
}

export interface MaterialEntity {
    id: string;
    class: string;
}

export interface DeviceMeta {
    // Shared
    width?: number;
    height?: number;
    pivotX?: number;
    pivotY?: number;

    // Axis / LinearGuide
    min?: number;
    max?: number;
    isVertical?: boolean;
    isReversed?: boolean;
    length?: number;
    sliderWidth?: number;

    // RotaryTable
    radius?: number;

    // Cylinder / SlideBlock / Gripper / SuctionPen
    moveTime?: number;
    ioOut?: number;
    ioInExtended?: number | null;
    ioInRetracted?: number | null;
    size?: number; // SlideBlock
    openWidth?: number;
    closeWidth?: number;
    diameter?: number;

    // Legacy support (optional)
    Min?: number;
    Max?: number;
    MoveTime?: number;
}

export interface Device {
    id: string;
    type: string;
    baseType: string;
    meta: DeviceMeta;
}

export interface MachineSchema {
    machineName: string;
    schemaVersion: string;
    scene: SceneNode;
    deviceRegistry: Device[];
}
