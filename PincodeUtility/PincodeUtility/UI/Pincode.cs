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
        /// 
        /// </summary>
        private BasicTriList _ui { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private uint _serialInputJoin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private string _password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private string _backdoorPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private ushort _pinLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private bool _enableStarText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private bool _enableBackdoor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PINEntry { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action PasswordCorrectDelegate;

        /// <summary>
        /// 
        /// </summary>
        public Action PasswordIncorrectDelegate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="backdoorPassword"></param>
        /// <param name="pinLimit"></param>
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
        /// 
        /// </summary>
        /// <param name="digit"></param>
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

                if(passwordCorrect && PasswordCorrectDelegate != null)
                    PasswordCorrectDelegate.Invoke();
                else if(!passwordCorrect && PasswordIncorrectDelegate != null)
                    PasswordIncorrectDelegate.Invoke();
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
        /// compare PINEntry to _password
        /// </summary>
        public bool PincodeCompare()
        {
            return (PINEntry.Equals(_password) || (_enableBackdoor && PINEntry.Equals(_backdoorPassword))) ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ClearText()
        {
            PINEntry = string.Empty;
            _ui.StringInput[_serialInputJoin].StringValue = PINEntry;
        }

        /// <summary>
        /// 
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
        /// 
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
        /// 
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