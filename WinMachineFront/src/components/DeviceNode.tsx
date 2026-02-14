import React, { useMemo } from 'react';
import type { SceneNode, Device } from '../types';
import { useDeviceTelemetry } from '../hooks/useTelemetry';

interface DeviceNodeProps {
    node: SceneNode;
    registry: Device[];
}

interface ComponentProps {
    device: Device | undefined;
    telemetryValue: number | undefined;
    children?: React.ReactNode;
}

// Helper to check if a device is a cylinder
const isCylinder = (device?: Device) => device?.baseType === 'Cylinder';

// --- Components ---

const RotaryTable: React.FC<ComponentProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const radius = meta?.radius || 40;

    // Rotation is internal to the Rotary Table surface
    const rotation = telemetryValue ?? 0;

    return (
        <div
            className="absolute rounded-full border-2 border-slate-500 bg-slate-800 flex items-center justify-center shadow-lg transition-transform ease-linear"
            style={{
                width: radius * 2,
                height: radius * 2,
                left: -radius,
                top: -radius,
                transform: `rotate(${rotation}deg)`,
                // Rotary table movement usually continuous (Axis), so default 75ms or smooth lerp is fine
                transitionDuration: '75ms'
            }}
        >
            <div className="w-2/3 h-2/3 rounded-full border border-dashed border-slate-600" />
            <div className="absolute w-2 h-2 bg-cyan-500/50 rounded-full" />

            <div className="absolute left-1/2 top-1/2 w-0 h-0 overflow-visible">
                {children}
            </div>
        </div>
    );
};

const LinearGuide: React.FC<ComponentProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const isVertical = meta?.isVertical ?? false;
    const length = meta?.length || 200;
    const width = meta?.width || 32;
    const sliderWidth = meta?.sliderWidth || (isVertical ? 32 : 50);

    const railX = isVertical ? -width / 2 : -length / 2;
    const railY = isVertical ? -length / 2 : -width / 2;

    // LinearGuide Component renders the Rail AND the Slider.
    // If Stroke is on the Node, the WHOLE thing moves.
    // BUT usually LinearGuide Node = Rail (Static) + Slider (Dynamic Child).
    // If this component draws both, we must apply transform to slider.

    // [v2.2 Check] If this LinearGuide is an AXIS, it tracks position using isVertical/isReversed.
    // If it's a Cylinder-based slide (less common for "Guide" type), Node stroke handles it.
    // Assuming LinearGuide is mostly AXIS (Continuous).

    // Default to 0 if undefined
    const baseValue = telemetryValue ?? 0;
    const sliderPos = (meta?.isReversed ? -baseValue : baseValue);

    return (
        <div className="absolute pointer-events-none">
            {/* Rail */}
            <div
                className="absolute bg-slate-900 border border-slate-700 shadow-inner rounded-sm"
                style={{
                    width: isVertical ? width : length,
                    height: isVertical ? length : width,
                    left: railX,
                    top: railY
                }}
            >
                <div className={`absolute bg-slate-800 ${isVertical ? 'w-1 h-full left-1/2 -ml-0.5' : 'h-1 w-full top-1/2 -mt-0.5'}`} />
            </div>

            {/* Slider (Moving Part) - Axis Logic */}
            <div
                className="absolute bg-slate-700 border border-slate-500 shadow-md flex items-center justify-center transition-transform ease-linear"
                style={{
                    width: isVertical ? width + 4 : sliderWidth,
                    height: isVertical ? sliderWidth : width + 4,
                    left: (isVertical ? -width / 2 - 2 : -sliderWidth / 2),
                    top: (isVertical ? -sliderWidth / 2 : -width / 2 - 2),
                    transform: isVertical
                        ? `translateY(${sliderPos}px)`
                        : `translateX(${sliderPos}px)`,
                    transitionDuration: '75ms'
                }}
            >
                <div className="w-2 h-2 bg-orange-500/50 rounded-full" />
                <div className="absolute left-1/2 top-1/2 w-0 h-0 overflow-visible">
                    {children}
                </div>
            </div>
        </div>
    );
};

