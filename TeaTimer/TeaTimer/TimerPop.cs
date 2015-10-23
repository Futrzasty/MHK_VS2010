using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TeaTimer
{
	/// <summary>
	/// Summary description for TimerPop.
	/// </summary>
	internal class TimerPop : FormFader.Fader
	{
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.PictureBox pictureBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TimerPop(string sName, string sIcon, string sSoundFile) : this(sName,sIcon,sSoundFile,false) {}
		public TimerPop(string sName, string sIcon, string sSoundFile, bool bMovable)
		{
			InitializeComponent();

			this.FormBorderStyle=bMovable?FormBorderStyle.FixedToolWindow:FormBorderStyle.None;
			lblName.Text=sName;
			this.Text=sName+" (Tea Timer)";
			if(sIcon!=null) TeaTimer.dispIcon(pictureBox1,sIcon);
			this.FadeForm(Fade.Up);
			if(sSoundFile!=null) TeaTimer.playSound(sSoundFile);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TimerPop));
			this.lblName = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.BackColor = System.Drawing.Color.Transparent;
			this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblName.ForeColor = System.Drawing.Color.FromArgb(((System.Byte)(205)), ((System.Byte)(0)), ((System.Byte)(0)));
			this.lblName.Location = new System.Drawing.Point(0, 10);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(264, 32);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "lblName";
			this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnOk
			// 
			this.btnOk.BackColor = System.Drawing.Color.Transparent;
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnOk.ForeColor = System.Drawing.Color.Black;
			this.btnOk.Location = new System.Drawing.Point(176, 72);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(120, 24);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(264, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(40, 40);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 3;
			this.pictureBox1.TabStop = false;
			// 
			// TimerPop
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnOk;
			this.ClientSize = new System.Drawing.Size(314, 111);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.lblName);
			this.ForeColor = System.Drawing.Color.DarkRed;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TimerPop";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TimerPop";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			//this.FadeForm(Fade.Down);
			this.Close();
			TeaTimer.stopSound();
		}
	}
}
