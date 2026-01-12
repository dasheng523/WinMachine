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
            groupBoxPens = new GroupBox();
            BtnLoadPen1Pick = new Button();
            BtnLoadPen2Pick = new Button();
            BtnLoadPen1Place = new Button();
            BtnLoadPen2Place = new Button();
            BtnUnloadPen1Pick = new Button();
            BtnUnloadPen2Pick = new Button();
            BtnUnloadPen1Place = new Button();
            BtnUnloadPen2Place = new Button();
            groupBoxTrays = new GroupBox();
            BtnPickLoadTray = new Button();
            BtnPickUnloadTray = new Button();
            BtnPushLoadTray = new Button();
            BtnPushUnloadTray = new Button();
            groupBoxScanTest = new GroupBox();
            BtnScanSeat12 = new Button();
            BtnScanSeat34 = new Button();
            BtnTestSeat1 = new Button();
            BtnTestSeat2 = new Button();
            BtnTestSeat3 = new Button();
            BtnTestSeat4 = new Button();
            groupBoxTransferSlider = new GroupBox();
            BtnTransferSeat12 = new Button();
            BtnTransferSeat34 = new Button();
            BtnSliderLeft = new Button();
            BtnSliderRight = new Button();
            SuspendLayout();

            // 
            // groupBoxPens
            // 
            groupBoxPens.Controls.Add(BtnUnloadPen2Place);
            groupBoxPens.Controls.Add(BtnUnloadPen1Place);
            groupBoxPens.Controls.Add(BtnUnloadPen2Pick);
            groupBoxPens.Controls.Add(BtnUnloadPen1Pick);
            groupBoxPens.Controls.Add(BtnLoadPen2Place);
            groupBoxPens.Controls.Add(BtnLoadPen1Place);
            groupBoxPens.Controls.Add(BtnLoadPen2Pick);
            groupBoxPens.Controls.Add(BtnLoadPen1Pick);
            groupBoxPens.Location = new Point(18, 18);
            groupBoxPens.Name = "groupBoxPens";
            groupBoxPens.Size = new Size(510, 170);
            groupBoxPens.TabIndex = 0;
            groupBoxPens.TabStop = false;
            groupBoxPens.Text = "吸笔 / 取放料";

            // 
            // BtnLoadPen1Pick
            // 
            BtnLoadPen1Pick.Location = new Point(18, 34);
            BtnLoadPen1Pick.Name = "BtnLoadPen1Pick";
            BtnLoadPen1Pick.Size = new Size(150, 40);
            BtnLoadPen1Pick.TabIndex = 0;
            BtnLoadPen1Pick.Text = "上料笔1取料";
            BtnLoadPen1Pick.UseVisualStyleBackColor = true;
            // 
            // BtnLoadPen2Pick
            // 
            BtnLoadPen2Pick.Location = new Point(174, 34);
            BtnLoadPen2Pick.Name = "BtnLoadPen2Pick";
            BtnLoadPen2Pick.Size = new Size(150, 40);
            BtnLoadPen2Pick.TabIndex = 1;
            BtnLoadPen2Pick.Text = "上料笔2取料";
            BtnLoadPen2Pick.UseVisualStyleBackColor = true;
            // 
            // BtnLoadPen1Place
            // 
            BtnLoadPen1Place.Location = new Point(330, 34);
            BtnLoadPen1Place.Name = "BtnLoadPen1Place";
            BtnLoadPen1Place.Size = new Size(150, 40);
            BtnLoadPen1Place.TabIndex = 2;
            BtnLoadPen1Place.Text = "上料笔1放料";
            BtnLoadPen1Place.UseVisualStyleBackColor = true;
            // 
            // BtnLoadPen2Place
            // 
            BtnLoadPen2Place.Location = new Point(18, 80);
            BtnLoadPen2Place.Name = "BtnLoadPen2Place";
            BtnLoadPen2Place.Size = new Size(150, 40);
            BtnLoadPen2Place.TabIndex = 3;
            BtnLoadPen2Place.Text = "上料笔2放料";
            BtnLoadPen2Place.UseVisualStyleBackColor = true;
            // 
            // BtnUnloadPen1Pick
            // 
            BtnUnloadPen1Pick.Location = new Point(174, 80);
            BtnUnloadPen1Pick.Name = "BtnUnloadPen1Pick";
            BtnUnloadPen1Pick.Size = new Size(150, 40);
            BtnUnloadPen1Pick.TabIndex = 4;
            BtnUnloadPen1Pick.Text = "摆盘笔1取料";
            BtnUnloadPen1Pick.UseVisualStyleBackColor = true;
            // 
            // BtnUnloadPen2Pick
            // 
            BtnUnloadPen2Pick.Location = new Point(330, 80);
            BtnUnloadPen2Pick.Name = "BtnUnloadPen2Pick";
            BtnUnloadPen2Pick.Size = new Size(150, 40);
            BtnUnloadPen2Pick.TabIndex = 5;
            BtnUnloadPen2Pick.Text = "摆盘笔2取料";
            BtnUnloadPen2Pick.UseVisualStyleBackColor = true;
            // 
            // BtnUnloadPen1Place
            // 
            BtnUnloadPen1Place.Location = new Point(18, 126);
            BtnUnloadPen1Place.Name = "BtnUnloadPen1Place";
            BtnUnloadPen1Place.Size = new Size(150, 40);
            BtnUnloadPen1Place.TabIndex = 6;
            BtnUnloadPen1Place.Text = "摆盘笔1放料";
            BtnUnloadPen1Place.UseVisualStyleBackColor = true;
            // 
            // BtnUnloadPen2Place
            // 
            BtnUnloadPen2Place.Location = new Point(174, 126);
            BtnUnloadPen2Place.Name = "BtnUnloadPen2Place";
            BtnUnloadPen2Place.Size = new Size(150, 40);
            BtnUnloadPen2Place.TabIndex = 7;
            BtnUnloadPen2Place.Text = "摆盘笔2放料";
            BtnUnloadPen2Place.UseVisualStyleBackColor = true;

            // 
            // groupBoxTrays
            // 
            groupBoxTrays.Controls.Add(BtnPushUnloadTray);
            groupBoxTrays.Controls.Add(BtnPushLoadTray);
            groupBoxTrays.Controls.Add(BtnPickUnloadTray);
            groupBoxTrays.Controls.Add(BtnPickLoadTray);
            groupBoxTrays.Location = new Point(546, 18);
            groupBoxTrays.Name = "groupBoxTrays";
            groupBoxTrays.Size = new Size(340, 170);
            groupBoxTrays.TabIndex = 1;
            groupBoxTrays.TabStop = false;
            groupBoxTrays.Text = "料盘";

            // 
            // BtnPickLoadTray
            // 
            BtnPickLoadTray.Location = new Point(18, 34);
            BtnPickLoadTray.Name = "BtnPickLoadTray";
            BtnPickLoadTray.Size = new Size(150, 40);
            BtnPickLoadTray.TabIndex = 0;
            BtnPickLoadTray.Text = "取上料盘";
            BtnPickLoadTray.UseVisualStyleBackColor = true;

            // 
            // BtnPickUnloadTray
            // 
            BtnPickUnloadTray.Location = new Point(174, 34);
            BtnPickUnloadTray.Name = "BtnPickUnloadTray";
            BtnPickUnloadTray.Size = new Size(150, 40);
            BtnPickUnloadTray.TabIndex = 1;
            BtnPickUnloadTray.Text = "取下料盘";
            BtnPickUnloadTray.UseVisualStyleBackColor = true;

            // 
            // BtnPushLoadTray
            // 
            BtnPushLoadTray.Location = new Point(18, 80);
            BtnPushLoadTray.Name = "BtnPushLoadTray";
            BtnPushLoadTray.Size = new Size(150, 40);
            BtnPushLoadTray.TabIndex = 2;
            BtnPushLoadTray.Text = "推上料盘";
            BtnPushLoadTray.UseVisualStyleBackColor = true;

            // 
            // BtnPushUnloadTray
            // 
            BtnPushUnloadTray.Location = new Point(174, 80);
            BtnPushUnloadTray.Name = "BtnPushUnloadTray";
            BtnPushUnloadTray.Size = new Size(150, 40);
            BtnPushUnloadTray.TabIndex = 3;
            BtnPushUnloadTray.Text = "推下料盘";
            BtnPushUnloadTray.UseVisualStyleBackColor = true;

            // 
            // groupBoxScanTest
            // 
            groupBoxScanTest.Controls.Add(BtnTestSeat4);
            groupBoxScanTest.Controls.Add(BtnTestSeat3);
            groupBoxScanTest.Controls.Add(BtnTestSeat2);
            groupBoxScanTest.Controls.Add(BtnTestSeat1);
            groupBoxScanTest.Controls.Add(BtnScanSeat34);
            groupBoxScanTest.Controls.Add(BtnScanSeat12);
            groupBoxScanTest.Location = new Point(18, 200);
            groupBoxScanTest.Name = "groupBoxScanTest";
            groupBoxScanTest.Size = new Size(510, 210);
            groupBoxScanTest.TabIndex = 2;
            groupBoxScanTest.TabStop = false;
            groupBoxScanTest.Text = "扫码 / 测试";

            // 
            // BtnScanSeat12
            // 
            BtnScanSeat12.Location = new Point(18, 34);
            BtnScanSeat12.Name = "BtnScanSeat12";
            BtnScanSeat12.Size = new Size(150, 40);
            BtnScanSeat12.TabIndex = 0;
            BtnScanSeat12.Text = "1,2座扫码";
            BtnScanSeat12.UseVisualStyleBackColor = true;

            // 
            // BtnScanSeat34
            // 
            BtnScanSeat34.Location = new Point(174, 34);
            BtnScanSeat34.Name = "BtnScanSeat34";
            BtnScanSeat34.Size = new Size(150, 40);
            BtnScanSeat34.TabIndex = 1;
            BtnScanSeat34.Text = "3,4座扫码";
            BtnScanSeat34.UseVisualStyleBackColor = true;

            // 
            // BtnTestSeat1
            // 
            BtnTestSeat1.Location = new Point(18, 90);
            BtnTestSeat1.Name = "BtnTestSeat1";
            BtnTestSeat1.Size = new Size(150, 40);
            BtnTestSeat1.TabIndex = 2;
            BtnTestSeat1.Text = "1号座测试";
            BtnTestSeat1.UseVisualStyleBackColor = true;

            // 
            // BtnTestSeat2
            // 
            BtnTestSeat2.Location = new Point(174, 90);
            BtnTestSeat2.Name = "BtnTestSeat2";
            BtnTestSeat2.Size = new Size(150, 40);
            BtnTestSeat2.TabIndex = 3;
            BtnTestSeat2.Text = "2号座测试";
            BtnTestSeat2.UseVisualStyleBackColor = true;

            // 
            // BtnTestSeat3
            // 
            BtnTestSeat3.Location = new Point(18, 136);
            BtnTestSeat3.Name = "BtnTestSeat3";
            BtnTestSeat3.Size = new Size(150, 40);
            BtnTestSeat3.TabIndex = 4;
            BtnTestSeat3.Text = "3号座测试";
            BtnTestSeat3.UseVisualStyleBackColor = true;

            // 
            // BtnTestSeat4
            // 
            BtnTestSeat4.Location = new Point(174, 136);
            BtnTestSeat4.Name = "BtnTestSeat4";
            BtnTestSeat4.Size = new Size(150, 40);
            BtnTestSeat4.TabIndex = 5;
            BtnTestSeat4.Text = "4号座测试";
            BtnTestSeat4.UseVisualStyleBackColor = true;

            // 
            // groupBoxTransferSlider
            // 
            groupBoxTransferSlider.Controls.Add(BtnSliderRight);
            groupBoxTransferSlider.Controls.Add(BtnSliderLeft);
            groupBoxTransferSlider.Controls.Add(BtnTransferSeat34);
            groupBoxTransferSlider.Controls.Add(BtnTransferSeat12);
            groupBoxTransferSlider.Location = new Point(546, 200);
            groupBoxTransferSlider.Name = "groupBoxTransferSlider";
            groupBoxTransferSlider.Size = new Size(340, 210);
            groupBoxTransferSlider.TabIndex = 3;
            groupBoxTransferSlider.TabStop = false;
            groupBoxTransferSlider.Text = "转移 / 滑块";

            // 
            // BtnTransferSeat12
            // 
            BtnTransferSeat12.Location = new Point(18, 34);
            BtnTransferSeat12.Name = "BtnTransferSeat12";
            BtnTransferSeat12.Size = new Size(150, 40);
            BtnTransferSeat12.TabIndex = 0;
            BtnTransferSeat12.Text = "1,2座转移";
            BtnTransferSeat12.UseVisualStyleBackColor = true;

            // 
            // BtnTransferSeat34
            // 
            BtnTransferSeat34.Location = new Point(174, 34);
            BtnTransferSeat34.Name = "BtnTransferSeat34";
            BtnTransferSeat34.Size = new Size(150, 40);
            BtnTransferSeat34.TabIndex = 1;
            BtnTransferSeat34.Text = "3,4座转移";
            BtnTransferSeat34.UseVisualStyleBackColor = true;

            // 
            // BtnSliderLeft
            // 
            BtnSliderLeft.Location = new Point(18, 90);
            BtnSliderLeft.Name = "BtnSliderLeft";
            BtnSliderLeft.Size = new Size(150, 40);
            BtnSliderLeft.TabIndex = 2;
            BtnSliderLeft.Text = "左移滑块";
            BtnSliderLeft.UseVisualStyleBackColor = true;

            // 
            // BtnSliderRight
            // 
            BtnSliderRight.Location = new Point(174, 90);
            BtnSliderRight.Name = "BtnSliderRight";
            BtnSliderRight.Size = new Size(150, 40);
            BtnSliderRight.TabIndex = 3;
            BtnSliderRight.Text = "右移滑块";
            BtnSliderRight.UseVisualStyleBackColor = true;

            // 
            // SingleStep
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(910, 440);
            Controls.Add(groupBoxTransferSlider);
            Controls.Add(groupBoxScanTest);
            Controls.Add(groupBoxTrays);
            Controls.Add(groupBoxPens);
            Name = "SingleStep";
            Text = "单步测试";
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxPens;
        private Button BtnLoadPen1Pick;
        private Button BtnLoadPen2Pick;
        private Button BtnLoadPen1Place;
        private Button BtnLoadPen2Place;
        private Button BtnUnloadPen1Pick;
        private Button BtnUnloadPen2Pick;
        private Button BtnUnloadPen1Place;
        private Button BtnUnloadPen2Place;

        private GroupBox groupBoxTrays;
        private Button BtnPickLoadTray;
        private Button BtnPickUnloadTray;
        private Button BtnPushLoadTray;
        private Button BtnPushUnloadTray;

        private GroupBox groupBoxScanTest;
        private Button BtnScanSeat12;
        private Button BtnScanSeat34;
        private Button BtnTestSeat1;
        private Button BtnTestSeat2;
        private Button BtnTestSeat3;
        private Button BtnTestSeat4;

        private GroupBox groupBoxTransferSlider;
        private Button BtnTransferSeat12;
        private Button BtnTransferSeat34;
        private Button BtnSliderLeft;
        private Button BtnSliderRight;
    }
}