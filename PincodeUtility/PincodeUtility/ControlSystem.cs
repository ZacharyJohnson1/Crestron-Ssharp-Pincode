using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;
using System.Collections.Generic;

namespace PincodeUtility
{
    public class ControlSystem : CrestronControlSystem
    {
        private XpanelForSmartGraphics xpanel;
        private UI ui;
        private Pincode pincode;

        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error(">>> Error in the constructor: {0}", e.Message);
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                ui = new UI();
                ui.DigitalChangeEvent += new UI.DigitalChangeEventHandler(ui_DigitalChangeEvent);
                ui.AnalogChangeEvent += new UI.AnalogChangeEventHandler(ui_AnalogChangeEvent);
                ui.SerialChangeEvent += new UI.SerialChangeEventHandler(ui_SerialChangeEvent);
                ui.SmartObjectChangeEvent += new UI.SmartObjectEventHandler(ui_SmartObjectChangeEvent);

                xpanel = new XpanelForSmartGraphics(0x03, this);
                xpanel.SigChange += new SigEventHandler(ui.xpanel_SigChange);
                xpanel.LoadSmartObjects(@"\USER\Pincode.sgd");
                foreach (KeyValuePair<uint, SmartObject> kvp in xpanel.SmartObjects)
                {
                    kvp.Value.SigChange += new SmartObjectSigChangeEventHandler(ui.SmartObject_SigChange);
                }
                xpanel.Register();

                //create Pincode object
                pincode = new Pincode(xpanel, 1, "1234");

                pincode.EnableBackdoorPassword("1988");
                pincode.SetPinLimit(4);
                pincode.EnableStarText();

                pincode.PasswordMiscOneDelegate = () => pincode.ClearText();
                pincode.PasswordMiscTwoDelegate = () => pincode.ValidatePINEntry();
                pincode.PasswordCorrectDelegate = () => xpanel.StringInput[1].StringValue = "Password Correct";
                pincode.PasswordIncorrectDelegate = () => xpanel.StringInput[1].StringValue = "Password Incorrect";
            }
            catch (Exception e)
            {
                ErrorLog.Error(">>> Error in InitializeSystem: {0}", e.Message);
            }
        }

        /// <summary>
        /// SmartObject signal event callback
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        void ui_SmartObjectChangeEvent(uint id, SmartObjectEventArgs args)
        {
            switch (args.SmartObjectArgs.ID)
            {
                case 1: //SmartObject id: 1
                {
                    if (args.Sig.BoolValue)
                    {
                        try
                        {
                            if (pincode != null)
                                pincode.PincodeEntry(args.Sig.Name);
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error(">>> Error in ui_SmartObjectChangeEvent: {0}", e.Message);
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Serial signal event callback
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        void ui_SerialChangeEvent(uint id, SigEventArgs args)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Analog signal event callback
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        void ui_AnalogChangeEvent(uint id, SigEventArgs args)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Digital signal event callback
        /// </summary>
        /// <param name="id"></param>
        /// <param name="args"></param>
        void ui_DigitalChangeEvent(uint id, SigEventArgs args)
        {
            if (args.Sig.BoolValue)
            {
                switch (args.Sig.Number)
                {
                    case 200:
                    {
                        try
                        {
                            if (pincode != null)
                                pincode.Backspace();
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error(e.Message);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }
        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }
        }
    }
}