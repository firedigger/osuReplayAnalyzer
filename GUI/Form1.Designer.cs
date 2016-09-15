namespace GUI
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.chooseReplayButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.chooseReplaysFolderButton = new System.Windows.Forms.Button();
            this.compareAllReplaysButton = new System.Windows.Forms.Button();
            this.analyzeReplayButton = new System.Windows.Forms.Button();
            this.compareReplaysButton = new System.Windows.Forms.Button();
            this.analyzeReplaysButton = new System.Windows.Forms.Button();
            this.openOsuDBButton = new System.Windows.Forms.Button();
            this.chooseMapButton = new System.Windows.Forms.Button();
            this.reportSaveButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.alertOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.saveReportToFileCheckBox = new System.Windows.Forms.CheckBox();
            this.currentTaskLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chooseReplayButton
            // 
            this.chooseReplayButton.Location = new System.Drawing.Point(12, 67);
            this.chooseReplayButton.Name = "chooseReplayButton";
            this.chooseReplayButton.Size = new System.Drawing.Size(104, 33);
            this.chooseReplayButton.TabIndex = 0;
            this.chooseReplayButton.Text = "Choose replays...";
            this.chooseReplayButton.UseVisualStyleBackColor = true;
            this.chooseReplayButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(602, 539);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 1;
            this.exitButton.Text = "exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // chooseReplaysFolderButton
            // 
            this.chooseReplaysFolderButton.Location = new System.Drawing.Point(184, 67);
            this.chooseReplaysFolderButton.Name = "chooseReplaysFolderButton";
            this.chooseReplaysFolderButton.Size = new System.Drawing.Size(147, 52);
            this.chooseReplaysFolderButton.TabIndex = 2;
            this.chooseReplaysFolderButton.Text = "Choose folder with replays...";
            this.chooseReplaysFolderButton.UseVisualStyleBackColor = true;
            this.chooseReplaysFolderButton.Click += new System.EventHandler(this.chooseReplaysFolderButton_Click);
            // 
            // compareAllReplaysButton
            // 
            this.compareAllReplaysButton.Location = new System.Drawing.Point(350, 310);
            this.compareAllReplaysButton.Name = "compareAllReplaysButton";
            this.compareAllReplaysButton.Size = new System.Drawing.Size(143, 40);
            this.compareAllReplaysButton.TabIndex = 3;
            this.compareAllReplaysButton.Text = "Compare all replays in folder";
            this.compareAllReplaysButton.UseVisualStyleBackColor = true;
            // 
            // analyzeReplayButton
            // 
            this.analyzeReplayButton.Location = new System.Drawing.Point(12, 127);
            this.analyzeReplayButton.Name = "analyzeReplayButton";
            this.analyzeReplayButton.Size = new System.Drawing.Size(104, 53);
            this.analyzeReplayButton.TabIndex = 4;
            this.analyzeReplayButton.Text = "Analyze selected replays";
            this.analyzeReplayButton.UseVisualStyleBackColor = true;
            this.analyzeReplayButton.Click += new System.EventHandler(this.analyzeReplayButton_Click);
            // 
            // compareReplaysButton
            // 
            this.compareReplaysButton.Location = new System.Drawing.Point(184, 329);
            this.compareReplaysButton.Name = "compareReplaysButton";
            this.compareReplaysButton.Size = new System.Drawing.Size(109, 23);
            this.compareReplaysButton.TabIndex = 5;
            this.compareReplaysButton.Text = "Compare replays";
            this.compareReplaysButton.UseVisualStyleBackColor = true;
            // 
            // analyzeReplaysButton
            // 
            this.analyzeReplaysButton.Location = new System.Drawing.Point(184, 127);
            this.analyzeReplaysButton.Name = "analyzeReplaysButton";
            this.analyzeReplaysButton.Size = new System.Drawing.Size(147, 53);
            this.analyzeReplaysButton.TabIndex = 6;
            this.analyzeReplaysButton.Text = "Analyze replays in folder";
            this.analyzeReplaysButton.UseVisualStyleBackColor = true;
            this.analyzeReplaysButton.Click += new System.EventHandler(this.analyzeReplaysButton_Click);
            // 
            // openOsuDBButton
            // 
            this.openOsuDBButton.Location = new System.Drawing.Point(549, 67);
            this.openOsuDBButton.Name = "openOsuDBButton";
            this.openOsuDBButton.Size = new System.Drawing.Size(75, 23);
            this.openOsuDBButton.TabIndex = 7;
            this.openOsuDBButton.Text = "open osuDB";
            this.openOsuDBButton.UseVisualStyleBackColor = true;
            this.openOsuDBButton.Click += new System.EventHandler(this.openOsuDBButton_Click);
            // 
            // chooseMapButton
            // 
            this.chooseMapButton.Location = new System.Drawing.Point(371, 67);
            this.chooseMapButton.Name = "chooseMapButton";
            this.chooseMapButton.Size = new System.Drawing.Size(122, 23);
            this.chooseMapButton.TabIndex = 8;
            this.chooseMapButton.Text = "Choose map(.osu)";
            this.chooseMapButton.UseVisualStyleBackColor = true;
            this.chooseMapButton.Click += new System.EventHandler(this.chooseMapButton_Click);
            // 
            // reportSaveButton
            // 
            this.reportSaveButton.Location = new System.Drawing.Point(17, 309);
            this.reportSaveButton.Name = "reportSaveButton";
            this.reportSaveButton.Size = new System.Drawing.Size(104, 43);
            this.reportSaveButton.TabIndex = 9;
            this.reportSaveButton.Text = "Report save location";
            this.reportSaveButton.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(17, 536);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(262, 23);
            this.progressBar1.TabIndex = 10;
            this.progressBar1.Value = 60;
            // 
            // alertOutputCheckBox
            // 
            this.alertOutputCheckBox.AutoSize = true;
            this.alertOutputCheckBox.Checked = true;
            this.alertOutputCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alertOutputCheckBox.Location = new System.Drawing.Point(12, 233);
            this.alertOutputCheckBox.Name = "alertOutputCheckBox";
            this.alertOutputCheckBox.Size = new System.Drawing.Size(80, 17);
            this.alertOutputCheckBox.TabIndex = 11;
            this.alertOutputCheckBox.Text = "Alert output";
            this.alertOutputCheckBox.UseVisualStyleBackColor = true;
            // 
            // saveReportToFileCheckBox
            // 
            this.saveReportToFileCheckBox.AutoSize = true;
            this.saveReportToFileCheckBox.Checked = true;
            this.saveReportToFileCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.saveReportToFileCheckBox.Location = new System.Drawing.Point(12, 270);
            this.saveReportToFileCheckBox.Name = "saveReportToFileCheckBox";
            this.saveReportToFileCheckBox.Size = new System.Drawing.Size(109, 17);
            this.saveReportToFileCheckBox.TabIndex = 12;
            this.saveReportToFileCheckBox.Text = "Save report to file";
            this.saveReportToFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // currentTaskLabel
            // 
            this.currentTaskLabel.AutoSize = true;
            this.currentTaskLabel.Location = new System.Drawing.Point(197, 520);
            this.currentTaskLabel.Name = "currentTaskLabel";
            this.currentTaskLabel.Size = new System.Drawing.Size(0, 13);
            this.currentTaskLabel.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 574);
            this.Controls.Add(this.currentTaskLabel);
            this.Controls.Add(this.saveReportToFileCheckBox);
            this.Controls.Add(this.alertOutputCheckBox);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.reportSaveButton);
            this.Controls.Add(this.chooseMapButton);
            this.Controls.Add(this.openOsuDBButton);
            this.Controls.Add(this.analyzeReplaysButton);
            this.Controls.Add(this.compareReplaysButton);
            this.Controls.Add(this.analyzeReplayButton);
            this.Controls.Add(this.compareAllReplaysButton);
            this.Controls.Add(this.chooseReplaysFolderButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.chooseReplayButton);
            this.Name = "Form1";
            this.Text = "osuReplayAnalyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button chooseReplayButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button chooseReplaysFolderButton;
        private System.Windows.Forms.Button compareAllReplaysButton;
        private System.Windows.Forms.Button analyzeReplayButton;
        private System.Windows.Forms.Button compareReplaysButton;
        private System.Windows.Forms.Button analyzeReplaysButton;
        private System.Windows.Forms.Button openOsuDBButton;
        private System.Windows.Forms.Button chooseMapButton;
        private System.Windows.Forms.Button reportSaveButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox alertOutputCheckBox;
        private System.Windows.Forms.CheckBox saveReportToFileCheckBox;
        private System.Windows.Forms.Label currentTaskLabel;
    }
}

