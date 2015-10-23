using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using FormFader;
using SimpleConfigLib;

namespace TeaTimer
{
	/// <summary>
	/// Summary description for TeaTimer.
	/// </summary>
	public class TeaTimer : FormFader.Fader
	{
		internal static readonly string VERSION=
			System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileMajorPart+"."+
			System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileMinorPart+"."+
			System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileBuildPart;
		
		private const string NONE="none";

		#region _TeaTimer
		internal class _TeaTimer : System.Windows.Forms.Timer, IComparable
		{
			#region Data Members
			private int _iTime;
			public int[] Time
			{
				get{return convFromMilli(_iTime);}
			}

			private string _sName;
			public string Name
			{
				get{return _sName+(Paused?" (paused)":"");}
				set{_sName=value;}
			}

			private string _sId=DateTime.Now.ToString();
			public string Id
			{
				get{return _sId;}
			}

			private DateTime _oStarted;
			public DateTime Started 
			{
				get {return _oStarted;}
			}

			public DateTime Finished 
			{
				get {return getFinishDate();}
			}

			private bool _bPaused=false;
			public bool Paused 
			{
				get{return _bPaused;}
			}

			private DateTime _oPaused;
			private int _iPausedTime=0;

			private DateTime _oAlarm=new DateTime(0);
			public bool AlarmMode 
			{
				get {return _oAlarm!=DateTime.MinValue;}
			}

			private string _oAudibleAlert=null;
			public string AudibleAlert 
			{
				get{return _oAudibleAlert;}
				set{_oAudibleAlert=value;}
			}
			private string _oVisualAlert=null;
			public string VisualAlert 
			{
				get{return _oVisualAlert;}
				set{_oVisualAlert=value;}
			}
			private string _oProcessAlert=null;
			public string ProcessAlert 
			{
				get{return _oProcessAlert;}
				set{_oProcessAlert=value;}
			}
			private bool _bRecurring=false;
			public bool Recurring 
			{
				get{return _bRecurring;}
				set{_bRecurring=value;}
			}
			#endregion
			
			#region Public Methods
			public _TeaTimer(string Name, int iDay, int iHour, int iMin, int iSec) 
			{
				_iTime=convToMilli(iDay,iHour,iMin,iSec,0);
				_sName=Name;
			}
			public _TeaTimer(string Name, DateTime oAlarm) 
			{
				_sName=Name;
				_oAlarm=oAlarm;
			}
			public void Go() 
			{
				_oStarted=DateTime.Now;
				if(AlarmMode) 
				{
					TimeSpan oSpan=_oAlarm-_oStarted;
					_iTime=convToMilli(oSpan.Days,oSpan.Hours,oSpan.Minutes,oSpan.Seconds+1,oSpan.Milliseconds);
					if(_iTime<=1000) throw(new TeaTimerException("The alarm is set for a date/time in the past."));
				}
				base.Interval=_iTime;
				base.Tick+=new EventHandler(_TeaTimer_Tick);
				base.Start();
			}
			public void Expire() 
			{
				base.Stop();
				Recurring=false;
				OnExpired(new EventArgs());
			}
			public void Pause() 
			{
				if(_bPaused) return;
				_bPaused=true;
				base.Stop();
				_oPaused=DateTime.Now;
				OnStatusChanged(new EventArgs());
			}
			public void Resume() 
			{
				if(!_bPaused) return;
				_iPausedTime=getPausedTime();
				_bPaused=false; //do this here or getElapsedTime will start giving bogus answers
				int[] iE=getTimeElapsed();
				base.Interval=_iTime-convToMilli(iE[0],iE[1],iE[2],iE[3],iE[4]);
				base.Start();
				OnStatusChanged(new EventArgs());
			}
			public override string ToString() 
			{
				return Name;
			}
			public int[] getTimeElapsed() 
			{
				TimeSpan oTS=DateTime.Now-Started;
				//substract time spent paused
				int[] iP=convFromMilli(getPausedTime());
				oTS=oTS.Subtract(new TimeSpan(iP[0],iP[1],iP[2],iP[3],iP[4]));
				//return
				int[] iRet=new int[5];
				iRet[0]=oTS.Days;
				iRet[1]=oTS.Hours;
				iRet[2]=oTS.Minutes;
				iRet[3]=oTS.Seconds;
				iRet[4]=oTS.Milliseconds;
				return iRet;
			}
			public int[] getTimeLeft() 
			{
				int iElapsed=convToMilli(getTimeElapsed())-1000; //off by one second correction
				int iTime=_iTime;
				return convFromMilli(iTime-iElapsed);
			}
			#endregion

			#region Private Methods
			private DateTime getFinishDate() 
			{
				int[] iPop=convFromMilli(_iTime+getPausedTime());
				DateTime oEnd=Started;
				oEnd=oEnd.AddDays((double)iPop[0]);
				oEnd=oEnd.AddHours((double)iPop[1]);
				oEnd=oEnd.AddMinutes((double)iPop[2]);
				oEnd=oEnd.AddSeconds((double)iPop[3]);
				return oEnd;
			}
			private int getPausedTime() 
			{
				if(!_bPaused) return _iPausedTime;
				TimeSpan oPaused=DateTime.Now-_oPaused;
				return _iPausedTime+convToMilli(oPaused.Days,oPaused.Hours,oPaused.Minutes,oPaused.Seconds,oPaused.Milliseconds);
			}
			private int[] convFromMilli(int iTime)
			{
				int[] iRet=new int[5];
				iRet[0]=(iTime/86400000);
				iTime-=(iRet[0]*86400000);
				iRet[1]=(iTime/3600000);
				iTime-=(iRet[1]*3600000);
				iRet[2]=(iTime/60000);
				iTime-=(iRet[2]*60000);
				iRet[3]=(iTime/1000);
				iTime-=(iRet[3]*1000);
				iRet[4]=iTime;
				return iRet;
			}
			private int convToMilli(int iDay, int iHour, int iMin, int iSec, int iMilli)
			{
				int iTime=0;
				iTime+=(iDay*86400000);
				iTime+=(iHour*3600000);
				iTime+=(iMin*60000);
				iTime+=(iSec*1000);
				iTime+=iMilli;
				return iTime;
			}
			private int convToMilli(int[] iTime) 
			{
				return convToMilli(iTime[0],iTime[1],iTime[2],iTime[3],iTime[4]);
			}
			#endregion

			#region IComparable Members

			public int CompareTo(object obj)
			{
				_TeaTimer oTT=(_TeaTimer)obj;
				int iRet;
				if(this.Paused && !oTT.Paused) iRet=1;
				else if(!this.Paused && oTT.Paused) iRet=-1;
				else iRet=this.Finished.CompareTo(oTT.Finished);
				return iRet;
			}

			#endregion

			#region TeaTimerException
			public class TeaTimerException : Exception 
			{
				public TeaTimerException(string msg) : base(msg) {}
			}
			#endregion

			#region Event Expired
			public event EventHandler Expired;
			protected virtual void OnExpired(EventArgs e) 
			{
				if(Expired!=null) Expired(this,e);
			}
			private void _TeaTimer_Tick(object sender, EventArgs e)
			{
				OnExpired(new EventArgs());
			}
			#endregion

			#region Event StatusChanged
			public event EventHandler StatusChanged;
			protected virtual void OnStatusChanged(EventArgs e) 
			{
				if(StatusChanged!=null) StatusChanged(this,e);
			}
			#endregion
		}
		#endregion

		#region SimpleDataStructures
		#region _SimpleDataStructure
		private abstract class _SimpleDataStructure : IComparable
		{
			public string sFile;
			public string sName;
			public _SimpleDataStructure(string sName, string sFile) 
			{
				this.sName=sName;
				this.sFile=sFile;
			}
			public override string ToString()
			{
				return sName;
			}
			#region IComparable Members

			public int CompareTo(object obj)
			{
				return sName.CompareTo(((_Sound)obj).sName);
			}

			#endregion
		}
		#endregion

		#region _None
		private class _None : _SimpleDataStructure 
		{
			public _None() : base(NONE,"") {}
		}
		#endregion

		#region _Sound
		private class _Sound : _SimpleDataStructure
		{
			public _Sound(string sName, string sFile) : base(sName,sFile) {}
		}
		#endregion

		#region _Icon
		private class _Icon : _SimpleDataStructure
		{
			public _Icon(string sName, string sFile) : base(sName,sFile) {}
		}
		#endregion

		#region _Process
		private class _Process : _SimpleDataStructure
		{
			public _Process(string sName, string sFile) : base(sName,sFile) {}
		}
		#endregion
		#endregion

		#region _Preset
		private struct _Preset 
		{
			public string sName, sSnd, sIcon, sProcess;
			public int iDay, iHour, iMin, iSec;
			public bool bDefSnd, bDefIcon, bRecurring;
		}
		#endregion

		#region Data Members
		private ArrayList _oTeaTimers=new ArrayList();
		private SimpleConfig _oConfig=new SimpleConfig("TeaTimer","General.config");
		private int _iIconMenuPresets=0;
		private int _iAlertX=-1, _iAlertY=-1;
		private _Preset[] _oPresets=new _Preset[6];
		private int _iSelectedPreset;
		private static bool _b24hour=false;
		private int _iStopwatchOpacity;
		private static Image imgError;

		private System.Windows.Forms.NumericUpDown numHour;
		private System.Windows.Forms.NumericUpDown numDay;
		private System.Windows.Forms.NumericUpDown numMin;
		private System.Windows.Forms.NumericUpDown numSec;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnGo;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Label v;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabCreate;
		private System.Windows.Forms.TabPage tabView;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblTime;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListBox lstTimers;
		private System.Windows.Forms.Label lblElapsed;
		private System.Windows.Forms.Timer timElapsedUpdate;
		private System.Windows.Forms.Label lblFinish;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ContextMenu iconMenu;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menTimers;
		private System.Windows.Forms.MenuItem menAbout;
		private System.Windows.Forms.MenuItem menQuit;
		private System.Windows.Forms.TabPage tabConfig;
		private System.Windows.Forms.CheckBox cbxPlaySnd;
		private System.Windows.Forms.Button btnPre1;
		private System.Windows.Forms.Button btnPre2;
		private System.Windows.Forms.Button btnPre3;
		private System.Windows.Forms.Button btnPre4;
		private System.Windows.Forms.Button btnPre5;
		private System.Windows.Forms.Button btnPre6;
		private System.Windows.Forms.TabPage tabPresets;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.OpenFileDialog diaPlaySnd;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.MenuItem menPresets;
		private System.Windows.Forms.MenuItem menPresetsSep;
		private System.Windows.Forms.Button btnHourPlus1;
		private System.Windows.Forms.Button btnHourPlus8;
		private System.Windows.Forms.Button btnMinPlus1;
		private System.Windows.Forms.Button btnMinPlus5;
		private System.Windows.Forms.Button btnSecPlus15;
		private System.Windows.Forms.Button btnSecPlus30;
		private System.Windows.Forms.Button btnMinPlus10;
		private System.Windows.Forms.Button btnPause;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Button btnHelp;
		private System.Windows.Forms.TabPage tabAlarm;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.CheckBox cbx24hour;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.NumericUpDown numAlarmSec;
		private System.Windows.Forms.NumericUpDown numAlarmMin;
		private System.Windows.Forms.NumericUpDown numAlarmHour;
		private System.Windows.Forms.Button btnAlarmGo;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TextBox txtAlarmName;
		private System.Windows.Forms.MonthCalendar calAlarm;
		private System.Windows.Forms.RadioButton radAM;
		private System.Windows.Forms.RadioButton radPM;
		private System.Windows.Forms.MenuItem menRunning;
		private System.Windows.Forms.TabControl tabCtrlConfigure;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabAudibleAlert;
		private System.Windows.Forms.TabPage tabVisualAlert;
		private System.Windows.Forms.Button btnAlertPosition;
		private System.Windows.Forms.CheckBox cbxVisualAlert;
		private System.Windows.Forms.Label lblAlertCoords;
		private System.Windows.Forms.ListBox lstSounds;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button btnRemoveSnd;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.TextBox txtIconName;
		private System.Windows.Forms.Button btnBrowseIcon;
		private System.Windows.Forms.TextBox txtIconFile;
		private System.Windows.Forms.Button btnAddIcon;
		private System.Windows.Forms.ListBox lstIcons;
		private System.Windows.Forms.Button btnRemoveIcon;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.TextBox txtSndName;
		private System.Windows.Forms.Button btnBrowseSnd;
		private System.Windows.Forms.TextBox txtSndFile;
		private System.Windows.Forms.Button btnAddSnd;
		private System.Windows.Forms.CheckBox cbxDefaultSnd;
		private System.Windows.Forms.ComboBox cmbTimerSnd;
		private System.Windows.Forms.Label lblTimerSnd;
		private System.Windows.Forms.Label lblTimerIcon;
		private System.Windows.Forms.ComboBox cmbTimerIcon;
		private System.Windows.Forms.Label lblAlarmSound;
		private System.Windows.Forms.ComboBox cmbAlarmSnd;
		private System.Windows.Forms.Label lblAlarmIcon;
		private System.Windows.Forms.ComboBox cmbAlarmIcon;
		private System.Windows.Forms.CheckBox cbxDefaultIcon;
		private System.Windows.Forms.OpenFileDialog diaBrowseIcons;
		private System.Windows.Forms.PictureBox pbxTimerIcon;
		private System.Windows.Forms.PictureBox pbxAlarmIcon;
		private System.Windows.Forms.PictureBox pbxIcon;
		private System.Windows.Forms.Button btnTimerPlay;
		private System.Windows.Forms.Button btnAlarmPlay;
		private System.Windows.Forms.Button btnPlaySnd;
		private System.Windows.Forms.RadioButton radPre1;
		private System.Windows.Forms.RadioButton radPre2;
		private System.Windows.Forms.RadioButton radPre3;
		private System.Windows.Forms.RadioButton radPre4;
		private System.Windows.Forms.RadioButton radPre5;
		private System.Windows.Forms.RadioButton radPre6;
		private System.Windows.Forms.NumericUpDown numSecPre;
		private System.Windows.Forms.NumericUpDown numMinPre;
		private System.Windows.Forms.NumericUpDown numHourPre;
		private System.Windows.Forms.NumericUpDown numDayPre;
		private System.Windows.Forms.TextBox txtPreName;
		private System.Windows.Forms.Button btnPrePlaySnd;
		private System.Windows.Forms.ComboBox cmbPreIcon;
		private System.Windows.Forms.ComboBox cmbPreSnd;
		private System.Windows.Forms.PictureBox pbxPreIcon;
		private System.Windows.Forms.Button btnPreSnd;
		private System.Windows.Forms.TextBox txtPreSnd;
		private System.Windows.Forms.Button btnPreIcon;
		private System.Windows.Forms.TextBox txtPreIcon;
		private System.Windows.Forms.Button btnPreSave;
		private System.Windows.Forms.PictureBox pbxViewIcon;
		private System.Windows.Forms.Button btnPreDelete;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnOpacity;
		private System.Windows.Forms.Label lblOpacity;
		private System.Windows.Forms.CheckBox cbxPreIcon;
		private System.Windows.Forms.CheckBox cbxPreSnd;
		private System.Windows.Forms.MenuItem menAlarm;
		private System.Windows.Forms.Button btnStopwatch;
		private System.Windows.Forms.CheckBox cbxShowStopwatch;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.LinkLabel lnkWebPage;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Label lblLeft;
		private System.Windows.Forms.TabPage tabProcessAlert;
		private System.Windows.Forms.ListBox lstProcesses;
		private System.Windows.Forms.Button btnRemoveProcess;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.Button btnAddProcess;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.TextBox txtProcessName;
		private System.Windows.Forms.Button btnBrowseProcess;
		private System.Windows.Forms.TextBox txtProcessFile;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.Button btnRunProcess;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Label label36;
		private System.Windows.Forms.Label label37;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.OpenFileDialog diaRunProcess;
		private System.Windows.Forms.ComboBox cmbTimerProcess;
		private System.Windows.Forms.ComboBox cmbAlarmProcess;
		private System.Windows.Forms.Button btnPreRunProcess;
		private System.Windows.Forms.Button btnPreBrowseProcess;
		private System.Windows.Forms.TextBox txtPreProcess;
		private System.Windows.Forms.ComboBox cmbPreProcess;
		private System.Windows.Forms.CheckBox cbxAudibleProcessAlert;
		private System.Windows.Forms.Label label39;
		private System.Windows.Forms.CheckBox cbxVisualProcessAlert;
		private System.Windows.Forms.Label label40;
		private System.Windows.Forms.Label lblVersion;
		private System.Windows.Forms.CheckBox cbxTimerRepeat;
		private System.Windows.Forms.CheckBox cbxPreRepeat;
		private System.Windows.Forms.Label lblPreProcess;
		private System.Windows.Forms.Label lblPreIcon;
		private System.Windows.Forms.Label lblPreSnd;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.CheckBox cbxViewVisual;
		private System.Windows.Forms.CheckBox cbxViewAudible;
		private System.Windows.Forms.Label lblViewAudible;
		private System.Windows.Forms.CheckBox cbxViewRepeat;
		private System.Windows.Forms.CheckBox cbxViewProcess;
		private System.Windows.Forms.Label lblViewProcess;
		private System.Windows.Forms.ToolTip tip;
		private System.Windows.Forms.LinkLabel lnkBugs;
		private System.ComponentModel.IContainer components;
		#endregion

		#region Contruct/Destruct/Main
		public TeaTimer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			for(int i=0; i<6; i++)
				_oPresets[i]=new _Preset();

			readConfig();

			notifyIcon.Icon=new Icon("icon.ico");
			notifyIcon.Text="No timers set";

			menRunning.Enabled=false;

			imgError=new Bitmap( System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TeaTimer.error.gif"));

			lblVersion.Text+=VERSION;

			resetForm();

			System.GC.Collect();
		}

