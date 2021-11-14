using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HidLibrary;

namespace VTSPentabPlugin
{
    class PentabInfoPicker
	{
		public class PentabInfo
		{
			public int pointX = -1;
            public int pointY = -1;

            public int presser = -1;

            public int tiltX = -1;
            public int tiltY = -1;

            public bool isOnTab = false;

			public int[] buttens = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			public bool[] sideButtions = new bool[2];

			public int getEventCode(byte[] buff) => (int)(buff[1] << 8) + (int)(buff[2] & 0xE0 );
			private int getPointX(byte[] buff) => ((int)(buff[3] & 0x7f) << 8) + (int)buff[4];
			private int getPointY(byte[] buff) => ((int)(buff[5] & 0x7f) << 8) + (int)buff[6];
			private int getPresser(byte[] buff) => ((int)(buff[7]) << 2) + (int)((buff[8] & 0xc0) >> 6);

			private int getTiltX(byte[] buff) => ((int)(buff[8] & 0x3f) << 1) + (int)((buff[9] & 0x80) >> 7);
			private int getTiltY(byte[] buff) => (int)(buff[9] & 0x7f);

			public string rawData = "";

			public void checkGetters(byte[] buff)
			{
				string result = "";

				result += "Event:" + getEventCode(buff) + "\t";
				result += "pointX = " + getPointX(buff) + ",\t";
				result += "pointY = " + getPointY(buff) + ",\t";
				result += "presser = " + getPresser(buff) + ",\t";
				result += "tiltX = " + getTiltX(buff) + ",\t";
				result += "tiltY = " + getTiltY(buff) + ",\t";
				updateButtons(buff);
				result += "buttons = { ";
				result += string.Join(",", buttens);
				result += "}";
				// System.Diagnostics.Debug.Print(result);
			}

			public void updateTouch(byte[] buff)
			{
				int eventCode = getEventCode(buff);
				if (eventCode == 704)
				{
					isOnTab = true;
				}
				else if (eventCode == 640)
				{
					isOnTab = false;
				}
			}

			public void updateButtons(byte[] buff)
			{
				for (int i = 0; i < 8; i++)
				{
					buttens[i] = (int)((buff[5] >> i) & 1);
				}
				buttens[8] = (int)((buff[4]) & 1);
			}

			public void updateSideButtons(byte[] buff)
			{
				sideButtions[0] = (buff[2] & 0b10) == 0b10;
				sideButtions[1] = (buff[2] & 0b100) == 0b100;
			}

			public void updatePoints(byte[] buff)
			{
				pointX = getPointX(buff);
				pointY = getPointY(buff);
			}

			public void updatePresser(byte[] buff)
			{
				presser = getPresser(buff);
			}

			public void updateTilt(byte[] buff)
			{
				tiltX = getTiltX(buff);
				tiltY = getTiltY(buff);
			}

            public PentabInfo Clone()
            {
                PentabInfo ret = new PentabInfo();
                ret.buttens = (int[])buttens.Clone();
                ret.isOnTab = isOnTab;
                ret.pointX = pointX;
                ret.pointY = pointY;
                ret.presser = presser;
                ret.tiltX = tiltX;
                ret.tiltY = tiltY;
                buttens.CopyTo(ret.buttens,0);
                ret.rawData = rawData;
                sideButtions.CopyTo(ret.sideButtions,0);
                return ret;
            }
            
            public string GetButtonText()
            {
	            string ret = "";
	            for (int i = 0; i < 9; i++)
	            {
		            ret += $"{i}:{buttens[i]} ";
	            }

	            ret += $" sideUnder:{(sideButtions[0]?1:0)} sideUpper:{(sideButtions[1]?1:0)}";

	            return ret;
            }
        }

        private CancellationTokenSource cancelSource;
        bool dead = false;

        PentabInfo pentabInfo = new PentabInfo();

        private HidDevice hitDevice = null;
        
		public bool Init(int venderId, int usage, int usagePage)
		{

			var deviceList = HidDevices.Enumerate();

			try
			{
				hitDevice =
					deviceList.First((it) =>
					{
						return (it.Attributes.VendorId == venderId) &&
						       (it.Capabilities.Usage == usage) &&
						       (it.Capabilities.UsagePage == usagePage);
					});
			}
			catch
			{
				return false;
			}

			if (hitDevice == null) return false;

            dead = false;
			cancelSource = new CancellationTokenSource();
			Task.Run(gettingTask,cancelSource.Token);

            return true;
        }

        public void Close()
        {
			if(cancelSource != null) cancelSource.Cancel();
			if( hitDevice != null ) hitDevice.CloseDevice();
            dead = true;
		}

        private async Task gettingTask()
		{
			try
			{
				while (!dead)
				{
					var readData = hitDevice.Read();

                    int eventCode = pentabInfo.getEventCode(readData.Data);
				    switch (eventCode)
				    {
					    case 544://10 0010 0000
						    pentabInfo.updatePoints(readData.Data);
						    break;
					    case 736://10 1110 0000
						    pentabInfo.updatePoints(readData.Data);
						    pentabInfo.updatePresser(readData.Data);
						    pentabInfo.updateTilt(readData.Data);
						    pentabInfo.updateSideButtons(readData.Data);
						    break;
					    case 704://10 1100 0000
					    case 640://10 1000 0000
						    pentabInfo.updateTouch(readData.Data);
						    break;
					    case 896://11 1000 0000
						    pentabInfo.updateButtons(readData.Data);
						    break;
					    case 49152://1100 0000 0000 0000
						    break;
					    default:
						    //                     string result = "eve =";
						    //                     result = eventCode + " ,data={";
						    //                     int i = 0;

						    //                     foreach (byte e in (byte[])buff)
						    //                     {
						    //                         result += i.ToString() + ":" + Convert.ToString(e, 2).PadLeft(8, '0') + ",";
						    //                         i++;
						    //                     }
						    //                     result = result.Remove(result.Length - 1);
						    //                     result += "}";

						    //                     System.Diagnostics.Debug.Print(result);
						    //pentabInfo.checkGetters(buff);
						    break;
				    }

				    pentabInfo.rawData = 
					    string.Join(
						    " | ",
						    readData.Data.Select(x=>Convert.ToString(x,2).PadLeft(8,'0'))
						    );
				    // Console.WriteLine(pentabInfo.rawData);
                }
			}
			catch { }
		}

        public PentabInfo GetState()
        {
            return pentabInfo.Clone();
        }
	}
}
