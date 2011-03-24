﻿#region Header

#region Project
/*
 * Project     :  LogViewer
 * Description :  Viewer for Log4Net XML logs (see About box for log4Net configuration).
 * Version     :  2.7 
 * Modified on :  1.0 15-3-2010 
 *                2.1 May 2010 OD
 *                2.6 26-jun-2010 OD - add quick filter on symbols, cancel filter on filter zoom/text symbol  
 *                2.7 --Jul-2010 OD - save window size, split position. Reset split.
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

#region usings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Interop;
using System.Reflection;
using WPF.Themes;
using log4net;
using log4net.Config;

#endregion


namespace LogViewer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : INotifyPropertyChanged
    {


        private string fileName = string.Empty;
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        private string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                RecentFileList.InsertFile(value);
                notifyPropertyChanged("FileName");
            }
        }

        private List<LogEntry> entries = new List<LogEntry>();
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>The entries.</value>
        public List<LogEntry> Entries
        {
            get { return entries; }
            set
            {
                entries = value;
                notifyPropertyChanged("Entries");
            }
        }

        public Window1()
        {
            //Enabling Log4Net logging
            XmlConfigurator.Configure();
            if (log.IsInfoEnabled) log.Info("Application [Log4Viewer] Start");
            if (log.IsDebugEnabled) log.Debug("Application running in Debug mode");
            log.Info("Initialzing windows components");
            InitializeComponent();
            Loaded += Window1_Loaded;
            log.Info("Setting default width to " + Properties.Settings.Default.AppWidth.ToString());
            Width = Properties.Settings.Default.AppWidth;
            log.Info("Setting default height to " + Properties.Settings.Default.AppHeight.ToString());
            Height = Properties.Settings.Default.AppHeight;
            log.Info("Setting default Grid Height to " + Properties.Settings.Default.Split.ToString());
            MainGrid.RowDefinitions[0].Height = new GridLength(Properties.Settings.Default.Split);
            
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("Setting the UI Culture to fr-FR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            log.Info("Setting Max Width to " + SystemParameters.PrimaryScreenWidth.ToString());
            MaxWidth = SystemParameters.PrimaryScreenWidth;
            log.Info("Initializing event hanlder for listview control");
            listView1.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ListView1_HeaderClicked));
            log.Info("Setting the RecentFileList to" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToString()));
            RecentFileList.UseXmlPersister(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YourLog4NetViewer"));
            //RecentFileList.UseXmlPersister(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "EnaxosLogViewer.filehistory.xml"));
            log.Info("Initializing the RecentFiles menu click");
            RecentFileList.MenuClick += (s, ee) => OpenFile(ee.Filepath);
            log.Info("Initializing Error Bitmap");
            imageError.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty, null);
            log.Info("Initializing Info Bitmap");
            imageInfo.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Information.Handle, Int32Rect.Empty, null);
            log.Info("Initializing Warn  Bitmap");
            imageWarn.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Warning.Handle, Int32Rect.Empty, null);
            log.Info("Initializing Debug Bitmap");
            imageDebug.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle, Int32Rect.Empty, null);
            Title = string.Format(Properties.Resources.WindowTitle, Assembly.GetExecutingAssembly().GetName().Version);
            log.Info("Setting the title as " + Title.ToString());
            log.Info("Applying ExpressionDark Theme");
            this.ApplyTheme("ExpressionDark");
            log.Info("Setting the GridView widths for each column");
            foreach (var gvc in GridView1.Columns)
            {
                gvc.Width = gvc.ActualWidth;
                gvc.Width = Double.NaN;
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        private void Clear()
        {
            log.Info("Clearing all the text controls");
            textBoxLevel.Text = string.Empty;
            textBoxTimeStamp.Text = string.Empty;
            textBoxMachineName.Text = string.Empty;
            textBoxThread.Text = string.Empty;
            textBoxItem.Text = string.Empty;
            textBoxHostName.Text = string.Empty;
            textBoxUserName.Text = string.Empty;
            textBoxApp.Text = string.Empty;
            textBoxClass.Text = string.Empty;
            textBoxMethod.Text = string.Empty;
            textBoxLine.Text = string.Empty;
            textBoxfile.Text = string.Empty;
            textBoxMessage.Text = string.Empty;
            textBoxThrowable.Text = string.Empty;
            textBoxLog.Text = string.Empty;
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="logFileName">Name of the file.</param>
        private void OpenFile(string logFileName)
        {
            log.Info("Clearing Merged Files");
            mergedFiles.Clear();
            log.Info("Loading file " + logFileName);
            LoadFile(logFileName);
        }

        private readonly List<string> mergedFiles = new List<string>();

        /// <summary>
        /// Loads the file.
        /// </summary>
        private void LoadFile(string logFileName, bool withMerge = false)
        {
            if (!withMerge)
            {
                log.Info("Clearing entries to load single log file");
                entries.Clear();
                log.Info("Notifying Entries property as changed");
                notifyPropertyChanged("Entries");
                log.Info("Resetting the Listview item source to nothing");
                listView1.ItemsSource = null;
                FileName = logFileName;
            }
            else
            {
                log.Info("Adding the log files that need to be merged to mergedFile object");
                if (mergedFiles.Count == 0) mergedFiles.Add(FileName);
                log.Info("If the same file is being added then return immediately");
                if (mergedFiles.Contains(logFileName)) return;
                log.Info("If not add the file to the merged list");
                mergedFiles.Add(logFileName);
            }

            log.Info("Clearing the log filter");
            logFilter.Clear();
            log.Info("Turning off the IsFiltered property of the FilterIndicator");
            FilterIndicator.IsFiltered = false;

            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var iIndex = 1;
            if (withMerge) iIndex = entries.Count + 1;

            Clear();

            try
            {
                log.Info("Initializing FileStream objet to open the log file");
                var oFileStream = new FileStream(logFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
                log.Info("Initializing a Stream Reader");
                var oStreamReader = new StreamReader(oFileStream);
                log.Info("Read all the contents in the log file to a StreamReader");
                var sBuffer = string.Format("<root>{0}</root>", oStreamReader.ReadToEnd());
                log.Info("Closing StreamReader and FileStream object");
                oStreamReader.Close();
                oFileStream.Close();

                #region Read File Buffer
                ////////////////////////////////////////////////////////////////////////////////
                log.Info("Reading File");
                var oStringReader = new StringReader(sBuffer);
                var oXmlTextReader = new XmlTextReader(oStringReader) { Namespaces = false };
                log.Info("Start reading the log file");
                while (oXmlTextReader.Read())
                {
                    if ((oXmlTextReader.NodeType != XmlNodeType.Element) || (oXmlTextReader.Name != "log4j:event"))
                        continue;
                    var logentry = new LogEntry { Item = iIndex };

                    var dSeconds = Convert.ToDouble(oXmlTextReader.GetAttribute("timestamp"));
                    logentry.TimeStamp = dt.AddMilliseconds(dSeconds).ToLocalTime();
                    logentry.Thread = oXmlTextReader.GetAttribute("thread");
                    logentry.LogFile = logFileName;

                    #region get level
                    ////////////////////////////////////////////////////////////////////////////////
                    logentry.Level = oXmlTextReader.GetAttribute("level");
                    switch (logentry.Level)
                    {
                        case "ERROR":
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Error);
                                break;
                            }
                        case "INFO":
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Info);
                                break;
                            }
                        case "DEBUG":
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Debug);
                                break;
                            }
                        case "WARN":
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Warn);
                                break;
                            }
                        case "FATAL":
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Fatal);
                                break;
                            }
                        default:
                            {
                                logentry.Image = LogEntry.Images(LogImageType.Custom);
                                break;
                            }
                    }
                    ////////////////////////////////////////////////////////////////////////////////
                    #endregion
                    
                    #region read xml
                    ////////////////////////////////////////////////////////////////////////////////
                    while (oXmlTextReader.Read())
                    {
                        var breakLoop = false;
                        switch (oXmlTextReader.Name)
                        {
                            case "log4j:event":
                                breakLoop = true;
                                break;
                            default:
                                switch (oXmlTextReader.Name)
                                {
                                    case ("log4j:message"):
                                        {
                                            logentry.Message = oXmlTextReader.ReadString();
                                            break;
                                        }
                                    case ("log4j:data"):
                                        {
                                            switch (oXmlTextReader.GetAttribute("name"))
                                            {
                                                case ("log4jmachinename"):
                                                    {
                                                        logentry.MachineName = oXmlTextReader.GetAttribute("value");
                                                        break;
                                                    }
                                                case ("log4net:HostName"):
                                                    {
                                                        logentry.HostName = oXmlTextReader.GetAttribute("value");
                                                        break;
                                                    }
                                                case ("log4net:UserName"):
                                                    {
                                                        logentry.UserName = oXmlTextReader.GetAttribute("value");
                                                        break;
                                                    }
                                                case ("log4japp"):
                                                    {
                                                        logentry.App = oXmlTextReader.GetAttribute("value");
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case ("log4j:throwable"):
                                        {
                                            logentry.Throwable = oXmlTextReader.ReadString();
                                            break;
                                        }
                                    case ("log4j:locationInfo"):
                                        {
                                            logentry.Class = oXmlTextReader.GetAttribute("class");
                                            logentry.Method = oXmlTextReader.GetAttribute("method");
                                            logentry.File = oXmlTextReader.GetAttribute("file");
                                            logentry.Line = oXmlTextReader.GetAttribute("line");
                                            break;
                                        }
                                }
                                break;
                        }
                        if (breakLoop) break;
                    }
                    ////////////////////////////////////////////////////////////////////////////////
                    #endregion

                    entries.Add(logentry);
                    iIndex++;
                }
                notifyPropertyChanged("Entries");
                log.Info("Completing of Log xml reading");
                ////////////////////////////////////////////////////////////////////////////////
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            #region Show Counts
            ////////////////////////////////////////////////////////////////////////////////
            log.Info("Calculating the number of ERROR in the log file");
            var errorCount =
                (
                    from entry in Entries
                    where entry.Level == "ERROR"
                    select entry
                ).Count();

            if (errorCount > 0)
            {
                labelErrorCount.Text = string.Format("{0:#,#}  ", errorCount);
                labelErrorCount.Visibility = Visibility.Visible;
                imageError.Visibility = Visibility.Visible;
            }
            else
            {
                labelErrorCount.Visibility = Visibility.Hidden;
                imageError.Visibility = Visibility.Hidden;
            }
            log.Info("ERROR count is " + errorCount.ToString());
            log.Info("Calculating the number of INFO in the log file");
            var infoCount =
                (
                    from entry in Entries
                    where entry.Level == "INFO"
                    select entry
                ).Count();

            if (infoCount > 0)
            {
                labelInfoCount.Text = string.Format("{0:#,#}  ", infoCount);
                labelInfoCount.Visibility = Visibility.Visible;
                imageInfo.Visibility = Visibility.Visible;
            }
            else
            {
                labelInfoCount.Visibility = Visibility.Hidden;
                imageInfo.Visibility = Visibility.Hidden;
            }
            log.Info("INFO count is " + infoCount.ToString());
            log.Info("Calculating the number of WARN in the log file");
            var warnCount =
                (
                    from entry in Entries
                    where entry.Level == "WARN"
                    select entry
                ).Count();

            if (warnCount > 0)
            {
                labelWarnCount.Text = string.Format("{0:#,#}  ", warnCount);
                labelWarnCount.Visibility = Visibility.Visible;
                imageWarn.Visibility = Visibility.Visible;
            }
            else
            {
                labelWarnCount.Visibility = Visibility.Hidden;
                imageWarn.Visibility = Visibility.Hidden;
            }
            log.Info("WARN count is " + warnCount.ToString());
            log.Info("Calculating the number of DEBUG in the log file");
            var debugCount =
                (
                    from entry in Entries
                    where entry.Level == "DEBUG"
                    select entry
                ).Count();

            if (debugCount > 0)
            {
                labelDebugCount.Text = string.Format("{0:#,#}  ", debugCount);
                labelDebugCount.Visibility = Visibility.Visible;
                imageDebug.Visibility = Visibility.Visible;
            }
            else
            {
                imageDebug.Visibility = Visibility.Hidden;
                labelDebugCount.Visibility = Visibility.Hidden;
            }
            log.Info("DEBUG count is " + debugCount.ToString());
            tbFiltered.Text = Entries.Count().ToString();
            FilterIndicator.IsFiltered = false;

            ////////////////////////////////////////////////////////////////////////////////
            #endregion
            log.Info("Initializing Listview to show the log entries");
            listView1.ItemsSource = null;
            log.Info("Loading Listview with Log Entries");
            listView1.ItemsSource = (from e in entries orderby e.TimeStamp select e).ToList();
            log.Info("Clearing Sort Adorner");
            clearSortAdorner();

            if (!withMerge)
            {
                textboxFileName.Text = logFileName;
                return;
            }

            var s = "";
            foreach (var sm in mergedFiles)
            {
                if (s != "") s += "; ";
                s += Path.GetFileName(sm);
            }

            textboxFileName.Text = s;
        }

        #region ListView Events
        ////////////////////////////////////////////////////////////////////////////////
        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            log.Info("Inside ListView selection changed and clearing the text controls");
            Clear();
            log.Info("Getting the current Log Entry");
            var logentry = listView1.SelectedItem as LogEntry;

            if (logentry == null) return;
            log.Info("Show the selected log entry on the UI");
            image1.Source = logentry.Image;
            textBoxLevel.Text = logentry.Level;
            textBoxTimeStamp.Text = logentry.TimeStamp.ToString(Properties.Resources.DisplayDatetimeFormat);
            textBoxMachineName.Text = logentry.MachineName;
            textBoxThread.Text = logentry.Thread;
            textBoxItem.Text = logentry.Item.ToString();
            textBoxHostName.Text = logentry.HostName;
            textBoxUserName.Text = logentry.UserName;
            textBoxApp.Text = logentry.App;
            textBoxClass.Text = logentry.Class;
            textBoxMethod.Text = logentry.Method;
            textBoxLine.Text = logentry.Line;
            textBoxLog.Text = logentry.LogFile;
            textBoxMessage.Text = logentry.Message;
            textBoxThrowable.Text = logentry.Throwable;
            textBoxfile.Text = logentry.File;
        }

        private ListSortDirection direction = ListSortDirection.Descending;
        private GridViewColumnHeader curSortCol = null;
        private SortAdorner curAdorner = null;


        /// <summary>
        /// Handles the HeaderClicked event of the ListView1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ListView1_HeaderClicked(object sender, RoutedEventArgs e)
        {
            log.Info("Handles the HeaderClicked event of the ListView control");
            var header = e.OriginalSource as GridViewColumnHeader;
            var source = e.Source as ListView;
            if (source == null) return;
            var dataView = CollectionViewSource.GetDefaultView(source.ItemsSource);
            if (dataView == null) return;
            log.Info("Clearing all Sort Descriptions");
            dataView.SortDescriptions.Clear();
            log.Info("Determining the Sort direction");
            direction = direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (header != null && header.Content!=null)
            {
                var description = new SortDescription(header.Content.ToString(), direction);
                log.Info("Adding sort description from header content");
                dataView.SortDescriptions.Add(description);
            }
            log.Info("Refreshing the data view");
            dataView.Refresh();
            log.Info("Clearing the Sort Adorner");
            clearSortAdorner();
            curSortCol = header;
            curAdorner = new SortAdorner(curSortCol, direction);
            if (curSortCol != null) AdornerLayer.GetAdornerLayer(curSortCol).Add(curAdorner);
        }

        private void clearSortAdorner()
        {
            if (curSortCol != null)
            {
                log.Info("Getting current column and remove the existing sort adorner");
                AdornerLayer.GetAdornerLayer(curSortCol).Remove(curAdorner);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        #endregion

        #region DragDrop
        ////////////////////////////////////////////////////////////////////////////////
        private void listView1_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    log.Info("Getting  file drop data");
                    var a = (Array)e.Data.GetData(DataFormats.FileDrop);
                    log.Info("Getting the Keyboard modifiers for validation");
                    var b = (Keyboard.Modifiers == ModifierKeys.Alt) || (a.Length > 1);
                    log.Info("Checking if dragdrop has data");
                    if (a != null)
                    {
                        log.Info("Iterating thorugh the array of dragdrop data");
                        foreach (var file in a)
                        {
                            var f = file.ToString(); // a.GetValue(0).ToString();
                            var bb = b;
                            Dispatcher.BeginInvoke(
                                DispatcherPriority.Background,
                                (Action)(() => LoadFile(f, bb)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in Drag Drop",ex);
                    MessageBox.Show("Error in Drag Drop: " + ex.Message);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        #endregion

        #region Menu Events
        ////////////////////////////////////////////////////////////////////////////////
        private void doMenuFileOpen()
        {
            log.Info("Initializing the open file dialog object");
            var oOpenFileDialog = new System.Windows.Forms.OpenFileDialog
                                      {
                                          Filter = Properties.Resources.XmlOpenFilter,
                                          DefaultExt = "xml",
                                          Multiselect = true,
                                          Title = Properties.Resources.OpenDialogTitle
                                      };
            if (oOpenFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            FileName = oOpenFileDialog.FileName;
            log.Info("FileName is " + FileName);
            for (var i = 0; i < oOpenFileDialog.FileNames.Length; i++)
            {
                if (log.IsDebugEnabled)
                {
                    log.Info("Loading file " + oOpenFileDialog.FileNames[i].ToString());
                }
                LoadFile(oOpenFileDialog.FileNames[i], i > 0);
            }
        }

        private void MergeFile_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Initializing the open file dialog for filer merge click event");
            var oOpenFileDialog = new System.Windows.Forms.OpenFileDialog
                                      {
                                          Filter = Properties.Resources.XmlOpenFilter,
                                          DefaultExt = "xml",
                                          Multiselect = true,
                                          Title = Properties.Resources.MergeOpenDialog
                                      };
            if (oOpenFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            foreach (var t in oOpenFileDialog.FileNames)
            {
                if (log.IsDebugEnabled)
                {
                    log.Info("Loading file " + t.ToString());
                    LoadFile(t, true);
                }
            }
        }

        private void doMenuRefresh()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                log.Info("Unable to refersh the empty source");
                MessageBox.Show(Properties.Resources.CantRefreshEmptySource);
                return;
            }
            if (mergedFiles.Count > 0)
            {
                log.Info("There is no refresh when multiple files are open");
                MessageBox.Show(Properties.Resources.NoRefreshOnMultipleSource);
                return;
            }
            log.Info("Loading file " + FileName);
            LoadFile(FileName);
            listView1.SelectedIndex = listView1.Items.Count - 1;
            if (listView1.Items.Count > 4)
            {
                listView1.SelectedIndex -= 3;
            }
            listView1.ScrollIntoView(listView1.SelectedItem);
            var lvi = listView1.ItemContainerGenerator.ContainerFromIndex(listView1.SelectedIndex) as ListViewItem;
            if (lvi == null) return;
            lvi.BringIntoView();
            lvi.Focus();
            FilterIndicator.IsFiltered = false;
            tbFiltered.Text = Entries.Count().ToString();
        }

        private static void doMenuAbout()
        {
            log.Info("Initializing about screen");
            var about = new About();
            log.Info("Displaying/Showing about screen");
            about.ShowDialog();
        }


        private LogFilter logFilter = new LogFilter();



        private void doMenuFilter()
        {
            log.Info("Cloning the log filter");
            var tempFilter = logFilter.Clone();
            var filter = new Filter(Entries, tempFilter) { Owner = this };
            log.Info("Showing the filter dialog");
            filter.ShowDialog();
            if (filter.DialogResult != true) return;
            log.Info("Cloning the log filter again after loading new filters");
            logFilter = tempFilter.Clone();
            logFilter.TrimAll();

            log.Info("Initialzing the query object");
            var query = (from e in Entries select e);

            if (!string.IsNullOrWhiteSpace(logFilter.App))
                query = query.Where(e => e.App.ToUpperInvariant().Contains(logFilter.App.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.Level)) query = query.Where(e => e.Level == logFilter.Level);
            if (!string.IsNullOrWhiteSpace(logFilter.Thread)) query = query.Where(e => e.Thread == logFilter.Thread);
            if (!string.IsNullOrWhiteSpace(logFilter.Message))
                query = query.Where(e => e.Message.ToUpperInvariant().Contains(logFilter.Message.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.MachineName))
                query = query.Where(e => e.MachineName.ToUpperInvariant().Contains(logFilter.MachineName.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.UserName))
                query = query.Where(e => e.UserName.ToUpperInvariant().Contains(logFilter.UserName.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.HostName))
                query = query.Where(e => e.HostName.ToUpperInvariant().Contains(logFilter.HostName.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.Throwable))
                query = query.Where(e => e.Throwable.ToUpperInvariant().Contains(logFilter.Throwable.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.Class))
                query = query.Where(e => e.Class.ToUpperInvariant().Contains(logFilter.Class.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.Method))
                query = query.Where(e => e.Method.ToUpperInvariant().Contains(logFilter.Method.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.File))
                query = query.Where(e => e.File.ToUpperInvariant().Contains(logFilter.File.ToUpperInvariant()));
            if (!string.IsNullOrWhiteSpace(logFilter.LogName))
                query = query.Where(e => e.LogFile.ToUpperInvariant().Contains(logFilter.LogName.ToUpperInvariant()));


            var c = query.Count();
            log.Info("Receieved queries " + query.Count().ToString());
            FilterIndicator.IsFiltered = c > 0 && (c != Entries.Count());
            logFilter.IsFiltered = FilterIndicator.IsFiltered;
            log.Info("Log Filter status " + logFilter.IsFiltered.ToString());
            LogFilter.FilteredEntries = FilterIndicator.IsFiltered ? query.ToList() : Entries;
            listView1.ItemsSource = logFilter.IsFiltered ? LogFilter.FilteredEntries : Entries;
            tbFiltered.Text = FilterIndicator.IsFiltered ? c + "/" + Entries.Count() : Entries.Count().ToString();
        }

        private void CancelFilter()
        {
            log.Info("Clearing Log Filter");
            logFilter.Clear();
            FilterIndicator.IsFiltered = false;
            LogFilter.FilteredEntries = Entries;
            listView1.ItemsSource = entries;
            tbFiltered.Text = entries.Count().ToString();
        }

        private void FilterIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!FilterIndicator.IsFiltered) return;
            CancelFilter();
        }

        private enum MessageLevel { Error, Warning, Debug, Info }

        private void QuickFilter(MessageLevel level)
        {
            string s;
            log.Info("Using quick filter for " + level.ToString());
            switch (level)
            {
                case MessageLevel.Error:
                    s = "ERROR";
                    break;
                case MessageLevel.Warning:
                    s = "WARN";
                    break;
                case MessageLevel.Debug:
                    s = "DEBUG";
                    break;
                case MessageLevel.Info:
                    s = "INFO";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
            logFilter.Clear();
            logFilter.Level = s;
            logFilter.TrimAll();

            log.Info("Initializing the query objet for " + level.ToString());
            var query = (from e in Entries select e);
            if (!string.IsNullOrWhiteSpace(logFilter.Level)) query = query.Where(e => e.Level == logFilter.Level);

            var c = query.Count();
            FilterIndicator.IsFiltered = c > 0 && (c != Entries.Count());
            logFilter.IsFiltered = FilterIndicator.IsFiltered;
            LogFilter.FilteredEntries = FilterIndicator.IsFiltered ? query.ToList() : Entries;
            listView1.ItemsSource = logFilter.IsFiltered ? LogFilter.FilteredEntries : Entries;
            tbFiltered.Text = FilterIndicator.IsFiltered ? c + "/" + Entries.Count() : Entries.Count().ToString();

        }

        private void imageError_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.Info("Enabling Quick filter for ERROR message level");
            QuickFilter(MessageLevel.Error);
        }

        private void imageInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.Info("Enabling Quick filter for INFO message level");
            QuickFilter(MessageLevel.Info);
        }

        private void imageWarn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.Info("Enabling Quick filter for WARN message level");
            QuickFilter(MessageLevel.Warning);
        }

        private void imageDebug_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            log.Info("Enabling Quick filter for DEBUG message level");
            QuickFilter(MessageLevel.Debug);
        }

        private int currentIndex;
        private void find(int loopDirection)
        {
            if (textBoxFind.Text.Length <= 0) return;
            log.Info("Finding in Loop Direction " + loopDirection);
            if (loopDirection == 0)
            {
                for (var i = currentIndex + 1; i < listView1.Items.Count; i++)
                {
                    var item = (LogEntry)listView1.Items[i];
                    if (!item.Message.ToUpper().Contains(textBoxFind.Text.ToUpper())) continue;
                    listView1.SelectedIndex = i;
                    listView1.ScrollIntoView(listView1.SelectedItem);
                    var lvi = listView1.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (lvi != null)
                    {
                        lvi.BringIntoView();
                        lvi.Focus();
                    }
                    currentIndex = i;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }
            else
            {
                for (var i = currentIndex - 1; i > 0 && i < listView1.Items.Count; i--)
                {
                    var item = (LogEntry)listView1.Items[i];
                    if (!item.Message.ToUpper().Contains(textBoxFind.Text.ToUpper())) continue;
                    listView1.SelectedIndex = i;
                    listView1.ScrollIntoView(listView1.SelectedItem);
                    var lvi = listView1.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (lvi != null)
                    {
                        lvi.BringIntoView();
                        lvi.Focus();
                    }
                    currentIndex = i;
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void buttonFindNext_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Triggering find next");
            find(0);
        }

        private void buttonFindPrevious_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Triggering find previous");
            find(1);
        }

        private void textBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (textBoxFind.Text.Length > 0)
            {
                find(0);
            }
        }

        private void textBoxFind_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                find(0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        #endregion



        public bool CanMerge
        {
            get { return entries != null && entries.Count() > 0; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void notifyPropertyChanged(string property)
        {
            if (property == "Entries") notifyPropertyChanged("CanMerge");
            if (PropertyChanged == null) return;
            var p = PropertyChanged;
            p(this, new PropertyChangedEventArgs(property));
        }

        private void label6b_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LogEntry.JustFileName = !LogEntry.JustFileName;
            log.Info("logentry file name on label6b mouse double click " + LogEntry.JustFileName.ToString());
            listView1_SelectionChanged(this, null);
        }

        #region commands

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            log.Info("Executing menu file open");
            doMenuFileOpen();
        }

        private void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void RefreshCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(string.IsNullOrWhiteSpace(FileName) || (mergedFiles.Count > 0));
            log.Info("Can Refresh execute " + e.CanExecute.ToString() );
        }

        private void RefreshExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            log.Info("Execute menu refresh");
            doMenuRefresh();
        }

        private void FilterCanExexute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrWhiteSpace(FileName) || mergedFiles.Count > 0;
            log.Info("Can filter execute " + e.CanExecute.ToString());
        }

        private void FilterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            log.Info("Do menu filter");
            doMenuFilter();
        }

        private void AboutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            log.Info("Executing about menu");
            doMenuAbout();
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            log.Info("Saving default app width,height,split to open next time");
            Properties.Settings.Default.AppWidth = Width;
            Properties.Settings.Default.AppHeight = Height;
            Properties.Settings.Default.Split = MainGrid.RowDefinitions[0].Height.Value;
            Properties.Settings.Default.Save();
        }

        private void ResetSeparator_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Defining the heigh when reset reset separator is clicked");
            MainGrid.RowDefinitions[0].Height = new GridLength(ActualHeight / 3);
        }
     }
}
