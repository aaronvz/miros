using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Desko.FullPage;

namespace SampleNet
{

    /// <summary>
    /// Event arguments representing an API operation result.
    /// </summary>
    public class EventArgsOperationResult : EventArgs
    {
        public PsaException Result;
        public long ElapsedMilliseconds;
    }

    /// <summary>
    /// Collection of helpers.
    /// </summary>
    public class Tools
    {

        /// <summary>
        /// Apply escape sequences for display.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        static public string ApplyEscape(string txt)
        {
            txt = txt.Replace("\\r", "\r");
            txt = txt.Replace("\\n", "\n");
            txt = txt.Replace("\\t", "\t");
            int i = 0;
            i = txt.IndexOf("\\x");

            string newTxt = "";

            var match = Regex.Match(txt, @"\\x([0-9a-fA-F][0-9a-fA-F])");

            int lastIndex = 0;
            while (match.Success)
            {
                newTxt += txt.Substring(lastIndex, match.Index - lastIndex);
                lastIndex = match.Index + match.Length;
                newTxt += (char)Convert.ToUInt32(match.Groups[1].Value, 16);
                match = match.NextMatch();
            }
            newTxt += txt.Substring(lastIndex);

            return newTxt;
        }

        /// <summary>
        /// Event handlers to be called on API operation result. 
        /// </summary>
        public static event EventHandler<EventArgsOperationResult> OnOperationResultAvailable;

        /// <summary>
        /// Delegate type for a save API call
        /// </summary>
        public delegate void SaveApiCall();

        /// <summary>
        /// Perform several API calls and provide default handling of exceptions.
        /// </summary>
        /// <param name="op">Operation with API calls.</param>
        public static void HandleApiExceptions(SaveApiCall op)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();

            EventArgsOperationResult eventArgs = new EventArgsOperationResult();
            try
            {
                op();
                eventArgs.Result = new PsaException(Result.Success, "");
            }
            catch (PsaException ex)
            {
                eventArgs.Result = ex;
            }
            catch (Desko.Mrz.MrzException ex)
            {
                eventArgs.Result = new PsaException(Result.Fail, "MRZ error (" + ex.Result.ToString() +"):" + ex.Message);
            }

            watch.Stop();
            eventArgs.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            if (OnOperationResultAvailable != null)
            {
                OnOperationResultAvailable(null, eventArgs);
            }
        }

        /// <summary>
        /// GUI operation delegate type.
        /// </summary>
        public delegate void GuiOperation();

        /// <summary>
        /// Pass operation to GUI thread if necessary.
        /// </summary>
        /// <param name="control">Calling control.</param>
        /// <param name="op">Operation to be performed by GUI thread.</param>
        static public void RunInGui(Control control, GuiOperation op)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(op);
            }
            else
            {
                op();
            }
        }

        /// <summary>
        /// Convert byte array to string by masking non-ASCII characters.
        /// </summary>
        /// <param name="data">Input byte array.</param>
        /// <returns>Masked string.</returns>
        static public string MaskNonAscii(byte[] data)
        {
            StringBuilder res = new StringBuilder();

            foreach (byte b in data)
            {
                if (b >= (byte)' ' && b <= (byte)'~' && b != (byte)'{')
                {
                    res.Append((char)b);
                }
                else
                {
                    res.Append(string.Format("[{0:X2}]", b));
                }
            }
            return res.ToString();
        }

    }
}
