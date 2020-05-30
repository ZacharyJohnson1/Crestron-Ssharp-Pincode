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
    public class Pincode
    {
        /// <summary>
        /// Generic touchpanel object to link to Pincode instance
        /// </summary>
        private BasicTriList _ui { get; set; }

        /// <summary>
        /// Serial input join to display password text
        /// </summary>
        private uint _serialInputJoin { get; set; }

        /// <summary>
        /// Primary password
        /// </summary>
        private string _password { get; set; }

        /// <summary>
        /// Backdoor password
        /// </summary>
        private string _backdoorPassword { get; set; }

        /// <summary>
        /// Limits the number of digits in passowrd
        /// </summary>
        private ushort _pinLimit { get; set; }

        /// <summary>
        /// When enabled, the password text on the UI is displayed as 'startext'
        /// </summary>
        private bool _enableStarText { get; set; }

        /// <summary>
        /// When enabled, allows a backdoor password to access system
        /// </summary>
        private bool _enableBackdoor { get; set; }

        /// <summary>
        /// Text entered by user, used to compare to stored password(s)
        /// </summary>
        public string PINEntry { get; set; }

        /// <summary>
        /// Action delegate invoked when the correct password is entered
        /// A method outside of the Pincode class can be assigned to it to offer flexability/customization
        /// </summary>
        public Action PasswordCorrectDelegate;

        /// <summary>
        /// Action delegate invoked when an incorrect password is entered
        /// A method outside of the Pincode class can be assigned to it to offer flexability/customization
        /// </summary>
        public Action PasswordIncorrectDelegate;

        /// <summary>
        /// Action delegate invoked when the Misc_1 button of a SmartObject keypad is entered
        /// A method outside of the Pincode class can be assigned to it to offer flexability/customization
        /// </summary>
        public Action PasswordMiscOneDelegate;

        /// <summary>
        /// Action delegate invoked when the Misc_2 button of a SmartObject keypad is entered
        /// A method outside of the Pincode class can be assigned to it to offer flexability/customization
        /// </summary>
        public Action PasswordMiscTwoDelegate;


        /// <summary>
        /// Initializes an instance of the Pincode class with user-defined behavior and UI hooks
        /// </summary>
        /// <param name="ui">generic touchpanel object</param>
        /// <param name="serialInputJoin">join number for serial input to output password text</param>
        /// <param name="password">password of pincode instance</param>
        /// <param name="backdoorPassword">backdoor password of pincode instance</param>
        /// <param name="pinLimit">set limit for number of pin digits allowed</param>
        /// <param name="enableBackdoor">enables backdoor password</param>
        /// <param name="enableStarText">enables star text as output to touchpanel</param>
        public void Initialize(BasicTriList ui, uint serialInputJoin, string password)
        {
            _ui = ui;
            _serialInputJoin = serialInputJoin;
            _password = password;
            _backdoorPassword = string.Empty;
            _pinLimit = 4;
            PINEntry = string.Empty;
            _enableBackdoor = false;
            _enableStarText = false;
        }

        /// <summary>
        /// Enables and sets the backdoor passowrd
        /// </summary>
        /// <param name="backdoorPassword">backdoor password</param>
        public void EnableBackdoorPassword(string backdoorPassword)
        {
            _backdoorPassword = backdoorPassword;
            _enableBackdoor = true;
        }

        /// <summary>
        /// Disables backdoor password
        /// </summary>
        public void DisableBackdoor()
        {
            _backdoorPassword = string.Empty;
            _enableBackdoor = false;
        }

        /// <summary>
        /// Enables star text as password output
        /// </summary>
        public void EnableStarText()
        {
            _enableStarText = true;
        }

        /// <summary>
        /// Disables star text as password output
        /// </summary>
        public void DisableStarText()
        {
            _enableStarText = false;
        }

        /// <summary>
        /// Sets the limit to the amount of digits a PIN can be
        /// </summary>
        /// <param name="pinLimit"></param>
        public void SetPinLimit(ushort pinLimit)
        {
            _pinLimit = pinLimit;
        }

        /// <summary>
        /// Method called when digit is entered on the ui
        /// checks if PINEntry matches password(s)
        /// </summary>
        /// <param name="input">digit 0-9 or one of two custom button names</param>
        public void PincodeEntry(string input)
        {

            if (input.Equals("Misc_1"))
            {
                if (PasswordMiscOneDelegate != null)
                {
                    Action passwordMiscOneDel = PasswordMiscOneDelegate;
                    passwordMiscOneDel.Invoke();
                }
                else
                {
                    LogError(">>> PasswordMiscOneDelegate is null. Assign a method to this delegate before use.");
                }

            }
            else if (input.Equals("Misc_2"))
            {
                if (PasswordMiscTwoDelegate != null)
                {
                    Action passwordMiscTwoDel = PasswordMiscTwoDelegate;
                    passwordMiscTwoDel.Invoke();
                }
                else
                {
                    LogError(">>> PasswordMiscOneDelegate is null. Assign a method to this delegate before use.");
                }
            }
            else if (PINEntry.Length < _pinLimit)
            {
                PINEntry += input;
                SetPINText(PINEntry);
            }
            else
            {
                CrestronConsole.PrintLine(">>> PIN limit exceeded");
            }
        }


        /// <summary>
        /// compares the entered password (PINEntry) with the Pincode instance's password(s)
        /// invokes Action delegates if password is correct/incorrect
        /// </summary>
        public void ValidatePINEntry()
        {
            bool passwordCorrect = PincodeCompare();
            ClearText();

            if (passwordCorrect && PasswordCorrectDelegate != null)
            {
                Action passwordCorrectDel = PasswordCorrectDelegate;
                passwordCorrectDel.Invoke();
            }
            else if (!passwordCorrect && PasswordIncorrectDelegate != null)
            {
                Action passwordIncorrectDel = PasswordIncorrectDelegate;
                passwordIncorrectDel.Invoke();
            }
        }

        /// <summary>
        /// Compare PINEntry to _password
        /// </summary>
        public bool PincodeCompare()
        {
            return (PINEntry.Equals(_password) || (_enableBackdoor && PINEntry.Equals(_backdoorPassword))) ? true : false;
        }

        /// <summary>
        /// Clears password text on touchpanel and PINEntry
        /// </summary>
        /// <returns></returns>
        public void ClearText()
        {
            PINEntry = string.Empty;
            if (_ui != null)
                _ui.StringInput[_serialInputJoin].StringValue = PINEntry;
            else
                LogError(">>> {0} is null. Assign a valid BasicTriList type to _ui in the Initalize method.");
        }

        /// <summary>
        /// Delete the last digit entered
        /// </summary>
        public void Backspace()
        {
            if (PINEntry.Length > 0)
            {
                PINEntry = PINEntry.Remove(PINEntry.Length - 1, 1);
                SetPINText(PINEntry);
            }
        }

        /// <summary>
        /// If starText is enabled, the password output to the touchpanel will be displayed
        /// as '*''s instead of digits
        /// </summary>
        private void GenerateStarText()
        {
            var starText = string.Empty;
            foreach (var ch in PINEntry)
                starText += "*";
            if(_ui != null)
                _ui.StringInput[_serialInputJoin].StringValue = starText;
            else
                LogError(">>> {0} is null. Assign a valid BasicTriList type to _ui in the Initalize method.");
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadPasswordFromFile()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void WritePasswordToFile()
        {

        }

        /// <summary>
        /// Sets the password text
        /// </summary>
        private void SetPINText(string text)
        {
            if (_enableStarText)
                GenerateStarText();
            else
            {
                if(_ui != null)
                    _ui.StringInput[_serialInputJoin].StringValue = text;
                else
                    LogError(">>> {0} is null. Assign a valid BasicTriList type to _ui in the Initalize method.");
            }
        }

        /// <summary>
        /// Prints error message to console and logs error in ErrorLog
        /// </summary>
        /// <param name="msg"></param>
        private void LogError(string msg)
        {
            CrestronConsole.PrintLine(msg);
            ErrorLog.Error(msg);
        }
    }
}