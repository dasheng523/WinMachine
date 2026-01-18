using System;
using System.Drawing;

namespace WinMachine;

internal enum TransferSide { Left, Right }

internal sealed record Workpiece(string Id, Color Color);

internal sealed class TransferStationModel
{
    // 扫码座：4个（0..3），其中 0/1 属于左侧，2/3 属于右侧
    public Workpiece?[] ScanSeats { get; } = new Workpiece?[4];

    // 测试座：4个（0..3），布局同上
    public Workpiece?[] TestSeats { get; } = new Workpiece?[4];

    // 左/右搬运模块的4个夹爪持有的物料（前2个对应扫码座那一侧，后2个对应测试座那一侧）
    public Workpiece?[] LeftHeld { get; } = new Workpiece?[4];
    public Workpiece?[] RightHeld { get; } = new Workpiece?[4];

    public static TransferStationModel CreateDemo()
    {
        var m = new TransferStationModel();

        m.ScanSeats[0] = new Workpiece("S1", Color.FromArgb(255, 65, 105, 225));
        m.ScanSeats[1] = new Workpiece("S2", Color.FromArgb(255, 46, 139, 87));
        m.ScanSeats[2] = new Workpiece("S3", Color.FromArgb(255, 255, 165, 0));
        m.ScanSeats[3] = new Workpiece("S4", Color.FromArgb(255, 186, 85, 211));

        m.TestSeats[0] = new Workpiece("T1", Color.FromArgb(255, 220, 20, 60));
        m.TestSeats[1] = new Workpiece("T2", Color.FromArgb(255, 30, 144, 255));
        m.TestSeats[2] = new Workpiece("T3", Color.FromArgb(255, 255, 215, 0));
        m.TestSeats[3] = new Workpiece("T4", Color.FromArgb(255, 0, 206, 209));

        return m;
    }

    public void Apply(TransferGripEvent ev)
    {
        int scanA = ev.Side == TransferSide.Left ? 0 : 2;
        int scanB = ev.Side == TransferSide.Left ? 1 : 3;
        int testA = scanA;
        int testB = scanB;

        var held = ev.Side == TransferSide.Left ? LeftHeld : RightHeld;

        if (ev.Action == TransferGripAction.Grab)
        {
            // 抓取：从扫码座与测试座同时取走
            held[0] = ScanSeats[scanA];
            held[1] = ScanSeats[scanB];
            held[2] = TestSeats[testA];
            held[3] = TestSeats[testB];

            ScanSeats[scanA] = null;
            ScanSeats[scanB] = null;
            TestSeats[testA] = null;
            TestSeats[testB] = null;
        }
        else if (ev.Action == TransferGripAction.ReleaseSwap)
        {
            // 释放并互换：扫码 -> 测试，测试 -> 扫码
            TestSeats[testA] = held[0];
            TestSeats[testB] = held[1];
            ScanSeats[scanA] = held[2];
            ScanSeats[scanB] = held[3];

            for (int i = 0; i < held.Length; i++) held[i] = null;
        }
    }
}
