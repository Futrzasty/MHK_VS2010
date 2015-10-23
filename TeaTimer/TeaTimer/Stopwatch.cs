using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;

namespace TeaTimer
{
	/// <summary>
	/// Summary description for SmallTimer.
	/// </summary>
	public class Stopwatch : FormFader.Fader
	{
		private System.Windows.Forms.PictureBox pbxIcon;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblTime;
		private System.Windows.Forms.Timer timUpdate;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.TrackBar tbarOpacity;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.Button btnStop;

		private TeaTimer._TeaTimer _oTT=null;
		private TeaTimer _oParent=null;
		private EventHandler _oExpired=null;
		private EventHandler _oStatusChanged=null;
		private Bitmap imgPause;
		private System.Windows.Forms.ToolTip tip;
		private Bitmap imgPlay;

		internal Stopwatch(TeaTimer._TeaTimer oTT, TeaTimer oParent, double dOpacity)
		{
			InitializeComponent();
			this._oTT=oTT;
			this._oParent=oParent;

			//get embedded images
			System.Reflection.Assembly oAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			imgPause=new Bitmap(oAssembly.GetManifestResourceStream("TeaTimer.smallpause.gif"));
			imgPlay=new Bitmap(oAssembly.GetManifestResourceStream("TeaTimer.smallplay.gif"));
			btnStop.Image=new Bitmap(oAssembly.GetManifestResourceStream("TeaTimer.smallstop.gif"));

			//tooltips
			tip.SetToolTip(btnStop,"Stop and Remove timer");
			tip.SetToolTip(btnPause,"Pause/Resume timer");
			tip.SetToolTip(tbarOpacity,"Adjust opacity");

			updatePauseBtn();
			TeaTimer.dispIcon(pbxIcon,oTT.VisualAlert);
			update();
			FadeForm(dOpacity);
			tbarOpacity.Value=(int)(dOpacity*100);

			//event handlers
			timUpdate.Tick+=new EventHandler(timUpdate_Tick);
			_oExpired=new EventHandler(_oTT_Expired);
			_oTT.Expired+=_oExpired;
			_oStatusChanged=new EventHandler(_oTT_StatusChanged);
			_oTT.StatusChanged+=_oStatusChanged;
			
			timUpdate.Start();
		}

