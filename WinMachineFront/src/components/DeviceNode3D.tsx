import React, { useMemo, useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import { Box, Cylinder, Text } from '@react-three/drei';
import * as THREE from 'three';
import { useSpring, animated, config } from '@react-spring/three';
import { useDeviceTelemetry, useMaterialState } from '../hooks/useTelemetry';
import type { SceneNode, Device, MaterialEntity } from '../types';

interface DeviceNode3DProps {
    node: SceneNode;
    registry: Device[];
}

interface Component3DProps {
    device: Device | undefined;
    telemetryValue: number | undefined;
    children?: React.ReactNode;
}

// Reuse helper
const isCylinder = (device?: Device) => device?.baseType === 'Cylinder';

// Materials
const metallicDark = new THREE.MeshStandardMaterial({ color: '#1e293b', roughness: 0.5, metalness: 0.8 }); // slate-800
const metallicLight = new THREE.MeshStandardMaterial({ color: '#475569', roughness: 0.4, metalness: 0.6 }); // slate-600
const accentCyan = new THREE.MeshStandardMaterial({ color: '#06b6d4', emissive: '#06b6d4', emissiveIntensity: 0.5 });
const accentOrange = new THREE.MeshStandardMaterial({ color: '#f97316', emissive: '#f97316', emissiveIntensity: 0.2 });
const railMaterial = new THREE.MeshStandardMaterial({ color: '#0f172a', roughness: 0.7, metalness: 0.5 }); // slate-900

const Material3D: React.FC<{ entity: MaterialEntity }> = ({ entity }) => {
    const isReady = entity.class === 'New';
    const color = isReady ? '#3b82f6' : '#eab308'; // Blue (New) vs Yellow (Processing)
    return (
        <group>
            <Box args={[16, 16, 8]} position={[0, 0, 4]} material-color={color} material-roughness={0.2} >
                {/* Edges */}
                <meshStandardMaterial color={color} roughness={0.5} />
            </Box>
            <Text position={[0, 0, 9]} fontSize={6} color="white" anchorX="center" anchorY="middle">
                {entity.id}
            </Text>
        </group>
    );
};

// --- 3D Components ---

const RotaryTable3D: React.FC<Component3DProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const radius = meta?.radius || 40;
    const height = 5;

    const rotation = telemetryValue ?? 0;
    const rad = THREE.MathUtils.degToRad(rotation);

    // Smooth rotation spring
    const { rot } = useSpring({
        rot: rad,
        config: { ...config.stiff, precision: 0.001 }
    });

    return (
        <group>
            <Cylinder args={[radius, radius, height, 32]} rotation={[Math.PI / 2, 0, 0]} material={metallicDark} position={[0, 0, -height / 2]} />
            <animated.group rotation-z={rot}>
                <Cylinder args={[radius * 0.95, radius * 0.95, 1, 32]} rotation={[Math.PI / 2, 0, 0]} position={[0, 0, 1]} material={metallicLight}>
                    <Box args={[5, radius * 0.8, 2]} position={[0, 0, 1]} material={accentCyan} />
                </Cylinder>
                {children}
            </animated.group>
        </group>
    );
};

const LinearGuide3D: React.FC<Component3DProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const isVertical = meta?.isVertical ?? false;
    const length = meta?.length || 200;
    const width = meta?.width || 32;
    const sliderWidth = meta?.sliderWidth || (isVertical ? 32 : 50);
    const height = 10;

    const baseValue = telemetryValue ?? 0;
    const sliderPos = (meta?.isReversed ? -baseValue : baseValue);

    // Smooth position spring
    const { pos } = useSpring({
        pos: sliderPos,
        config: { mass: 1, tension: 170, friction: 26 } // Default spring
    });

    const railSize: [number, number, number] = isVertical ? [width, length, height] : [length, width, height];

    return (
        <group>
            <Box args={railSize} material={railMaterial} />
            <Box args={isVertical ? [width * 0.4, length, height + 1] : [length, width * 0.4, height + 1]} material={metallicDark} position={[0, 0, 1]} />

            <animated.group position={pos.to(p => [
                isVertical ? 0 : p,
                isVertical ? p : 0,
                height
            ])}>
                <Box args={[sliderWidth, sliderWidth, height + 5]} material={metallicLight}>
                    <meshStandardMaterial color="#334155" />
                </Box>
                <Box args={[8, 8, 2]} position={[0, 0, (height + 5) / 2 + 1]} material={accentOrange} />
                {children}
            </animated.group>
        </group>
    );
};

const SlideBlock3D: React.FC<Component3DProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const size = meta?.size || 40;
    const isVertical = meta?.isVertical ?? false;

    let targetPos = 0;
    if (!isCylinder(device)) {
        targetPos = telemetryValue ?? 0;
        if (meta?.isReversed) targetPos = -targetPos;
    }

    const { pos } = useSpring({
        pos: targetPos,
        config: config.stiff
    });

    return (
        <animated.group position={pos.to(p => [
            isVertical ? 0 : p,
            isVertical ? p : 0,
            0
        ])}>
            <Box args={[size, size, size]} material={metallicLight}>
                <meshStandardMaterial color="#52525b" />
            </Box>
            <Text position={[0, 0, size / 2 + 0.1]} fontSize={8} color="white" anchorX="center" anchorY="middle">SLIDE</Text>
            {children}
        </animated.group>
    );
};

