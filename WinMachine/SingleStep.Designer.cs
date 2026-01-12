namespace WinMachine
{
    partial class SingleStep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnSuckOnDisk = new Button();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            SuspendLayout();
            // 
            // BtnSuckOnDisk
            // 
            BtnSuckOnDisk.Location = new Point(37, 39);
            BtnSuckOnDisk.Name = "BtnSuckOnDisk";
            BtnSuckOnDisk.Size = new Size(120, 40);
            BtnSuckOnDisk.TabIndex = 0;
            BtnSuckOnDisk.Text = "上料笔1取料";
            BtnSuckOnDisk.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(163, 39);
            button1.Name = "button1";
            button1.Size = new Size(120, 40);
            button1.TabIndex = 1;
            button1.Text = "上料笔2取料";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(415, 39);
            button2.Name = "button2";
            button2.Size = new Size(120, 40);
            button2.TabIndex = 3;
            button2.Text = "上料笔2放料";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(289, 39);
            button3.Name = "button3";
            button3.Size = new Size(120, 40);
            button3.TabIndex = 2;
            button3.Text = "上料笔1放料";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(415, 85);
            button4.Name = "button4";
            button4.Size = new Size(120, 40);
            button4.TabIndex = 7;
            button4.Text = "摆盘笔2放料";
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(289, 85);
            button5.Name = "button5";
            button5.Size = new Size(120, 40);
            button5.TabIndex = 6;
            button5.Text = "摆盘笔1放料";
            button5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Location = new Point(163, 85);
            button6.Name = "button6";
            button6.Size = new Size(120, 40);
            button6.TabIndex = 5;
            button6.Text = "摆盘笔2取料";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new Point(37, 85);
            button7.Name = "button7";
            button7.Size = new Size(120, 40);
            button7.TabIndex = 4;
            button7.Text = "摆盘笔1取料";
            button7.UseVisualStyleBackColor = true;
            // 
            // SingleStep
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 503);
            Controls.Add(button4);
            Controls.Add(button5);
            Controls.Add(button6);
            Controls.Add(button7);
            Controls.Add(button2);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(BtnSuckOnDisk);
            Name = "SingleStep";
            Text = "单步测试";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnSuckOnDisk;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
    }
}