		internal Stopwatch(EventHandler oScrollHandler, double dOpacity) 
		{
			InitializeComponent();
			TeaTimer.dispIcon(pbxIcon,"App.ico");
			this.Text="Set my opactiy";
			lblName.Text="Slide me -->";
			btnPause.Visible=false;
			btnStop.Visible=false;
			FadeForm(dOpacity);
			tbarOpacity.Value=(int)(dOpacity*100);
			tbarOpacity.Scroll+=oScrollHandler;
			tip.SetToolTip(tbarOpacity,"Adjust opacity");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				//remove events
				if(_oExpired!=null) _oTT.Expired-=_oExpired;
				if(_oStatusChanged!=null) _oTT.StatusChanged-=_oStatusChanged;

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
			this.components = new System.ComponentModel.Container();
			this.pbxIcon = new System.Windows.Forms.PictureBox();
			this.lblName = new System.Windows.Forms.Label();
			this.lblTime = new System.Windows.Forms.Label();
			this.timUpdate = new System.Windows.Forms.Timer(this.components);
			this.tbarOpacity = new System.Windows.Forms.TrackBar();
			this.btnPause = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.tip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.tbarOpacity)).BeginInit();
			this.SuspendLayout();
			// 
			// pbxIcon
			// 
			this.pbxIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxIcon.Location = new System.Drawing.Point(0, 3);
			this.pbxIcon.Name = "pbxIcon";
			this.pbxIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxIcon.TabIndex = 0;
			this.pbxIcon.TabStop = false;
			this.pbxIcon.DoubleClick += new System.EventHandler(this.pbxIcon_DoubleClick);
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(40, 0);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(72, 14);
			this.lblName.TabIndex = 1;
			this.lblName.Text = "Timer Name";
			this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTime
			// 
			this.lblTime.BackColor = System.Drawing.SystemColors.ControlText;
			this.lblTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTime.ForeColor = System.Drawing.SystemColors.Control;
			this.lblTime.Location = new System.Drawing.Point(40, 16);
			this.lblTime.Name = "lblTime";
			this.lblTime.Size = new System.Drawing.Size(146, 31);
			this.lblTime.TabIndex = 2;
			this.lblTime.Text = "1:19:45:34";
			this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblTime.DoubleClick += new System.EventHandler(this.lblTime_DoubleClick);
			// 
			// timUpdate
			// 
			this.timUpdate.Interval = 1000;
			// 
			// tbarOpacity
			// 
			this.tbarOpacity.BackColor = System.Drawing.SystemColors.Control;
			this.tbarOpacity.Location = new System.Drawing.Point(138, -3);
			this.tbarOpacity.Maximum = 100;
			this.tbarOpacity.Minimum = 10;
			this.tbarOpacity.Name = "tbarOpacity";
			this.tbarOpacity.Size = new System.Drawing.Size(54, 42);
			this.tbarOpacity.TabIndex = 3;
			this.tbarOpacity.TickStyle = System.Windows.Forms.TickStyle.None;
			this.tbarOpacity.Value = 10;
			this.tbarOpacity.Scroll += new System.EventHandler(this.tbarOpacity_Scroll);
			// 
			// btnPause
			// 
			this.btnPause.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.btnPause.Location = new System.Drawing.Point(128, 0);
			this.btnPause.Name = "btnPause";
			this.btnPause.Size = new System.Drawing.Size(19, 16);
			this.btnPause.TabIndex = 4;
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// btnStop
			// 
			this.btnStop.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.btnStop.Location = new System.Drawing.Point(107, 0);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(19, 16);
			this.btnStop.TabIndex = 5;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// tip
			// 
			this.tip.ShowAlways = true;
			// 
			// Stopwatch
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(183, 46);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnPause);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.lblTime);
			this.Controls.Add(this.tbarOpacity);
			this.Controls.Add(this.pbxIcon);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Stopwatch";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Stopwatch";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.tbarOpacity)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void timUpdate_Tick(object sender, EventArgs e)
		{
			update();
		}

		private void update() 
		{
			this.Text=lblName.Text=_oTT.Name;
			lblTime.Text=TeaTimer.getFormattedTime(_oTT.getTimeLeft());
			//tooltip
			StringBuilder sTemp=new StringBuilder();
			sTemp.Append(_oTT.Name);
			sTemp.Append("\nTotal: ");
			sTemp.Append(TeaTimer.getFormattedTime(_oTT.Time));
			sTemp.Append("\nElapsed: ");
			sTemp.Append(TeaTimer.getFormattedTime(_oTT.getTimeElapsed()));
			sTemp.Append("\nWill pop: ");
			sTemp.Append(_oTT.Paused?"*** PAUSED ***":TeaTimer.getTimeString(_oTT.Finished));
			sTemp.Append("\nVisual alert: ");
			sTemp.Append(_oTT.VisualAlert.Length>0?"yes":"no");
			sTemp.Append("\nAudible alert: ");
			sTemp.Append(_oTT.AudibleAlert.Length>0?"yes":"no");
			sTemp.Append("\nProgram: ");
			sTemp.Append(_oTT.ProcessAlert.Length>0?"yes":"no");
			sTemp.Append("\nRepeating: ");
			sTemp.Append(_oTT.Recurring?"yes":"no");
			tip.SetToolTip(lblTime,sTemp.ToString());
			tip.SetToolTip(pbxIcon,sTemp.ToString());
		}
		private void updatePauseBtn() 
		{
			if(_oTT!=null) btnPause.Image=(_oTT.Paused?imgPlay:imgPause);
		}


		private void tbarOpacity_Scroll(object sender, System.EventArgs e)
		{
			this.Opacity=(tbarOpacity.Value/100.0);
		}

		private void _oTT_Expired(object sender, EventArgs e)
		{
			if(!_oTT.Recurring) this.Close();
		}
		private void _oTT_StatusChanged(object sender, EventArgs e) 
		{
			update();
			updatePauseBtn();
		}

		private void btnPause_Click(object sender, System.EventArgs e)
		{
			_oParent.pauseTeaTimer(_oTT);
			updatePauseBtn();
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			_oParent.stopTeaTimer(_oTT);
		}

		private void lblTime_DoubleClick(object sender, System.EventArgs e)
		{
			showTimerScreen();
		}

		private void pbxIcon_DoubleClick(object sender, System.EventArgs e)
		{
			showTimerScreen();
		}

		private void showTimerScreen() 
		{
			_oParent.showTimerScreen(_oTT);
		}
	}
}
