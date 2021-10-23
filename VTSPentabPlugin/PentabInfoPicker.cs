using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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

			public int getEventCode(byte[] buff) => (int)(buff[1] << 8) + (int)buff[2];
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
				if (eventCode == 706)
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
				
			}

			public void updatePoints(byte[] buff)
			{
				lock (this)
				{
					pointX = getPointX(buff);
					pointY = getPointY(buff);
				}
			}

			public void updatePresser(byte[] buff)
			{
				lock (this)
				{
					presser = getPresser(buff);
				}
			}

			public void updateTilt(byte[] buff)
			{
				lock (this)
				{
					tiltX = getTiltX(buff);
					tiltY = getTiltY(buff);
				}
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
                return ret;
            }
            
            public string GetButtonText()
            {
	            string ret = "";
	            for (int i = 0; i < 9; i++)
	            {
		            ret += $"{i}:{buttens[i]} ";
	            }

	            return ret;
            }
        }
		class HIDevice
        {
            public IntPtr handle;
            public string path;

            public HIDD_ATTRIBUTES att;

            public HIDP_CAPS caps;

            public IntPtr preparse;
        }

		#region HID Connections
		[StructLayout(LayoutKind.Sequential)]
		public struct SP_DEVICE_INTERFACE_DATA
		{
			public int cbSize;
			public Guid InterfaceClassGuid;
			public int Flags;
			public IntPtr Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct SP_DEVINFO_DATA
		{
			public uint cbSize;
			public Guid classGuid;
			public uint devInst;
			public IntPtr reserved;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			internal int Size;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal string DevicePath;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct HIDD_ATTRIBUTES
		{
			internal int Size;
			internal ushort VendorID;
			internal ushort ProductID;
			internal short VersionNumber;
		}

		[DllImport("hid.dll", EntryPoint = "HidD_GetHidGuid", SetLastError = true)]
		static extern void HidD_GetHidGuid(out Guid Guid);

		[DllImport("hid.dll")]
		static internal extern Boolean HidD_GetAttributes(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

		[DllImport("hid.dll")]
		static internal extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

		[DllImport("hid.dll")]
		static internal extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, ref IntPtr preparsedData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SetupDiGetClassDevs(
											  ref Guid ClassGuid,
											  [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
											  IntPtr hwndParent,
											  uint Flags
											 );

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern Boolean SetupDiEnumDeviceInterfaces(
									  IntPtr hDevInfo,
									  IntPtr devInfo,
									  ref Guid interfaceClassGuid,
									  UInt32 memberIndex,
									  ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
									);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, EntryPoint = "SetupDiGetDeviceInterfaceDetail")]
		static extern bool SetupDiGetDeviceInterfaceDetailBuffer(
			IntPtr deviceInfoSet,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			IntPtr deviceInterfaceDetailData,
			int deviceInterfaceDetailDataSize,
			ref int requiredSize,
			IntPtr deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		static extern bool SetupDiGetDeviceInterfaceDetail(
			IntPtr deviceInfoSet,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
			int deviceInterfaceDetailDataSize,
			ref int requiredSize,
			IntPtr deviceInfoData);


		[DllImport("kernel32", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
		static extern System.IntPtr CreateFileW
		(
				string FileName,          // file name
				uint DesiredAccess,       // access mode
				uint ShareMode,           // share mode
				IntPtr SecurityAttributes,  // Security Attributes
				uint CreationDisposition, // how to create
				uint FlagsAndAttributes,  // file attributes
				IntPtr hTemplateFile         // handle to template file
		);

		[DllImport("kernel32.dll", SetLastError = true)]
		static internal extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, [In] ref System.Threading.NativeOverlapped lpOverlapped);
		
		[DllImport("kernel32", SetLastError = true)]
		static extern bool CloseHandle
		(
			System.IntPtr hObject // handle to object
		);
		
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		static internal extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES securityAttributes, bool bManualReset, bool bInitialState, IntPtr lpName);

		[DllImport("kernel32.dll")]
		static internal extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

		const int DIGCF_DEFAULT = 0x1;
		const int DIGCF_PRESENT = 0x2;
		const int DIGCF_ALLCLASSES = 0x4;
		const int DIGCF_PROFILE = 0x8;
		const int DIGCF_DEVICEINTERFACE = 0x10;

		const int CREATE_NEW = 1;
		const int CREATE_ALWAYS = 2;
		const int OPEN_EXISTING = 3;
		const int OPEN_ALWAYS = 4;
		const int TRUNCATE_EXISTING = 5;

		internal const int INVALID_HANDLE_VALUE = -1;

		internal const short FILE_SHARE_READ = 0x1;
		internal const short FILE_SHARE_WRITE = 0x2;

		internal const uint GENERIC_READ = 0x80000000;
		internal const uint GENERIC_WRITE = 0x40000000;

		internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

		internal const long ERROR_IO_PENDING = 997L;

		internal const uint WAIT_OBJECT_0 = 0;

		[StructLayout(LayoutKind.Sequential)]
		internal struct HIDP_CAPS
		{
			internal ushort Usage;
			internal ushort UsagePage;
			internal ushort InputReportByteLength;
			internal ushort OutputReportByteLength;
			internal ushort FeatureReportByteLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			internal ushort[] Reserved;
			internal ushort NumberLinkCollectionNodes;
			internal ushort NumberInputButtonCaps;
			internal ushort NumberInputValueCaps;
			internal ushort NumberInputDataIndices;
			internal ushort NumberOutputButtonCaps;
			internal ushort NumberOutputValueCaps;
			internal ushort NumberOutputDataIndices;
			internal ushort NumberFeatureButtonCaps;
			internal ushort NumberFeatureValueCaps;
			internal ushort NumberFeatureDataIndices;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public bool bInheritHandle;
		}//*/

		#endregion HID Connections


        List<HIDevice> m_deviceList = new List<HIDevice>();
        int m_select = 0;
        Thread m_Thread;
        bool dead = false;

        PentabInfo pentabInfo = new PentabInfo();

		public bool Init(int venderId, int usage, int usagePage)
        {

			Guid guid = new Guid();

			HidD_GetHidGuid(out guid);

			IntPtr hDevInfo = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

			SP_DEVICE_INTERFACE_DATA spid = new SP_DEVICE_INTERFACE_DATA();
			spid.cbSize = Marshal.SizeOf(spid);

			uint devNumber = 0;

			while (SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, ref guid, devNumber, ref spid))
			{
				try
				{
					int Reguired = 0;
					SetupDiGetDeviceInterfaceDetailBuffer(hDevInfo, ref spid, IntPtr.Zero, 0, ref Reguired, IntPtr.Zero);

					var interfaceDetail = new SP_DEVICE_INTERFACE_DETAIL_DATA { Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8 };

					int predictedLength = Reguired;

					if (SetupDiGetDeviceInterfaceDetail(hDevInfo, ref spid, ref interfaceDetail, predictedLength, ref Reguired, IntPtr.Zero))
					{
						HIDevice nDevice = new HIDevice();
						nDevice.path = interfaceDetail.DevicePath;

						nDevice.handle = CreateFileW(nDevice.path
													, GENERIC_READ | GENERIC_WRITE
													, FILE_SHARE_READ | FILE_SHARE_WRITE
													, IntPtr.Zero
													, OPEN_EXISTING
													, FILE_FLAG_OVERLAPPED
													, IntPtr.Zero);

						if (nDevice.handle != (IntPtr)INVALID_HANDLE_VALUE)
						{
                            HidD_GetAttributes(nDevice.handle, ref nDevice.att);

							HidD_GetPreparsedData(nDevice.handle, ref nDevice.preparse);

							HidP_GetCaps(nDevice.preparse, ref nDevice.caps);

							if (nDevice.caps.Usage == usage & nDevice.caps.UsagePage == usagePage & nDevice.att.VendorID == venderId)
								m_deviceList.Add(nDevice);
						}
					}
					else
					{
						//int errorCode = Marshal.GetLastWin32Error();
					}
				}
				catch
				{

				}
				devNumber++;
			}

            if (m_deviceList.Count == 0)
            {
                return false;
            }

            dead = false;
            m_Thread = new Thread(new ThreadStart(getThread));
			m_Thread.Start();

            return true;
        }

        public void Close()
        {
			if(m_Thread != null) m_Thread.Interrupt();
            foreach (HIDevice e in m_deviceList)
            {
                CloseHandle(e.handle);
            }
            dead = true;
		}

        private void getThread()
		{
			try
			{
				SECURITY_ATTRIBUTES security = new SECURITY_ATTRIBUTES();
				NativeOverlapped overlap = new NativeOverlapped();

				security.lpSecurityDescriptor = IntPtr.Zero;
				security.bInheritHandle = true;
				security.nLength = Marshal.SizeOf(security);

				overlap.OffsetLow = 0;
				overlap.OffsetHigh = 0;
				overlap.EventHandle = CreateEvent(ref security, false, true, IntPtr.Zero);

				byte[] buff = null;
				Array.Resize(ref buff, m_deviceList[m_select].caps.InputReportByteLength);

				uint readedLength = 0;
				bool realfiledata;



				while (!dead)
				{
					if (m_select >= m_deviceList.Count) break;

					realfiledata = ReadFile(m_deviceList[m_select].handle, buff, (uint)buff.Length, out readedLength, ref overlap);

					if (!realfiledata && Marshal.GetLastWin32Error() != ERROR_IO_PENDING) continue;

					while (true)
					{
						if (WaitForSingleObject(overlap.EventHandle, 1000) == WAIT_OBJECT_0)
						{
							break;
						}

						if (dead)
						{
							return;
						}

					}

                    lock (pentabInfo)
                    {
                        int eventCode = pentabInfo.getEventCode(buff);
					    switch (eventCode)
					    {
						    case 544:
							    pentabInfo.updatePoints(buff);
							    break;
						    case 736:
						    case 737:
							    pentabInfo.updatePoints(buff);
							    pentabInfo.updatePresser(buff);
							    pentabInfo.updateTilt(buff);
							    break;
						    case 706:
						    case 640:
							    pentabInfo.updateTouch(buff);
							    break;
						    case 896:
							    pentabInfo.updateButtons(buff);
							    break;
						    case 49152:
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
							    buff.Select(x=>Convert.ToString(x,2).PadLeft(8,'0'))
							    );
                    }
				}
			}
			catch (ThreadAbortException e)
			{

			}
		}

        public PentabInfo GetState()
        {
            lock (pentabInfo)
            {
                return pentabInfo.Clone();
            }
        }
	}
}