		[STAThread]
		static void Main() 
		{
			Application.Run(new TeaTimer());
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
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TeaTimer));
			this.numHour = new System.Windows.Forms.NumericUpDown();
			this.numDay = new System.Windows.Forms.NumericUpDown();
			this.numMin = new System.Windows.Forms.NumericUpDown();
			this.numSec = new System.Windows.Forms.NumericUpDown();
			this.v = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnGo = new System.Windows.Forms.Button();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.iconMenu = new System.Windows.Forms.ContextMenu();
			this.menPresets = new System.Windows.Forms.MenuItem();
			this.menPresetsSep = new System.Windows.Forms.MenuItem();
			this.menRunning = new System.Windows.Forms.MenuItem();
			this.menAlarm = new System.Windows.Forms.MenuItem();
			this.menTimers = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menAbout = new System.Windows.Forms.MenuItem();
			this.menQuit = new System.Windows.Forms.MenuItem();
			this.txtName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabCreate = new System.Windows.Forms.TabPage();
			this.cbxTimerRepeat = new System.Windows.Forms.CheckBox();
			this.label32 = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.cmbTimerProcess = new System.Windows.Forms.ComboBox();
			this.btnTimerPlay = new System.Windows.Forms.Button();
			this.lblTimerIcon = new System.Windows.Forms.Label();
			this.cmbTimerIcon = new System.Windows.Forms.ComboBox();
			this.lblTimerSnd = new System.Windows.Forms.Label();
			this.cmbTimerSnd = new System.Windows.Forms.ComboBox();
			this.btnMinPlus10 = new System.Windows.Forms.Button();
			this.btnMinPlus5 = new System.Windows.Forms.Button();
			this.btnMinPlus1 = new System.Windows.Forms.Button();
			this.btnHourPlus8 = new System.Windows.Forms.Button();
			this.btnHourPlus1 = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnPre6 = new System.Windows.Forms.Button();
			this.btnPre5 = new System.Windows.Forms.Button();
			this.btnPre4 = new System.Windows.Forms.Button();
			this.btnPre3 = new System.Windows.Forms.Button();
			this.btnPre2 = new System.Windows.Forms.Button();
			this.btnPre1 = new System.Windows.Forms.Button();
			this.btnSecPlus30 = new System.Windows.Forms.Button();
			this.btnSecPlus15 = new System.Windows.Forms.Button();
			this.pbxTimerIcon = new System.Windows.Forms.PictureBox();
			this.tabAlarm = new System.Windows.Forms.TabPage();
			this.label33 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.cmbAlarmProcess = new System.Windows.Forms.ComboBox();
			this.btnAlarmPlay = new System.Windows.Forms.Button();
			this.pbxAlarmIcon = new System.Windows.Forms.PictureBox();
			this.lblAlarmIcon = new System.Windows.Forms.Label();
			this.cmbAlarmIcon = new System.Windows.Forms.ComboBox();
			this.lblAlarmSound = new System.Windows.Forms.Label();
			this.cmbAlarmSnd = new System.Windows.Forms.ComboBox();
			this.radPM = new System.Windows.Forms.RadioButton();
			this.radAM = new System.Windows.Forms.RadioButton();
			this.txtAlarmName = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.btnAlarmGo = new System.Windows.Forms.Button();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.numAlarmSec = new System.Windows.Forms.NumericUpDown();
			this.numAlarmMin = new System.Windows.Forms.NumericUpDown();
			this.label16 = new System.Windows.Forms.Label();
			this.numAlarmHour = new System.Windows.Forms.NumericUpDown();
			this.label15 = new System.Windows.Forms.Label();
			this.calAlarm = new System.Windows.Forms.MonthCalendar();
			this.tabView = new System.Windows.Forms.TabPage();
			this.label34 = new System.Windows.Forms.Label();
			this.btnStopwatch = new System.Windows.Forms.Button();
			this.btnPause = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbxViewRepeat = new System.Windows.Forms.CheckBox();
			this.lblViewProcess = new System.Windows.Forms.Label();
			this.cbxViewProcess = new System.Windows.Forms.CheckBox();
			this.lblViewAudible = new System.Windows.Forms.Label();
			this.cbxViewAudible = new System.Windows.Forms.CheckBox();
			this.cbxViewVisual = new System.Windows.Forms.CheckBox();
			this.lblLeft = new System.Windows.Forms.Label();
			this.label26 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.lblFinish = new System.Windows.Forms.Label();
			this.lblElapsed = new System.Windows.Forms.Label();
			this.lblTime = new System.Windows.Forms.Label();
			this.lblName = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.pbxViewIcon = new System.Windows.Forms.PictureBox();
			this.lstTimers = new System.Windows.Forms.ListBox();
			this.tabPresets = new System.Windows.Forms.TabPage();
			this.cbxPreRepeat = new System.Windows.Forms.CheckBox();
			this.btnPreRunProcess = new System.Windows.Forms.Button();
			this.btnPreBrowseProcess = new System.Windows.Forms.Button();
			this.txtPreProcess = new System.Windows.Forms.TextBox();
			this.lblPreProcess = new System.Windows.Forms.Label();
			this.cmbPreProcess = new System.Windows.Forms.ComboBox();
			this.cbxPreSnd = new System.Windows.Forms.CheckBox();
			this.cbxPreIcon = new System.Windows.Forms.CheckBox();
			this.btnPreDelete = new System.Windows.Forms.Button();
			this.btnPreSave = new System.Windows.Forms.Button();
			this.btnPreIcon = new System.Windows.Forms.Button();
			this.txtPreIcon = new System.Windows.Forms.TextBox();
			this.btnPreSnd = new System.Windows.Forms.Button();
			this.txtPreSnd = new System.Windows.Forms.TextBox();
			this.pbxPreIcon = new System.Windows.Forms.PictureBox();
			this.btnPrePlaySnd = new System.Windows.Forms.Button();
			this.lblPreIcon = new System.Windows.Forms.Label();
			this.cmbPreIcon = new System.Windows.Forms.ComboBox();
			this.lblPreSnd = new System.Windows.Forms.Label();
			this.cmbPreSnd = new System.Windows.Forms.ComboBox();
			this.radPre6 = new System.Windows.Forms.RadioButton();
			this.radPre5 = new System.Windows.Forms.RadioButton();
			this.radPre4 = new System.Windows.Forms.RadioButton();
			this.radPre3 = new System.Windows.Forms.RadioButton();
			this.radPre2 = new System.Windows.Forms.RadioButton();
			this.radPre1 = new System.Windows.Forms.RadioButton();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.numSecPre = new System.Windows.Forms.NumericUpDown();
			this.numMinPre = new System.Windows.Forms.NumericUpDown();
			this.numHourPre = new System.Windows.Forms.NumericUpDown();
			this.numDayPre = new System.Windows.Forms.NumericUpDown();
			this.txtPreName = new System.Windows.Forms.TextBox();
			this.tabConfig = new System.Windows.Forms.TabPage();
			this.tabCtrlConfigure = new System.Windows.Forms.TabControl();
			this.tabAudibleAlert = new System.Windows.Forms.TabPage();
			this.label37 = new System.Windows.Forms.Label();
			this.btnPlaySnd = new System.Windows.Forms.Button();
			this.cbxDefaultSnd = new System.Windows.Forms.CheckBox();
			this.lstSounds = new System.Windows.Forms.ListBox();
			this.cbxPlaySnd = new System.Windows.Forms.CheckBox();
			this.btnRemoveSnd = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnAddSnd = new System.Windows.Forms.Button();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.txtSndName = new System.Windows.Forms.TextBox();
			this.btnBrowseSnd = new System.Windows.Forms.Button();
			this.txtSndFile = new System.Windows.Forms.TextBox();
			this.tabVisualAlert = new System.Windows.Forms.TabPage();
			this.label36 = new System.Windows.Forms.Label();
			this.pbxIcon = new System.Windows.Forms.PictureBox();
			this.cbxDefaultIcon = new System.Windows.Forms.CheckBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.label40 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.txtIconName = new System.Windows.Forms.TextBox();
			this.btnBrowseIcon = new System.Windows.Forms.Button();
			this.txtIconFile = new System.Windows.Forms.TextBox();
			this.btnAddIcon = new System.Windows.Forms.Button();
			this.btnRemoveIcon = new System.Windows.Forms.Button();
			this.btnAlertPosition = new System.Windows.Forms.Button();
			this.cbxVisualAlert = new System.Windows.Forms.CheckBox();
			this.lblAlertCoords = new System.Windows.Forms.Label();
			this.lstIcons = new System.Windows.Forms.ListBox();
			this.tabProcessAlert = new System.Windows.Forms.TabPage();
			this.label20 = new System.Windows.Forms.Label();
			this.cbxAudibleProcessAlert = new System.Windows.Forms.CheckBox();
			this.cbxVisualProcessAlert = new System.Windows.Forms.CheckBox();
			this.btnRunProcess = new System.Windows.Forms.Button();
			this.label29 = new System.Windows.Forms.Label();
			this.lstProcesses = new System.Windows.Forms.ListBox();
			this.btnRemoveProcess = new System.Windows.Forms.Button();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.btnAddProcess = new System.Windows.Forms.Button();
			this.label27 = new System.Windows.Forms.Label();
			this.label28 = new System.Windows.Forms.Label();
			this.txtProcessName = new System.Windows.Forms.TextBox();
			this.btnBrowseProcess = new System.Windows.Forms.Button();
			this.txtProcessFile = new System.Windows.Forms.TextBox();
			this.label39 = new System.Windows.Forms.Label();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.label38 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.lblOpacity = new System.Windows.Forms.Label();
			this.cbxShowStopwatch = new System.Windows.Forms.CheckBox();
			this.btnOpacity = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.cbx24hour = new System.Windows.Forms.CheckBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.timElapsedUpdate = new System.Windows.Forms.Timer(this.components);
			this.diaPlaySnd = new System.Windows.Forms.OpenFileDialog();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnHelp = new System.Windows.Forms.Button();
			this.diaBrowseIcons = new System.Windows.Forms.OpenFileDialog();
			this.lnkWebPage = new System.Windows.Forms.LinkLabel();
			this.diaRunProcess = new System.Windows.Forms.OpenFileDialog();
			this.lblVersion = new System.Windows.Forms.Label();
			this.tip = new System.Windows.Forms.ToolTip(this.components);
			this.lnkBugs = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.numHour)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numDay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numSec)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabCreate.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.tabAlarm.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numAlarmSec)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numAlarmMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numAlarmHour)).BeginInit();
			this.tabView.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPresets.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numSecPre)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMinPre)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numHourPre)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numDayPre)).BeginInit();
			this.tabConfig.SuspendLayout();
			this.tabCtrlConfigure.SuspendLayout();
			this.tabAudibleAlert.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabVisualAlert.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.tabProcessAlert.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// numHour
			// 
			this.numHour.BackColor = System.Drawing.Color.LightYellow;
			this.numHour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numHour.ForeColor = System.Drawing.Color.Black;
			this.numHour.Location = new System.Drawing.Point(8, 72);
			this.numHour.Maximum = new System.Decimal(new int[] {
																	999,
																	0,
																	0,
																	0});
			this.numHour.Name = "numHour";
			this.numHour.Size = new System.Drawing.Size(40, 20);
			this.numHour.TabIndex = 20;
			this.numHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numHour.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numHour.Enter += new System.EventHandler(this.highlight_OnFocus);
			this.numHour.ValueChanged += new System.EventHandler(this.numHour_ValueChanged);
			this.numHour.Leave += new System.EventHandler(this.numHour_ValueChanged);
			// 
			// numDay
			// 
			this.numDay.BackColor = System.Drawing.Color.LightYellow;
			this.numDay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numDay.ForeColor = System.Drawing.Color.Black;
			this.numDay.Location = new System.Drawing.Point(8, 40);
			this.numDay.Maximum = new System.Decimal(new int[] {
																   999,
																   0,
																   0,
																   0});
			this.numDay.Name = "numDay";
			this.numDay.Size = new System.Drawing.Size(40, 20);
			this.numDay.TabIndex = 10;
			this.numDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numDay.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numDay.Enter += new System.EventHandler(this.highlight_OnFocus);
			this.numDay.ValueChanged += new System.EventHandler(this.numDay_ValueChanged);
			this.numDay.Leave += new System.EventHandler(this.numHour_ValueChanged);
			// 
			// numMin
			// 
			this.numMin.BackColor = System.Drawing.Color.LightYellow;
			this.numMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numMin.ForeColor = System.Drawing.Color.Black;
			this.numMin.Location = new System.Drawing.Point(8, 120);
			this.numMin.Maximum = new System.Decimal(new int[] {
																   999,
																   0,
																   0,
																   0});
			this.numMin.Name = "numMin";
			this.numMin.Size = new System.Drawing.Size(40, 20);
			this.numMin.TabIndex = 30;
			this.numMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numMin.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numMin.Enter += new System.EventHandler(this.highlight_OnFocus);
			this.numMin.ValueChanged += new System.EventHandler(this.numMin_ValueChanged);
			this.numMin.Leave += new System.EventHandler(this.numHour_ValueChanged);
			// 
			// numSec
			// 
			this.numSec.BackColor = System.Drawing.Color.LightYellow;
			this.numSec.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numSec.ForeColor = System.Drawing.Color.Black;
			this.numSec.Location = new System.Drawing.Point(8, 168);
			this.numSec.Maximum = new System.Decimal(new int[] {
																   999,
																   0,
																   0,
																   0});
			this.numSec.Name = "numSec";
			this.numSec.Size = new System.Drawing.Size(40, 20);
			this.numSec.TabIndex = 40;
			this.numSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numSec.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numSec.Enter += new System.EventHandler(this.highlight_OnFocus);
			this.numSec.ValueChanged += new System.EventHandler(this.numSec_ValueChanged);
			this.numSec.Leave += new System.EventHandler(this.numHour_ValueChanged);
			// 
			// v
			// 
			this.v.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.v.ForeColor = System.Drawing.Color.Black;
			this.v.Location = new System.Drawing.Point(48, 40);
			this.v.Name = "v";
			this.v.Size = new System.Drawing.Size(64, 24);
			this.v.TabIndex = 4;
			this.v.Text = "day(s)";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(48, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 24);
			this.label2.TabIndex = 5;
			this.label2.Text = "hour(s)";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(48, 120);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 24);
			this.label3.TabIndex = 6;
			this.label3.Text = "minute(s)";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.ForeColor = System.Drawing.Color.Black;
			this.label4.Location = new System.Drawing.Point(48, 168);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 24);
			this.label4.TabIndex = 7;
			this.label4.Text = "second(s)";
			// 
			// btnGo
			// 
			this.btnGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnGo.ForeColor = System.Drawing.Color.Crimson;
			this.btnGo.Location = new System.Drawing.Point(8, 216);
			this.btnGo.Name = "btnGo";
			this.btnGo.TabIndex = 50;
			this.btnGo.Text = "Start";
			this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.iconMenu;
			this.notifyIcon.Text = "";
			this.notifyIcon.Visible = true;
			this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
			// 
			// iconMenu
			// 
			this.iconMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menPresets,
																					 this.menPresetsSep,
																					 this.menRunning,
																					 this.menAlarm,
																					 this.menTimers,
																					 this.menuItem2,
																					 this.menAbout,
																					 this.menQuit});
			// 
			// menPresets
			// 
			this.menPresets.Enabled = false;
			this.menPresets.Index = 0;
			this.menPresets.Text = "Presets";
			// 
			// menPresetsSep
			// 
			this.menPresetsSep.Index = 1;
			this.menPresetsSep.Text = "-";
			// 
			// menRunning
			// 
			this.menRunning.Index = 2;
			this.menRunning.Text = "Running Timers";
			// 
			// menAlarm
			// 
			this.menAlarm.Index = 3;
			this.menAlarm.Text = "Set Alarm";
			this.menAlarm.Click += new System.EventHandler(this.menAlarm_Click);
			// 
			// menTimers
			// 
			this.menTimers.DefaultItem = true;
			this.menTimers.Index = 4;
			this.menTimers.Text = "Set Timer";
			this.menTimers.Click += new System.EventHandler(this.menTimers_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 5;
			this.menuItem2.Text = "-";
			// 
			// menAbout
			// 
			this.menAbout.Index = 6;
			this.menAbout.Text = "About";
			this.menAbout.Click += new System.EventHandler(this.menAbout_Click);
			// 
			// menQuit
			// 
			this.menQuit.Index = 7;
			this.menQuit.Text = "Quit";
			this.menQuit.Click += new System.EventHandler(this.menQuit_Click);
			// 
			// txtName
			// 
			this.txtName.AutoSize = false;
			this.txtName.BackColor = System.Drawing.Color.LightYellow;
			this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtName.ForeColor = System.Drawing.Color.Black;
			this.txtName.Location = new System.Drawing.Point(72, 8);
			this.txtName.MaxLength = 16;
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(96, 20);
			this.txtName.TabIndex = 1;
			this.txtName.Text = "";
			this.txtName.Click += new System.EventHandler(this.highlight_OnFocus);
			this.txtName.Enter += new System.EventHandler(this.highlight_OnFocus);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 24);
			this.label1.TabIndex = 12;
			this.label1.Text = "Name:";
			// 
			// tabControl1
			// 
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl1.Controls.Add(this.tabCreate);
			this.tabControl1.Controls.Add(this.tabAlarm);
			this.tabControl1.Controls.Add(this.tabView);
			this.tabControl1.Controls.Add(this.tabPresets);
			this.tabControl1.Controls.Add(this.tabConfig);
			this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabControl1.HotTrack = true;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = new System.Drawing.Point(2, 3);
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(312, 336);
			this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			this.tabControl1.TabIndex = 0;
			this.tabControl1.Click += new System.EventHandler(this.tabView_Click);
			// 
			// tabCreate
			// 
			this.tabCreate.BackColor = System.Drawing.Color.Transparent;
			this.tabCreate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabCreate.BackgroundImage")));
			this.tabCreate.Controls.Add(this.cbxTimerRepeat);
			this.tabCreate.Controls.Add(this.label32);
			this.tabCreate.Controls.Add(this.label30);
			this.tabCreate.Controls.Add(this.cmbTimerProcess);
			this.tabCreate.Controls.Add(this.btnTimerPlay);
			this.tabCreate.Controls.Add(this.lblTimerIcon);
			this.tabCreate.Controls.Add(this.cmbTimerIcon);
			this.tabCreate.Controls.Add(this.lblTimerSnd);
			this.tabCreate.Controls.Add(this.cmbTimerSnd);
			this.tabCreate.Controls.Add(this.btnMinPlus10);
			this.tabCreate.Controls.Add(this.btnMinPlus5);
			this.tabCreate.Controls.Add(this.btnMinPlus1);
			this.tabCreate.Controls.Add(this.btnHourPlus8);
			this.tabCreate.Controls.Add(this.btnHourPlus1);
			this.tabCreate.Controls.Add(this.groupBox2);
			this.tabCreate.Controls.Add(this.txtName);
			this.tabCreate.Controls.Add(this.label1);
			this.tabCreate.Controls.Add(this.numHour);
			this.tabCreate.Controls.Add(this.numDay);
			this.tabCreate.Controls.Add(this.numMin);
			this.tabCreate.Controls.Add(this.v);
			this.tabCreate.Controls.Add(this.label2);
			this.tabCreate.Controls.Add(this.label3);
			this.tabCreate.Controls.Add(this.btnGo);
			this.tabCreate.Controls.Add(this.btnSecPlus30);
			this.tabCreate.Controls.Add(this.numSec);
			this.tabCreate.Controls.Add(this.label4);
			this.tabCreate.Controls.Add(this.btnSecPlus15);
			this.tabCreate.Controls.Add(this.pbxTimerIcon);
			this.tabCreate.Location = new System.Drawing.Point(4, 25);
			this.tabCreate.Name = "tabCreate";
			this.tabCreate.Size = new System.Drawing.Size(304, 307);
			this.tabCreate.TabIndex = 0;
			this.tabCreate.Tag = "create";
			this.tabCreate.Text = "Set Timer";
			// 
			// cbxTimerRepeat
			// 
			this.cbxTimerRepeat.BackColor = System.Drawing.Color.Transparent;
			this.cbxTimerRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxTimerRepeat.ForeColor = System.Drawing.Color.Black;
			this.cbxTimerRepeat.Location = new System.Drawing.Point(88, 224);
			this.cbxTimerRepeat.Name = "cbxTimerRepeat";
			this.cbxTimerRepeat.Size = new System.Drawing.Size(72, 16);
			this.cbxTimerRepeat.TabIndex = 51;
			this.cbxTimerRepeat.Text = "Repeating";
			// 
			// label32
			// 
			this.label32.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label32.ForeColor = System.Drawing.Color.DarkGreen;
			this.label32.Location = new System.Drawing.Point(176, 8);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(72, 64);
			this.label32.TabIndex = 68;
			this.label32.Text = "Set a custom timer or launch a preset.";
			this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label30
			// 
			this.label30.ForeColor = System.Drawing.Color.Black;
			this.label30.Location = new System.Drawing.Point(200, 248);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(80, 16);
			this.label30.TabIndex = 67;
			this.label30.Text = "Program to run";
			// 
			// cmbTimerProcess
			// 
			this.cmbTimerProcess.BackColor = System.Drawing.Color.LightYellow;
			this.cmbTimerProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTimerProcess.ForeColor = System.Drawing.Color.Black;
			this.cmbTimerProcess.Location = new System.Drawing.Point(200, 264);
			this.cmbTimerProcess.Name = "cmbTimerProcess";
			this.cmbTimerProcess.Size = new System.Drawing.Size(96, 21);
			this.cmbTimerProcess.TabIndex = 73;
			// 
			// btnTimerPlay
			// 
			this.btnTimerPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTimerPlay.ForeColor = System.Drawing.Color.DarkRed;
			this.btnTimerPlay.Image = ((System.Drawing.Image)(resources.GetObject("btnTimerPlay.Image")));
			this.btnTimerPlay.Location = new System.Drawing.Point(48, 248);
			this.btnTimerPlay.Name = "btnTimerPlay";
			this.btnTimerPlay.Size = new System.Drawing.Size(16, 16);
			this.btnTimerPlay.TabIndex = 71;
			this.btnTimerPlay.Click += new System.EventHandler(this.btnTimerPlay_Click);
			// 
			// lblTimerIcon
			// 
			this.lblTimerIcon.ForeColor = System.Drawing.Color.Black;
			this.lblTimerIcon.Location = new System.Drawing.Point(104, 248);
			this.lblTimerIcon.Name = "lblTimerIcon";
			this.lblTimerIcon.Size = new System.Drawing.Size(40, 16);
			this.lblTimerIcon.TabIndex = 65;
			this.lblTimerIcon.Text = "Icon";
			// 
			// cmbTimerIcon
			// 
			this.cmbTimerIcon.BackColor = System.Drawing.Color.LightYellow;
			this.cmbTimerIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTimerIcon.ForeColor = System.Drawing.Color.Black;
			this.cmbTimerIcon.Location = new System.Drawing.Point(104, 264);
			this.cmbTimerIcon.Name = "cmbTimerIcon";
			this.cmbTimerIcon.Size = new System.Drawing.Size(96, 21);
			this.cmbTimerIcon.TabIndex = 72;
			this.cmbTimerIcon.SelectedIndexChanged += new System.EventHandler(this.cmbTimerIcon_SelectedIndexChanged);
			// 
			// lblTimerSnd
			// 
			this.lblTimerSnd.ForeColor = System.Drawing.Color.Black;
			this.lblTimerSnd.Location = new System.Drawing.Point(8, 248);
			this.lblTimerSnd.Name = "lblTimerSnd";
			this.lblTimerSnd.Size = new System.Drawing.Size(40, 16);
			this.lblTimerSnd.TabIndex = 63;
			this.lblTimerSnd.Text = "Sound";
			// 
			// cmbTimerSnd
			// 
			this.cmbTimerSnd.BackColor = System.Drawing.Color.LightYellow;
			this.cmbTimerSnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTimerSnd.ForeColor = System.Drawing.Color.Black;
			this.cmbTimerSnd.Location = new System.Drawing.Point(8, 264);
			this.cmbTimerSnd.Name = "cmbTimerSnd";
			this.cmbTimerSnd.Size = new System.Drawing.Size(96, 21);
			this.cmbTimerSnd.TabIndex = 70;
			// 
			// btnMinPlus10
			// 
			this.btnMinPlus10.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnMinPlus10.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnMinPlus10.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnMinPlus10.Location = new System.Drawing.Point(72, 144);
			this.btnMinPlus10.Name = "btnMinPlus10";
			this.btnMinPlus10.Size = new System.Drawing.Size(32, 16);
			this.btnMinPlus10.TabIndex = 33;
			this.btnMinPlus10.Text = "+10";
			this.btnMinPlus10.Click += new System.EventHandler(this.btnMinPlus_Click);
			// 
			// btnMinPlus5
			// 
			this.btnMinPlus5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnMinPlus5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnMinPlus5.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnMinPlus5.Location = new System.Drawing.Point(40, 144);
			this.btnMinPlus5.Name = "btnMinPlus5";
			this.btnMinPlus5.Size = new System.Drawing.Size(32, 16);
			this.btnMinPlus5.TabIndex = 32;
			this.btnMinPlus5.Text = "+5";
			this.btnMinPlus5.Click += new System.EventHandler(this.btnMinPlus_Click);
			// 
			// btnMinPlus1
			// 
			this.btnMinPlus1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnMinPlus1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnMinPlus1.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnMinPlus1.Location = new System.Drawing.Point(8, 144);
			this.btnMinPlus1.Name = "btnMinPlus1";
			this.btnMinPlus1.Size = new System.Drawing.Size(32, 16);
			this.btnMinPlus1.TabIndex = 31;
			this.btnMinPlus1.Text = "+1";
			this.btnMinPlus1.Click += new System.EventHandler(this.btnMinPlus_Click);
			// 
			// btnHourPlus8
			// 
			this.btnHourPlus8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnHourPlus8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnHourPlus8.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnHourPlus8.Location = new System.Drawing.Point(40, 96);
			this.btnHourPlus8.Name = "btnHourPlus8";
			this.btnHourPlus8.Size = new System.Drawing.Size(32, 16);
			this.btnHourPlus8.TabIndex = 22;
			this.btnHourPlus8.Text = "+8";
			this.btnHourPlus8.Click += new System.EventHandler(this.btnHourPlus_Click);
			// 
			// btnHourPlus1
			// 
			this.btnHourPlus1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnHourPlus1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnHourPlus1.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnHourPlus1.Location = new System.Drawing.Point(8, 96);
			this.btnHourPlus1.Name = "btnHourPlus1";
			this.btnHourPlus1.Size = new System.Drawing.Size(32, 16);
			this.btnHourPlus1.TabIndex = 21;
			this.btnHourPlus1.Text = "+1";
			this.btnHourPlus1.Click += new System.EventHandler(this.btnHourPlus_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.btnPre6);
			this.groupBox2.Controls.Add(this.btnPre5);
			this.groupBox2.Controls.Add(this.btnPre4);
			this.groupBox2.Controls.Add(this.btnPre3);
			this.groupBox2.Controls.Add(this.btnPre2);
			this.groupBox2.Controls.Add(this.btnPre1);
			this.groupBox2.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox2.Location = new System.Drawing.Point(160, 72);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(136, 168);
			this.groupBox2.TabIndex = 60;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Presets";
			// 
			// btnPre6
			// 
			this.btnPre6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre6.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre6.Location = new System.Drawing.Point(8, 136);
			this.btnPre6.Name = "btnPre6";
			this.btnPre6.Size = new System.Drawing.Size(120, 24);
			this.btnPre6.TabIndex = 65;
			this.btnPre6.Tag = "6";
			this.btnPre6.Text = "not set";
			this.btnPre6.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnPre5
			// 
			this.btnPre5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre5.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre5.Location = new System.Drawing.Point(8, 112);
			this.btnPre5.Name = "btnPre5";
			this.btnPre5.Size = new System.Drawing.Size(120, 24);
			this.btnPre5.TabIndex = 64;
			this.btnPre5.Tag = "5";
			this.btnPre5.Text = "not set";
			this.btnPre5.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnPre4
			// 
			this.btnPre4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre4.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre4.Location = new System.Drawing.Point(8, 88);
			this.btnPre4.Name = "btnPre4";
			this.btnPre4.Size = new System.Drawing.Size(120, 24);
			this.btnPre4.TabIndex = 63;
			this.btnPre4.Tag = "4";
			this.btnPre4.Text = "not set";
			this.btnPre4.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnPre3
			// 
			this.btnPre3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre3.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre3.Location = new System.Drawing.Point(8, 64);
			this.btnPre3.Name = "btnPre3";
			this.btnPre3.Size = new System.Drawing.Size(120, 24);
			this.btnPre3.TabIndex = 62;
			this.btnPre3.Tag = "3";
			this.btnPre3.Text = "not set";
			this.btnPre3.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnPre2
			// 
			this.btnPre2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre2.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre2.Location = new System.Drawing.Point(8, 40);
			this.btnPre2.Name = "btnPre2";
			this.btnPre2.Size = new System.Drawing.Size(120, 24);
			this.btnPre2.TabIndex = 61;
			this.btnPre2.Tag = "2";
			this.btnPre2.Text = "not set";
			this.btnPre2.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnPre1
			// 
			this.btnPre1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPre1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPre1.ForeColor = System.Drawing.Color.Crimson;
			this.btnPre1.Location = new System.Drawing.Point(8, 16);
			this.btnPre1.Name = "btnPre1";
			this.btnPre1.Size = new System.Drawing.Size(120, 24);
			this.btnPre1.TabIndex = 60;
			this.btnPre1.Tag = "1";
			this.btnPre1.Text = "not set";
			this.btnPre1.Click += new System.EventHandler(this.btnPre_Click);
			// 
			// btnSecPlus30
			// 
			this.btnSecPlus30.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnSecPlus30.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnSecPlus30.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnSecPlus30.Location = new System.Drawing.Point(40, 192);
			this.btnSecPlus30.Name = "btnSecPlus30";
			this.btnSecPlus30.Size = new System.Drawing.Size(32, 16);
			this.btnSecPlus30.TabIndex = 42;
			this.btnSecPlus30.Text = "+30";
			this.btnSecPlus30.Click += new System.EventHandler(this.btnSecPlus_Click);
			// 
			// btnSecPlus15
			// 
			this.btnSecPlus15.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnSecPlus15.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnSecPlus15.ForeColor = System.Drawing.Color.DarkGreen;
			this.btnSecPlus15.Location = new System.Drawing.Point(8, 192);
			this.btnSecPlus15.Name = "btnSecPlus15";
			this.btnSecPlus15.Size = new System.Drawing.Size(32, 16);
			this.btnSecPlus15.TabIndex = 41;
			this.btnSecPlus15.Text = "+15";
			this.btnSecPlus15.Click += new System.EventHandler(this.btnSecPlus_Click);
			// 
			// pbxTimerIcon
			// 
			this.pbxTimerIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxTimerIcon.Location = new System.Drawing.Point(256, 8);
			this.pbxTimerIcon.Name = "pbxTimerIcon";
			this.pbxTimerIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxTimerIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbxTimerIcon.TabIndex = 61;
			this.pbxTimerIcon.TabStop = false;
			// 
			// tabAlarm
			// 
			this.tabAlarm.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabAlarm.BackgroundImage")));
			this.tabAlarm.Controls.Add(this.label33);
			this.tabAlarm.Controls.Add(this.label31);
			this.tabAlarm.Controls.Add(this.cmbAlarmProcess);
			this.tabAlarm.Controls.Add(this.btnAlarmPlay);
			this.tabAlarm.Controls.Add(this.pbxAlarmIcon);
			this.tabAlarm.Controls.Add(this.lblAlarmIcon);
			this.tabAlarm.Controls.Add(this.cmbAlarmIcon);
			this.tabAlarm.Controls.Add(this.lblAlarmSound);
			this.tabAlarm.Controls.Add(this.cmbAlarmSnd);
			this.tabAlarm.Controls.Add(this.radPM);
			this.tabAlarm.Controls.Add(this.radAM);
			this.tabAlarm.Controls.Add(this.txtAlarmName);
			this.tabAlarm.Controls.Add(this.label19);
			this.tabAlarm.Controls.Add(this.btnAlarmGo);
			this.tabAlarm.Controls.Add(this.label18);
			this.tabAlarm.Controls.Add(this.label17);
			this.tabAlarm.Controls.Add(this.numAlarmSec);
			this.tabAlarm.Controls.Add(this.numAlarmMin);
			this.tabAlarm.Controls.Add(this.label16);
			this.tabAlarm.Controls.Add(this.numAlarmHour);
			this.tabAlarm.Controls.Add(this.label15);
			this.tabAlarm.Controls.Add(this.calAlarm);
			this.tabAlarm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabAlarm.Location = new System.Drawing.Point(4, 25);
			this.tabAlarm.Name = "tabAlarm";
			this.tabAlarm.Size = new System.Drawing.Size(304, 307);
			this.tabAlarm.TabIndex = 4;
			this.tabAlarm.Tag = "alarm";
			this.tabAlarm.Text = "Set Alarm";
			// 
			// label33
			// 
			this.label33.BackColor = System.Drawing.Color.Transparent;
			this.label33.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label33.ForeColor = System.Drawing.Color.DarkGreen;
			this.label33.Location = new System.Drawing.Point(8, 8);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(240, 16);
			this.label33.TabIndex = 71;
			this.label33.Text = "Set an alarm from this screen.";
			this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label31
			// 
			this.label31.BackColor = System.Drawing.Color.Transparent;
			this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label31.ForeColor = System.Drawing.Color.Black;
			this.label31.Location = new System.Drawing.Point(200, 248);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(88, 16);
			this.label31.TabIndex = 70;
			this.label31.Text = "Program to run";
			// 
			// cmbAlarmProcess
			// 
			this.cmbAlarmProcess.BackColor = System.Drawing.Color.LightYellow;
			this.cmbAlarmProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAlarmProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbAlarmProcess.ForeColor = System.Drawing.Color.Black;
			this.cmbAlarmProcess.Location = new System.Drawing.Point(200, 264);
			this.cmbAlarmProcess.Name = "cmbAlarmProcess";
			this.cmbAlarmProcess.Size = new System.Drawing.Size(96, 21);
			this.cmbAlarmProcess.TabIndex = 33;
			// 
			// btnAlarmPlay
			// 
			this.btnAlarmPlay.BackColor = System.Drawing.Color.Transparent;
			this.btnAlarmPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAlarmPlay.ForeColor = System.Drawing.Color.DarkRed;
			this.btnAlarmPlay.Image = ((System.Drawing.Image)(resources.GetObject("btnAlarmPlay.Image")));
			this.btnAlarmPlay.Location = new System.Drawing.Point(48, 248);
			this.btnAlarmPlay.Name = "btnAlarmPlay";
			this.btnAlarmPlay.Size = new System.Drawing.Size(16, 16);
			this.btnAlarmPlay.TabIndex = 31;
			this.btnAlarmPlay.Click += new System.EventHandler(this.btnAlarmPlay_Click);
			// 
			// pbxAlarmIcon
			// 
			this.pbxAlarmIcon.BackColor = System.Drawing.Color.Transparent;
			this.pbxAlarmIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxAlarmIcon.Location = new System.Drawing.Point(256, 8);
			this.pbxAlarmIcon.Name = "pbxAlarmIcon";
			this.pbxAlarmIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxAlarmIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbxAlarmIcon.TabIndex = 68;
			this.pbxAlarmIcon.TabStop = false;
			// 
			// lblAlarmIcon
			// 
			this.lblAlarmIcon.BackColor = System.Drawing.Color.Transparent;
			this.lblAlarmIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblAlarmIcon.ForeColor = System.Drawing.Color.Black;
			this.lblAlarmIcon.Location = new System.Drawing.Point(104, 248);
			this.lblAlarmIcon.Name = "lblAlarmIcon";
			this.lblAlarmIcon.Size = new System.Drawing.Size(40, 16);
			this.lblAlarmIcon.TabIndex = 67;
			this.lblAlarmIcon.Text = "Icon";
			// 
			// cmbAlarmIcon
			// 
			this.cmbAlarmIcon.BackColor = System.Drawing.Color.LightYellow;
			this.cmbAlarmIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAlarmIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbAlarmIcon.ForeColor = System.Drawing.Color.Black;
			this.cmbAlarmIcon.Location = new System.Drawing.Point(104, 264);
			this.cmbAlarmIcon.Name = "cmbAlarmIcon";
			this.cmbAlarmIcon.Size = new System.Drawing.Size(96, 21);
			this.cmbAlarmIcon.TabIndex = 32;
			this.cmbAlarmIcon.SelectedIndexChanged += new System.EventHandler(this.cmbAlarmIcon_SelectedIndexChanged);
			// 
			// lblAlarmSound
			// 
			this.lblAlarmSound.BackColor = System.Drawing.Color.Transparent;
			this.lblAlarmSound.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblAlarmSound.ForeColor = System.Drawing.Color.Black;
			this.lblAlarmSound.Location = new System.Drawing.Point(8, 248);
			this.lblAlarmSound.Name = "lblAlarmSound";
			this.lblAlarmSound.Size = new System.Drawing.Size(40, 16);
			this.lblAlarmSound.TabIndex = 65;
			this.lblAlarmSound.Text = "Sound";
			// 
			// cmbAlarmSnd
			// 
			this.cmbAlarmSnd.BackColor = System.Drawing.Color.LightYellow;
			this.cmbAlarmSnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbAlarmSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbAlarmSnd.ForeColor = System.Drawing.Color.Black;
			this.cmbAlarmSnd.Location = new System.Drawing.Point(8, 264);
			this.cmbAlarmSnd.Name = "cmbAlarmSnd";
			this.cmbAlarmSnd.Size = new System.Drawing.Size(96, 21);
			this.cmbAlarmSnd.TabIndex = 30;
			// 
			// radPM
			// 
			this.radPM.BackColor = System.Drawing.Color.Transparent;
			this.radPM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPM.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.radPM.ForeColor = System.Drawing.Color.Black;
			this.radPM.Location = new System.Drawing.Point(248, 72);
			this.radPM.Name = "radPM";
			this.radPM.Size = new System.Drawing.Size(40, 16);
			this.radPM.TabIndex = 14;
			this.radPM.Text = "PM";
			// 
			// radAM
			// 
			this.radAM.BackColor = System.Drawing.Color.Transparent;
			this.radAM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radAM.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.radAM.ForeColor = System.Drawing.Color.Black;
			this.radAM.Location = new System.Drawing.Point(248, 56);
			this.radAM.Name = "radAM";
			this.radAM.Size = new System.Drawing.Size(40, 16);
			this.radAM.TabIndex = 13;
			this.radAM.Text = "AM";
			// 
			// txtAlarmName
			// 
			this.txtAlarmName.AutoSize = false;
			this.txtAlarmName.BackColor = System.Drawing.Color.LightYellow;
			this.txtAlarmName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtAlarmName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtAlarmName.ForeColor = System.Drawing.Color.Black;
			this.txtAlarmName.Location = new System.Drawing.Point(72, 32);
			this.txtAlarmName.MaxLength = 16;
			this.txtAlarmName.Name = "txtAlarmName";
			this.txtAlarmName.Size = new System.Drawing.Size(104, 20);
			this.txtAlarmName.TabIndex = 1;
			this.txtAlarmName.Text = "";
			this.txtAlarmName.Click += new System.EventHandler(this.highlight_OnFocus);
			this.txtAlarmName.Enter += new System.EventHandler(this.highlight_OnFocus);
			// 
			// label19
			// 
			this.label19.BackColor = System.Drawing.Color.Transparent;
			this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label19.ForeColor = System.Drawing.Color.Black;
			this.label19.Location = new System.Drawing.Point(8, 32);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(64, 24);
			this.label19.TabIndex = 39;
			this.label19.Text = "Name:";
			// 
			// btnAlarmGo
			// 
			this.btnAlarmGo.BackColor = System.Drawing.Color.Transparent;
			this.btnAlarmGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAlarmGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnAlarmGo.ForeColor = System.Drawing.Color.Crimson;
			this.btnAlarmGo.Location = new System.Drawing.Point(8, 168);
			this.btnAlarmGo.Name = "btnAlarmGo";
			this.btnAlarmGo.Size = new System.Drawing.Size(80, 48);
			this.btnAlarmGo.TabIndex = 20;
			this.btnAlarmGo.Text = "Start";
			this.btnAlarmGo.Click += new System.EventHandler(this.btnAlarmGo_Click);
			// 
			// label18
			// 
			this.label18.BackColor = System.Drawing.Color.Transparent;
			this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label18.ForeColor = System.Drawing.Color.Black;
			this.label18.Location = new System.Drawing.Point(11, 97);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(64, 40);
			this.label18.TabIndex = 35;
			this.label18.Text = "Choose date:";
			this.label18.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label17
			// 
			this.label17.BackColor = System.Drawing.Color.Transparent;
			this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label17.ForeColor = System.Drawing.Color.Black;
			this.label17.Location = new System.Drawing.Point(8, 64);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(80, 16);
			this.label17.TabIndex = 34;
			this.label17.Text = "Set time:";
			// 
			// numAlarmSec
			// 
			this.numAlarmSec.BackColor = System.Drawing.Color.LightYellow;
			this.numAlarmSec.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numAlarmSec.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.numAlarmSec.ForeColor = System.Drawing.Color.Black;
			this.numAlarmSec.Location = new System.Drawing.Point(200, 64);
			this.numAlarmSec.Maximum = new System.Decimal(new int[] {
																		59,
																		0,
																		0,
																		0});
			this.numAlarmSec.Name = "numAlarmSec";
			this.numAlarmSec.Size = new System.Drawing.Size(40, 20);
			this.numAlarmSec.TabIndex = 12;
			this.numAlarmSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numAlarmSec.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numAlarmSec.Enter += new System.EventHandler(this.highlight_OnFocus);
			// 
			// numAlarmMin
			// 
			this.numAlarmMin.BackColor = System.Drawing.Color.LightYellow;
			this.numAlarmMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numAlarmMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.numAlarmMin.ForeColor = System.Drawing.Color.Black;
			this.numAlarmMin.Location = new System.Drawing.Point(144, 64);
			this.numAlarmMin.Maximum = new System.Decimal(new int[] {
																		59,
																		0,
																		0,
																		0});
			this.numAlarmMin.Name = "numAlarmMin";
			this.numAlarmMin.Size = new System.Drawing.Size(40, 20);
			this.numAlarmMin.TabIndex = 11;
			this.numAlarmMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numAlarmMin.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numAlarmMin.Enter += new System.EventHandler(this.highlight_OnFocus);
			// 
			// label16
			// 
			this.label16.BackColor = System.Drawing.Color.Transparent;
			this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label16.ForeColor = System.Drawing.Color.Crimson;
			this.label16.Location = new System.Drawing.Point(184, 56);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(16, 32);
			this.label16.TabIndex = 31;
			this.label16.Text = ":";
			// 
			// numAlarmHour
			// 
			this.numAlarmHour.BackColor = System.Drawing.Color.LightYellow;
			this.numAlarmHour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numAlarmHour.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.numAlarmHour.ForeColor = System.Drawing.Color.Black;
			this.numAlarmHour.Location = new System.Drawing.Point(88, 64);
			this.numAlarmHour.Maximum = new System.Decimal(new int[] {
																		 12,
																		 0,
																		 0,
																		 0});
			this.numAlarmHour.Name = "numAlarmHour";
			this.numAlarmHour.Size = new System.Drawing.Size(40, 20);
			this.numAlarmHour.TabIndex = 10;
			this.numAlarmHour.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numAlarmHour.Click += new System.EventHandler(this.highlight_OnFocus);
			this.numAlarmHour.Enter += new System.EventHandler(this.highlight_OnFocus);
			// 
			// label15
			// 
			this.label15.BackColor = System.Drawing.Color.Transparent;
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label15.ForeColor = System.Drawing.Color.Crimson;
			this.label15.Location = new System.Drawing.Point(128, 56);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(16, 32);
			this.label15.TabIndex = 29;
			this.label15.Text = ":";
			// 
			// calAlarm
			// 
			this.calAlarm.BackColor = System.Drawing.Color.LightYellow;
			this.calAlarm.ForeColor = System.Drawing.Color.Black;
			this.calAlarm.Location = new System.Drawing.Point(96, 88);
			this.calAlarm.MaxSelectionCount = 1;
			this.calAlarm.Name = "calAlarm";
			this.calAlarm.TabIndex = 15;
			this.calAlarm.TitleBackColor = System.Drawing.Color.DarkGreen;
			this.calAlarm.TitleForeColor = System.Drawing.Color.White;
			// 
			// tabView
			// 
			this.tabView.BackColor = System.Drawing.Color.Transparent;
			this.tabView.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabView.BackgroundImage")));
			this.tabView.Controls.Add(this.label34);
			this.tabView.Controls.Add(this.btnStopwatch);
			this.tabView.Controls.Add(this.btnPause);
			this.tabView.Controls.Add(this.btnStop);
			this.tabView.Controls.Add(this.groupBox1);
			this.tabView.Controls.Add(this.lstTimers);
			this.tabView.Location = new System.Drawing.Point(4, 25);
			this.tabView.Name = "tabView";
			this.tabView.Size = new System.Drawing.Size(304, 307);
			this.tabView.TabIndex = 1;
			this.tabView.Tag = "view";
			this.tabView.Text = "View Timers";
			// 
			// label34
			// 
			this.label34.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label34.ForeColor = System.Drawing.Color.DarkGreen;
			this.label34.Location = new System.Drawing.Point(8, 8);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(288, 16);
			this.label34.TabIndex = 69;
			this.label34.Text = "View and control currently active timers and alarms.";
			this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnStopwatch
			// 
			this.btnStopwatch.BackColor = System.Drawing.Color.Transparent;
			this.btnStopwatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStopwatch.ForeColor = System.Drawing.Color.Black;
			this.btnStopwatch.Location = new System.Drawing.Point(136, 208);
			this.btnStopwatch.Name = "btnStopwatch";
			this.btnStopwatch.Size = new System.Drawing.Size(144, 24);
			this.btnStopwatch.TabIndex = 2;
			this.btnStopwatch.Text = "Show Stopwatch";
			this.btnStopwatch.Click += new System.EventHandler(this.btnStopwatch_Click);
			// 
			// btnPause
			// 
			this.btnPause.BackColor = System.Drawing.Color.Transparent;
			this.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPause.ForeColor = System.Drawing.Color.Black;
			this.btnPause.Location = new System.Drawing.Point(136, 256);
			this.btnPause.Name = "btnPause";
			this.btnPause.Size = new System.Drawing.Size(144, 24);
			this.btnPause.TabIndex = 4;
			this.btnPause.Text = "Pause / Resume";
			this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
			// 
			// btnStop
			// 
			this.btnStop.BackColor = System.Drawing.Color.Transparent;
			this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnStop.ForeColor = System.Drawing.Color.Black;
			this.btnStop.Location = new System.Drawing.Point(136, 232);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(144, 24);
			this.btnStop.TabIndex = 3;
			this.btnStop.Text = "Stop and Remove";
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbxViewRepeat);
			this.groupBox1.Controls.Add(this.lblViewProcess);
			this.groupBox1.Controls.Add(this.cbxViewProcess);
			this.groupBox1.Controls.Add(this.lblViewAudible);
			this.groupBox1.Controls.Add(this.cbxViewAudible);
			this.groupBox1.Controls.Add(this.cbxViewVisual);
			this.groupBox1.Controls.Add(this.lblLeft);
			this.groupBox1.Controls.Add(this.label26);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.lblFinish);
			this.groupBox1.Controls.Add(this.lblElapsed);
			this.groupBox1.Controls.Add(this.lblTime);
			this.groupBox1.Controls.Add(this.lblName);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.pbxViewIcon);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox1.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox1.Location = new System.Drawing.Point(120, 32);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(176, 170);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Timer Information";
			// 
			// cbxViewRepeat
			// 
			this.cbxViewRepeat.AutoCheck = false;
			this.cbxViewRepeat.BackColor = System.Drawing.Color.Transparent;
			this.cbxViewRepeat.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.cbxViewRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxViewRepeat.ForeColor = System.Drawing.Color.Black;
			this.cbxViewRepeat.Location = new System.Drawing.Point(88, 120);
			this.cbxViewRepeat.Name = "cbxViewRepeat";
			this.cbxViewRepeat.Size = new System.Drawing.Size(80, 16);
			this.cbxViewRepeat.TabIndex = 70;
			this.cbxViewRepeat.Text = "Repeating";
			// 
			// lblViewProcess
			// 
			this.lblViewProcess.Location = new System.Drawing.Point(88, 152);
			this.lblViewProcess.Name = "lblViewProcess";
			this.lblViewProcess.Size = new System.Drawing.Size(88, 14);
			this.lblViewProcess.TabIndex = 69;
			this.lblViewProcess.Text = "sample";
			// 
			// cbxViewProcess
			// 
			this.cbxViewProcess.AutoCheck = false;
			this.cbxViewProcess.BackColor = System.Drawing.Color.Transparent;
			this.cbxViewProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxViewProcess.ForeColor = System.Drawing.Color.Black;
			this.cbxViewProcess.Location = new System.Drawing.Point(8, 152);
			this.cbxViewProcess.Name = "cbxViewProcess";
			this.cbxViewProcess.Size = new System.Drawing.Size(88, 16);
			this.cbxViewProcess.TabIndex = 68;
			this.cbxViewProcess.Text = "Program:";
			// 
			// lblViewAudible
			// 
			this.lblViewAudible.Location = new System.Drawing.Point(88, 136);
			this.lblViewAudible.Name = "lblViewAudible";
			this.lblViewAudible.Size = new System.Drawing.Size(88, 13);
			this.lblViewAudible.TabIndex = 67;
			this.lblViewAudible.Text = "sample";
			// 
			// cbxViewAudible
			// 
			this.cbxViewAudible.AutoCheck = false;
			this.cbxViewAudible.BackColor = System.Drawing.Color.Transparent;
			this.cbxViewAudible.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxViewAudible.ForeColor = System.Drawing.Color.Black;
			this.cbxViewAudible.Location = new System.Drawing.Point(8, 136);
			this.cbxViewAudible.Name = "cbxViewAudible";
			this.cbxViewAudible.Size = new System.Drawing.Size(88, 16);
			this.cbxViewAudible.TabIndex = 66;
			this.cbxViewAudible.Text = "Audible alert:";
			// 
			// cbxViewVisual
			// 
			this.cbxViewVisual.AutoCheck = false;
			this.cbxViewVisual.BackColor = System.Drawing.Color.Transparent;
			this.cbxViewVisual.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.cbxViewVisual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxViewVisual.ForeColor = System.Drawing.Color.Black;
			this.cbxViewVisual.Location = new System.Drawing.Point(8, 120);
			this.cbxViewVisual.Name = "cbxViewVisual";
			this.cbxViewVisual.Size = new System.Drawing.Size(80, 16);
			this.cbxViewVisual.TabIndex = 65;
			this.cbxViewVisual.Text = "Visual alert";
			// 
			// lblLeft
			// 
			this.lblLeft.ForeColor = System.Drawing.Color.Crimson;
			this.lblLeft.Location = new System.Drawing.Point(48, 88);
			this.lblLeft.Name = "lblLeft";
			this.lblLeft.Size = new System.Drawing.Size(128, 16);
			this.lblLeft.TabIndex = 64;
			// 
			// label26
			// 
			this.label26.ForeColor = System.Drawing.Color.Black;
			this.label26.Location = new System.Drawing.Point(0, 88);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(48, 16);
			this.label26.TabIndex = 63;
			this.label26.Text = "Left:";
			this.label26.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label8
			// 
			this.label8.ForeColor = System.Drawing.Color.Black;
			this.label8.Location = new System.Drawing.Point(0, 104);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 16);
			this.label8.TabIndex = 7;
			this.label8.Text = "Will pop:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblFinish
			// 
			this.lblFinish.Location = new System.Drawing.Point(48, 104);
			this.lblFinish.Name = "lblFinish";
			this.lblFinish.Size = new System.Drawing.Size(128, 16);
			this.lblFinish.TabIndex = 6;
			// 
			// lblElapsed
			// 
			this.lblElapsed.ForeColor = System.Drawing.Color.Crimson;
			this.lblElapsed.Location = new System.Drawing.Point(48, 72);
			this.lblElapsed.Name = "lblElapsed";
			this.lblElapsed.Size = new System.Drawing.Size(128, 16);
			this.lblElapsed.TabIndex = 5;
			// 
			// lblTime
			// 
			this.lblTime.Location = new System.Drawing.Point(48, 56);
			this.lblTime.Name = "lblTime";
			this.lblTime.Size = new System.Drawing.Size(128, 16);
			this.lblTime.TabIndex = 4;
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(56, 16);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(104, 40);
			this.lblName.TabIndex = 3;
			// 
			// label7
			// 
			this.label7.ForeColor = System.Drawing.Color.Black;
			this.label7.Location = new System.Drawing.Point(0, 72);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(48, 16);
			this.label7.TabIndex = 2;
			this.label7.Text = "Elapsed:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label6
			// 
			this.label6.ForeColor = System.Drawing.Color.Black;
			this.label6.Location = new System.Drawing.Point(8, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 16);
			this.label6.TabIndex = 1;
			this.label6.Text = "Time:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// pbxViewIcon
			// 
			this.pbxViewIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxViewIcon.Location = new System.Drawing.Point(8, 16);
			this.pbxViewIcon.Name = "pbxViewIcon";
			this.pbxViewIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxViewIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbxViewIcon.TabIndex = 62;
			this.pbxViewIcon.TabStop = false;
			// 
			// lstTimers
			// 
			this.lstTimers.BackColor = System.Drawing.Color.LightYellow;
			this.lstTimers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lstTimers.ForeColor = System.Drawing.Color.Black;
			this.lstTimers.Location = new System.Drawing.Point(8, 32);
			this.lstTimers.Name = "lstTimers";
			this.lstTimers.Size = new System.Drawing.Size(104, 249);
			this.lstTimers.TabIndex = 1;
			this.lstTimers.SelectedIndexChanged += new System.EventHandler(this.lstTimers_SelectedIndexChanged);
			// 
			// tabPresets
			// 
			this.tabPresets.BackColor = System.Drawing.Color.Transparent;
			this.tabPresets.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPresets.BackgroundImage")));
			this.tabPresets.Controls.Add(this.cbxPreRepeat);
			this.tabPresets.Controls.Add(this.btnPreRunProcess);
			this.tabPresets.Controls.Add(this.btnPreBrowseProcess);
			this.tabPresets.Controls.Add(this.txtPreProcess);
			this.tabPresets.Controls.Add(this.lblPreProcess);
			this.tabPresets.Controls.Add(this.cmbPreProcess);
			this.tabPresets.Controls.Add(this.cbxPreSnd);
			this.tabPresets.Controls.Add(this.cbxPreIcon);
			this.tabPresets.Controls.Add(this.btnPreDelete);
			this.tabPresets.Controls.Add(this.btnPreSave);
			this.tabPresets.Controls.Add(this.btnPreIcon);
			this.tabPresets.Controls.Add(this.txtPreIcon);
			this.tabPresets.Controls.Add(this.btnPreSnd);
			this.tabPresets.Controls.Add(this.txtPreSnd);
			this.tabPresets.Controls.Add(this.pbxPreIcon);
			this.tabPresets.Controls.Add(this.btnPrePlaySnd);
			this.tabPresets.Controls.Add(this.lblPreIcon);
			this.tabPresets.Controls.Add(this.cmbPreIcon);
			this.tabPresets.Controls.Add(this.lblPreSnd);
			this.tabPresets.Controls.Add(this.cmbPreSnd);
			this.tabPresets.Controls.Add(this.radPre6);
			this.tabPresets.Controls.Add(this.radPre5);
			this.tabPresets.Controls.Add(this.radPre4);
			this.tabPresets.Controls.Add(this.radPre3);
			this.tabPresets.Controls.Add(this.radPre2);
			this.tabPresets.Controls.Add(this.radPre1);
			this.tabPresets.Controls.Add(this.label14);
			this.tabPresets.Controls.Add(this.label13);
			this.tabPresets.Controls.Add(this.label12);
			this.tabPresets.Controls.Add(this.label11);
			this.tabPresets.Controls.Add(this.label10);
			this.tabPresets.Controls.Add(this.label9);
			this.tabPresets.Controls.Add(this.numSecPre);
			this.tabPresets.Controls.Add(this.numMinPre);
			this.tabPresets.Controls.Add(this.numHourPre);
			this.tabPresets.Controls.Add(this.numDayPre);
			this.tabPresets.Controls.Add(this.txtPreName);
			this.tabPresets.Location = new System.Drawing.Point(4, 25);
			this.tabPresets.Name = "tabPresets";
			this.tabPresets.Size = new System.Drawing.Size(304, 307);
			this.tabPresets.TabIndex = 3;
			this.tabPresets.Tag = "presets";
			this.tabPresets.Text = "Edit Presets";
			// 
			// cbxPreRepeat
			// 
			this.cbxPreRepeat.BackColor = System.Drawing.Color.Transparent;
			this.cbxPreRepeat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxPreRepeat.ForeColor = System.Drawing.Color.Black;
			this.cbxPreRepeat.Location = new System.Drawing.Point(8, 240);
			this.cbxPreRepeat.Name = "cbxPreRepeat";
			this.cbxPreRepeat.Size = new System.Drawing.Size(72, 16);
			this.cbxPreRepeat.TabIndex = 60;
			this.cbxPreRepeat.Text = "Repeating";
			// 
			// btnPreRunProcess
			// 
			this.btnPreRunProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnPreRunProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreRunProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreRunProcess.ForeColor = System.Drawing.Color.Black;
			this.btnPreRunProcess.Location = new System.Drawing.Point(256, 208);
			this.btnPreRunProcess.Name = "btnPreRunProcess";
			this.btnPreRunProcess.Size = new System.Drawing.Size(40, 24);
			this.btnPreRunProcess.TabIndex = 53;
			this.btnPreRunProcess.Text = "Run";
			this.btnPreRunProcess.Click += new System.EventHandler(this.btnPreRunProcess_Click);
			// 
			// btnPreBrowseProcess
			// 
			this.btnPreBrowseProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnPreBrowseProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreBrowseProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreBrowseProcess.ForeColor = System.Drawing.Color.Black;
			this.btnPreBrowseProcess.Location = new System.Drawing.Point(184, 208);
			this.btnPreBrowseProcess.Name = "btnPreBrowseProcess";
			this.btnPreBrowseProcess.Size = new System.Drawing.Size(64, 24);
			this.btnPreBrowseProcess.TabIndex = 52;
			this.btnPreBrowseProcess.Text = "Browse...";
			this.btnPreBrowseProcess.Click += new System.EventHandler(this.btnPreBrowseProcess_Click);
			// 
			// txtPreProcess
			// 
			this.txtPreProcess.AutoSize = false;
			this.txtPreProcess.BackColor = System.Drawing.Color.LightYellow;
			this.txtPreProcess.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPreProcess.ForeColor = System.Drawing.Color.Black;
			this.txtPreProcess.Location = new System.Drawing.Point(96, 208);
			this.txtPreProcess.Name = "txtPreProcess";
			this.txtPreProcess.Size = new System.Drawing.Size(80, 20);
			this.txtPreProcess.TabIndex = 51;
			this.txtPreProcess.Text = "";
			this.txtPreProcess.TextChanged += new System.EventHandler(this.txtPreProcess_TextChanged);
			// 
			// lblPreProcess
			// 
			this.lblPreProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPreProcess.ForeColor = System.Drawing.Color.Black;
			this.lblPreProcess.Location = new System.Drawing.Point(8, 192);
			this.lblPreProcess.Name = "lblPreProcess";
			this.lblPreProcess.Size = new System.Drawing.Size(88, 16);
			this.lblPreProcess.TabIndex = 80;
			this.lblPreProcess.Text = "Program to run";
			// 
			// cmbPreProcess
			// 
			this.cmbPreProcess.BackColor = System.Drawing.Color.LightYellow;
			this.cmbPreProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPreProcess.ForeColor = System.Drawing.Color.Black;
			this.cmbPreProcess.Location = new System.Drawing.Point(8, 208);
			this.cmbPreProcess.Name = "cmbPreProcess";
			this.cmbPreProcess.Size = new System.Drawing.Size(80, 21);
			this.cmbPreProcess.TabIndex = 50;
			this.cmbPreProcess.SelectedIndexChanged += new System.EventHandler(this.cmbPreProcess_SelectedIndexChanged);
			// 
			// cbxPreSnd
			// 
			this.cbxPreSnd.BackColor = System.Drawing.Color.Transparent;
			this.cbxPreSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxPreSnd.ForeColor = System.Drawing.Color.Black;
			this.cbxPreSnd.Location = new System.Drawing.Point(96, 96);
			this.cbxPreSnd.Name = "cbxPreSnd";
			this.cbxPreSnd.Size = new System.Drawing.Size(120, 16);
			this.cbxPreSnd.TabIndex = 31;
			this.cbxPreSnd.Text = "Use default";
			this.cbxPreSnd.CheckedChanged += new System.EventHandler(this.cbxPreSnd_CheckedChanged);
			// 
			// cbxPreIcon
			// 
			this.cbxPreIcon.BackColor = System.Drawing.Color.Transparent;
			this.cbxPreIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxPreIcon.ForeColor = System.Drawing.Color.Black;
			this.cbxPreIcon.Location = new System.Drawing.Point(96, 144);
			this.cbxPreIcon.Name = "cbxPreIcon";
			this.cbxPreIcon.Size = new System.Drawing.Size(120, 16);
			this.cbxPreIcon.TabIndex = 41;
			this.cbxPreIcon.Text = "Use default";
			this.cbxPreIcon.CheckedChanged += new System.EventHandler(this.cbxPreIcon_CheckedChanged);
			// 
			// btnPreDelete
			// 
			this.btnPreDelete.BackColor = System.Drawing.Color.Transparent;
			this.btnPreDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreDelete.ForeColor = System.Drawing.Color.Black;
			this.btnPreDelete.Location = new System.Drawing.Point(152, 264);
			this.btnPreDelete.Name = "btnPreDelete";
			this.btnPreDelete.Size = new System.Drawing.Size(144, 24);
			this.btnPreDelete.TabIndex = 101;
			this.btnPreDelete.Text = "Delete Preset";
			this.btnPreDelete.Click += new System.EventHandler(this.btnPreDelete_Click);
			// 
			// btnPreSave
			// 
			this.btnPreSave.BackColor = System.Drawing.Color.Transparent;
			this.btnPreSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreSave.ForeColor = System.Drawing.Color.Black;
			this.btnPreSave.Location = new System.Drawing.Point(8, 264);
			this.btnPreSave.Name = "btnPreSave";
			this.btnPreSave.Size = new System.Drawing.Size(136, 24);
			this.btnPreSave.TabIndex = 100;
			this.btnPreSave.Text = "Save Preset";
			this.btnPreSave.Click += new System.EventHandler(this.btnPreSave_Click);
			// 
			// btnPreIcon
			// 
			this.btnPreIcon.BackColor = System.Drawing.Color.Transparent;
			this.btnPreIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreIcon.ForeColor = System.Drawing.Color.Black;
			this.btnPreIcon.Location = new System.Drawing.Point(184, 160);
			this.btnPreIcon.Name = "btnPreIcon";
			this.btnPreIcon.Size = new System.Drawing.Size(64, 24);
			this.btnPreIcon.TabIndex = 42;
			this.btnPreIcon.Text = "Browse...";
			this.btnPreIcon.Click += new System.EventHandler(this.btnPreIcon_Click);
			// 
			// txtPreIcon
			// 
			this.txtPreIcon.AutoSize = false;
			this.txtPreIcon.BackColor = System.Drawing.Color.LightYellow;
			this.txtPreIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPreIcon.Enabled = false;
			this.txtPreIcon.ForeColor = System.Drawing.Color.Black;
			this.txtPreIcon.Location = new System.Drawing.Point(96, 160);
			this.txtPreIcon.Name = "txtPreIcon";
			this.txtPreIcon.ReadOnly = true;
			this.txtPreIcon.Size = new System.Drawing.Size(80, 20);
			this.txtPreIcon.TabIndex = 76;
			this.txtPreIcon.Text = "";
			// 
			// btnPreSnd
			// 
			this.btnPreSnd.BackColor = System.Drawing.Color.Transparent;
			this.btnPreSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPreSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPreSnd.ForeColor = System.Drawing.Color.Black;
			this.btnPreSnd.Location = new System.Drawing.Point(200, 112);
			this.btnPreSnd.Name = "btnPreSnd";
			this.btnPreSnd.Size = new System.Drawing.Size(64, 24);
			this.btnPreSnd.TabIndex = 32;
			this.btnPreSnd.Text = "Browse...";
			this.btnPreSnd.Click += new System.EventHandler(this.btnPreSnd_Click);
			// 
			// txtPreSnd
			// 
			this.txtPreSnd.AutoSize = false;
			this.txtPreSnd.BackColor = System.Drawing.Color.LightYellow;
			this.txtPreSnd.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPreSnd.Enabled = false;
			this.txtPreSnd.ForeColor = System.Drawing.Color.Black;
			this.txtPreSnd.Location = new System.Drawing.Point(96, 112);
			this.txtPreSnd.Name = "txtPreSnd";
			this.txtPreSnd.ReadOnly = true;
			this.txtPreSnd.Size = new System.Drawing.Size(96, 20);
			this.txtPreSnd.TabIndex = 74;
			this.txtPreSnd.Text = "";
			// 
			// pbxPreIcon
			// 
			this.pbxPreIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxPreIcon.Location = new System.Drawing.Point(256, 152);
			this.pbxPreIcon.Name = "pbxPreIcon";
			this.pbxPreIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxPreIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbxPreIcon.TabIndex = 73;
			this.pbxPreIcon.TabStop = false;
			// 
			// btnPrePlaySnd
			// 
			this.btnPrePlaySnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPrePlaySnd.ForeColor = System.Drawing.Color.DarkRed;
			this.btnPrePlaySnd.Image = ((System.Drawing.Image)(resources.GetObject("btnPrePlaySnd.Image")));
			this.btnPrePlaySnd.Location = new System.Drawing.Point(272, 112);
			this.btnPrePlaySnd.Name = "btnPrePlaySnd";
			this.btnPrePlaySnd.Size = new System.Drawing.Size(24, 23);
			this.btnPrePlaySnd.TabIndex = 33;
			this.btnPrePlaySnd.Click += new System.EventHandler(this.btnPrePlaySnd_Click);
			// 
			// lblPreIcon
			// 
			this.lblPreIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPreIcon.ForeColor = System.Drawing.Color.Black;
			this.lblPreIcon.Location = new System.Drawing.Point(8, 144);
			this.lblPreIcon.Name = "lblPreIcon";
			this.lblPreIcon.Size = new System.Drawing.Size(40, 16);
			this.lblPreIcon.TabIndex = 72;
			this.lblPreIcon.Text = "Icon";
			// 
			// cmbPreIcon
			// 
			this.cmbPreIcon.BackColor = System.Drawing.Color.LightYellow;
			this.cmbPreIcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPreIcon.ForeColor = System.Drawing.Color.Black;
			this.cmbPreIcon.Location = new System.Drawing.Point(8, 160);
			this.cmbPreIcon.Name = "cmbPreIcon";
			this.cmbPreIcon.Size = new System.Drawing.Size(80, 21);
			this.cmbPreIcon.TabIndex = 40;
			this.cmbPreIcon.SelectedIndexChanged += new System.EventHandler(this.cmbPreIcon_SelectedIndexChanged);
			// 
			// lblPreSnd
			// 
			this.lblPreSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPreSnd.ForeColor = System.Drawing.Color.Black;
			this.lblPreSnd.Location = new System.Drawing.Point(8, 96);
			this.lblPreSnd.Name = "lblPreSnd";
			this.lblPreSnd.Size = new System.Drawing.Size(40, 16);
			this.lblPreSnd.TabIndex = 71;
			this.lblPreSnd.Text = "Sound";
			// 
			// cmbPreSnd
			// 
			this.cmbPreSnd.BackColor = System.Drawing.Color.LightYellow;
			this.cmbPreSnd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbPreSnd.ForeColor = System.Drawing.Color.Black;
			this.cmbPreSnd.Location = new System.Drawing.Point(8, 112);
			this.cmbPreSnd.Name = "cmbPreSnd";
			this.cmbPreSnd.Size = new System.Drawing.Size(80, 21);
			this.cmbPreSnd.TabIndex = 30;
			this.cmbPreSnd.SelectedIndexChanged += new System.EventHandler(this.cmbPreSnd_SelectedIndexChanged);
			// 
			// radPre6
			// 
			this.radPre6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre6.ForeColor = System.Drawing.Color.Black;
			this.radPre6.Location = new System.Drawing.Point(256, 24);
			this.radPre6.Name = "radPre6";
			this.radPre6.Size = new System.Drawing.Size(40, 16);
			this.radPre6.TabIndex = 15;
			this.radPre6.Tag = "6";
			this.radPre6.Text = "#6";
			this.radPre6.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// radPre5
			// 
			this.radPre5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre5.ForeColor = System.Drawing.Color.Black;
			this.radPre5.Location = new System.Drawing.Point(208, 24);
			this.radPre5.Name = "radPre5";
			this.radPre5.Size = new System.Drawing.Size(40, 16);
			this.radPre5.TabIndex = 14;
			this.radPre5.Tag = "5";
			this.radPre5.Text = "#5";
			this.radPre5.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// radPre4
			// 
			this.radPre4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre4.ForeColor = System.Drawing.Color.Black;
			this.radPre4.Location = new System.Drawing.Point(160, 24);
			this.radPre4.Name = "radPre4";
			this.radPre4.Size = new System.Drawing.Size(40, 16);
			this.radPre4.TabIndex = 13;
			this.radPre4.Tag = "4";
			this.radPre4.Text = "#4";
			this.radPre4.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// radPre3
			// 
			this.radPre3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre3.ForeColor = System.Drawing.Color.Black;
			this.radPre3.Location = new System.Drawing.Point(112, 24);
			this.radPre3.Name = "radPre3";
			this.radPre3.Size = new System.Drawing.Size(40, 16);
			this.radPre3.TabIndex = 12;
			this.radPre3.Tag = "3";
			this.radPre3.Text = "#3";
			this.radPre3.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// radPre2
			// 
			this.radPre2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre2.ForeColor = System.Drawing.Color.Black;
			this.radPre2.Location = new System.Drawing.Point(64, 24);
			this.radPre2.Name = "radPre2";
			this.radPre2.Size = new System.Drawing.Size(40, 16);
			this.radPre2.TabIndex = 11;
			this.radPre2.Tag = "2";
			this.radPre2.Text = "#2";
			this.radPre2.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// radPre1
			// 
			this.radPre1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radPre1.ForeColor = System.Drawing.Color.Black;
			this.radPre1.Location = new System.Drawing.Point(16, 24);
			this.radPre1.Name = "radPre1";
			this.radPre1.Size = new System.Drawing.Size(40, 16);
			this.radPre1.TabIndex = 10;
			this.radPre1.Tag = "1";
			this.radPre1.Text = "#1";
			this.radPre1.CheckedChanged += new System.EventHandler(this.radPre_CheckedChanged);
			// 
			// label14
			// 
			this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label14.ForeColor = System.Drawing.Color.DarkGreen;
			this.label14.Location = new System.Drawing.Point(8, 8);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(288, 16);
			this.label14.TabIndex = 61;
			this.label14.Text = "Use this screen to customize the six preset buttons.";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label13
			// 
			this.label13.BackColor = System.Drawing.Color.Transparent;
			this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label13.ForeColor = System.Drawing.Color.Black;
			this.label13.Location = new System.Drawing.Point(256, 48);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(40, 16);
			this.label13.TabIndex = 35;
			this.label13.Text = "Secs";
			// 
			// label12
			// 
			this.label12.BackColor = System.Drawing.Color.Transparent;
			this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label12.ForeColor = System.Drawing.Color.Black;
			this.label12.Location = new System.Drawing.Point(208, 48);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(40, 16);
			this.label12.TabIndex = 34;
			this.label12.Text = "Mins";
			// 
			// label11
			// 
			this.label11.BackColor = System.Drawing.Color.Transparent;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label11.ForeColor = System.Drawing.Color.Black;
			this.label11.Location = new System.Drawing.Point(160, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(40, 16);
			this.label11.TabIndex = 33;
			this.label11.Text = "Hours";
			// 
			// label10
			// 
			this.label10.BackColor = System.Drawing.Color.Transparent;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label10.ForeColor = System.Drawing.Color.Black;
			this.label10.Location = new System.Drawing.Point(112, 48);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(40, 16);
			this.label10.TabIndex = 32;
			this.label10.Text = "Days";
			// 
			// label9
			// 
			this.label9.BackColor = System.Drawing.Color.Transparent;
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label9.ForeColor = System.Drawing.Color.Black;
			this.label9.Location = new System.Drawing.Point(8, 48);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(40, 16);
			this.label9.TabIndex = 31;
			this.label9.Text = "Name";
			// 
			// numSecPre
			// 
			this.numSecPre.BackColor = System.Drawing.Color.LightYellow;
			this.numSecPre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numSecPre.ForeColor = System.Drawing.Color.Black;
			this.numSecPre.Location = new System.Drawing.Point(256, 64);
			this.numSecPre.Maximum = new System.Decimal(new int[] {
																	  999,
																	  0,
																	  0,
																	  0});
			this.numSecPre.Name = "numSecPre";
			this.numSecPre.Size = new System.Drawing.Size(40, 20);
			this.numSecPre.TabIndex = 24;
			this.numSecPre.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numSecPre.ValueChanged += new System.EventHandler(this.numSecPre_ValueChanged);
			// 
			// numMinPre
			// 
			this.numMinPre.BackColor = System.Drawing.Color.LightYellow;
			this.numMinPre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numMinPre.ForeColor = System.Drawing.Color.Black;
			this.numMinPre.Location = new System.Drawing.Point(208, 64);
			this.numMinPre.Maximum = new System.Decimal(new int[] {
																	  999,
																	  0,
																	  0,
																	  0});
			this.numMinPre.Name = "numMinPre";
			this.numMinPre.Size = new System.Drawing.Size(40, 20);
			this.numMinPre.TabIndex = 23;
			this.numMinPre.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numMinPre.ValueChanged += new System.EventHandler(this.numMinPre_ValueChanged);
			// 
			// numHourPre
			// 
			this.numHourPre.BackColor = System.Drawing.Color.LightYellow;
			this.numHourPre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numHourPre.ForeColor = System.Drawing.Color.Black;
			this.numHourPre.Location = new System.Drawing.Point(160, 64);
			this.numHourPre.Maximum = new System.Decimal(new int[] {
																	   999,
																	   0,
																	   0,
																	   0});
			this.numHourPre.Name = "numHourPre";
			this.numHourPre.Size = new System.Drawing.Size(40, 20);
			this.numHourPre.TabIndex = 22;
			this.numHourPre.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numHourPre.ValueChanged += new System.EventHandler(this.numHourPre_ValueChanged);
			// 
			// numDayPre
			// 
			this.numDayPre.BackColor = System.Drawing.Color.LightYellow;
			this.numDayPre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numDayPre.ForeColor = System.Drawing.Color.Black;
			this.numDayPre.Location = new System.Drawing.Point(112, 64);
			this.numDayPre.Maximum = new System.Decimal(new int[] {
																	  999,
																	  0,
																	  0,
																	  0});
			this.numDayPre.Name = "numDayPre";
			this.numDayPre.Size = new System.Drawing.Size(40, 20);
			this.numDayPre.TabIndex = 21;
			this.numDayPre.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numDayPre.ValueChanged += new System.EventHandler(this.numDayPre_ValueChanged);
			// 
			// txtPreName
			// 
			this.txtPreName.AutoSize = false;
			this.txtPreName.BackColor = System.Drawing.Color.LightYellow;
			this.txtPreName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtPreName.ForeColor = System.Drawing.Color.Black;
			this.txtPreName.Location = new System.Drawing.Point(8, 64);
			this.txtPreName.MaxLength = 16;
			this.txtPreName.Name = "txtPreName";
			this.txtPreName.Size = new System.Drawing.Size(96, 20);
			this.txtPreName.TabIndex = 20;
			this.txtPreName.Text = "";
			this.txtPreName.TextChanged += new System.EventHandler(this.txtPreName_TextChanged);
			// 
			// tabConfig
			// 
			this.tabConfig.BackColor = System.Drawing.Color.Transparent;
			this.tabConfig.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabConfig.BackgroundImage")));
			this.tabConfig.Controls.Add(this.tabCtrlConfigure);
			this.tabConfig.Location = new System.Drawing.Point(4, 25);
			this.tabConfig.Name = "tabConfig";
			this.tabConfig.Size = new System.Drawing.Size(304, 307);
			this.tabConfig.TabIndex = 2;
			this.tabConfig.Tag = "config";
			this.tabConfig.Text = "Configure";
			// 
			// tabCtrlConfigure
			// 
			this.tabCtrlConfigure.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabCtrlConfigure.Controls.Add(this.tabAudibleAlert);
			this.tabCtrlConfigure.Controls.Add(this.tabVisualAlert);
			this.tabCtrlConfigure.Controls.Add(this.tabProcessAlert);
			this.tabCtrlConfigure.Controls.Add(this.tabGeneral);
			this.tabCtrlConfigure.Location = new System.Drawing.Point(0, 0);
			this.tabCtrlConfigure.Multiline = true;
			this.tabCtrlConfigure.Name = "tabCtrlConfigure";
			this.tabCtrlConfigure.SelectedIndex = 0;
			this.tabCtrlConfigure.Size = new System.Drawing.Size(304, 312);
			this.tabCtrlConfigure.TabIndex = 23;
			// 
			// tabAudibleAlert
			// 
			this.tabAudibleAlert.BackColor = System.Drawing.Color.Transparent;
			this.tabAudibleAlert.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabAudibleAlert.BackgroundImage")));
			this.tabAudibleAlert.Controls.Add(this.label37);
			this.tabAudibleAlert.Controls.Add(this.btnPlaySnd);
			this.tabAudibleAlert.Controls.Add(this.cbxDefaultSnd);
			this.tabAudibleAlert.Controls.Add(this.lstSounds);
			this.tabAudibleAlert.Controls.Add(this.cbxPlaySnd);
			this.tabAudibleAlert.Controls.Add(this.btnRemoveSnd);
			this.tabAudibleAlert.Controls.Add(this.groupBox3);
			this.tabAudibleAlert.Location = new System.Drawing.Point(4, 25);
			this.tabAudibleAlert.Name = "tabAudibleAlert";
			this.tabAudibleAlert.Size = new System.Drawing.Size(296, 283);
			this.tabAudibleAlert.TabIndex = 0;
			this.tabAudibleAlert.Text = "Audible Alerts";
			// 
			// label37
			// 
			this.label37.BackColor = System.Drawing.Color.Transparent;
			this.label37.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label37.ForeColor = System.Drawing.Color.DarkGreen;
			this.label37.Location = new System.Drawing.Point(8, 8);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(280, 32);
			this.label37.TabIndex = 71;
			this.label37.Text = "On this screen you may customize sounds to be played when timers pop.";
			this.label37.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnPlaySnd
			// 
			this.btnPlaySnd.BackColor = System.Drawing.Color.Transparent;
			this.btnPlaySnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnPlaySnd.ForeColor = System.Drawing.Color.DarkRed;
			this.btnPlaySnd.Image = ((System.Drawing.Image)(resources.GetObject("btnPlaySnd.Image")));
			this.btnPlaySnd.Location = new System.Drawing.Point(264, 224);
			this.btnPlaySnd.Name = "btnPlaySnd";
			this.btnPlaySnd.Size = new System.Drawing.Size(24, 23);
			this.btnPlaySnd.TabIndex = 32;
			this.btnPlaySnd.Click += new System.EventHandler(this.btnPlaySnd_Click);
			// 
			// cbxDefaultSnd
			// 
			this.cbxDefaultSnd.BackColor = System.Drawing.Color.Transparent;
			this.cbxDefaultSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxDefaultSnd.ForeColor = System.Drawing.Color.Black;
			this.cbxDefaultSnd.Location = new System.Drawing.Point(120, 200);
			this.cbxDefaultSnd.Name = "cbxDefaultSnd";
			this.cbxDefaultSnd.Size = new System.Drawing.Size(120, 16);
			this.cbxDefaultSnd.TabIndex = 30;
			this.cbxDefaultSnd.Text = "Default";
			this.cbxDefaultSnd.CheckedChanged += new System.EventHandler(this.cbxDefaultSnd_CheckedChanged);
			// 
			// lstSounds
			// 
			this.lstSounds.BackColor = System.Drawing.Color.LightYellow;
			this.lstSounds.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lstSounds.ForeColor = System.Drawing.Color.Black;
			this.lstSounds.Location = new System.Drawing.Point(8, 72);
			this.lstSounds.Name = "lstSounds";
			this.lstSounds.Size = new System.Drawing.Size(104, 184);
			this.lstSounds.TabIndex = 15;
			this.lstSounds.SelectedIndexChanged += new System.EventHandler(this.lstSounds_SelectedIndexChanged);
			// 
			// cbxPlaySnd
			// 
			this.cbxPlaySnd.BackColor = System.Drawing.Color.Transparent;
			this.cbxPlaySnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxPlaySnd.ForeColor = System.Drawing.Color.Black;
			this.cbxPlaySnd.Location = new System.Drawing.Point(8, 48);
			this.cbxPlaySnd.Name = "cbxPlaySnd";
			this.cbxPlaySnd.Size = new System.Drawing.Size(168, 16);
			this.cbxPlaySnd.TabIndex = 10;
			this.cbxPlaySnd.Text = "Play audible alerts by default.";
			this.cbxPlaySnd.CheckedChanged += new System.EventHandler(this.cbxPlaySnd_CheckedChanged);
			// 
			// btnRemoveSnd
			// 
			this.btnRemoveSnd.BackColor = System.Drawing.Color.Transparent;
			this.btnRemoveSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRemoveSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnRemoveSnd.ForeColor = System.Drawing.Color.Black;
			this.btnRemoveSnd.Location = new System.Drawing.Point(120, 224);
			this.btnRemoveSnd.Name = "btnRemoveSnd";
			this.btnRemoveSnd.Size = new System.Drawing.Size(136, 24);
			this.btnRemoveSnd.TabIndex = 31;
			this.btnRemoveSnd.Text = "Remove";
			this.btnRemoveSnd.Click += new System.EventHandler(this.btnRemoveSnd_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.btnAddSnd);
			this.groupBox3.Controls.Add(this.label24);
			this.groupBox3.Controls.Add(this.label25);
			this.groupBox3.Controls.Add(this.txtSndName);
			this.groupBox3.Controls.Add(this.btnBrowseSnd);
			this.groupBox3.Controls.Add(this.txtSndFile);
			this.groupBox3.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox3.Location = new System.Drawing.Point(120, 72);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(168, 128);
			this.groupBox3.TabIndex = 16;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Add sound";
			// 
			// btnAddSnd
			// 
			this.btnAddSnd.BackColor = System.Drawing.Color.Transparent;
			this.btnAddSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnAddSnd.ForeColor = System.Drawing.Color.Black;
			this.btnAddSnd.Location = new System.Drawing.Point(112, 96);
			this.btnAddSnd.Name = "btnAddSnd";
			this.btnAddSnd.Size = new System.Drawing.Size(48, 24);
			this.btnAddSnd.TabIndex = 23;
			this.btnAddSnd.Text = "Add";
			this.btnAddSnd.Click += new System.EventHandler(this.btnAddSnd_Click);
			// 
			// label24
			// 
			this.label24.ForeColor = System.Drawing.Color.Black;
			this.label24.Location = new System.Drawing.Point(8, 56);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(96, 12);
			this.label24.TabIndex = 23;
			this.label24.Text = "File:";
			// 
			// label25
			// 
			this.label25.ForeColor = System.Drawing.Color.Black;
			this.label25.Location = new System.Drawing.Point(8, 16);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(96, 12);
			this.label25.TabIndex = 22;
			this.label25.Text = "Name:";
			// 
			// txtSndName
			// 
			this.txtSndName.AutoSize = false;
			this.txtSndName.BackColor = System.Drawing.Color.LightYellow;
			this.txtSndName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSndName.ForeColor = System.Drawing.Color.Black;
			this.txtSndName.Location = new System.Drawing.Point(8, 32);
			this.txtSndName.Name = "txtSndName";
			this.txtSndName.Size = new System.Drawing.Size(152, 20);
			this.txtSndName.TabIndex = 20;
			this.txtSndName.Text = "";
			// 
			// btnBrowseSnd
			// 
			this.btnBrowseSnd.BackColor = System.Drawing.Color.Transparent;
			this.btnBrowseSnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseSnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnBrowseSnd.ForeColor = System.Drawing.Color.Black;
			this.btnBrowseSnd.Location = new System.Drawing.Point(8, 96);
			this.btnBrowseSnd.Name = "btnBrowseSnd";
			this.btnBrowseSnd.Size = new System.Drawing.Size(64, 24);
			this.btnBrowseSnd.TabIndex = 22;
			this.btnBrowseSnd.Text = "Browse...";
			this.btnBrowseSnd.Click += new System.EventHandler(this.btnBrowseSnd_Click);
			// 
			// txtSndFile
			// 
			this.txtSndFile.AutoSize = false;
			this.txtSndFile.BackColor = System.Drawing.Color.LightYellow;
			this.txtSndFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSndFile.ForeColor = System.Drawing.Color.Black;
			this.txtSndFile.Location = new System.Drawing.Point(8, 72);
			this.txtSndFile.Name = "txtSndFile";
			this.txtSndFile.ReadOnly = true;
			this.txtSndFile.Size = new System.Drawing.Size(152, 20);
			this.txtSndFile.TabIndex = 21;
			this.txtSndFile.Text = "";
			this.txtSndFile.TextChanged += new System.EventHandler(this.snd_TextChanged);
			// 
			// tabVisualAlert
			// 
			this.tabVisualAlert.BackColor = System.Drawing.Color.Transparent;
			this.tabVisualAlert.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabVisualAlert.BackgroundImage")));
			this.tabVisualAlert.Controls.Add(this.label36);
			this.tabVisualAlert.Controls.Add(this.pbxIcon);
			this.tabVisualAlert.Controls.Add(this.cbxDefaultIcon);
			this.tabVisualAlert.Controls.Add(this.groupBox5);
			this.tabVisualAlert.Controls.Add(this.btnRemoveIcon);
			this.tabVisualAlert.Controls.Add(this.btnAlertPosition);
			this.tabVisualAlert.Controls.Add(this.cbxVisualAlert);
			this.tabVisualAlert.Controls.Add(this.lblAlertCoords);
			this.tabVisualAlert.Controls.Add(this.lstIcons);
			this.tabVisualAlert.Location = new System.Drawing.Point(4, 25);
			this.tabVisualAlert.Name = "tabVisualAlert";
			this.tabVisualAlert.Size = new System.Drawing.Size(296, 283);
			this.tabVisualAlert.TabIndex = 2;
			this.tabVisualAlert.Text = "Visual Alerts";
			// 
			// label36
			// 
			this.label36.BackColor = System.Drawing.Color.Transparent;
			this.label36.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label36.ForeColor = System.Drawing.Color.DarkGreen;
			this.label36.Location = new System.Drawing.Point(8, 8);
			this.label36.Name = "label36";
			this.label36.Size = new System.Drawing.Size(280, 32);
			this.label36.TabIndex = 70;
			this.label36.Text = "You may customize icons to be associated with timers.  You may also customize how" +
				" the alert box appears.";
			// 
			// pbxIcon
			// 
			this.pbxIcon.BackColor = System.Drawing.Color.Transparent;
			this.pbxIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pbxIcon.Location = new System.Drawing.Point(248, 216);
			this.pbxIcon.Name = "pbxIcon";
			this.pbxIcon.Size = new System.Drawing.Size(40, 40);
			this.pbxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbxIcon.TabIndex = 69;
			this.pbxIcon.TabStop = false;
			// 
			// cbxDefaultIcon
			// 
			this.cbxDefaultIcon.BackColor = System.Drawing.Color.Transparent;
			this.cbxDefaultIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxDefaultIcon.ForeColor = System.Drawing.Color.Black;
			this.cbxDefaultIcon.Location = new System.Drawing.Point(120, 208);
			this.cbxDefaultIcon.Name = "cbxDefaultIcon";
			this.cbxDefaultIcon.Size = new System.Drawing.Size(120, 16);
			this.cbxDefaultIcon.TabIndex = 30;
			this.cbxDefaultIcon.Text = "Default";
			this.cbxDefaultIcon.CheckedChanged += new System.EventHandler(this.cbxDefaultIcon_CheckedChanged);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.label40);
			this.groupBox5.Controls.Add(this.label22);
			this.groupBox5.Controls.Add(this.label23);
			this.groupBox5.Controls.Add(this.txtIconName);
			this.groupBox5.Controls.Add(this.btnBrowseIcon);
			this.groupBox5.Controls.Add(this.txtIconFile);
			this.groupBox5.Controls.Add(this.btnAddIcon);
			this.groupBox5.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox5.Location = new System.Drawing.Point(120, 80);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(168, 128);
			this.groupBox5.TabIndex = 28;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Add icon";
			// 
			// label40
			// 
			this.label40.ForeColor = System.Drawing.Color.Crimson;
			this.label40.Location = new System.Drawing.Point(64, 56);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(104, 16);
			this.label40.TabIndex = 24;
			this.label40.Text = "Use 40x40 (pixels)";
			// 
			// label22
			// 
			this.label22.ForeColor = System.Drawing.Color.Black;
			this.label22.Location = new System.Drawing.Point(8, 56);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(32, 16);
			this.label22.TabIndex = 17;
			this.label22.Text = "File:";
			// 
			// label23
			// 
			this.label23.ForeColor = System.Drawing.Color.Black;
			this.label23.Location = new System.Drawing.Point(8, 16);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(96, 16);
			this.label23.TabIndex = 16;
			this.label23.Text = "Name:";
			// 
			// txtIconName
			// 
			this.txtIconName.AutoSize = false;
			this.txtIconName.BackColor = System.Drawing.Color.LightYellow;
			this.txtIconName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtIconName.ForeColor = System.Drawing.Color.Black;
			this.txtIconName.Location = new System.Drawing.Point(8, 32);
			this.txtIconName.Name = "txtIconName";
			this.txtIconName.Size = new System.Drawing.Size(152, 20);
			this.txtIconName.TabIndex = 20;
			this.txtIconName.Text = "";
			// 
			// btnBrowseIcon
			// 
			this.btnBrowseIcon.BackColor = System.Drawing.Color.Transparent;
			this.btnBrowseIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnBrowseIcon.ForeColor = System.Drawing.Color.Black;
			this.btnBrowseIcon.Location = new System.Drawing.Point(8, 96);
			this.btnBrowseIcon.Name = "btnBrowseIcon";
			this.btnBrowseIcon.Size = new System.Drawing.Size(64, 24);
			this.btnBrowseIcon.TabIndex = 22;
			this.btnBrowseIcon.Text = "Browse...";
			this.btnBrowseIcon.Click += new System.EventHandler(this.btnBrowseIcon_Click);
			// 
			// txtIconFile
			// 
			this.txtIconFile.AutoSize = false;
			this.txtIconFile.BackColor = System.Drawing.Color.LightYellow;
			this.txtIconFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtIconFile.ForeColor = System.Drawing.Color.Black;
			this.txtIconFile.Location = new System.Drawing.Point(8, 72);
			this.txtIconFile.Name = "txtIconFile";
			this.txtIconFile.ReadOnly = true;
			this.txtIconFile.Size = new System.Drawing.Size(152, 20);
			this.txtIconFile.TabIndex = 21;
			this.txtIconFile.Text = "";
			this.txtIconFile.TextChanged += new System.EventHandler(this.icon_TextChanged);
			// 
			// btnAddIcon
			// 
			this.btnAddIcon.BackColor = System.Drawing.Color.Transparent;
			this.btnAddIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnAddIcon.ForeColor = System.Drawing.Color.Black;
			this.btnAddIcon.Location = new System.Drawing.Point(112, 96);
			this.btnAddIcon.Name = "btnAddIcon";
			this.btnAddIcon.Size = new System.Drawing.Size(48, 24);
			this.btnAddIcon.TabIndex = 23;
			this.btnAddIcon.Text = "Add";
			this.btnAddIcon.Click += new System.EventHandler(this.btnAddIcon_Click);
			// 
			// btnRemoveIcon
			// 
			this.btnRemoveIcon.BackColor = System.Drawing.Color.Transparent;
			this.btnRemoveIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRemoveIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnRemoveIcon.ForeColor = System.Drawing.Color.Black;
			this.btnRemoveIcon.Location = new System.Drawing.Point(120, 232);
			this.btnRemoveIcon.Name = "btnRemoveIcon";
			this.btnRemoveIcon.Size = new System.Drawing.Size(120, 24);
			this.btnRemoveIcon.TabIndex = 31;
			this.btnRemoveIcon.Text = "Remove";
			this.btnRemoveIcon.Click += new System.EventHandler(this.btnRemoveIcon_Click);
			// 
			// btnAlertPosition
			// 
			this.btnAlertPosition.BackColor = System.Drawing.Color.Transparent;
			this.btnAlertPosition.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAlertPosition.ForeColor = System.Drawing.Color.Black;
			this.btnAlertPosition.Location = new System.Drawing.Point(136, 48);
			this.btnAlertPosition.Name = "btnAlertPosition";
			this.btnAlertPosition.Size = new System.Drawing.Size(56, 24);
			this.btnAlertPosition.TabIndex = 11;
			this.btnAlertPosition.Text = "Position";
			this.btnAlertPosition.Click += new System.EventHandler(this.btnAlertPosition_Click);
			// 
			// cbxVisualAlert
			// 
			this.cbxVisualAlert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxVisualAlert.ForeColor = System.Drawing.Color.Black;
			this.cbxVisualAlert.Location = new System.Drawing.Point(8, 48);
			this.cbxVisualAlert.Name = "cbxVisualAlert";
			this.cbxVisualAlert.Size = new System.Drawing.Size(120, 32);
			this.cbxVisualAlert.TabIndex = 10;
			this.cbxVisualAlert.Text = "Display visual alerts by default.";
			// 
			// lblAlertCoords
			// 
			this.lblAlertCoords.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblAlertCoords.ForeColor = System.Drawing.Color.DarkGreen;
			this.lblAlertCoords.Location = new System.Drawing.Point(200, 56);
			this.lblAlertCoords.Name = "lblAlertCoords";
			this.lblAlertCoords.Size = new System.Drawing.Size(64, 16);
			this.lblAlertCoords.TabIndex = 26;
			this.lblAlertCoords.Text = "0,0";
			this.lblAlertCoords.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lstIcons
			// 
			this.lstIcons.BackColor = System.Drawing.Color.LightYellow;
			this.lstIcons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lstIcons.ForeColor = System.Drawing.Color.Black;
			this.lstIcons.Location = new System.Drawing.Point(8, 88);
			this.lstIcons.Name = "lstIcons";
			this.lstIcons.Size = new System.Drawing.Size(104, 171);
			this.lstIcons.TabIndex = 15;
			this.lstIcons.SelectedIndexChanged += new System.EventHandler(this.lstIcons_SelectedIndexChanged);
			// 
			// tabProcessAlert
			// 
			this.tabProcessAlert.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabProcessAlert.BackgroundImage")));
			this.tabProcessAlert.Controls.Add(this.label20);
			this.tabProcessAlert.Controls.Add(this.cbxAudibleProcessAlert);
			this.tabProcessAlert.Controls.Add(this.cbxVisualProcessAlert);
			this.tabProcessAlert.Controls.Add(this.btnRunProcess);
			this.tabProcessAlert.Controls.Add(this.label29);
			this.tabProcessAlert.Controls.Add(this.lstProcesses);
			this.tabProcessAlert.Controls.Add(this.btnRemoveProcess);
			this.tabProcessAlert.Controls.Add(this.groupBox6);
			this.tabProcessAlert.Controls.Add(this.label39);
			this.tabProcessAlert.ForeColor = System.Drawing.Color.Black;
			this.tabProcessAlert.Location = new System.Drawing.Point(4, 25);
			this.tabProcessAlert.Name = "tabProcessAlert";
			this.tabProcessAlert.Size = new System.Drawing.Size(296, 283);
			this.tabProcessAlert.TabIndex = 3;
			this.tabProcessAlert.Text = "Programs";
			// 
			// label20
			// 
			this.label20.BackColor = System.Drawing.Color.Transparent;
			this.label20.ForeColor = System.Drawing.Color.Black;
			this.label20.Location = new System.Drawing.Point(211, 62);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(64, 16);
			this.label20.TabIndex = 64;
			this.label20.Text = "if available.";
			// 
			// cbxAudibleProcessAlert
			// 
			this.cbxAudibleProcessAlert.BackColor = System.Drawing.Color.Transparent;
			this.cbxAudibleProcessAlert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxAudibleProcessAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cbxAudibleProcessAlert.ForeColor = System.Drawing.Color.Black;
			this.cbxAudibleProcessAlert.Location = new System.Drawing.Point(123, 61);
			this.cbxAudibleProcessAlert.Name = "cbxAudibleProcessAlert";
			this.cbxAudibleProcessAlert.Size = new System.Drawing.Size(88, 16);
			this.cbxAudibleProcessAlert.TabIndex = 32;
			this.cbxAudibleProcessAlert.Text = "audible alert";
			// 
			// cbxVisualProcessAlert
			// 
			this.cbxVisualProcessAlert.BackColor = System.Drawing.Color.Transparent;
			this.cbxVisualProcessAlert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxVisualProcessAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cbxVisualProcessAlert.ForeColor = System.Drawing.Color.Black;
			this.cbxVisualProcessAlert.Location = new System.Drawing.Point(40, 61);
			this.cbxVisualProcessAlert.Name = "cbxVisualProcessAlert";
			this.cbxVisualProcessAlert.Size = new System.Drawing.Size(80, 16);
			this.cbxVisualProcessAlert.TabIndex = 31;
			this.cbxVisualProcessAlert.Text = "visual alert";
			// 
			// btnRunProcess
			// 
			this.btnRunProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnRunProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRunProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnRunProcess.ForeColor = System.Drawing.Color.Black;
			this.btnRunProcess.Location = new System.Drawing.Point(224, 232);
			this.btnRunProcess.Name = "btnRunProcess";
			this.btnRunProcess.Size = new System.Drawing.Size(64, 24);
			this.btnRunProcess.TabIndex = 63;
			this.btnRunProcess.Text = "Run";
			this.btnRunProcess.Click += new System.EventHandler(this.btnRunProcess_Click);
			// 
			// label29
			// 
			this.label29.BackColor = System.Drawing.Color.Transparent;
			this.label29.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label29.ForeColor = System.Drawing.Color.DarkGreen;
			this.label29.Location = new System.Drawing.Point(8, 8);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(280, 32);
			this.label29.TabIndex = 62;
			this.label29.Text = "Here you may define programs you wish to run when timers pop.";
			this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lstProcesses
			// 
			this.lstProcesses.BackColor = System.Drawing.Color.LightYellow;
			this.lstProcesses.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lstProcesses.ForeColor = System.Drawing.Color.Black;
			this.lstProcesses.Location = new System.Drawing.Point(8, 88);
			this.lstProcesses.Name = "lstProcesses";
			this.lstProcesses.Size = new System.Drawing.Size(104, 171);
			this.lstProcesses.TabIndex = 34;
			this.lstProcesses.SelectedIndexChanged += new System.EventHandler(this.lstProcesses_SelectedIndexChanged);
			// 
			// btnRemoveProcess
			// 
			this.btnRemoveProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnRemoveProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRemoveProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnRemoveProcess.ForeColor = System.Drawing.Color.Black;
			this.btnRemoveProcess.Location = new System.Drawing.Point(120, 232);
			this.btnRemoveProcess.Name = "btnRemoveProcess";
			this.btnRemoveProcess.Size = new System.Drawing.Size(96, 24);
			this.btnRemoveProcess.TabIndex = 37;
			this.btnRemoveProcess.Text = "Remove";
			this.btnRemoveProcess.Click += new System.EventHandler(this.btnRemoveProcess_Click);
			// 
			// groupBox6
			// 
			this.groupBox6.BackColor = System.Drawing.Color.Transparent;
			this.groupBox6.Controls.Add(this.btnAddProcess);
			this.groupBox6.Controls.Add(this.label27);
			this.groupBox6.Controls.Add(this.label28);
			this.groupBox6.Controls.Add(this.txtProcessName);
			this.groupBox6.Controls.Add(this.btnBrowseProcess);
			this.groupBox6.Controls.Add(this.txtProcessFile);
			this.groupBox6.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox6.Location = new System.Drawing.Point(120, 88);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(168, 128);
			this.groupBox6.TabIndex = 35;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Add program";
			// 
			// btnAddProcess
			// 
			this.btnAddProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnAddProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnAddProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnAddProcess.ForeColor = System.Drawing.Color.Black;
			this.btnAddProcess.Location = new System.Drawing.Point(112, 96);
			this.btnAddProcess.Name = "btnAddProcess";
			this.btnAddProcess.Size = new System.Drawing.Size(48, 24);
			this.btnAddProcess.TabIndex = 23;
			this.btnAddProcess.Text = "Add";
			this.btnAddProcess.Click += new System.EventHandler(this.btnAddProcess_Click);
			// 
			// label27
			// 
			this.label27.ForeColor = System.Drawing.Color.Black;
			this.label27.Location = new System.Drawing.Point(8, 56);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(96, 12);
			this.label27.TabIndex = 23;
			this.label27.Text = "File:";
			// 
			// label28
			// 
			this.label28.ForeColor = System.Drawing.Color.Black;
			this.label28.Location = new System.Drawing.Point(8, 16);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(96, 12);
			this.label28.TabIndex = 22;
			this.label28.Text = "Name:";
			// 
			// txtProcessName
			// 
			this.txtProcessName.AutoSize = false;
			this.txtProcessName.BackColor = System.Drawing.Color.LightYellow;
			this.txtProcessName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtProcessName.ForeColor = System.Drawing.Color.Black;
			this.txtProcessName.Location = new System.Drawing.Point(8, 32);
			this.txtProcessName.Name = "txtProcessName";
			this.txtProcessName.Size = new System.Drawing.Size(152, 20);
			this.txtProcessName.TabIndex = 20;
			this.txtProcessName.Text = "";
			// 
			// btnBrowseProcess
			// 
			this.btnBrowseProcess.BackColor = System.Drawing.Color.Transparent;
			this.btnBrowseProcess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowseProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnBrowseProcess.ForeColor = System.Drawing.Color.Black;
			this.btnBrowseProcess.Location = new System.Drawing.Point(8, 96);
			this.btnBrowseProcess.Name = "btnBrowseProcess";
			this.btnBrowseProcess.Size = new System.Drawing.Size(64, 24);
			this.btnBrowseProcess.TabIndex = 22;
			this.btnBrowseProcess.Text = "Browse...";
			this.btnBrowseProcess.Click += new System.EventHandler(this.btnBrowseProcess_Click);
			// 
			// txtProcessFile
			// 
			this.txtProcessFile.AutoSize = false;
			this.txtProcessFile.BackColor = System.Drawing.Color.LightYellow;
			this.txtProcessFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtProcessFile.ForeColor = System.Drawing.Color.Black;
			this.txtProcessFile.Location = new System.Drawing.Point(8, 72);
			this.txtProcessFile.Name = "txtProcessFile";
			this.txtProcessFile.Size = new System.Drawing.Size(152, 20);
			this.txtProcessFile.TabIndex = 21;
			this.txtProcessFile.Text = "";
			this.txtProcessFile.TextChanged += new System.EventHandler(this.process_TextChanged);
			// 
			// label39
			// 
			this.label39.BackColor = System.Drawing.Color.Transparent;
			this.label39.ForeColor = System.Drawing.Color.Black;
			this.label39.Location = new System.Drawing.Point(8, 48);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(272, 32);
			this.label39.TabIndex = 30;
			this.label39.Text = "When a timer starts an external program, I also want a(n)";
			// 
			// tabGeneral
			// 
			this.tabGeneral.BackColor = System.Drawing.Color.Transparent;
			this.tabGeneral.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabGeneral.BackgroundImage")));
			this.tabGeneral.Controls.Add(this.label38);
			this.tabGeneral.Controls.Add(this.groupBox4);
			this.tabGeneral.Controls.Add(this.cbx24hour);
			this.tabGeneral.Location = new System.Drawing.Point(4, 25);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(296, 283);
			this.tabGeneral.TabIndex = 1;
			this.tabGeneral.Text = "General";
			// 
			// label38
			// 
			this.label38.BackColor = System.Drawing.Color.Transparent;
			this.label38.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label38.ForeColor = System.Drawing.Color.DarkGreen;
			this.label38.Location = new System.Drawing.Point(8, 8);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(280, 16);
			this.label38.TabIndex = 71;
			this.label38.Text = "These are general program options.";
			this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.lblOpacity);
			this.groupBox4.Controls.Add(this.cbxShowStopwatch);
			this.groupBox4.Controls.Add(this.btnOpacity);
			this.groupBox4.Controls.Add(this.label5);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.groupBox4.ForeColor = System.Drawing.Color.DarkGreen;
			this.groupBox4.Location = new System.Drawing.Point(8, 56);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(144, 112);
			this.groupBox4.TabIndex = 30;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Stopwatch options";
			// 
			// lblOpacity
			// 
			this.lblOpacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblOpacity.ForeColor = System.Drawing.Color.DarkGreen;
			this.lblOpacity.Location = new System.Drawing.Point(80, 40);
			this.lblOpacity.Name = "lblOpacity";
			this.lblOpacity.Size = new System.Drawing.Size(40, 16);
			this.lblOpacity.TabIndex = 28;
			this.lblOpacity.Text = "0%";
			this.lblOpacity.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// cbxShowStopwatch
			// 
			this.cbxShowStopwatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbxShowStopwatch.ForeColor = System.Drawing.Color.Black;
			this.cbxShowStopwatch.Location = new System.Drawing.Point(8, 64);
			this.cbxShowStopwatch.Name = "cbxShowStopwatch";
			this.cbxShowStopwatch.Size = new System.Drawing.Size(128, 40);
			this.cbxShowStopwatch.TabIndex = 29;
			this.cbxShowStopwatch.Text = "Automatically show stopwatch when a timer is set.";
			// 
			// btnOpacity
			// 
			this.btnOpacity.BackColor = System.Drawing.Color.Transparent;
			this.btnOpacity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOpacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnOpacity.ForeColor = System.Drawing.Color.Black;
			this.btnOpacity.Location = new System.Drawing.Point(24, 32);
			this.btnOpacity.Name = "btnOpacity";
			this.btnOpacity.Size = new System.Drawing.Size(48, 24);
			this.btnOpacity.TabIndex = 27;
			this.btnOpacity.Text = "Set";
			this.btnOpacity.Click += new System.EventHandler(this.btnOpacity_Click);
			// 
			// label5
			// 
			this.label5.ForeColor = System.Drawing.Color.Black;
			this.label5.Location = new System.Drawing.Point(8, 16);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(128, 16);
			this.label5.TabIndex = 26;
			this.label5.Text = "Initial stopwatch opacity:";
			// 
			// cbx24hour
			// 
			this.cbx24hour.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cbx24hour.ForeColor = System.Drawing.Color.Black;
			this.cbx24hour.Location = new System.Drawing.Point(8, 32);
			this.cbx24hour.Name = "cbx24hour";
			this.cbx24hour.Size = new System.Drawing.Size(112, 24);
			this.cbx24hour.TabIndex = 20;
			this.cbx24hour.Text = "Use 24 hour time.";
			this.cbx24hour.CheckedChanged += new System.EventHandler(this.cbx24hour_CheckedChanged);
			// 
			// btnClose
			// 
			this.btnClose.BackColor = System.Drawing.SystemColors.Control;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnClose.ForeColor = System.Drawing.Color.Black;
			this.btnClose.Location = new System.Drawing.Point(8, 320);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(88, 23);
			this.btnClose.TabIndex = 1000;
			this.btnClose.Text = "Close";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// timElapsedUpdate
			// 
			this.timElapsedUpdate.Enabled = true;
			this.timElapsedUpdate.Interval = 1000;
			this.timElapsedUpdate.Tick += new System.EventHandler(this.timElapsedUpdate_Tick);
			// 
			// diaPlaySnd
			// 
			this.diaPlaySnd.DefaultExt = "wav";
			this.diaPlaySnd.Filter = "Audio files (*.wav, *.mp3, *.wma, *.mid)|*.wav;*.wave;*.mp3;*.wma;*.mid;*.midi";
			this.diaPlaySnd.RestoreDirectory = true;
			this.diaPlaySnd.Title = "Tea Timer sound file";
			// 
			// btnApply
			// 
			this.btnApply.BackColor = System.Drawing.SystemColors.Control;
			this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnApply.ForeColor = System.Drawing.Color.Black;
			this.btnApply.Location = new System.Drawing.Point(112, 320);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(88, 23);
			this.btnApply.TabIndex = 1001;
			this.btnApply.Text = "Apply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnHelp
			// 
			this.btnHelp.BackColor = System.Drawing.SystemColors.Control;
			this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnHelp.ForeColor = System.Drawing.Color.Black;
			this.btnHelp.Location = new System.Drawing.Point(216, 320);
			this.btnHelp.Name = "btnHelp";
			this.btnHelp.Size = new System.Drawing.Size(88, 23);
			this.btnHelp.TabIndex = 1002;
			this.btnHelp.Text = "Help";
			this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
			// 
			// diaBrowseIcons
			// 
			this.diaBrowseIcons.DefaultExt = "ico";
			this.diaBrowseIcons.Filter = "Image files (*.ico, *.jpg, *.gif, *.png, *.bmp)|*.ico;*.gif;*.bmp;*.jpg;*.png";
			this.diaBrowseIcons.RestoreDirectory = true;
			this.diaBrowseIcons.Title = "Tea Timer icon";
			// 
			// lnkWebPage
			// 
			this.lnkWebPage.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkWebPage.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkWebPage.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkWebPage.Location = new System.Drawing.Point(80, 344);
			this.lnkWebPage.Name = "lnkWebPage";
			this.lnkWebPage.Size = new System.Drawing.Size(152, 16);
			this.lnkWebPage.TabIndex = 1003;
			this.lnkWebPage.TabStop = true;
			this.lnkWebPage.Text = "http://www.ericeubank.com";
			this.lnkWebPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkWebPage.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkWebPage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebPage_LinkClicked);
			// 
			// diaRunProcess
			// 
			this.diaRunProcess.DefaultExt = "ico";
			this.diaRunProcess.Filter = "All files (*.*)|*.*";
			this.diaRunProcess.RestoreDirectory = true;
			this.diaRunProcess.Title = "Tea Timer external program to run";
			// 
			// lblVersion
			// 
			this.lblVersion.ForeColor = System.Drawing.Color.Black;
			this.lblVersion.Location = new System.Drawing.Point(224, 344);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(88, 16);
			this.lblVersion.TabIndex = 1004;
			this.lblVersion.Text = "ver. ";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lnkBugs
			// 
			this.lnkBugs.ForeColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.lnkBugs.LinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.Location = new System.Drawing.Point(0, 344);
			this.lnkBugs.Name = "lnkBugs";
			this.lnkBugs.Size = new System.Drawing.Size(40, 16);
			this.lnkBugs.TabIndex = 1005;
			this.lnkBugs.TabStop = true;
			this.lnkBugs.Text = "Bugs?";
			this.lnkBugs.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lnkBugs.VisitedLinkColor = System.Drawing.Color.MediumBlue;
			this.lnkBugs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkBugs_LinkClicked);
			// 
			// TeaTimer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(314, 359);
			this.ControlBox = false;
			this.Controls.Add(this.lnkBugs);
			this.Controls.Add(this.lnkWebPage);
			this.Controls.Add(this.btnHelp);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.lblVersion);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TeaTimer";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TeaTimer";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.numHour)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numDay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numSec)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabCreate.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.tabAlarm.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numAlarmSec)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numAlarmMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numAlarmHour)).EndInit();
			this.tabView.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.tabPresets.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numSecPre)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMinPre)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numHourPre)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numDayPre)).EndInit();
			this.tabConfig.ResumeLayout(false);
			this.tabCtrlConfigure.ResumeLayout(false);
			this.tabAudibleAlert.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabVisualAlert.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.tabProcessAlert.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Timers
		private void timElapsedUpdate_Tick(object sender, System.EventArgs e)
		{
			//for View Timers screen
			_TeaTimer oTT;
			try 
			{
				oTT=(_TeaTimer)lstTimers.SelectedItem;
			}
			catch (Exception ex) 
			{
				//some very random exception was thrown, oh well, better luck next time;
				//HACK! :-)
				return;
			}

			if(oTT!=null && !oTT.Paused) 
			{
				lblElapsed.Text=getFormattedTime(oTT.getTimeElapsed());
				lblLeft.Text=getFormattedTime(oTT.getTimeLeft());
			}

			//for tray icon tooltip
			if(_oTeaTimers.Count>0) 
			{
				oTT=(_TeaTimer)_oTeaTimers[0];
				notifyIcon.Text="Current timer: "+oTT.Name+": "+getFormattedTime(oTT.getTimeElapsed())+"/"+getFormattedTime(oTT.Time);
			}
			else notifyIcon.Text="No timers set";
		}
		private void teaTimer_Tick(object sender, System.EventArgs e)
		{
			bool bProcessAlert=false, bVisualAlert=false;
			_TeaTimer oTT=(_TeaTimer)sender;
			oTT.Stop();
			removeTimer(oTT);

			//check for process alert
			bProcessAlert=(oTT.ProcessAlert.Length>0) ;

			//visual and audible alert
			if(oTT.VisualAlert.Length>0) 
			{
				if(bProcessAlert && !cbxVisualProcessAlert.Checked) goto EndVisualAlert;
				bVisualAlert=true;
				TimerPop oPop=new TimerPop(oTT.Name,oTT.VisualAlert,
					((bProcessAlert && !cbxAudibleProcessAlert.Checked)?null:oTT.AudibleAlert));
				if(_iAlertX!=-1 && _iAlertY!=-1) 
				{
					oPop.Left=_iAlertX;
					oPop.Top=_iAlertY;
				}
				oPop.Show();
			}
			EndVisualAlert:{}

			//audible alert only
			if(!bVisualAlert && oTT.AudibleAlert.Length>0) 
			{
				if(bProcessAlert && !cbxAudibleProcessAlert.Checked) goto EndAudibleAlert;
				playSound(oTT.AudibleAlert);
			}
			EndAudibleAlert:{}

			//execute process alert
			if(bProcessAlert) runProcess(oTT.ProcessAlert);

			//if this timer is recurring, add it again
			if(oTT.Recurring) addTimerNoAlert(oTT);
		}
		private void removeTimer(object oTeaTimer) 
		{
			timElapsedUpdate.Stop();
			
			lstTimers.ClearSelected();
			_oTeaTimers.Remove(oTeaTimer);
			rebuildTimersList();

			timElapsedUpdate.Start();
		}
		private void addTimer(_TeaTimer oTeaTimer) 
		{
			addTimerNoAlert(oTeaTimer);
			MessageBox.Show(oTeaTimer.Name+" started.\nDuration: "+getFormattedTime(oTeaTimer.Time)+"\nWill pop on: "+getTimeString(oTeaTimer.Finished),oTeaTimer.Name+" (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Information);
			if(cbxShowStopwatch.Checked) showStopwatch(oTeaTimer);
		}
		private void addTimerNoAlert(_TeaTimer oTeaTimer) 
		{
			timElapsedUpdate.Stop();

			_oTeaTimers.Add(oTeaTimer);
			oTeaTimer.Go();
			rebuildTimersList();
			timElapsedUpdate.Start();
		}
		#endregion

		#region Helpers
		private void rebuildTimersList() 
		{
			_oTeaTimers.Sort();
			lstTimers.Items.Clear();
			resetForm();
			menRunning.MenuItems.Clear();
			menRunning.Enabled=false;
			for(int i=0; i<_oTeaTimers.Count; i++)
			{
				//view timers
				_TeaTimer oTT=(_TeaTimer)_oTeaTimers[i];
				lstTimers.Items.Add(oTT);

				//running timers
				MenuItem oMI=new MenuItem(oTT.Name);
				//running timers submenu
				oMI.MenuItems.Add(0,new MenuItem("Show Stopwatch",new EventHandler(menRunningStopwatch_Click)));
				oMI.MenuItems.Add(1,new MenuItem("View",new EventHandler(menRunningView_Click)));
				oMI.MenuItems[1].DefaultItem=true;
				oMI.MenuItems.Add(2,new MenuItem("Stop and Remove",new EventHandler(menRunningStop_Click)));
				oMI.MenuItems.Add(3,new MenuItem((oTT.Paused?"Resume":"Pause"),new EventHandler(menRunningPause_Click)));
				//add menu
				menRunning.MenuItems.Add(i,oMI);
			}
			menRunning.Enabled=menRunning.MenuItems.Count>0;
		}
		private void resetForm() 
		{
			//Create timer
			pbxTimerIcon.Image=null;
			if(tabControl1.SelectedTab==tabCreate)
			{
				txtName.Text="";
				numDay.Value=numHour.Value=numMin.Value=numSec.Value=0;
				buildCombo(cmbTimerSnd,lstSounds);
				buildCombo(cmbTimerIcon,lstIcons);
				buildCombo(cmbTimerProcess,lstProcesses);
				if(cbxPlaySnd.Checked) cmbTimerSnd.SelectedIndex=1;
				if(cbxVisualAlert.Checked) cmbTimerIcon.SelectedIndex=1;
				cbxTimerRepeat.Checked=false;
			}

			//Alarm
			pbxAlarmIcon.Image=null;
			if(tabControl1.SelectedTab==tabAlarm)
			{
				txtAlarmName.Text="";
				numAlarmHour.Maximum=999;
				DateTime oNow=DateTime.Now;
				if(cbx24hour.Checked) 
				{
					numAlarmHour.Value=oNow.Hour;
				}
				else 
				{
					numAlarmHour.Value=oNow.Hour-((oNow.Hour>12)?12:0);
					radPM.Checked=(oNow.Hour>12)?true:false;
					radAM.Checked=!radPM.Checked;
				}
				numAlarmMin.Value=oNow.Minute;
				numAlarmSec.Value=oNow.Second;
				calAlarm.SetDate(DateTime.Now);
				buildCombo(cmbAlarmSnd,lstSounds);
				buildCombo(cmbAlarmIcon,lstIcons);
				buildCombo(cmbAlarmProcess,lstProcesses);
				if(cbxPlaySnd.Checked) cmbAlarmSnd.SelectedIndex=1;
				if(cbxVisualAlert.Checked) cmbAlarmIcon.SelectedIndex=1;
			}
			
			//View
			pbxViewIcon.Image=null;
			if(tabControl1.SelectedTab==tabView)
			{
				lstTimers.ClearSelected();
				lblName.Text=lblTime.Text=lblElapsed.Text=lblLeft.Text=lblFinish.Text=lblViewProcess.Text=
				lblViewAudible.Text="";
				cbxViewVisual.Checked=cbxViewAudible.Checked=cbxViewProcess.Checked=cbxViewRepeat.Checked=false;
			}

			//Config
			pbxIcon.Image=null;
			if(tabControl1.SelectedTab==tabConfig)
			{
				//visual
				if(_iAlertX!=-1 && _iAlertY!=-1) lblAlertCoords.Text=_iAlertX+","+_iAlertY;
				else lblAlertCoords.Text="centered";
				txtIconFile.Text=txtIconName.Text="";
				btnAddIcon.Enabled=false;
				btnRemoveIcon.Enabled=false;
				lstIcons.SelectedItem=null;
				cbxDefaultIcon.Enabled=false;
				pbxIcon.Image=null;

				//audible
				txtSndFile.Text=txtSndName.Text="";
				btnAddSnd.Enabled=false;
				btnRemoveSnd.Enabled=false;
				lstSounds.SelectedItem=null;
				cbxDefaultSnd.Enabled=false;
				btnPlaySnd.Enabled=false;

				//processes
				txtProcessFile.Text=txtProcessName.Text="";
				btnAddProcess.Enabled=false;
				btnRemoveProcess.Enabled=false;
				lstProcesses.SelectedItem=null;
				btnRunProcess.Enabled=false;
			}

			//Config general
			lblOpacity.Text=_iStopwatchOpacity+"%";

			//presets
			pbxPreIcon.Image=null;
			if(tabControl1.SelectedTab==tabPresets)
			{
				radPre1.Checked=false;
				radPre1.Checked=true;
			}

			enableBtns();
		}
		private void enableBtns() 
		{
			//create
			if(tabControl1.SelectedTab==tabCreate)
			{
				btnGo.Enabled=(numDay.Value + numHour.Value + numMin.Value + numSec.Value)==0?false:true;
			}
			
			//view
			if(tabControl1.SelectedTab==tabView)
			{
				_TeaTimer oTT=(_TeaTimer)lstTimers.SelectedItem;
				btnStopwatch.Enabled=btnStop.Enabled=btnPause.Enabled=(oTT!=null);
				btnPause.Text="Pause/Resume";
				if(oTT!=null) btnPause.Text=(oTT.Paused?"Resume":"Pause");
			}

			//alarm
			if(tabControl1.SelectedTab==tabAlarm)
			{
				radAM.Visible=radPM.Visible=!cbx24hour.Checked;
				numAlarmHour.Maximum=(cbx24hour.Checked?23:12);
			}

			//presets
			if(tabControl1.SelectedTab==tabPresets) 
			{
				cmbPreSnd.Enabled=cmbPreIcon.Enabled=btnPrePlaySnd.Enabled=btnPreSnd.Enabled=btnPreIcon.Enabled=
					btnPreSave.Enabled=btnPrePlaySnd.Enabled=cmbPreProcess.Enabled=txtPreProcess.Enabled=
					btnPreBrowseProcess.Enabled=btnPreRunProcess.Enabled=cbxPreRepeat.Enabled=
					lblPreSnd.Enabled=lblPreIcon.Enabled=lblPreProcess.Enabled=
					(txtPreName.Text!="" && (numDayPre.Value>0||numHourPre.Value>0||numMinPre.Value>0||numSecPre.Value>0));
				btnPreRunProcess.Enabled=(txtPreProcess.Text.Length>0);
				btnPrePlaySnd.Enabled=(txtPreSnd.Text.Length>0);
				dispIcon(pbxPreIcon,txtPreIcon.Text);
			}
		}
		public static string getFormattedTime(int[] iTime) 
		{
			StringBuilder s=new StringBuilder();
			s.Append(iTime[0]);
			s.Append(":");
			s.Append(iTime[1]);
			s.Append(":");
			s.Append(iTime[2].ToString().PadLeft(2,'0'));
			s.Append(":");
			s.Append(iTime[3].ToString().PadLeft(2,'0'));
			return s.ToString();
		}

		public static string getTimeString(DateTime oDT) 
		{
			return (_b24hour?
				oDT.Date.ToShortDateString()+" "+oDT.ToString("H:mm:ss"):
				oDT.ToString());
		}
		private void buildCombo(ComboBox cmb, ListBox lst)
		{
			cmb.Items.Clear();
			cmb.Items.Add(new _None());
			foreach(_SimpleDataStructure o in lst.Items)
				cmb.Items.Add(o);
			cmb.SelectedIndex=0;
		}
		public static void dispIcon(PictureBox pbx, string sFile) 
		{
			if(sFile==null || sFile.Length==0) 
			{
				pbx.Image=null;
				return;
			}
			try {pbx.Image=new Bitmap(sFile,true);}
			catch(Exception e){pbx.Image=imgError;}
		}
		internal void stopTeaTimer(_TeaTimer oTT) 
		{
			oTT.Expire();
			removeTimer(oTT);
		}
		internal void pauseTeaTimer(_TeaTimer oTT) 
		{
			if(oTT.Paused) oTT.Resume();
			else oTT.Pause();
			rebuildTimersList();
		}
		private void showStopwatch(_TeaTimer oTT) 
		{
			new Stopwatch(oTT,this,(_iStopwatchOpacity/100.0));
		}
		public static void playSound(string sFile) 
		{
			//WavPlay.WavPlayer.Play(sFile);
			MCIPlayer.Play(sFile);
		}
		public static void stopSound() 
		{
			//WavPlay.WavPlayer.StopPlay();
			MCIPlayer.Stop();
		}
		internal void showTimerScreen(_TeaTimer oTT) 
		{
			tabControl1.SelectedTab=tabView;
			resetForm();
			lstTimers.SelectedItem=oTT;
			FadeForm(Fade.Up);
		}
		private void runProcess(string sFile) 
		{
			if(sFile==null || sFile.Length==0) return;
			string[] s=sFile.Split(new char[] {' '});
			string t="";
			for(int i=1;i<s.Length;i++) t+=s[i]+" ";
			RetryRunProcess:
			try
			{
				System.Diagnostics.Process.Start(s[0],t);
			}
			catch(Exception e)
			{
				DialogResult oResult=MessageBox.Show("I was unable to launch\n\""+sFile+"\"","Error (Tea Timer)",MessageBoxButtons.RetryCancel,MessageBoxIcon.Error);
				if(oResult==DialogResult.Retry) goto RetryRunProcess;
			}
		}
		private bool hasDupName(string sName, ListBox lst) 
		{
			foreach(_SimpleDataStructure o in lst.Items)
				if(o.sName==sName) return true;
			return false;
		}
		#endregion

		#region Events
		#region Create Timer
		private void btnTimerPlay_Click(object sender, System.EventArgs e)
		{
			playSound(((_SimpleDataStructure)cmbTimerSnd.SelectedItem).sFile);
		}
		private void cmbTimerIcon_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			dispIcon(pbxTimerIcon,((_SimpleDataStructure)cmbTimerIcon.SelectedItem).sFile);
		}
		private void btnHourPlus_Click(object sender, System.EventArgs e)
		{
			numHour.Value+=Convert.ToDecimal(((Button)sender).Text);
			enableBtns();
		}
		private void btnMinPlus_Click(object sender, System.EventArgs e)
		{
			numMin.Value+=Convert.ToDecimal(((Button)sender).Text);
			enableBtns();
		}
		private void btnSecPlus_Click(object sender, System.EventArgs e)
		{
			numSec.Value+=Convert.ToDecimal(((Button)sender).Text);
			enableBtns();
		}
		private void btnGo_Click(object sender, System.EventArgs e)
		{
			if(txtName.Text=="") txtName.Text="Unnamed timer";
			_TeaTimer oTT=new _TeaTimer(txtName.Text,(int)numDay.Value,(int)numHour.Value,(int)numMin.Value,(int)numSec.Value);
			oTT.Tick += new EventHandler(teaTimer_Tick);
			oTT.AudibleAlert=((_SimpleDataStructure)cmbTimerSnd.SelectedItem).sFile;
			oTT.VisualAlert=((_SimpleDataStructure)cmbTimerIcon.SelectedItem).sFile;
			oTT.ProcessAlert=((_SimpleDataStructure)cmbTimerProcess.SelectedItem).sFile;
			oTT.Recurring=cbxTimerRepeat.Checked;
			addTimer(oTT);
			resetForm();
		}
		private void numDay_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}

		private void numHour_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();		
		}

		private void numMin_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();		
		}

		private void numSec_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();		
		}
		#endregion

		#region Set Alarm
		private void btnAlarmPlay_Click(object sender, System.EventArgs e)
		{
			playSound(((_SimpleDataStructure)cmbAlarmSnd.SelectedItem).sFile);
		}
		private void cmbAlarmIcon_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			dispIcon(pbxAlarmIcon,((_SimpleDataStructure)cmbAlarmIcon.SelectedItem).sFile);
		}
		private void btnAlarmGo_Click(object sender, System.EventArgs e)
		{
			if(txtAlarmName.Text=="")txtAlarmName.Text="Unnamed alarm";
			DateTime oAlarm=new DateTime(
				calAlarm.SelectionStart.Year,calAlarm.SelectionStart.Month,calAlarm.SelectionStart.Day,
				(int)numAlarmHour.Value+(cbx24hour.Checked?0:(radPM.Checked?12:0)),
				(int)numAlarmMin.Value,(int)numAlarmSec.Value);
			_TeaTimer oTT=new _TeaTimer(txtAlarmName.Text,oAlarm);
			oTT.Tick += new EventHandler(teaTimer_Tick);
			oTT.AudibleAlert=((_SimpleDataStructure)cmbAlarmSnd.SelectedItem).sFile;
			oTT.VisualAlert=((_SimpleDataStructure)cmbAlarmIcon.SelectedItem).sFile;
			oTT.ProcessAlert=((_SimpleDataStructure)cmbAlarmProcess.SelectedItem).sFile;
			try 
			{
				addTimer(oTT);
			}
			catch(_TeaTimer.TeaTimerException ex) 
			{
				MessageBox.Show("You cannot specify a time in the past!","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				removeTimer(oTT);
			}
			resetForm();
		}
		#endregion

		#region Other
		private void tabView_Click(object sender, EventArgs e)
		{
			resetForm();
		}
		private void btnClose_Click(object sender, System.EventArgs e)
		{
			FadeForm(Fade.Down);
			stopSound();
			setConfig();
			readConfig();
			tabControl1.SelectedTab=null;
			resetForm();
			System.GC.Collect();
		}
		private void btnApply_Click(object sender, System.EventArgs e)
		{
			setConfig();
			readConfig();
			resetForm();
		}
		private void btnHelp_Click(object sender, System.EventArgs e)
		{
			new RtfBox("Help (Tea Timer)","README.txt",RichTextBoxStreamType.PlainText).Show();
		}

		private void highlight_OnFocus(object sender, System.EventArgs e) 
		{
			if(sender is TextBox) 
				((TextBox)sender).SelectAll();
			if(sender is NumericUpDown)
				((NumericUpDown)sender).Select(0,3);			
		}
		private void lnkWebPage_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://www.ericeubank.com");
		}
		private void lnkBugs_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(@"http://sourceforge.net/tracker/?func=browse&group_id=142664&atid=753090");
		}
		#endregion

		#region View Timers
		private void lstTimers_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			timElapsedUpdate.Stop();

			_TeaTimer oTT=(_TeaTimer)lstTimers.SelectedItem;
			if(oTT!=null) 
			{
				lblName.Text=oTT.Name;
				lblTime.Text=getFormattedTime(oTT.Time);
				lblElapsed.Text=getFormattedTime(oTT.getTimeElapsed());
				lblLeft.Text=getFormattedTime(oTT.getTimeLeft());
				lblFinish.Text=(oTT.Paused?"*** P A U S E D ***":getTimeString(oTT.Finished));
				cbxViewVisual.Checked=(oTT.VisualAlert.Length>0);
				cbxViewAudible.Checked=(oTT.AudibleAlert.Length>0);
				lblViewAudible.Text=oTT.AudibleAlert;
				tip.SetToolTip(lblViewAudible,oTT.AudibleAlert);
				cbxViewProcess.Checked=(oTT.ProcessAlert.Length>0);
				lblViewProcess.Text=oTT.ProcessAlert;
				tip.SetToolTip(lblViewProcess,oTT.ProcessAlert);
				cbxViewRepeat.Checked=oTT.Recurring;
				dispIcon(pbxViewIcon,oTT.VisualAlert);
				enableBtns();
			}
			else resetForm();

			timElapsedUpdate.Start();
		}
		private void btnStop_Click(object sender, System.EventArgs e)
		{
			stopTeaTimer((_TeaTimer)lstTimers.SelectedItem);
		}
		private void btnPause_Click(object sender, System.EventArgs e)
		{
			_TeaTimer oTT=(_TeaTimer)lstTimers.SelectedItem;
			pauseTeaTimer(oTT);
			lblFinish.Text=(oTT.Paused?"*** P A U S E D ***":oTT.Finished.ToString());
			lblElapsed.Text=getFormattedTime(oTT.getTimeElapsed());
			lstTimers.SelectedItem=oTT;
		}
		private void btnStopwatch_Click(object sender, System.EventArgs e)
		{
			showStopwatch((_TeaTimer)lstTimers.SelectedItem);
		}
		#endregion

		#region Icon Menu
		private void menRunningStopwatch_Click(object sender, EventArgs e) 
		{
			showStopwatch((_TeaTimer)_oTeaTimers[((MenuItem)((MenuItem)sender).Parent).Index]);
		}
		private void menRunningStop_Click(object sender, EventArgs e) 
		{
			resetForm();
			_TeaTimer oTT=(_TeaTimer)_oTeaTimers[((MenuItem)((MenuItem)sender).Parent).Index];
			stopTeaTimer(oTT);
		}
		private void menRunningPause_Click(object sender, EventArgs e) 
		{
			resetForm();
			_TeaTimer oTT=(_TeaTimer)_oTeaTimers[((MenuItem)((MenuItem)sender).Parent).Index];
			pauseTeaTimer(oTT);
		}
		private void menRunningView_Click(object sender, EventArgs e)
		{
			resetForm();
			tabControl1.SelectedTab=tabView;
			lstTimers.SelectedIndex=((MenuItem)((MenuItem)sender).Parent).Index;
			FadeForm(Fade.Up);
		}
		private void menTimers_Click(object sender, System.EventArgs e)
		{
			tabControl1.SelectedTab=tabCreate;
			resetForm();
			FadeForm(Fade.Up);
		}
		private void menAlarm_Click(object sender, System.EventArgs e)
		{
			tabControl1.SelectedTab=tabAlarm;
			resetForm();
			FadeForm(Fade.Up);
		}

		private void menAbout_Click(object sender, System.EventArgs e)
		{
			new About().Show();
		}

		private void menQuit_Click(object sender, System.EventArgs e)
		{
			notifyIcon.Visible=false;
			notifyIcon.Dispose();
			Application.Exit();
		}

		private void notifyIcon_DoubleClick(object sender, EventArgs e)
		{
			tabControl1.SelectedTab=tabCreate;
			resetForm();
			FadeForm(Fade.Up);
		}
		#endregion

		#region Configuration
		#region General
		private void btnOpacity_Click(object sender, System.EventArgs e)
		{
			Stopwatch oSW=new Stopwatch(new EventHandler(btnOpacity_tbarOpacity_Scroll),(_iStopwatchOpacity/100.0));
		}

		private void btnOpacity_tbarOpacity_Scroll(object sender, EventArgs e)
		{
			_iStopwatchOpacity=((TrackBar)sender).Value;
			lblOpacity.Text=_iStopwatchOpacity+"%";

		}
		private void cbx24hour_CheckedChanged(object sender, System.EventArgs e)
		{
			_b24hour=cbx24hour.Checked;
			enableBtns();
		}
		#endregion

		#region Audible
		private void btnPlaySnd_Click(object sender, System.EventArgs e)
		{
			playSound(((_Sound)lstSounds.SelectedItem).sFile);
		}
		private void cbxPlaySnd_CheckedChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}

		private void btnBrowseSnd_Click(object sender, System.EventArgs e)
		{
			if(diaPlaySnd.ShowDialog()==DialogResult.OK) 
			{
				txtSndFile.Text=diaPlaySnd.FileName;
			}
		}
		private void btnAddSnd_Click(object sender, System.EventArgs e)
		{
			_Sound oS=new _Sound((txtSndName.Text==""?"Sound "+(lstSounds.Items.Count+1):txtSndName.Text),txtSndFile.Text);
			if(oS.sName==NONE) 
			{
				MessageBox.Show("You may not use the name \""+NONE+"\"","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}
			if(hasDupName(oS.sName,lstSounds)) 
			{
				MessageBox.Show("Please choose a unique name.","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}
			//if this is a midi file, warn the user that it may be slow
			if(oS.sFile.ToLower().EndsWith(".mid") || oS.sFile.ToLower().EndsWith(".midi"))
				MessageBox.Show("Warning: Midi files are a little slow to begin playing on some systems.","Midi warning (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Warning);

			lstSounds.Items.Add(oS);
			resetForm();
		}
		private void btnRemoveSnd_Click(object sender, System.EventArgs e)
		{
			lstSounds.Items.Remove(lstSounds.SelectedItem);
			resetForm();
		}
		private void lstSounds_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			btnRemoveSnd.Enabled=(lstSounds.Items.Count>1);
			cbxDefaultSnd.Enabled=(lstSounds.SelectedIndex!=0);
			cbxDefaultSnd.Checked=(lstSounds.SelectedIndex==0);
			btnPlaySnd.Enabled=(lstSounds.SelectedItem!=null);
		}
		private void snd_TextChanged(object sender, System.EventArgs e) 
		{
			btnAddSnd.Enabled=txtSndFile.Text.Length>0;
		}
		private void cbxDefaultSnd_CheckedChanged(object sender, System.EventArgs e)
		{
			if(lstSounds.SelectedIndex==0 || cbxDefaultSnd.Checked==false) return;
			_Sound oS=(_Sound)lstSounds.SelectedItem;
			lstSounds.Items.Remove(oS);
			lstSounds.Items.Insert(0,oS);
			resetForm();
		}
		#endregion

		#region Visual
		private void icon_TextChanged(object sender, System.EventArgs e) 
		{
			btnAddIcon.Enabled=txtIconFile.Text.Length>0;
		}
		private void cbxDefaultIcon_CheckedChanged(object sender, System.EventArgs e)
		{
			if(lstIcons.SelectedIndex==0 || cbxDefaultIcon.Checked==false) return;
			_Icon oI=(_Icon)lstIcons.SelectedItem;
			lstIcons.Items.Remove(oI);
			lstIcons.Items.Insert(0,oI);
			resetForm();
		}
		private void btnBrowseIcon_Click(object sender, System.EventArgs e)
		{
			if(diaBrowseIcons.ShowDialog()==DialogResult.OK) 
			{
				txtIconFile.Text=diaBrowseIcons.FileName;
			}
		}

		private void btnAddIcon_Click(object sender, System.EventArgs e)
		{
			_Icon oI=new _Icon((txtIconName.Text==""?"Icon "+(lstIcons.Items.Count+1):txtIconName.Text),txtIconFile.Text);
			if(oI.sName==NONE) 
			{
				MessageBox.Show("You may not use the name \""+NONE+"\"","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}
			if(hasDupName(oI.sName,lstIcons)) 
			{
				MessageBox.Show("Please choose a unique name.","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}
			lstIcons.Items.Add(oI);
			resetForm();
		}

		private void btnRemoveIcon_Click(object sender, System.EventArgs e)
		{
			lstIcons.Items.Remove(lstIcons.SelectedItem);
			resetForm();
		}

		private void lstIcons_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			btnRemoveIcon.Enabled=(lstIcons.Items.Count>1);
			cbxDefaultIcon.Enabled=(lstIcons.SelectedIndex!=0);
			cbxDefaultIcon.Checked=(lstIcons.SelectedIndex==0);
			if(lstIcons.SelectedItem!=null)
				dispIcon(pbxIcon,((_Icon)lstIcons.SelectedItem).sFile);
		}
		private void cbxVisualAlert_CheckedChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}
		private void btnAlertPosition_Click(object sender, System.EventArgs e)
		{
			TimerPop oPos=new TimerPop("Position me!",null,null,true);
			_iAlertX=_iAlertY=-1;
			lblAlertCoords.Text="centered";
			oPos.LocationChanged+=new EventHandler(TimerPop_Coords);
			oPos.Show();
		}
		private void TimerPop_Coords(object sender, System.EventArgs e) 
		{
			TimerPop oPos=(TimerPop)sender;
			oPos.Left=Math.Max(oPos.Left,0);
			oPos.Top=Math.Max(oPos.Top,0);
			lblAlertCoords.Text=oPos.Left+","+oPos.Top;
			_iAlertX=oPos.Left;
			_iAlertY=oPos.Top;
		}
		#endregion

		#region Processes
		private void btnRunProcess_Click(object sender, System.EventArgs e)
		{
			runProcess(((_Process)lstProcesses.SelectedItem).sFile);
		}
		private void btnBrowseProcess_Click(object sender, System.EventArgs e)
		{
			if(diaRunProcess.ShowDialog()==DialogResult.OK) 
			{
				txtProcessFile.Text=diaRunProcess.FileName;
			}
		}
		private void btnAddProcess_Click(object sender, System.EventArgs e)
		{
			_Process oP=new _Process((txtProcessName.Text==""?"Process "+(lstProcesses.Items.Count+1):txtProcessName.Text),txtProcessFile.Text);
			if(hasDupName(oP.sName,lstProcesses)) 
			{
				MessageBox.Show("Please choose a unique name.","Error (Tea Timer)",MessageBoxButtons.OK,MessageBoxIcon.Error);
				return;
			}
			lstProcesses.Items.Add(oP);
			resetForm();
		}
		private void btnRemoveProcess_Click(object sender, System.EventArgs e)
		{
			lstProcesses.Items.Remove(lstProcesses.SelectedItem);
			resetForm();
		}
		private void lstProcesses_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			btnRemoveProcess.Enabled=btnRunProcess.Enabled=(lstProcesses.SelectedItem!=null);
		}
		private void process_TextChanged(object sender, System.EventArgs e) 
		{
			btnAddProcess.Enabled=txtProcessFile.Text.Length>0;
		}
		#endregion
		#endregion

		#region Presets
		//preset clicked
		private void btnPre_Click(object sender, System.EventArgs e)
		{
			int i=(sender.GetType().Equals((new Button()).GetType())?
				Convert.ToInt32(((Button)sender).Tag):
				iconMenu.MenuItems.IndexOf(menPresets)+Convert.ToInt32(((MenuItem)sender).Text.Substring(1,1))
				)-1;

			_TeaTimer oTT=new _TeaTimer(_oPresets[i].sName,_oPresets[i].iDay,_oPresets[i].iHour,_oPresets[i].iMin,_oPresets[i].iSec);
			oTT.AudibleAlert=(_oPresets[i].bDefSnd?((_Sound)lstSounds.Items[0]).sFile:_oPresets[i].sSnd);
			oTT.VisualAlert=(_oPresets[i].bDefIcon?((_Icon)lstIcons.Items[0]).sFile:_oPresets[i].sIcon);
			oTT.ProcessAlert=_oPresets[i].sProcess;
			oTT.Recurring=_oPresets[i].bRecurring;
			oTT.Tick += new EventHandler(teaTimer_Tick);
			addTimer(oTT);
		}
		//radio buttons
		private void radPre_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!((RadioButton)sender).Checked) return;

			int i=Convert.ToInt32(((RadioButton)sender).Tag)-1;
			txtPreName.Text=_oPresets[i].sName;
			buildCombo(cmbPreSnd,lstSounds);
			buildCombo(cmbPreIcon,lstIcons);
			buildCombo(cmbPreProcess,lstProcesses);

			if(txtPreName.Text!="") 
			{
				//time
				numDayPre.Value=Convert.ToDecimal(_oPresets[i].iDay);
				numHourPre.Value=Convert.ToDecimal(_oPresets[i].iHour);
				numMinPre.Value=Convert.ToDecimal(_oPresets[i].iMin);
				numSecPre.Value=Convert.ToDecimal(_oPresets[i].iSec);
				//sound
				txtPreSnd.Text=_oPresets[i].sSnd;
				cbxPreSnd.Checked=_oPresets[i].bDefSnd;
				if(txtPreSnd.Text!="") cmbPreSnd.SelectedItem=null;
				//icon
				txtPreIcon.Text=_oPresets[i].sIcon;
				cbxPreIcon.Checked=_oPresets[i].bDefIcon;
				dispIcon(pbxPreIcon,_oPresets[i].sIcon);
				if(txtPreIcon.Text!="") cmbPreIcon.SelectedItem=null;
				//process
				txtPreProcess.Text=_oPresets[i].sProcess;
				if(txtPreProcess.Text!="") cmbPreProcess.SelectedItem=null;
				//other
				cbxPreRepeat.Checked=_oPresets[i].bRecurring;
			}
			else 
			{
				numDayPre.Value=numHourPre.Value=numMinPre.Value=numSecPre.Value=0;
				cbxPreSnd.Checked=true;
				cbxPreIcon.Checked=true;
				dispIcon(pbxPreIcon,((_Icon)lstIcons.Items[0]).sFile);
				cbxPreRepeat.Checked=false;

			}
			enableBtns();
			_iSelectedPreset=i;
		}
		//process
		private void txtPreProcess_TextChanged(object sender, EventArgs e)
		{
			enableBtns();
		}
		private void btnPreBrowseProcess_Click(object sender, System.EventArgs e)
		{
			if(diaRunProcess.ShowDialog()==DialogResult.OK) 
			{
				txtPreProcess.Text=diaRunProcess.FileName;
				cmbPreProcess.SelectedItem=null;
				enableBtns();
			}
		}
		private void btnPreRunProcess_Click(object sender, System.EventArgs e)
		{
			runProcess(txtPreProcess.Text);
		}
		private void cmbPreProcess_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(cmbPreProcess.SelectedItem!=null) 
			{
				txtPreProcess.Text=((_SimpleDataStructure)cmbPreProcess.SelectedItem).sFile;
			}
			enableBtns();
		}
		//icon
		private void cbxPreIcon_CheckedChanged(object sender, System.EventArgs e)
		{
			cbxPreIcon.Enabled=!cbxPreIcon.Checked;
			if(!cbxPreIcon.Checked) return;
			txtPreIcon.Text="";
			cmbPreIcon.SelectedItem=null;
			enableBtns();
		}
		private void btnPreIcon_Click(object sender, System.EventArgs e)
		{
			if(diaBrowseIcons.ShowDialog()==DialogResult.OK) 
			{
				txtPreIcon.Text=diaBrowseIcons.FileName;
				cmbPreIcon.SelectedItem=null;
				dispIcon(pbxPreIcon,diaBrowseIcons.FileName);
				cbxPreIcon.Checked=false;
				enableBtns();
			}
		}
		private void cmbPreIcon_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(cmbPreIcon.SelectedItem!=null) 
			{
				txtPreIcon.Text=((_SimpleDataStructure)cmbPreIcon.SelectedItem).sFile;
				cbxPreIcon.Checked=false;
			}
			enableBtns();
		}
		//sound
		private void cbxPreSnd_CheckedChanged(object sender, System.EventArgs e)
		{
			cbxPreSnd.Enabled=!cbxPreSnd.Checked;
			if(!cbxPreSnd.Checked) return;
			txtPreSnd.Text="";
			cmbPreSnd.SelectedItem=null;
			enableBtns();
		}
		private void btnPreSnd_Click(object sender, System.EventArgs e)
		{
			if(diaPlaySnd.ShowDialog()==DialogResult.OK) 
			{
				txtPreSnd.Text=diaPlaySnd.FileName;
				cmbPreSnd.SelectedItem=null;
				cbxPreSnd.Checked=false;
				enableBtns();
			}
		}
		private void btnPrePlaySnd_Click(object sender, System.EventArgs e)
		{
			playSound(txtPreSnd.Text);
		}
		private void cmbPreSnd_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(cmbPreSnd.SelectedItem!=null) 
			{
				txtPreSnd.Text=((_SimpleDataStructure)cmbPreSnd.SelectedItem).sFile;
				cbxPreSnd.Checked=false;
			}
			enableBtns();
		}
		//general
		private void txtPreName_TextChanged(object sender, EventArgs e)
		{
			enableBtns();
		}
		private void numDayPre_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}
		private void numHourPre_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}
		private void numMinPre_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}
		private void numSecPre_ValueChanged(object sender, System.EventArgs e)
		{
			enableBtns();
		}
		private void btnPreSave_Click(object sender, System.EventArgs e)
		{
			int i=_iSelectedPreset;
			_oPresets[i].sName=txtPreName.Text;
			_oPresets[i].sSnd=txtPreSnd.Text;
			_oPresets[i].sIcon=txtPreIcon.Text;
			_oPresets[i].sProcess=txtPreProcess.Text;
			_oPresets[i].iDay=(int)numDayPre.Value;
			_oPresets[i].iHour=(int)numHourPre.Value;
			_oPresets[i].iMin=(int)numMinPre.Value;
			_oPresets[i].iSec=(int)numSecPre.Value;
			_oPresets[i].bDefIcon=cbxPreIcon.Checked;
			_oPresets[i].bDefSnd=cbxPreSnd.Checked;
			_oPresets[i].bRecurring=cbxPreRepeat.Checked;
			MessageBox.Show("Preset #"+(i+1)+" saved.","Tea Timer",MessageBoxButtons.OK,MessageBoxIcon.Information);
			setConfig();
			readConfig();
		}
		private void btnPreDelete_Click(object sender, System.EventArgs e)
		{
			_oPresets[_iSelectedPreset].sName="";
			_oPresets[_iSelectedPreset].sSnd="";
			_oPresets[_iSelectedPreset].sIcon="";
			_oPresets[_iSelectedPreset].sProcess="";
			_oPresets[_iSelectedPreset].iDay=0;
			_oPresets[_iSelectedPreset].iHour=0;
			_oPresets[_iSelectedPreset].iMin=0;
			_oPresets[_iSelectedPreset].iSec=0;
			_oPresets[_iSelectedPreset].bDefIcon=false;
			_oPresets[_iSelectedPreset].bDefSnd=false;
			_oPresets[_iSelectedPreset].bRecurring=false;
			MessageBox.Show("Preset #"+(_iSelectedPreset+1)+" deleted.","Tea Timer",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
			setConfig();
			readConfig();
			resetForm();
		}
		#endregion
		#endregion

		#region Config
		private void readConfig() 
		{
			//visual alert
			cbxVisualAlert.Checked=Convert.ToBoolean(_oConfig.getValue("general","alerts","visual","true"));
			lstIcons.Items.Clear();
			string[] sIconFiles=_oConfig.getValues("general","alerts","iconfiles",new string[] {"teapot.gif","beaker.gif","clock.gif","coffee.gif"});
			string[] sIconNames=_oConfig.getValues("general","alerts","iconnames",new string[] {"Teapot","Beaker","Clock","Coffee Cup"});
			for(int i=0;i<sIconFiles.Length;i++) 
			{
				_Icon oI=new _Icon(sIconNames[i],sIconFiles[i]);
				lstIcons.Items.Add(oI);
			}
			_iAlertX=Convert.ToInt32(_oConfig.getValue("general","alerts","visualX","-1"));
			_iAlertY=Convert.ToInt32(_oConfig.getValue("general","alerts","visualY","-1"));

			//sound alert
			cbxPlaySnd.Checked=Convert.ToBoolean(_oConfig.getValue("general","alerts","audible","true"));
			lstSounds.Items.Clear();
			string[] sSndFiles=_oConfig.getValues("general","alerts","soundfiles",new string[] {"teapot_long.wav","teapot_short.wav"});
			string[] sSndNames=_oConfig.getValues("general","alerts","soundnames",new string[] {"Teapot Long","Teapot Short"});
			for(int i=0;i<sSndFiles.Length;i++) 
			{
				_Sound oS=new _Sound(sSndNames[i],sSndFiles[i]);
				lstSounds.Items.Add(oS);
			}

			//process alert
			cbxVisualProcessAlert.Checked=Convert.ToBoolean(_oConfig.getValue("general","alerts","processvisual","true"));
			cbxAudibleProcessAlert.Checked=Convert.ToBoolean(_oConfig.getValue("general","alerts","processaudible","true"));
			cbxPlaySnd.Checked=Convert.ToBoolean(_oConfig.getValue("general","alerts","audible","true"));
			lstProcesses.Items.Clear();
			string[] sProcessFiles=_oConfig.getValues("general","alerts","processfiles",new string[] {});
			string[] sProcessNames=_oConfig.getValues("general","alerts","processnames",new string[] {});
			for(int i=0;i<sProcessFiles.Length;i++) 
			{
				_Process oP=new _Process(sProcessNames[i],sProcessFiles[i]);
				lstProcesses.Items.Add(oP);
			}

			//Time format
			cbx24hour.Checked=Convert.ToBoolean(_oConfig.getValue("general","time format","hour24","false"));
			_b24hour=cbx24hour.Checked;

			//stopwatch
			_iStopwatchOpacity=Convert.ToInt32(_oConfig.getValue("general","stopwatch","opacity","80"));
			cbxShowStopwatch.Checked=Convert.ToBoolean(_oConfig.getValue("general","stopwatch","show","false"));

			//presets
			int iPresetsIdx=iconMenu.MenuItems.IndexOf(menPresets);
			while(_iIconMenuPresets>0)
			{
				iconMenu.MenuItems.RemoveAt(iPresetsIdx+(_iIconMenuPresets--));
			}
			for(int i=0; i<6; i++) 
			{
				Button btnPre=null;
				string sDefName="", sDefSnd="", sDefIcon="", sDefProcess="";
				int iDefDay=0, iDefHour=0, iDefMin=0, iDefSec=0;
				bool bDefSnd=true, bDefIcon=true, bRecurring=false;
				switch(i+1) 
				{
					case 1:
						btnPre=btnPre1;
						sDefName="Boil Water";
						iDefMin=5;
						iDefSec=30;
						sDefIcon="clock.gif";
						bDefIcon=false;
						break;
					case 2:
						btnPre=btnPre2;
						sDefName="Earl Grey";
						iDefMin=4;
						break;
					case 3:
						btnPre=btnPre3;
						sDefName="Green Tea";
						iDefMin=2;
						break;
					case 4:
						btnPre=btnPre4;
						sDefName="Brew Coffee";
						iDefMin=8;
						sDefIcon="coffee.gif";
						bDefIcon=false;
						break;
					case 5:
						btnPre=btnPre5;
						break;
					case 6:
						btnPre=btnPre6;
						break;
				}
				btnPre.Text=_oPresets[i].sName=_oConfig.getValue("presets",i.ToString(),"name",sDefName);
				btnPre.Enabled=(_oPresets[i].sName==""?false:true);
				MenuItem oMI=new MenuItem("#"+(i+1)+" "+(_oPresets[i].sName==""?"not set":_oPresets[i].sName),new EventHandler(btnPre_Click));
				oMI.Enabled=(_oPresets[i].sName==""?false:true);
				iconMenu.MenuItems.Add(iPresetsIdx+(++_iIconMenuPresets),oMI);
				_oPresets[i].iDay=Convert.ToInt32(_oConfig.getValue("presets",i.ToString(),"days",iDefDay.ToString()));
				_oPresets[i].iHour=Convert.ToInt32(_oConfig.getValue("presets",i.ToString(),"hours",iDefHour.ToString()));
				_oPresets[i].iMin=Convert.ToInt32(_oConfig.getValue("presets",i.ToString(),"minutes",iDefMin.ToString()));
				_oPresets[i].iSec=Convert.ToInt32(_oConfig.getValue("presets",i.ToString(),"seconds",iDefSec.ToString()));
				_oPresets[i].sSnd=_oConfig.getValue("presets",i.ToString(),"sound",sDefSnd);
				_oPresets[i].bDefSnd=Convert.ToBoolean(_oConfig.getValue("presets",i.ToString(),"defsound",bDefSnd.ToString()));
				_oPresets[i].sIcon=_oConfig.getValue("presets",i.ToString(),"icon",sDefIcon);
				_oPresets[i].bDefIcon=Convert.ToBoolean(_oConfig.getValue("presets",i.ToString(),"deficon",bDefIcon.ToString()));
				_oPresets[i].sProcess=_oConfig.getValue("presets",i.ToString(),"process",sDefProcess);
				_oPresets[i].bRecurring=Convert.ToBoolean(_oConfig.getValue("presets",i.ToString(),"recurring",bRecurring.ToString()));
			}
		}
		private void setConfig() 
		{
			_oConfig.setValue("teatimer","info","version",VERSION);

			//visual alert
			_oConfig.setValue("general","alerts","visual",cbxVisualAlert.Checked.ToString());
			string[] sIconFiles=new string[lstIcons.Items.Count];
			string[] sIconNames=new string[lstIcons.Items.Count];
			int i=0;
			foreach(_Icon oIcon in lstIcons.Items) 
			{
				sIconFiles[i]=oIcon.sFile;
				sIconNames[i]=oIcon.sName;
				i++;
			}
			_oConfig.setValues("general","alerts","iconfiles",sIconFiles);
			_oConfig.setValues("general","alerts","iconnames",sIconNames);
			_oConfig.setValue("general","alerts","visualX",_iAlertX.ToString());
			_oConfig.setValue("general","alerts","visualY",_iAlertY.ToString());

			//sound alerts
			_oConfig.setValue("general","alerts","audible",cbxPlaySnd.Checked.ToString());
			string[] sSndFiles=new string[lstSounds.Items.Count];
			string[] sSndNames=new string[lstSounds.Items.Count];
			i=0;
			foreach(_Sound oSound in lstSounds.Items) 
			{
				sSndFiles[i]=oSound.sFile;
				sSndNames[i]=oSound.sName;
				i++;
			}
			_oConfig.setValues("general","alerts","soundfiles",sSndFiles);
			_oConfig.setValues("general","alerts","soundnames",sSndNames);

			//process alerts
			_oConfig.setValue("general","alerts","processvisual",cbxVisualProcessAlert.Checked.ToString());
			_oConfig.setValue("general","alerts","processaudible",cbxAudibleProcessAlert.Checked.ToString());
			string[] sProcessFiles=new string[lstProcesses.Items.Count];
			string[] sProcessNames=new string[lstProcesses.Items.Count];
			i=0;
			foreach(_Process oProcess in lstProcesses.Items) 
			{
				sProcessFiles[i]=oProcess.sFile;
				sProcessNames[i]=oProcess.sName;
				i++;
			}
			_oConfig.setValues("general","alerts","processfiles",sProcessFiles);
			_oConfig.setValues("general","alerts","processnames",sProcessNames);

			//time format
			_oConfig.setValue("general","time format","hour24",cbx24hour.Checked.ToString());

			//stopwatch
			_oConfig.setValue("general","stopwatch","opacity",_iStopwatchOpacity.ToString());
			_oConfig.setValue("general","stopwatch","show",cbxShowStopwatch.Checked.ToString());

			//presets
			for(i=0; i<6; i++) 
			{
				_oConfig.setValue("presets",i.ToString(),"name",_oPresets[i].sName);
				_oConfig.setValue("presets",i.ToString(),"days",_oPresets[i].iDay.ToString());
				_oConfig.setValue("presets",i.ToString(),"hours",_oPresets[i].iHour.ToString());
				_oConfig.setValue("presets",i.ToString(),"minutes",_oPresets[i].iMin.ToString());
				_oConfig.setValue("presets",i.ToString(),"seconds",_oPresets[i].iSec.ToString());
				_oConfig.setValue("presets",i.ToString(),"sound",_oPresets[i].sSnd);
				_oConfig.setValue("presets",i.ToString(),"defsound",_oPresets[i].bDefSnd.ToString());
				_oConfig.setValue("presets",i.ToString(),"icon",_oPresets[i].sIcon);
				_oConfig.setValue("presets",i.ToString(),"deficon",_oPresets[i].bDefIcon.ToString());
				_oConfig.setValue("presets",i.ToString(),"process",_oPresets[i].sProcess);
				_oConfig.setValue("presets",i.ToString(),"recurring",_oPresets[i].bRecurring.ToString());
			}
		}
		#endregion
	}
}
