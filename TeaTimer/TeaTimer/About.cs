using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TeaTimer
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
	public class About : FormFader.Fader
	{
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.Button btnReadme;
		private System.Windows.Forms.Button btnCl;
		private System.Windows.Forms.Button btnTodo;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel lnkWebPage;
		private System.Windows.Forms.LinkLabel lnkGNU;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.LinkLabel lnkFeatures;
		private System.Windows.Forms.LinkLabel lnkBugs;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public About()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			lblVersion.Text+=TeaTimer.VERSION;

			this.FadeForm(Fade.Up);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(About));
			this.lblVersion = new System.Windows.Forms.Label();
			this.btnClose = new System.Windows.Forms.Button();
			this.btnReadme = new System.Windows.Forms.Button();
			this.btnCl = new System.Windows.Forms.Button();
			this.btnTodo = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.lnkGNU = new System.Windows.Forms.LinkLabel();
			this.lnkWebPage = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.lnkFeatures = new System.Windows.Forms.LinkLabel();
			this.lnkBugs = new System.Windows.Forms.LinkLabel();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblVersion
			// 
			this.lblVersion.BackColor = System.Drawing.Color.Transparent;
			this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblVersion.ForeColor = System.Drawing.Color.Black;
			this.lblVersion.Location = new System.Drawing.Point(16, 8);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(232, 32);
			this.lblVersion.TabIndex = 1;
			this.lblVersion.Text = "Tea Timer ";
			// 
			// btnClose
			// 
			this.btnClose.BackColor = System.Drawing.Color.Transparent;
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnClose.ForeColor = System.Drawing.Color.Black;
			this.btnClose.Location = new System.Drawing.Point(16, 112);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(288, 23);
			this.btnClose.TabIndex = 6;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// btnReadme
			// 
			this.btnReadme.BackColor = System.Drawing.Color.Transparent;
			this.btnReadme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnReadme.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnReadme.ForeColor = System.Drawing.Color.Black;
			this.btnReadme.Location = new System.Drawing.Point(16, 144);
			this.btnReadme.Name = "btnReadme";
			this.btnReadme.TabIndex = 8;
			this.btnReadme.Text = "Readme";
			this.btnReadme.Click += new System.EventHandler(this.btnReadme_Click);
			// 
			// btnCl
			// 
			this.btnCl.BackColor = System.Drawing.Color.Transparent;
			this.btnCl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnCl.ForeColor = System.Drawing.Color.Black;
			this.btnCl.Location = new System.Drawing.Point(120, 144);
			this.btnCl.Name = "btnCl";
			this.btnCl.Size = new System.Drawing.Size(80, 23);
			this.btnCl.TabIndex = 9;
			this.btnCl.Text = "Change Log";
			this.btnCl.Click += new System.EventHandler(this.btnCl_Click);
			// 
			// btnTodo
			// 
			this.btnTodo.BackColor = System.Drawing.Color.Transparent;
			this.btnTodo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTodo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnTodo.ForeColor = System.Drawing.Color.Black;
			this.btnTodo.Location = new System.Drawing.Point(232, 144);
			this.btnTodo.Name = "btnTodo";
			this.btnTodo.TabIndex = 10;
			this.btnTodo.Text = "Todo";
			this.btnTodo.Click += new System.EventHandler(this.btnTodo_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(264, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(40, 40);
			this.pictureBox1.TabIndex = 11;
			this.pictureBox1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.AutoScroll = true;
			this.panel1.BackColor = System.Drawing.Color.LightYellow;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.linkLabel1);
			this.panel1.Controls.Add(this.lnkGNU);
			this.panel1.Controls.Add(this.lnkWebPage);
			this.panel1.Controls.Add(this.label2);
			this.panel1.ForeColor = System.Drawing.Color.Black;
			this.panel1.Location = new System.Drawing.Point(24, 48);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(264, 56);
			this.panel1.TabIndex = 12;
			// 
			// linkLabel1
			// 
			this.linkLabel1.LinkColor = System.Drawing.Color.MediumBlue;
			this.linkLabel1.Location = new System.Drawing.Point(8, 48);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(224, 14);
			this.linkLabel1.TabIndex = 3;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Audio playback is based off code by Slain.";
			this.linkLabel1.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			// 
			// lnkGNU
			// 
			this.lnkGNU.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkGNU.Location = new System.Drawing.Point(8, 32);
			this.lnkGNU.Name = "lnkGNU";
			this.lnkGNU.Size = new System.Drawing.Size(176, 16);
			this.lnkGNU.TabIndex = 2;
			this.lnkGNU.TabStop = true;
			this.lnkGNU.Text = "Licensed under the GNU GPL";
			this.lnkGNU.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkGNU.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGNU_LinkClicked);
			// 
			// lnkWebPage
			// 
			this.lnkWebPage.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkWebPage.Location = new System.Drawing.Point(8, 16);
			this.lnkWebPage.Name = "lnkWebPage";
			this.lnkWebPage.Size = new System.Drawing.Size(144, 16);
			this.lnkWebPage.TabIndex = 1;
			this.lnkWebPage.TabStop = true;
			this.lnkWebPage.Text = "http://www.ericeubank.com";
			this.lnkWebPage.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkWebPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebPage_LinkClicked);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(176, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Copyright © 2005 Eric W. Eubank";
			// 
			// lnkFeatures
			// 
			this.lnkFeatures.BackColor = System.Drawing.Color.Transparent;
			this.lnkFeatures.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkFeatures.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.Location = new System.Drawing.Point(152, 168);
			this.lnkFeatures.Name = "lnkFeatures";
			this.lnkFeatures.Size = new System.Drawing.Size(104, 16);
			this.lnkFeatures.TabIndex = 1009;
			this.lnkFeatures.TabStop = true;
			this.lnkFeatures.Text = "Request a feature.";
			this.lnkFeatures.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkFeatures.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFeatures_LinkClicked);
			// 
			// lnkBugs
			// 
			this.lnkBugs.BackColor = System.Drawing.Color.Transparent;
			this.lnkBugs.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkBugs.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.Location = new System.Drawing.Point(64, 168);
			this.lnkBugs.Name = "lnkBugs";
			this.lnkBugs.Size = new System.Drawing.Size(80, 16);
			this.lnkBugs.TabIndex = 1008;
			this.lnkBugs.TabStop = true;
			this.lnkBugs.Text = "Report a bug.";
			this.lnkBugs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkBugs.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBugs_LinkClicked);
			// 
			// About
			// 
			this.AcceptButton = this.btnClose;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(320, 183);
			this.Controls.Add(this.lnkFeatures);
			this.Controls.Add(this.lnkBugs);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.btnTodo);
			this.Controls.Add(this.btnCl);
			this.Controls.Add(this.btnReadme);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.lblVersion);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "About";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About Tea Timer";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			//this.FadeForm(Fade.Down);
			this.Close();
		}

		private void btnReadme_Click(object sender, System.EventArgs e)
		{
			new RtfBox("Tea Timer Readme","README.txt",RichTextBoxStreamType.PlainText).Show();
		}

		private void btnCl_Click(object sender, System.EventArgs e)
		{
			new RtfBox("Tea Timer Change Log","CHANGELOG.txt",RichTextBoxStreamType.PlainText).Show();
		}

		private void btnTodo_Click(object sender, System.EventArgs e)
		{
			new RtfBox("Tea Timer Todo","TODO.txt",RichTextBoxStreamType.PlainText).Show();
		}

		private void lnkWebPage_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.ericeubank.com");
		}

		private void lnkGNU_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.gnu.org/copyleft/gpl.html");
		}

		private void lnkBugs_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://sourceforge.net/tracker/?func=browse&group_id=142664&atid=753090");
		}

		private void lnkFeatures_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://sourceforge.net/tracker/?atid=753093&group_id=142664&func=browse");
		}
	}
}
