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
        private BasicTriList _ui { get; set; }
        private uint _serialInputJoin { get; set; }
        private string _password { get; set; }
        private string _backdoorPassword { get; set; }
        private ushort _pinLimit { get; set; }
        private bool _enableStarText { get; set; }
        private bool _enableBackdoor { get; set; }
        public string PINEntry { get; set; }

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
            _enableStarText = enableStarText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="digit"></param>
        public void PincodeEntry(string input)
        {
            if (PINEntry.Contains("Correct") || PINEntry.Contains("Incorrect") || input.Equals("Misc_1"))
                SetText(ClearText());

            if (PINEntry.Length < _pinLimit && input != "Misc_1" && input != "Misc_2")
            {
                PINEntry += input;
                if(_enableStarText)
                    GenerateStarText();
                else
                    SetText(PINEntry);
            }
            else if (input.Equals("Misc_2"))
            {
                PINEntry = PincodeCompare() ? "Correct" : "Incorrect";
                SetText(PINEntry);
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
        public string ClearText()
        {
            PINEntry = string.Empty;
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Backspace()
        {
            PINEntry.Remove(PINEntry.Length - 1, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GenerateStarText()
        {
            var starText = string.Empty;
            foreach (var ch in PINEntry)
                starText += "*";
            SetText(starText);
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
        /// <param name="text"></param>
        private void SetText(string text)
        {
            _ui.StringInput[_serialInputJoin].StringValue = text;
        }
    }
}