const SlideBlock: React.FC<ComponentProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const size = meta?.size || 40;

    // SlideBlock is generally a Cylinder.
    // [v2.2] Translation is now handled by DeviceNode's `stroke` logic if it's a Node-level movement.
    // So here we just render the block at (0,0).
    // Unless this component is used in a context without stroke (Axis?).

    // If Stroke defined on Node -> DeviceNode moved. We stay at 0,0 locally.
    // If this is an Axis, we move.

    let localPos = 0;
    if (!isCylinder(device)) {
        // Fallback for Axis type SlideBlock (continuous) if any
        localPos = telemetryValue ?? 0;
        if (meta?.isReversed) localPos = -localPos;
    }

    const isVertical = meta?.isVertical ?? false;

    return (
        <div className="absolute pointer-events-none">
            <div
                className="absolute bg-zinc-700 border border-zinc-500 shadow-sm flex items-center justify-center transition-transform ease-linear"
                style={{
                    width: size,
                    height: size,
                    left: -size / 2,
                    top: -size / 2,
                    transform: localPos !== 0 ? (isVertical ? `translateY(${localPos}px)` : `translateX(${localPos}px)`) : undefined,
                    transitionDuration: '75ms'
                }}
            >
                <div className="text-[9px] text-zinc-400">SLIDE</div>
                <div className="absolute left-1/2 top-1/2 w-0 h-0 overflow-visible">
                    {children}
                </div>
            </div>
        </div>
    );
};

const Gripper: React.FC<ComponentProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const width = meta?.width || 32;
    const height = meta?.height || 32;
    const openWidth = meta?.openWidth || 40;
    const closeWidth = meta?.closeWidth || 10;
    const moveTime = meta?.moveTime || meta?.MoveTime || 75;

    // Gripper internal logic: Fingers move.
    // Stroke on Node handles the *whole gripper* moving (if mounted on a slide).
    let currentWidth = openWidth;
    if (telemetryValue !== undefined) {
        if (isCylinder(device)) {
            // 0 is closed, 1 is open (based on v2.1 doc: 0 代表缩回/关闭，1 代表伸出/打开)
            currentWidth = (telemetryValue === 1) ? openWidth : closeWidth;
        } else {
            currentWidth = telemetryValue;
        }
    }

    const fingerOffset = currentWidth / 2;

    return (
        <div
            className="absolute bg-slate-800 border border-slate-600 shadow-sm flex flex-col items-center justify-between transition-all"
            style={{
                width: width,
                height: height,
                left: -width / 2,
                top: -height / 2,
                // Only animate internal props here
                transitionDuration: `${moveTime}ms`
            }}
        >
            <div className="relative w-full h-full">
                <div
                    className="absolute top-0 w-2 h-4 bg-gray-400 rounded-b-sm transition-all"
                    style={{
                        left: `calc(50% - ${fingerOffset}px - 4px)`,
                        transitionDuration: `${moveTime}ms`
                    }}
                />
                <div
                    className="absolute top-0 w-2 h-4 bg-gray-400 rounded-b-sm transition-all"
                    style={{
                        left: `calc(50% + ${fingerOffset}px - 4px)`,
                        transitionDuration: `${moveTime}ms`
                    }}
                />
            </div>

            <div className="mb-1 text-[8px] text-slate-500">GRP</div>
            <div className="absolute left-1/2 top-1/2 w-0 h-0 overflow-visible">
                {children}
            </div>
        </div>
    );
};

const SuctionPen: React.FC<ComponentProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const diameter = meta?.diameter || 10;
    const moveTime = meta?.moveTime || meta?.MoveTime || 75;

    // Suction pen might move down on 1, up on 0
    const isActive = isCylinder(device) ? telemetryValue === 1 : (telemetryValue ?? 0) > 0;

    return (
        <div
            className="absolute flex items-center justify-center transition-all"
            style={{
                width: diameter * 2,
                height: diameter * 2,
                left: -diameter,
                top: -diameter,
                transform: isActive ? 'scale(1.1)' : 'scale(1.0)',
                transitionDuration: `${moveTime}ms`
            }}
        >
            <div className={`absolute inset-0 rounded-full border transition-colors ${isActive ? 'bg-cyan-500/40 border-cyan-400' : 'bg-yellow-600/30 border-yellow-600/60'}`} />
            <div className={`w-1/2 h-1/2 rounded-full shadow-lg transition-colors ${isActive ? 'bg-cyan-400 animate-pulse' : 'bg-yellow-500'}`} />
            <div className="absolute left-1/2 top-1/2 w-0 h-0 overflow-visible">
                {children}
            </div>
        </div>
    );
};

const Group: React.FC<ComponentProps> = ({ children }) => {
    return <>{children}</>;
};


// --- Main Node Renderer ---

