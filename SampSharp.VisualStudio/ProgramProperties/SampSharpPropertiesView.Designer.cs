using System.ComponentModel;

namespace SampSharp.VisualStudio.ProgramProperties
{
    partial class SampSharpPropertiesView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.browseRuntimeDirectoryButton = new System.Windows.Forms.Button();
            this.monoLocationTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gameModeTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Specify the locations of your mono runtime:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.gameModeTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.browseRuntimeDirectoryButton);
            this.groupBox1.Controls.Add(this.monoLocationTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(8);
            this.groupBox1.Size = new System.Drawing.Size(646, 231);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "SampSharp";
            // 
            // browseRuntimeDirectoryButton
            // 
            this.browseRuntimeDirectoryButton.Location = new System.Drawing.Point(303, 58);
            this.browseRuntimeDirectoryButton.Name = "browseRuntimeDirectoryButton";
            this.browseRuntimeDirectoryButton.Size = new System.Drawing.Size(25, 22);
            this.browseRuntimeDirectoryButton.TabIndex = 6;
            this.browseRuntimeDirectoryButton.Text = "...";
            this.browseRuntimeDirectoryButton.UseVisualStyleBackColor = true;
            this.browseRuntimeDirectoryButton.Click += new System.EventHandler(this.browseRuntimeDirectoryButton_Click);
            // 
            // monoLocationTextBox
            // 
            this.monoLocationTextBox.Location = new System.Drawing.Point(14, 59);
            this.monoLocationTextBox.Name = "monoLocationTextBox";
            this.monoLocationTextBox.Size = new System.Drawing.Size(283, 20);
            this.monoLocationTextBox.TabIndex = 4;
            this.monoLocationTextBox.Text = "SampSharpGameMode1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Mono Runtime Directory:";
            // 
            // gameModeTextBox
            // 
            this.gameModeTextBox.Location = new System.Drawing.Point(14, 98);
            this.gameModeTextBox.Name = "gameModeTextBox";
            this.gameModeTextBox.Size = new System.Drawing.Size(314, 20);
            this.gameModeTextBox.TabIndex = 8;
            this.gameModeTextBox.Text = "GameMode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Game Mode Entrypoint:";
            // 
            // SampSharpPropertiesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SampSharpPropertiesView";
            this.Size = new System.Drawing.Size(652, 262);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox monoLocationTextBox;
        private System.Windows.Forms.Button browseRuntimeDirectoryButton;
        private System.Windows.Forms.TextBox gameModeTextBox;
        private System.Windows.Forms.Label label2;
    }
}
