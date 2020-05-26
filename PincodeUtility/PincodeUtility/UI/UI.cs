using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PincodeUtility
{
    public class UI
    {
        #region Events
        public delegate void DigitalChangeEventHandler(uint id, SigEventArgs args);
        public event DigitalChangeEventHandler DigitalChangeEvent;

        public delegate void AnalogChangeEventHandler(uint id, SigEventArgs args);
        public event AnalogChangeEventHandler AnalogChangeEvent;

        public delegate void SerialChangeEventHandler(uint id, SigEventArgs args);
        public event SerialChangeEventHandler SerialChangeEvent;

        public delegate void SmartObjectEventHandler(uint id, SmartObjectEventArgs args);
        public event SmartObjectEventHandler SmartObjectChangeEvent;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        public void xpanel_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                {
                    DigitalChangeEvent(currentDevice.ID, args);
                    break;
                }
                case eSigType.UShort:
                {
                    AnalogChangeEvent(currentDevice.ID, args);
                    break;
                }
                case eSigType.String:
                {
                    SerialChangeEvent(currentDevice.ID, args);
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        public void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            SmartObjectChangeEvent(currentDevice.ID, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="joinNum"></param>
        /// <param name="text"></param>
        /// <param name="ui"></param>
        public void SetSerialJoin(BasicTriList ui, uint joinNum, string text)
        {
            ui.StringInput[joinNum].StringValue = text;
        }
    }
}