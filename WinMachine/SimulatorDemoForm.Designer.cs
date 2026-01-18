
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

        private System.Windows.Forms.Panel pnlAxisX;
        private System.Windows.Forms.Panel pnlAxisY;
        private System.Windows.Forms.Panel pnlAxisZ;
        private System.Windows.Forms.Panel pnlAxisRotate;

        private System.Windows.Forms.Panel pnlCylGripper;
        private System.Windows.Forms.Panel pnlCylClamp;
        private System.Windows.Forms.Panel pnlVac1;

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
            pnlAxisX = new System.Windows.Forms.Panel();
            pnlAxisY = new System.Windows.Forms.Panel();
            pnlAxisZ = new System.Windows.Forms.Panel();
            pnlAxisRotate = new System.Windows.Forms.Panel();
            pnlCylGripper = new System.Windows.Forms.Panel();
            pnlCylClamp = new System.Windows.Forms.Panel();
            pnlVac1 = new System.Windows.Forms.Panel();

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

            // Axis panels (simple blocks)
            pnlAxisX.BackColor = System.Drawing.Color.SteelBlue;
            pnlAxisX.Name = "pnlAxisX";
            pnlAxisX.Size = new System.Drawing.Size(40, 16);
            pnlAxisX.Location = new System.Drawing.Point(40, 60);

            pnlAxisY.BackColor = System.Drawing.Color.MediumSeaGreen;
            pnlAxisY.Name = "pnlAxisY";
            pnlAxisY.Size = new System.Drawing.Size(40, 16);
            pnlAxisY.Location = new System.Drawing.Point(40, 110);

            pnlAxisZ.BackColor = System.Drawing.Color.Orange;
            pnlAxisZ.Name = "pnlAxisZ";
            pnlAxisZ.Size = new System.Drawing.Size(16, 40);
            pnlAxisZ.Location = new System.Drawing.Point(220, 60);

            pnlAxisRotate.BackColor = System.Drawing.Color.MediumPurple;
            pnlAxisRotate.Name = "pnlAxisRotate";
            pnlAxisRotate.Size = new System.Drawing.Size(24, 24);
            pnlAxisRotate.Location = new System.Drawing.Point(220, 140);

            // Cylinder/Vacuum panels
            pnlCylGripper.BackColor = System.Drawing.Color.DimGray;
            pnlCylGripper.Name = "pnlCylGripper";
            pnlCylGripper.Size = new System.Drawing.Size(40, 10);
            pnlCylGripper.Location = new System.Drawing.Point(320, 60);

            pnlCylClamp.BackColor = System.Drawing.Color.DimGray;
            pnlCylClamp.Name = "pnlCylClamp";
            pnlCylClamp.Size = new System.Drawing.Size(40, 10);
            pnlCylClamp.Location = new System.Drawing.Point(320, 90);

            pnlVac1.BackColor = System.Drawing.Color.DimGray;
            pnlVac1.Name = "pnlVac1";
            pnlVac1.Size = new System.Drawing.Size(18, 18);
            pnlVac1.Location = new System.Drawing.Point(320, 130);

            pnlCanvas.Controls.Add(pnlAxisX);
            pnlCanvas.Controls.Add(pnlAxisY);
            pnlCanvas.Controls.Add(pnlAxisZ);
            pnlCanvas.Controls.Add(pnlAxisRotate);
            pnlCanvas.Controls.Add(pnlCylGripper);
            pnlCanvas.Controls.Add(pnlCylClamp);
            pnlCanvas.Controls.Add(pnlVac1);

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
