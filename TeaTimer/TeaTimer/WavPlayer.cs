using System;
using System.Runtime.InteropServices;

namespace WavPlay
{
	/// <summary>
	/// DEPRECATED.
	/// </summary>
	public class WavPlayer
	{
		[DllImport("WinMM.dll")]
		protected static extern bool  PlaySound(byte[]wfname, int fuSound);

		public static void Play(string sFile) 
		{
			Play(sFile,(int)SoundFlags.SND_ASYNC);
		}
		public static void Play(string sFile,int iSoundFlags)
		{
			byte[] bName = new Byte[256];    //Max path length
			bName = System.Text.Encoding.ASCII.GetBytes(sFile);
			PlaySound(bName,iSoundFlags);
		}
		public static void StopPlay()
		{
			PlaySound(null,(int)SoundFlags.SND_PURGE);
		}

		public enum SoundFlags : int 
		{
			SND_SYNC = 0x0000,  // play synchronously (default) 
			SND_ASYNC = 0x0001,  // play asynchronously 
			SND_NODEFAULT = 0x0002,  // silence (!default) if sound not found 
			SND_MEMORY = 0x0004,  // pszSound points to a memory file
			SND_LOOP = 0x0008,  // loop the sound until next sndPlaySound 
			SND_NOSTOP = 0x0010,  // don't stop any currently playing sound 
			SND_NOWAIT = 0x00002000, // don't wait if the driver is busy 
			SND_ALIAS = 0x00010000, // name is a registry alias 
			SND_ALIAS_ID = 0x00110000, // alias is a predefined ID
			SND_FILENAME = 0x00020000, // name is file name 
			SND_RESOURCE = 0x00040004,  // name is resource name or atom
			SND_PURGE = 0x0040
		}
	}
}