export const DeviceNode: React.FC<DeviceNodeProps> = ({ node, registry }) => {
    if (!node) return null;

    // Find the device object
    const device = useMemo(() => registry.find(d => d.id === node.linkedDeviceId), [registry, node.linkedDeviceId]);
    const telemetryValue = useDeviceTelemetry(node.linkedDeviceId, undefined);

    // [v2.2] Transformation Logic
    // Formula: LocalMatrix = T(Offset) * R(Rotation) * T(Stroke * ConnectionState)

    // 1. Static Offset
    const offX = node.offset?.x ?? 0;
    const offY = -(node.offset?.y ?? 0); // SVG/CSS Y-axis inversion usually needed
    const offZ = node.offset?.z ?? 0;

    // 2. Initial Rotation (Euler Z->Y->X order is standard but CSS order varies. CSS transform is right-to-left)
    const rotX = node.rotation?.x ?? 0;
    const rotY = node.rotation?.y ?? 0;
    const rotZ = node.rotation?.z ?? 0;

    // 3. Dynamic Stroke (Action Vector)
    // Only applies if device is active (1) or has analog value mapping to stroke?
    // Doc says: "Stroke ... Displacement when state 0->1". Mainly for Cylinder.
    let dynX = 0, dynY = 0, dynZ = 0;

    if (node.stroke) {
        let state = 0;
        if (isCylinder(device)) {
            // Binary Cylinder: 0 or 1
            state = (telemetryValue === 1) ? 1 : 0;
        } else {
            // Axis or other: Telemetry is the multiplier? Or Stroke isn't used this way?
            // Doc says "Axis ... Lerp to physical coordinate".
            // Stroke concept is specifically mentioned for Cylinder in the changelog.
            // For Axis, we might stick to internal component logic or assume stroke defines axis vector.
            // But let's follow the "Cylinder De-physicalization" instruction for Stroke.
            state = 0;
        }

        dynX = node.stroke.x * state;
        dynY = -(node.stroke.y) * state; // Y-Inversion
        dynZ = node.stroke.z * state;
    }

    // [v2.2] MoveTime for animation
    const moveTime = device?.meta?.moveTime || 75;

    // Construct CSS Transform String
    // Order: Translate(Offset) -> Rotate -> Translate(Dynamic)
    // Note: In CSS, transforms are applied from right to left visually (or strictly sequentially in the string).
    // T_global * T_local ... 
    // Matrix definition: T(Off) * R * T(Stroke)
    // CSS: translateX(off) ... rotate(rot) ... translateX(stroke)

    const transform = `
        translate3d(${offX}px, ${offY}px, ${offZ}px)
        rotateX(${rotX}deg) rotateY(${rotY}deg) rotateZ(${rotZ}deg)
        translate3d(${dynX}px, ${dynY}px, ${dynZ}px)
    `;

    const zIndex = Math.floor(offZ);

    const renderedChildren = node.children?.map((child, idx) => (
        <DeviceNode key={`${child.name}-${idx}`} node={child} registry={registry} />
    ));

    let Component;
    switch (node.nodeType) {
        case 'RotaryTable':
            Component = RotaryTable;
            break;
        case 'LinearGuide':
            // LinearGuide (Rail + Slider) usually doesn't move as a whole via stroke.
            // But if it's the moving part, it might.
            Component = LinearGuide;
            break;
        case 'SlideBlock':
            Component = SlideBlock;
            break;
        case 'Gripper':
            Component = Gripper;
            break;
        case 'SuctionPen':
            Component = SuctionPen;
            break;
        case 'Group':
        default:
            Component = Group;
            break;
    }

    return (
        <div
            className="absolute transition-transform ease-linear"
            style={{
                transform,
                zIndex,
                transitionDuration: isCylinder(device) ? `${moveTime}ms` : '75ms'
            }}
            data-name={node.name}
            data-device={node.linkedDeviceId}
        >
            {/* Pass telemetry to components for Internal Animation (like Gripper fingers) */}
            <Component device={device} telemetryValue={telemetryValue}>
                {renderedChildren}
            </Component>

            {/* Hover Label */}
            <div className="absolute -top-4 left-full ml-1 text-[9px] text-cyan-500/40 font-mono whitespace-nowrap opacity-0 hover:opacity-100 pointer-events-auto transition-opacity select-none bg-black/50 px-1 rounded transform -rotate-x-0">
                {node.name} {telemetryValue !== undefined && node.linkedDeviceId ? `(${telemetryValue.toFixed(1)})` : ''}
            </div>
        </div>
    );
};