const Gripper3D: React.FC<Component3DProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const width = meta?.width || 32;
    const height = meta?.height || 32;
    const openWidth = meta?.openWidth || 40;
    const closeWidth = meta?.closeWidth || 10;
    const moveTime = meta?.moveTime || 300; // Use moveTime for animation duration

    let targetWidth = openWidth;
    if (telemetryValue !== undefined) {
        if (isCylinder(device)) {
            targetWidth = (telemetryValue === 1) ? openWidth : closeWidth;
        } else {
            targetWidth = telemetryValue;
        }
    }

    // Use duration-based spring for cylinder-like movement
    const { fingerOffset } = useSpring({
        fingerOffset: targetWidth / 2,
        config: { duration: moveTime }
    });

    return (
        <group>
            <Box args={[width, height, 15]} material={metallicDark} />
            <group position={[0, 0, 10]}>
                <animated.group position-x={fingerOffset.to(o => -o - 2)}>
                    <Box args={[4, 10, 20]} material={metallicLight} />
                </animated.group>
                <animated.group position-x={fingerOffset.to(o => o + 2)}>
                    <Box args={[4, 10, 20]} material={metallicLight} />
                </animated.group>
            </group>
            {children}
        </group>
    );
};

const SuctionPen3D: React.FC<Component3DProps> = ({ device, telemetryValue, children }) => {
    const meta = device?.meta;
    const diameter = meta?.diameter || 10;
    const isActive = isCylinder(device) ? telemetryValue === 1 : (telemetryValue ?? 0) > 0;
    const moveTime = meta?.moveTime || 200;

    const { zPos } = useSpring({
        zPos: isActive ? 15 : 0,
        config: { duration: moveTime }
    });

    return (
        <group>
            <Cylinder args={[diameter, diameter, 20, 16]} rotation={[Math.PI / 2, 0, 0]} material={metallicDark} />
            <animated.group position-z={zPos}>
                <Cylinder
                    args={[diameter / 2, diameter / 2, 10, 16]}
                    rotation={[Math.PI / 2, 0, 0]}
                    position={[0, 0, 10]}
                    material={isActive ? accentCyan : new THREE.MeshStandardMaterial({ color: '#ca8a04' })}
                />
            </animated.group>
            {children}
        </group>
    );
};

const Group3D: React.FC<Component3DProps> = ({ children }) => <group>{children}</group>;

const MaterialSlot3D: React.FC<Component3DProps> = ({ children }) => (
    <group>
        {/* Helper visual for debug purposes (transparent sphere) */}
        <mesh visible={false}>
            <sphereGeometry args={[5]} />
            <meshBasicMaterial color="lime" wireframe />
        </mesh>
        {children}
    </group>
);

// --- DeviceNode3D Implementation ---

export const DeviceNode3D: React.FC<DeviceNode3DProps> = ({ node, registry }) => {
    const device = useMemo(() => registry.find(d => d.id === node.linkedDeviceId), [registry, node.linkedDeviceId]);
    const telemetryValue = useDeviceTelemetry(node.linkedDeviceId, undefined);
    const material = useMaterialState(node.linkedDeviceId);

    const offX = node.offset?.x ?? 0;
    const offY = -(node.offset?.y ?? 0);
    const offZ = node.offset?.z ?? 0;

    const rotX = THREE.MathUtils.degToRad(node.rotation?.x ?? 0);
    const rotY = THREE.MathUtils.degToRad(node.rotation?.y ?? 0);
    const rotZ = THREE.MathUtils.degToRad(node.rotation?.z ?? 0);

    // Stroke animation
    let targetX = 0, targetY = 0, targetZ = 0;
    if (node.stroke) {
        let state = 0;
        if (isCylinder(device)) {
            state = (telemetryValue === 1) ? 1 : 0;
        }
        targetX = node.stroke.x * state;
        targetY = -(node.stroke.y) * state;
        targetZ = node.stroke.z * state;
    }

    const moveTime = device?.meta?.moveTime || 300;

    const { dynPos } = useSpring({
        dynPos: [targetX, targetY, targetZ],
        config: isCylinder(device) ? { duration: moveTime } : config.stiff
    });

    const renderedChildren = node.children?.map((child, idx) => (
        <DeviceNode3D key={`${child.name}-${idx}`} node={child} registry={registry} />
    ));

    let Component;
    switch (node.nodeType) {
        case 'RotaryTable': Component = RotaryTable3D; break;
        case 'LinearGuide': Component = LinearGuide3D; break;
        case 'SlideBlock': Component = SlideBlock3D; break;
        case 'Gripper': Component = Gripper3D; break;
        case 'SuctionPen': Component = SuctionPen3D; break;
        case 'MaterialSlot': Component = MaterialSlot3D; break;
        case 'Group': default: Component = Group3D; break;
    }

    const content = (
        <>
            {material && <Material3D entity={material} />}
            {renderedChildren}
        </>
    );

    return (
        <group position={[offX, offY, offZ]} rotation={[rotX, rotY, rotZ]}>
            <animated.group position={dynPos as any}>
                <Component device={device} telemetryValue={telemetryValue}>
                    {content}
                </Component>
            </animated.group>
        </group>
    );
};
