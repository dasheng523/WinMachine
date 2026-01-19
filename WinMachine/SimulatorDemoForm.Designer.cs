
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
            splitMain = new SplitContainer();
            lstScenarios = new ListBox();
            btnRun = new Button();
            btnCancel = new Button();
            lblActiveStep = new Label();
            txtLog = new TextBox();
            pnlCanvas = new Panel();
            pnlLayoutSurface = new Panel();
            pnlAxisX = new Panel();
            pnlAxisZ1 = new Panel();
            pnlAxisZ2 = new Panel();
            pnlCylinderSlide = new Panel();
            pnlLeftGrip = new Panel();
            pnlRightGrip = new Panel();
            pnlSeatL1 = new Panel();
            pnlSeatL2 = new Panel();
            pnlSeatR1 = new Panel();
            pnlSeatR2 = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            pnlCanvas.SuspendLayout();
            pnlLayoutSurface.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 0);
            splitMain.Margin = new Padding(4);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(lstScenarios);
            splitMain.Panel1.Controls.Add(btnRun);
            splitMain.Panel1.Controls.Add(btnCancel);
            splitMain.Panel1.Controls.Add(lblActiveStep);
            splitMain.Panel1.Controls.Add(txtLog);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlCanvas);
            splitMain.Size = new Size(1414, 753);
            splitMain.SplitterDistance = 156;
            splitMain.SplitterWidth = 5;
            splitMain.TabIndex = 0;
            // 
            // lstScenarios
            // 
            lstScenarios.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstScenarios.IntegralHeight = false;
            lstScenarios.Location = new Point(15, 14);
            lstScenarios.Margin = new Padding(4);
            lstScenarios.Name = "lstScenarios";
            lstScenarios.Size = new Size(431, 305);
            lstScenarios.TabIndex = 0;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(15, 329);
            btnRun.Margin = new Padding(4);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(154, 38);
            btnRun.TabIndex = 1;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(180, 329);
            btnCancel.Margin = new Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(154, 38);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblActiveStep
            // 
            lblActiveStep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblActiveStep.Location = new Point(15, 376);
            lblActiveStep.Margin = new Padding(4, 0, 4, 0);
            lblActiveStep.Name = "lblActiveStep";
            lblActiveStep.Size = new Size(432, 47);
            lblActiveStep.TabIndex = 3;
            lblActiveStep.Text = "Active: -";
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(15, 428);
            txtLog.Margin = new Padding(4);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(431, 940);
            txtLog.TabIndex = 4;
            // 
            // pnlCanvas
            // 
            pnlCanvas.BackColor = Color.FromArgb(30, 30, 30);
            pnlCanvas.Controls.Add(pnlLayoutSurface);
            pnlCanvas.Dock = DockStyle.Fill;
            pnlCanvas.Location = new Point(0, 0);
            pnlCanvas.Margin = new Padding(4);
            pnlCanvas.Name = "pnlCanvas";
            pnlCanvas.Size = new Size(1253, 753);
            pnlCanvas.TabIndex = 0;
            // 
            // pnlLayoutSurface
            // 
            pnlLayoutSurface.BackColor = Color.FromArgb(24, 24, 24);
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
            pnlLayoutSurface.Dock = DockStyle.Fill;
            pnlLayoutSurface.Location = new Point(0, 0);
            pnlLayoutSurface.Margin = new Padding(4);
            pnlLayoutSurface.Name = "pnlLayoutSurface";
            pnlLayoutSurface.Size = new Size(1253, 753);
            pnlLayoutSurface.TabIndex = 0;
            // 
            // pnlAxisX
            // 
            pnlAxisX.BackColor = Color.FromArgb(45, 52, 60);
            pnlAxisX.BorderStyle = BorderStyle.FixedSingle;
            pnlAxisX.Location = new Point(843, 457);
            pnlAxisX.Margin = new Padding(4);
            pnlAxisX.Name = "pnlAxisX";
            pnlAxisX.Size = new Size(308, 82);
            pnlAxisX.TabIndex = 0;
            // 
            // pnlAxisZ1
            // 
            pnlAxisZ1.BackColor = Color.FromArgb(45, 52, 60);
            pnlAxisZ1.BorderStyle = BorderStyle.FixedSingle;
            pnlAxisZ1.Location = new Point(843, 551);
            pnlAxisZ1.Margin = new Padding(4);
            pnlAxisZ1.Name = "pnlAxisZ1";
            pnlAxisZ1.Size = new Size(308, 82);
            pnlAxisZ1.TabIndex = 1;
            // 
            // pnlAxisZ2
            // 
            pnlAxisZ2.BackColor = Color.FromArgb(45, 52, 60);
            pnlAxisZ2.BorderStyle = BorderStyle.FixedSingle;
            pnlAxisZ2.Location = new Point(843, 645);
            pnlAxisZ2.Margin = new Padding(4);
            pnlAxisZ2.Name = "pnlAxisZ2";
            pnlAxisZ2.Size = new Size(308, 82);
            pnlAxisZ2.TabIndex = 2;
            // 
            // pnlCylinderSlide
            // 
            pnlCylinderSlide.BackColor = Color.FromArgb(50, 58, 66);
            pnlCylinderSlide.BorderStyle = BorderStyle.FixedSingle;
            pnlCylinderSlide.Location = new Point(399, 64);
            pnlCylinderSlide.Margin = new Padding(4);
            pnlCylinderSlide.Name = "pnlCylinderSlide";
            pnlCylinderSlide.Size = new Size(385, 141);
            pnlCylinderSlide.TabIndex = 3;
            // 
            // pnlLeftGrip
            // 
            pnlLeftGrip.BackColor = Color.FromArgb(50, 58, 66);
            pnlLeftGrip.BorderStyle = BorderStyle.FixedSingle;
            pnlLeftGrip.Location = new Point(193, 64);
            pnlLeftGrip.Margin = new Padding(4);
            pnlLeftGrip.Name = "pnlLeftGrip";
            pnlLeftGrip.Size = new Size(154, 141);
            pnlLeftGrip.TabIndex = 4;
            // 
            // pnlRightGrip
            // 
            pnlRightGrip.BackColor = Color.FromArgb(50, 58, 66);
            pnlRightGrip.BorderStyle = BorderStyle.FixedSingle;
            pnlRightGrip.Location = new Point(836, 64);
            pnlRightGrip.Margin = new Padding(4);
            pnlRightGrip.Name = "pnlRightGrip";
            pnlRightGrip.Size = new Size(154, 141);
            pnlRightGrip.TabIndex = 5;
            // 
            // pnlSeatL1
            // 
            pnlSeatL1.BackColor = Color.FromArgb(36, 39, 44);
            pnlSeatL1.BorderStyle = BorderStyle.FixedSingle;
            pnlSeatL1.Location = new Point(164, 547);
            pnlSeatL1.Margin = new Padding(4);
            pnlSeatL1.Name = "pnlSeatL1";
            pnlSeatL1.Size = new Size(115, 94);
            pnlSeatL1.TabIndex = 6;
            // 
            // pnlSeatL2
            // 
            pnlSeatL2.BackColor = Color.FromArgb(36, 39, 44);
            pnlSeatL2.BorderStyle = BorderStyle.FixedSingle;
            pnlSeatL2.Location = new Point(293, 547);
            pnlSeatL2.Margin = new Padding(4);
            pnlSeatL2.Name = "pnlSeatL2";
            pnlSeatL2.Size = new Size(115, 94);
            pnlSeatL2.TabIndex = 7;
            // 
            // pnlSeatR1
            // 
            pnlSeatR1.BackColor = Color.FromArgb(36, 39, 44);
            pnlSeatR1.BorderStyle = BorderStyle.FixedSingle;
            pnlSeatR1.Location = new Point(473, 547);
            pnlSeatR1.Margin = new Padding(4);
            pnlSeatR1.Name = "pnlSeatR1";
            pnlSeatR1.Size = new Size(115, 94);
            pnlSeatR1.TabIndex = 8;
            // 
            // pnlSeatR2
            // 
            pnlSeatR2.BackColor = Color.FromArgb(36, 39, 44);
            pnlSeatR2.BorderStyle = BorderStyle.FixedSingle;
            pnlSeatR2.Location = new Point(602, 547);
            pnlSeatR2.Margin = new Padding(4);
            pnlSeatR2.Name = "pnlSeatR2";
            pnlSeatR2.Size = new Size(115, 94);
            pnlSeatR2.TabIndex = 9;
            // 
            // SimulatorDemoForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1414, 753);
            Controls.Add(splitMain);
            Margin = new Padding(4);
            Name = "SimulatorDemoForm";
            Text = "WinMachine - Visual Simulation";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel1.PerformLayout();
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            pnlCanvas.ResumeLayout(false);
            pnlLayoutSurface.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlAxisX;
        private Panel pnlAxisZ1;
        private Panel pnlAxisZ2;
    }
}
