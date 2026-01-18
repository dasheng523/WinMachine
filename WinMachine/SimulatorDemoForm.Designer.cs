
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

        private WinMachine.TransferStationView transferStationView;

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
            transferStationView = new WinMachine.TransferStationView();

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

            transferStationView.Dock = System.Windows.Forms.DockStyle.Fill;
            transferStationView.Name = "transferStationView";
            pnlCanvas.Controls.Add(transferStationView);

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
