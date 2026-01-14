namespace WinMachine
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnZController = new Button();
            BtnSystemOptions = new Button();
            BtnSingleStep = new Button();
            SuspendLayout();
            // 
            // BtnZController
            // 
            BtnZController.Location = new Point(547, 108);
            BtnZController.Name = "BtnZController";
            BtnZController.Size = new Size(94, 29);
            BtnZController.TabIndex = 0;
            BtnZController.Text = "正运动控制";
            BtnZController.UseVisualStyleBackColor = true;
            // 
            // BtnSystemOptions
            // 
            BtnSystemOptions.Location = new Point(547, 157);
            BtnSystemOptions.Name = "BtnSystemOptions";
            BtnSystemOptions.Size = new Size(94, 29);
            BtnSystemOptions.TabIndex = 1;
            BtnSystemOptions.Text = "系统配置";
            BtnSystemOptions.UseVisualStyleBackColor = true;

            // 
            // BtnSingleStep
            // 
            BtnSingleStep.Location = new Point(547, 206);
            BtnSingleStep.Name = "BtnSingleStep";
            BtnSingleStep.Size = new Size(94, 29);
            BtnSingleStep.TabIndex = 2;
            BtnSingleStep.Text = "单步测试";
            BtnSingleStep.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(BtnSingleStep);
            Controls.Add(BtnSystemOptions);
            Controls.Add(BtnZController);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnZController;
        private Button BtnSystemOptions;
        private Button BtnSingleStep;
    }
}


