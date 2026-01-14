using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Machine.Framework.Devices.Motion.Abstractions;
using Machine.Framework.Core.Core;

namespace WinMachine
{
    partial class ZControllerView
    {
        private IContainer components = null;
        private GroupBox groupBoxAxis;
        private Button btnJogPos;
        private Button btnJogNeg;
        private Button btnStop;
        private Label lblPosTitle;
        private Label lblPosition;
        private GroupBox groupBoxIO;
        private Label lblOutIndex;
        private NumericUpDown numOutputIndex;
        private Button btnSetOutput;
        private Label lblInIndex;
        private NumericUpDown numInputIndex;
        private Label lblInStatus;
        private System.Windows.Forms.Timer timerStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            groupBoxAxis = new GroupBox();
            btnJogPos = new Button();
            btnJogNeg = new Button();
            btnStop = new Button();
            lblPosTitle = new Label();
            lblPosition = new Label();
            groupBoxIO = new GroupBox();
            numOutputIndex = new NumericUpDown();
            btnSetOutput = new Button();
            lblOutIndex = new Label();
            numInputIndex = new NumericUpDown();
            lblInIndex = new Label();
            lblInStatus = new Label();
            timerStatus = new System.Windows.Forms.Timer(components);

            groupBoxAxis.SuspendLayout();
            groupBoxIO.SuspendLayout();
            ((ISupportInitialize)numOutputIndex).BeginInit();
            ((ISupportInitialize)numInputIndex).BeginInit();
            SuspendLayout();

            // groupBoxAxis
            groupBoxAxis.Controls.Add(lblPosition);
            groupBoxAxis.Controls.Add(lblPosTitle);
            groupBoxAxis.Controls.Add(btnStop);
            groupBoxAxis.Controls.Add(btnJogNeg);
            groupBoxAxis.Controls.Add(btnJogPos);
            groupBoxAxis.Location = new Point(12, 12);
            groupBoxAxis.Name = "groupBoxAxis";
            groupBoxAxis.Size = new Size(360, 150);
            groupBoxAxis.TabIndex = 0;
            groupBoxAxis.TabStop = false;
            groupBoxAxis.Text = "�?0 控制 (JOG)";

            // btnJogPos
            btnJogPos.Location = new Point(130, 90);
            btnJogPos.Name = "btnJogPos";
            btnJogPos.Size = new Size(100, 40);
            btnJogPos.Text = "正向运动";
            btnJogPos.MouseDown += (s, e) => OnJogMouseDown(MotionDirection.Positive);
            btnJogPos.MouseUp += (s, e) => OnJogMouseUp();

            // btnJogNeg
            btnJogNeg.Location = new Point(20, 90);
            btnJogNeg.Name = "btnJogNeg";
            btnJogNeg.Size = new Size(100, 40);
            btnJogNeg.Text = "负向运动";
            btnJogNeg.MouseDown += (s, e) => OnJogMouseDown(MotionDirection.Negative);
            btnJogNeg.MouseUp += (s, e) => OnJogMouseUp();

            // btnStop
            btnStop.BackColor = Color.MistyRose;
            btnStop.Location = new Point(240, 90);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 40);
            btnStop.Text = "停止";
            btnStop.Click += (s, e) => OnStopClick();

            // lblPosTitle
            lblPosTitle.AutoSize = true;
            lblPosTitle.Location = new Point(20, 40);
            lblPosTitle.Text = "当前坐标:";

            // lblPosition
            lblPosition.AutoSize = true;
            lblPosition.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPosition.ForeColor = Color.Blue;
            lblPosition.Location = new Point(100, 38);
            lblPosition.Text = "0.000";

            // groupBoxIO
            groupBoxIO.Controls.Add(lblInStatus);
            groupBoxIO.Controls.Add(lblInIndex);
            groupBoxIO.Controls.Add(numInputIndex);
            groupBoxIO.Controls.Add(lblOutIndex);
            groupBoxIO.Controls.Add(btnSetOutput);
            groupBoxIO.Controls.Add(numOutputIndex);
            groupBoxIO.Location = new Point(12, 175);
            groupBoxIO.Name = "groupBoxIO";
            groupBoxIO.Size = new Size(360, 160);
            groupBoxIO.TabIndex = 1;
            groupBoxIO.TabStop = false;
            groupBoxIO.Text = "IO 控制";

            // numOutputIndex
            numOutputIndex.Location = new Point(100, 35);
            numOutputIndex.Name = "numOutputIndex";
            numOutputIndex.Size = new Size(80, 27);

            // lblOutIndex
            lblOutIndex.AutoSize = true;
            lblOutIndex.Location = new Point(20, 37);
            lblOutIndex.Text = "输出口索�?";

            // btnSetOutput
            btnSetOutput.Location = new Point(200, 32);
            btnSetOutput.Name = "btnSetOutput";
            btnSetOutput.Size = new Size(130, 35);
            btnSetOutput.Text = "切换�?低电�?;
            btnSetOutput.Click += (s, e) => OnToggleOutput();

            // numInputIndex
            numInputIndex.Location = new Point(100, 95);
            numInputIndex.Name = "numInputIndex";
            numInputIndex.Size = new Size(80, 27);

            // lblInIndex
            lblInIndex.AutoSize = true;
            lblInIndex.Location = new Point(20, 97);
            lblInIndex.Text = "输入口索�?";

            // lblInStatus
            lblInStatus.AutoSize = true;
            lblInStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInStatus.Location = new Point(200, 96);
            lblInStatus.Text = "状�? 低电�?;

            // timerStatus
            timerStatus.Enabled = true;
            timerStatus.Interval = 100;
            timerStatus.Tick += (s, e) => OnTimerTick();

            // ZControllerView
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 351);
            Controls.Add(groupBoxIO);
            Controls.Add(groupBoxAxis);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ZControllerView";
            StartPosition = FormStartPosition.CenterParent;
            Text = "针对�?0 的调试控制器";
            groupBoxAxis.ResumeLayout(false);
            groupBoxAxis.PerformLayout();
            groupBoxIO.ResumeLayout(false);
            groupBoxIO.PerformLayout();
            ((ISupportInitialize)numOutputIndex).EndInit();
            ((ISupportInitialize)numInputIndex).EndInit();
            ResumeLayout(false);
        }
    }
}


