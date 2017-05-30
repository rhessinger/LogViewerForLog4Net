#region Header

#region Project
/*
 * Project     :  LogViewer
 * Description :  Viewer for Log4Net XML logs (see About box for log4Net configuration).
 * Version     :  2.7 
 * Modified on :  1.0 15-3-2010 
 *                2.1 May 2010 OD
 *                2.6 26-jun-2010 OD - add quick filter on symbols, cancel filter on filter zoom/text symbol  
 *                2.7 --Jul-2010 OD - save window size, split position. Reset split.
 *                2.x -- Open Source Project on CodePlex
 *                
 *
 * Copyrights  : (c) 2010 Olivier Dahan for the enhanced version - www.e-naxos.com
 *               (c) 2009 Ken C. Len for original version
 *               
 * LogViewer is a free software distributed on CodePlex : http://yourlog4netviewer.codeplex.com/ under the Microsoft Reciprocal License (Ms-RL)
 *
 */
#endregion

#region Microsoft Reciprocal License (Ms-RL)
/* 
 * Microsoft Reciprocal License (Ms-RL)
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 3. Conditions and Limitations
 * (A) Reciprocal Grants- For any file you distribute that contains code from the software (in source code or binary format), you must provide recipients the source code to that file along with a copy of this license, which license will govern that file. You may license other files that are entirely your own work and do not contain code from the software under any terms you choose.
 * (B) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (C) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (D) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (E) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
 * 
 */
#endregion


#endregion

#region History

// 2010-26-03  OD      Some light corrections
// 2015-07-10  RH      Added 


#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;

namespace LogViewer
{
    /// <summary>
    /// A log entry
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        private static readonly Dictionary<LogImageType, BitmapSource> ImageList =
            new Dictionary<LogImageType, BitmapSource>
                {
                {LogImageType.Debug, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle, Int32Rect.Empty, null)},
                {LogImageType.Error, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty, null)},
                {LogImageType.Fatal, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Hand.Handle, Int32Rect.Empty, null)},
                {LogImageType.Info, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle, Int32Rect.Empty, null)},
                {LogImageType.Warn, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle, Int32Rect.Empty, null)},
                {LogImageType.Custom, Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Asterisk.Handle, Int32Rect.Empty, null)}
            };

        /// <summary>
        /// Image for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static BitmapSource Images(LogImageType type)
        {
            return ImageList[type];
        }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>The item.</value>
        public int Item { get; set; }

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTime TimeStamp { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public BitmapSource Image { get; set; } = ImageList[LogImageType.Custom];

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        /// <value>The thread.</value>
        public string Thread { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        public string MachineName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the identity.
        /// </summary>
        /// <value>The name of the identity.</value>
        public string Identity { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the NDC.
        /// </summary>
        /// <value>The NDC.</value>
        public string Ndc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>The application.</value>
        public string App { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the throwable.
        /// </summary>
        /// <value>The throwable.</value>
        public string Throwable { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>The class.</value>
        public string Class { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public string File { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>The line.</value>
        public string Line { get; set; } = string.Empty;


        private string _logfile = string.Empty;
        /// <summary>
        /// Gets or sets the log file name (used in merge mode).
        /// </summary>
        /// <value>The log file.</value>
        public string LogFile
        {
            get => JustFileName ? Path.GetFileName(_logfile) : _logfile;
            set => _logfile = value;
        }

        /// <summary>
        /// Gets the full log path.
        /// </summary>
        /// <value>The full log path.</value>
        public string FullLogPath => LogFile;

        /// <summary>
        /// Gets the short log file.
        /// </summary>
        /// <value>The short log file.</value>
        public string ShortLogFile => Path.GetFileName(LogFile);

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="LogFile"/> property is returning the full path of just the file name.
        /// </summary>
        /// <value><c>true</c> if [just file name]; otherwise, <c>false</c>.</value>
        public static bool JustFileName { get; set; } = true;


        private string _logger;

        /// <summary>
        /// Gets or sets a value indicating the logger used
        /// </summary>
        /// <value>The logger.</value>
        public string Logger
        {
			get => _logger;
            set => _logger = value;
        }

    }
}
