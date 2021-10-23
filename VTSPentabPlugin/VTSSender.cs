using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VTS;
using VTS.Models.Impl;
using VTS.Networking;
using VTS.Networking.Impl;

namespace VTSPentabPlugin
{
    class VTSSender : VTSPlugin
    {
        private readonly Thread _updateThread;

        private List<InjectionInfo> _sendList = new List<InjectionInfo>();
        public InjectionInfo _onTable;
        public InjectionInfo _posX;
        public InjectionInfo _posY;
        public InjectionInfo _tiltX;
        public InjectionInfo _tiltY;
        public InjectionInfo _pre;
        public InjectionInfo[] _buttons;
        public InjectionInfo _underSideButton;
        public InjectionInfo _upperSideButton;

        private static int buttonCount = 9;

        public VTSSender()
        {
            this._pluginAuthor = "Ganeesya";
            this._pluginName = "Pentab";
            this.Initialize(
                new VTSWebSocket(),
                new WebSocketImpl(),
                new JsonUtilityImpl(),
                new TokenStorageImpl(),
                ()=>{},
                ()=>{},
                ()=>{});
            _updateThread = new Thread(SocketUpdate);
            _updateThread.Start();

            _sendList.Add(_onTable = new InjectionInfo("PentabOnTable", "If the pen is on the tablet, return 1."));
            _sendList.Add(_posX = new InjectionInfo("PentabPositionX", "Returns the horizontal position of the pen on the tablet."));
            _sendList.Add(_posY = new InjectionInfo("PentabPositionY", "Returns the vertical position of the pen on the tablet."));
            _sendList.Add(_tiltX = new InjectionInfo("PentabTiltX", "Returns the tilt along the horizontal direction of the pen."));
            _sendList.Add(_tiltY = new InjectionInfo("PentabTiltY", "Returns the tilt along the vertical direction of the pen."));
            _sendList.Add(_pre = new InjectionInfo("PentabPresser", "Returns the pen pressure."));

            _buttons = new InjectionInfo[buttonCount];
            for (int i = 0; i < buttonCount; i++)
            {
                _buttons[i] = new InjectionInfo($"PentabButton{i}", $"Retuns the pentab button{i} down.");
            }
            _sendList.Add(_underSideButton = new InjectionInfo("PentabUnderSideButton", "Returns the pen side button under down."));
            _sendList.Add(_upperSideButton = new InjectionInfo("PentabUpperSideButton", "Returns the pen side button upper down."));

            RegistrationCustomInput();
        }

        public async void RegistrationCustomInput()
        {
            while (!IsAuthenticated)
            {
                await Task.Delay(10);
            }
            _sendList.ForEach((ele) =>
            {
                AddCustomParameter(ele.ConvertCustomParameter(),
                    (r) =>
                    {
                        Debug.Print($"Custom Input >{ele.name}< is Safe Add");
                    },
                    (e) =>
                    {
                        Debug.Print($"Custom Input >{ele.name}< is error");
                        Debug.Print(e.data.message);
                    });
            });
        }

        public void SendCustomInput()
        {
            if( !IsAuthenticated ) return;

            var array =
                _sendList.ConvertAll((ele) => { return ele.ConvertInjectionValue(); }).ToArray();

            InjectParameterValues(array,
                (r) =>
                {
                    // Debug.Print($"Injection is Safe");
                },
                (e) =>
                {
                    Debug.Print(e.data.message);
                });
        }

        private async void SocketUpdate()
        {
            while (true)
            {
                try
                {
                    Socket.Update();
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
                await Task.Delay(2);
            }
        }

        public void Close()
        {
            this._updateThread.Interrupt();
            this.Socket.Close();
        }
    }


}
