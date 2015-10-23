using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TeaTimer
{
	/// <summary>
	/// Summary description for RtfBox.
	/// </summary>
	public class RtfBox : FormFader.Fader
	{
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.RichTextBox rtf;
		private System.Windows.Forms.LinkLabel lnkBugs;
		private System.Windows.Forms.LinkLabel lnkFeatures;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RtfBox(string sName, string sFile, RichTextBoxStreamType oType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			FadeForm(Fade.Up);
			try 
			{
				rtf.LoadFile(sFile,oType);
			} 
			catch (Exception e) 
			{
				rtf.Text="Error loading "+sFile+"\n"+e;
			}
			this.Text=sName;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RtfBox));
			this.rtf = new System.Windows.Forms.RichTextBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.lnkBugs = new System.Windows.Forms.LinkLabel();
			this.lnkFeatures = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// rtf
			// 
			this.rtf.BackColor = System.Drawing.Color.LightYellow;
			this.rtf.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.rtf.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.rtf.ForeColor = System.Drawing.Color.Black;
			this.rtf.Location = new System.Drawing.Point(8, 8);
			this.rtf.Name = "rtf";
			this.rtf.ReadOnly = true;
			this.rtf.Size = new System.Drawing.Size(568, 408);
			this.rtf.TabIndex = 0;
			this.rtf.Text = "";
			// 
			// btnClose
			// 
			this.btnClose.BackColor = System.Drawing.Color.Transparent;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnClose.ForeColor = System.Drawing.Color.Black;
			this.btnClose.Location = new System.Drawing.Point(8, 424);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(376, 23);
			this.btnClose.TabIndex = 1;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// lnkBugs
			// 
			this.lnkBugs.BackColor = System.Drawing.Color.Transparent;
			this.lnkBugs.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkBugs.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.Location = new System.Drawing.Point(392, 432);
			this.lnkBugs.Name = "lnkBugs";
			this.lnkBugs.Size = new System.Drawing.Size(80, 16);
			this.lnkBugs.TabIndex = 1006;
			this.lnkBugs.TabStop = true;
			this.lnkBugs.Text = "Report a bug.";
			this.lnkBugs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkBugs.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBugs_LinkClicked);
			// 
			// lnkFeatures
			// 
			this.lnkFeatures.BackColor = System.Drawing.Color.Transparent;
			this.lnkFeatures.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkFeatures.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.Location = new System.Drawing.Point(472, 432);
			this.lnkFeatures.Name = "lnkFeatures";
			this.lnkFeatures.Size = new System.Drawing.Size(104, 16);
			this.lnkFeatures.TabIndex = 1007;
			this.lnkFeatures.TabStop = true;
			this.lnkFeatures.Text = "Request a feature.";
			this.lnkFeatures.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkFeatures.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkFeatures.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFeatures_LinkClicked);
			// 
			// RtfBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = new System.Drawing.Size(586, 455);
			this.ControlBox = false;
			this.Controls.Add(this.lnkFeatures);
			this.Controls.Add(this.lnkBugs);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.rtf);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RtfBox";
			this.Text = "RtfBox";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void btnClose_Click(object sender, System.EventArgs e)
		{
			//FadeForm(Fade.Down);
			this.Close();
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
