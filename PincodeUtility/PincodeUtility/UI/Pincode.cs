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
        /// Can be assigned to a method outside of the Pincode class
        /// </summary>
        public Action PasswordCorrectDelegate;

        /// <summary>
        /// Action delegate invoked when an incorrect password is entered
        /// Can be assigned to a method outside of the Pincode class
        /// </summary>
        public Action PasswordIncorrectDelegate;


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
        public void Initialize(BasicTriList ui, uint serialInputJoin, string password, string backdoorPassword, ushort pinLimit, bool enableBackdoor, bool enableStarText)
        {
            _ui = ui;
            _serialInputJoin = serialInputJoin;
            _password = password;
            _backdoorPassword = backdoorPassword;
            _pinLimit = pinLimit;
            PINEntry = string.Empty;
            _enableBackdoor = enableBackdoor;
            _enableStarText = enableStarText;
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
                ClearText();
            }
            else if (input.Equals("Misc_2"))
            {
                bool passwordCorrect = PincodeCompare();
                ClearText();

                if (passwordCorrect && PasswordCorrectDelegate != null)
                {
                    var passwordCorrectDel = PasswordCorrectDelegate;
                    passwordCorrectDel.Invoke();
                }
                else if (!passwordCorrect && PasswordIncorrectDelegate != null)
                {
                    var passwordIncorrectDel = PasswordIncorrectDelegate;
                    passwordIncorrectDel.Invoke();
                }
            }
            else if (PINEntry.Length < _pinLimit)
            {
                PINEntry += input;
                SetPINText(PINEntry);
            }
            else
                CrestronConsole.PrintLine("PIN limit exceeded");
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
            _ui.StringInput[_serialInputJoin].StringValue = PINEntry;
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
            _ui.StringInput[_serialInputJoin].StringValue = starText;
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
                _ui.StringInput[_serialInputJoin].StringValue = text;
        }
    }
}