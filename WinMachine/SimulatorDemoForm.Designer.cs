
namespace WinMachine
{
    partial class SimulatorDemoForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.pnl_X = new System.Windows.Forms.Panel();
            this.pnl_Z = new System.Windows.Forms.Panel();
            this.pnl_Pen = new System.Windows.Forms.Panel();
            this.btn_MoveX = new System.Windows.Forms.Button();
            this.btn_MoveZ = new System.Windows.Forms.Button();
            this.btn_Home = new System.Windows.Forms.Button();
            this.lbl_Title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pnl_X
            // 
            this.pnl_X.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pnl_X.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_X.Location = new System.Drawing.Point(50, 80);
            this.pnl_X.Name = "pnl_X";
            this.pnl_X.Size = new System.Drawing.Size(600, 60);
            this.pnl_X.TabIndex = 0;
            // 
            // pnl_Z
            // 
            this.pnl_Z.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.pnl_Z.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_Z.Location = new System.Drawing.Point(700, 80);
            this.pnl_Z.Name = "pnl_Z";
            this.pnl_Z.Size = new System.Drawing.Size(60, 400);
            this.pnl_Z.TabIndex = 1;
            // 
            // pnl_Pen
            // 
            this.pnl_Pen.BackColor = System.Drawing.Color.Gold;
            this.pnl_Pen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_Pen.Location = new System.Drawing.Point(800, 80);
            this.pnl_Pen.Name = "pnl_Pen";
            this.pnl_Pen.Size = new System.Drawing.Size(40, 20);
            this.pnl_Pen.TabIndex = 2;
            // 
            // btn_MoveX
            // 
            this.btn_MoveX.Location = new System.Drawing.Point(50, 500);
            this.btn_MoveX.Name = "btn_MoveX";
            this.btn_MoveX.Size = new System.Drawing.Size(120, 40);
            this.btn_MoveX.TabIndex = 3;
            this.btn_MoveX.Text = "Move X to 80%";
            this.btn_MoveX.UseVisualStyleBackColor = true;
            this.btn_MoveX.Click += new System.EventHandler(this.btn_MoveX_Click);
            // 
            // btn_MoveZ
            // 
            this.btn_MoveZ.Location = new System.Drawing.Point(190, 500);
            this.btn_MoveZ.Name = "btn_MoveZ";
            this.btn_MoveZ.Size = new System.Drawing.Size(120, 40);
            this.btn_MoveZ.TabIndex = 4;
            this.btn_MoveZ.Text = "Move Z to 50%";
            this.btn_MoveZ.UseVisualStyleBackColor = true;
            this.btn_MoveZ.Click += new System.EventHandler(this.btn_MoveZ_Click);
            // 
            // btn_Home
            // 
            this.btn_Home.Location = new System.Drawing.Point(330, 500);
            this.btn_Home.Name = "btn_Home";
            this.btn_Home.Size = new System.Drawing.Size(120, 40);
            this.btn_Home.TabIndex = 5;
            this.btn_Home.Text = "Go Home";
            this.btn_Home.UseVisualStyleBackColor = true;
            this.btn_Home.Click += new System.EventHandler(this.btn_Home_Click);
            // 
            // lbl_Title
            // 
            this.lbl_Title.AutoSize = true;
            this.lbl_Title.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbl_Title.ForeColor = System.Drawing.Color.White;
            this.lbl_Title.Location = new System.Drawing.Point(44, 25);
            this.lbl_Title.Name = "lbl_Title";
            this.lbl_Title.Size = new System.Drawing.Size(325, 30);
            this.lbl_Title.TabIndex = 6;
            this.lbl_Title.Text = "Simulator DSL Demo (Rx Driven)";
            // 
            // SimulatorDemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.lbl_Title);
            this.Controls.Add(this.btn_Home);
            this.Controls.Add(this.btn_MoveZ);
            this.Controls.Add(this.btn_MoveX);
            this.Controls.Add(this.pnl_Pen);
            this.Controls.Add(this.pnl_Z);
            this.Controls.Add(this.pnl_X);
            this.Name = "SimulatorDemoForm";
            this.Text = "WinMachine Simulator";
            this.Load += new System.EventHandler(this.SimulatorDemoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnl_X;
        private System.Windows.Forms.Panel pnl_Z;
        private System.Windows.Forms.Panel pnl_Pen;
        private System.Windows.Forms.Button btn_MoveX;
        private System.Windows.Forms.Button btn_MoveZ;
        private System.Windows.Forms.Button btn_Home;
        private System.Windows.Forms.Label lbl_Title;
    }
}
