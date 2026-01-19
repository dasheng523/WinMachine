
namespace WinMachine
{
    partial class SimulatorDemoForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ListBox lstScenarios;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblActiveStep;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel pnlCanvas;

        private System.Windows.Forms.Panel pnlLayoutSurface;
        private System.Windows.Forms.Panel pnlAxisX;
        private System.Windows.Forms.Panel pnlAxisZ1;
        private System.Windows.Forms.Panel pnlAxisZ2;
        private System.Windows.Forms.Panel pnlCylinderSlide;
        private System.Windows.Forms.Panel pnlLeftGrip;
        private System.Windows.Forms.Panel pnlRightGrip;
        private System.Windows.Forms.Panel pnlSeatL1;
        private System.Windows.Forms.Panel pnlSeatL2;
        private System.Windows.Forms.Panel pnlSeatR1;
        private System.Windows.Forms.Panel pnlSeatR2;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            splitMain = new System.Windows.Forms.SplitContainer();
            lstScenarios = new System.Windows.Forms.ListBox();
            btnRun = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lblActiveStep = new System.Windows.Forms.Label();
            txtLog = new System.Windows.Forms.TextBox();
            pnlCanvas = new System.Windows.Forms.Panel();
            pnlLayoutSurface = new System.Windows.Forms.Panel();
            pnlAxisX = new System.Windows.Forms.Panel();
            pnlAxisZ1 = new System.Windows.Forms.Panel();
            pnlAxisZ2 = new System.Windows.Forms.Panel();
            pnlCylinderSlide = new System.Windows.Forms.Panel();
            pnlLeftGrip = new System.Windows.Forms.Panel();
            pnlRightGrip = new System.Windows.Forms.Panel();
            pnlSeatL1 = new System.Windows.Forms.Panel();
            pnlSeatL2 = new System.Windows.Forms.Panel();
            pnlSeatR1 = new System.Windows.Forms.Panel();
            pnlSeatR2 = new System.Windows.Forms.Panel();

            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            SuspendLayout();

            // splitMain
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.SplitterDistance = 360;
            splitMain.Name = "splitMain";

            // Left panel - scenario controls
            lstScenarios.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lstScenarios.IntegralHeight = false;
            lstScenarios.Location = new System.Drawing.Point(12, 12);
            lstScenarios.Name = "lstScenarios";
            lstScenarios.Size = new System.Drawing.Size(336, 260);

            btnRun.Location = new System.Drawing.Point(12, 280);
            btnRun.Name = "btnRun";
            btnRun.Size = new System.Drawing.Size(120, 32);
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;

            btnCancel.Location = new System.Drawing.Point(140, 280);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(120, 32);
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;

            lblActiveStep.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblActiveStep.Location = new System.Drawing.Point(12, 320);
            lblActiveStep.Name = "lblActiveStep";
            lblActiveStep.Size = new System.Drawing.Size(336, 40);
            lblActiveStep.Text = "Active: -";

            txtLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtLog.Location = new System.Drawing.Point(12, 364);
            txtLog.Multiline = true;
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new System.Drawing.Size(336, 260);

            splitMain.Panel1.Controls.Add(lstScenarios);
            splitMain.Panel1.Controls.Add(btnRun);
            splitMain.Panel1.Controls.Add(btnCancel);
            splitMain.Panel1.Controls.Add(lblActiveStep);
            splitMain.Panel1.Controls.Add(txtLog);

            // Right panel - canvas
            pnlCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCanvas.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            pnlCanvas.Name = "pnlCanvas";

            // Layout surface (Designer 可自由摆放)
            pnlLayoutSurface.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLayoutSurface.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            pnlLayoutSurface.Name = "pnlLayoutSurface";

            // Axis panels
            pnlAxisX.BackColor = System.Drawing.Color.FromArgb(45, 52, 60);
            pnlAxisX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlAxisX.Location = new System.Drawing.Point(24, 20);
            pnlAxisX.Name = "pnlAxisX";
            pnlAxisX.Size = new System.Drawing.Size(240, 70);

