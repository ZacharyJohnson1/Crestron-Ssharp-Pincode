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

        /// <summary>
        /// Digital signal change event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public delegate void DigitalChangeEventHandler(uint id, SigEventArgs args);
        public event DigitalChangeEventHandler DigitalChangeEvent;

        /// <summary>
        /// Analog signal change event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public delegate void AnalogChangeEventHandler(uint id, SigEventArgs args);
        public event AnalogChangeEventHandler AnalogChangeEvent;

        /// <summary>
        /// Serial signal change event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public delegate void SerialChangeEventHandler(uint id, SigEventArgs args);
        public event SerialChangeEventHandler SerialChangeEvent;

        /// <summary>
        /// SmartObject signal change event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        public delegate void SmartObjectEventHandler(uint id, SmartObjectEventArgs args);
        public event SmartObjectEventHandler SmartObjectChangeEvent;

        #endregion

        /// <summary>
        /// Callback when a digital,analog or serial signal event occurs
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        public void xpanel_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                {
                    DigitalChangeEvent.Invoke(currentDevice.ID, args);
                    break;
                }
                case eSigType.UShort:
                {
                    AnalogChangeEvent.Invoke(currentDevice.ID, args);
                    break;
                }
                case eSigType.String:
                {
                    SerialChangeEvent.Invoke(currentDevice.ID, args);
                    break;
                }
            }
        }

        /// <summary>
        /// Callback when a SmartObject signal event occurs
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        public void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            SmartObjectChangeEvent.Invoke(currentDevice.ID, args);
        }

        /// <summary>
        /// Utility to set a serial input on a touchpanel given a ui object, join number and custom text
        /// </summary>
        /// <param name="joinNum">join number of serial input</param>
        /// <param name="text">custom message to output</param>
        /// <param name="ui">generic ui object</param>
        public void SetSerialJoin(BasicTriList ui, uint joinNum, string text)
        {
            ui.StringInput[joinNum].StringValue = text;
        }
    }
}