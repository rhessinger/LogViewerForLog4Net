#region Header

/*
 * Customer    :  eurotvs
 * Project     :  LogViewer
 * Description :  Viewer for XML logs from Rafale and consoles
 * Version     :  1.0 beta 
 * Modified on :  1.0 15-3-2010
 *                
 *
 * Copyright 2008 Olivier Dahan - www.e-naxos.com
 *
 */

// 
// www.e-naxos.com
//  Warning : software guarantee is limited in time, all defaults must be detected by the customer during the test period as defined by the contract. 
//            Modifying any file will cancelled the software guarantee, being the contract or the legal one.
//  Attention : la garantie du logiciel est limitée dans le temps. Tous les défauts doivent être détectés par le client durant la période de test définie par le contrat.
//              Modifier tout fichier de l'ensemble fourni annule immédiatement la garantie légale ainsi que la garantie contractuelle.

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
        private static readonly Dictionary<LogImageType, BitmapSource> imageList =
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
            return imageList[type];
        }

        public int Item { get; set; }

        private DateTime timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        /// <value>The time stamp.</value>
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        private BitmapSource image = imageList[LogImageType.Custom];
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public BitmapSource Image
        {
            get { return image; }
            set { image = value; }
        }

        private string level = string.Empty;
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public string Level
        {
            get { return level; }
            set { level = value; }
        }

        private string thread = string.Empty;
        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        /// <value>The thread.</value>
        public string Thread
        {
            get { return thread; }
            set { thread = value; }
        }

        private string message = string.Empty;
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string machineName = string.Empty;
        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        /// <value>The name of the machine.</value>
        public string MachineName
        {
            get { return machineName; }
            set { machineName = value; }
        }

        private string userName = string.Empty;
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        private string hostName = string.Empty;
        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>The name of the host.</value>
        public string HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }

        private string app = string.Empty;
        /// <summary>
        /// Gets or sets the app.
        /// </summary>
        /// <value>The app.</value>
        public string App
        {
            get { return app; }
            set { app = value; }
        }

        private string throwable = string.Empty;
        /// <summary>
        /// Gets or sets the throwable.
        /// </summary>
        /// <value>The throwable.</value>
        public string Throwable
        {
            get { return throwable; }
            set { throwable = value; }
        }

        private string @class = string.Empty;
        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>The class.</value>
        public string Class
        {
            get { return @class; }
            set { @class = value; }
        }

        private string method = string.Empty;
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>The method.</value>
        public string Method
        {
            get { return method; }
            set { method = value; }
        }

        private string file = string.Empty;
        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>The file.</value>
        public string File
        {
            get { return file; }
            set { file = value; }
        }

        private string line = string.Empty;
        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>The line.</value>
        public string Line
        {
            get { return line; }
            set { line = value; }
        }

        private string logFile = string.Empty;

        /// <summary>
        /// Gets or sets the log file name (used in merge mode).
        /// </summary>
        /// <value>The log file.</value>
        public string LogFile
        {
            get { return justFileName ? Path.GetFileName(logFile) : logFile; }
            set { logFile = value; }
        }

        public string FullLogPath
        { get { return LogFile; } }

        public string ShortLogFile
        { get { return Path.GetFileName(LogFile); } }

        private static bool justFileName;

        /// <summary>
        /// Gets or sets a value indicating whether LogFile property is returing the full path of just the file name.
        /// </summary>
        /// <value><c>true</c> if [just file name]; otherwise, <c>false</c>.</value>
        public static bool JustFileName
        {
            get { return justFileName; }
            set { justFileName = value; }
        }

    }
}
