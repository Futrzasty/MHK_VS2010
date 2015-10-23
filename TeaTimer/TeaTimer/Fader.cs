using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace FormFader
{
	/// <summary>
	/// Summary description for Fader.
	/// </summary>
	public class Fader : System.Windows.Forms.Form
	{
		public enum Fade
		{
			Up,
			Down
		}

		private double _dTargetOpacity=0.0;
		private bool _bFadeUp;
		private double _dStep=0.04;
		public double Step 
		{
			get{return _dStep;}
			set{_dStep=value;}
		}

		private bool _bClosing=false;
		private bool _bFading=false;

		private System.Windows.Forms.Timer timFade;
		private System.ComponentModel.IContainer components;

		public Fader()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.Opacity=0.0;
			this.Hide();
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
			this.components = new System.ComponentModel.Container();
			this.timFade = new System.Windows.Forms.Timer(this.components);
			// 
			// timFade
			// 
			this.timFade.Interval = 10;
			this.timFade.Tick += new System.EventHandler(this.timFade_Tick);
			// 
			// Fader
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "Fader";
			this.Text = "Fader";

		}
		#endregion

		protected void FadeForm(Fade oFade) 
		{
			if(_bFading) return;
			if(oFade==Fade.Up) _dTargetOpacity=1.00;
			if(oFade==Fade.Down) _dTargetOpacity=0.0;
			
			FadeForm();
		}

		protected void FadeForm(double dTargetOpacity) 
		{
			if(_bFading) return;
			this._dTargetOpacity=dTargetOpacity;
			FadeForm();
		}

		private void FadeForm() 
		{
			if(_bFading) return;
			if(_dTargetOpacity==this.Opacity) return;
			_bFading=true;
			_bFadeUp=(_dTargetOpacity>this.Opacity);
			this.Show();
			this.timFade.Start();
		}

		
		private void timFade_Tick(object sender, System.EventArgs e)
		{
			this.Opacity+=(_bFadeUp?Step:-Step);
			if((this.Opacity>(_dTargetOpacity-Step)) && (this.Opacity<(_dTargetOpacity+Step))) 
			{
				this.timFade.Stop();
				this.Opacity=_dTargetOpacity;
				_bFading=false;
				if(this.Opacity<=0) 
				{
					this.Hide();
					if(_bClosing) this.Close();
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{  
			_bClosing=true;
			FadeForm(Fade.Down);
			if(Opacity>0) e.Cancel=true;
			base.OnClosing(e);
		}

	}
}