            pnlAxisZ1.BackColor = System.Drawing.Color.FromArgb(45, 52, 60);
            pnlAxisZ1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlAxisZ1.Location = new System.Drawing.Point(24, 100);
            pnlAxisZ1.Name = "pnlAxisZ1";
            pnlAxisZ1.Size = new System.Drawing.Size(240, 70);

            pnlAxisZ2.BackColor = System.Drawing.Color.FromArgb(45, 52, 60);
            pnlAxisZ2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlAxisZ2.Location = new System.Drawing.Point(24, 180);
            pnlAxisZ2.Name = "pnlAxisZ2";
            pnlAxisZ2.Size = new System.Drawing.Size(240, 70);

            // Cylinder / gripper panels
            pnlCylinderSlide.BackColor = System.Drawing.Color.FromArgb(50, 58, 66);
            pnlCylinderSlide.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlCylinderSlide.Location = new System.Drawing.Point(280, 20);
            pnlCylinderSlide.Name = "pnlCylinderSlide";
            pnlCylinderSlide.Size = new System.Drawing.Size(300, 70);

            pnlLeftGrip.BackColor = System.Drawing.Color.FromArgb(50, 58, 66);
            pnlLeftGrip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlLeftGrip.Location = new System.Drawing.Point(600, 20);
            pnlLeftGrip.Name = "pnlLeftGrip";
            pnlLeftGrip.Size = new System.Drawing.Size(120, 120);

            pnlRightGrip.BackColor = System.Drawing.Color.FromArgb(50, 58, 66);
            pnlRightGrip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlRightGrip.Location = new System.Drawing.Point(730, 20);
            pnlRightGrip.Name = "pnlRightGrip";
            pnlRightGrip.Size = new System.Drawing.Size(120, 120);

            // Seats (left 2 + right 2)
            pnlSeatL1.BackColor = System.Drawing.Color.FromArgb(36, 39, 44);
            pnlSeatL1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlSeatL1.Location = new System.Drawing.Point(320, 140);
            pnlSeatL1.Name = "pnlSeatL1";
            pnlSeatL1.Size = new System.Drawing.Size(90, 80);

            pnlSeatL2.BackColor = System.Drawing.Color.FromArgb(36, 39, 44);
            pnlSeatL2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlSeatL2.Location = new System.Drawing.Point(420, 140);
            pnlSeatL2.Name = "pnlSeatL2";
            pnlSeatL2.Size = new System.Drawing.Size(90, 80);

            pnlSeatR1.BackColor = System.Drawing.Color.FromArgb(36, 39, 44);
            pnlSeatR1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlSeatR1.Location = new System.Drawing.Point(560, 140);
            pnlSeatR1.Name = "pnlSeatR1";
            pnlSeatR1.Size = new System.Drawing.Size(90, 80);

            pnlSeatR2.BackColor = System.Drawing.Color.FromArgb(36, 39, 44);
            pnlSeatR2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlSeatR2.Location = new System.Drawing.Point(660, 140);
            pnlSeatR2.Name = "pnlSeatR2";
            pnlSeatR2.Size = new System.Drawing.Size(90, 80);

            pnlLayoutSurface.Controls.Add(pnlAxisX);
            pnlLayoutSurface.Controls.Add(pnlAxisZ1);
            pnlLayoutSurface.Controls.Add(pnlAxisZ2);
            pnlLayoutSurface.Controls.Add(pnlCylinderSlide);
            pnlLayoutSurface.Controls.Add(pnlLeftGrip);
            pnlLayoutSurface.Controls.Add(pnlRightGrip);
            pnlLayoutSurface.Controls.Add(pnlSeatL1);
            pnlLayoutSurface.Controls.Add(pnlSeatL2);
            pnlLayoutSurface.Controls.Add(pnlSeatR1);
            pnlLayoutSurface.Controls.Add(pnlSeatR2);

            pnlCanvas.Controls.Add(pnlLayoutSurface);

            splitMain.Panel2.Controls.Add(pnlCanvas);

            // Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 640);
            Controls.Add(splitMain);
            Name = "SimulatorDemoForm";
            Text = "WinMachine - Visual Simulation";

            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel1.PerformLayout();
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);

            ResumeLayout(false);
        }

        #endregion
    }
}
