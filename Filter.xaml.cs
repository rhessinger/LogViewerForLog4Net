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
/* 06.01.2012   JB      Added support for NDC / Identity.
 *                      Fixed the Binding of the ComboBoxes so that manually-typed text is persisted to the LogFilter..
 *                      Fixed Clear button behavior (temp, until LogFilter implements INotifyPropertyChanged */
// 2015-07-10  RH      Added Logger

#endregion


using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter
    {
        private readonly LogFilter _logFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="logFilter">The log filter.</param>
        public Filter(IEnumerable<LogEntry> entries, LogFilter logFilter)
        {
            Opacity = 0d;
            InitializeComponent();
            populateDropDown(entries);
            this._logFilter = logFilter;
            MainGrid.DataContext = this._logFilter;
        }

        private void populateDropDown(IEnumerable<LogEntry> entries)
        {
            var logEntries = entries as LogEntry[] ?? entries.ToArray();
            comboBoxLevel.ItemsSource = (from e in logEntries select e.Level).Distinct().ToList();
            comboBoxUserName.ItemsSource = (from e in logEntries select e.UserName).Distinct().OrderBy(e => e).ToList();
            comboBoxThread.ItemsSource = (from e in logEntries select e.Thread).Distinct().OrderBy(e => e).ToList();
            comboBoxMachineName.ItemsSource = (from e in logEntries select e.MachineName).Distinct().OrderBy(e => e).ToList();
            comboBoxHostName.ItemsSource = (from e in logEntries select e.HostName).Distinct().OrderBy(e => e).ToList();
            comboBoxApplication.ItemsSource = (from e in logEntries select e.App).Distinct().OrderBy(e => e).ToList();
            comboBoxClass.ItemsSource = (from e in logEntries select e.Class).Distinct().OrderBy(e => e).ToList();
            comboBoxMethod.ItemsSource = (from e in logEntries select e.Method).Distinct().OrderBy(e => e).ToList();
            comboBoxFile.ItemsSource = (from e in logEntries select e.File).Distinct().OrderBy(e => e).ToList();
            comboLogFile.ItemsSource = (from e in logEntries select e.LogFile).Distinct().OrderBy(e => e).ToList();
            comboLogger.ItemsSource = (from e in logEntries select e.Logger).Distinct().OrderBy(e => e).ToList();

            //comboBoxNDC.ItemsSource = (from e in entries select e.NDC).Distinct().OrderBy(e => e).ToList();
            comboBoxIdentity.ItemsSource = (from e in logEntries select e.Identity).Distinct().OrderBy(e => e).ToList();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            _logFilter.Clear();

            this.textBoxMessage.Text = string.Empty;
            this.textBoxThrowable.Text = string.Empty;

            this.comboBoxNDC.Text = string.Empty;
            comboBoxIdentity.Text = string.Empty;
            comboBoxLevel.Text = string.Empty;
            comboBoxUserName.Text = string.Empty;
            comboBoxThread.Text = string.Empty;
            comboBoxMachineName.Text = string.Empty;
            comboBoxHostName.Text = string.Empty;
            comboBoxApplication.Text = string.Empty;
            comboBoxClass.Text = string.Empty;
            comboBoxMethod.Text = string.Empty;
            comboBoxFile.Text = string.Empty;
            comboLogFile.Text = string.Empty;
            comboLogger.Text = string.Empty;

            comboBoxIdentity.SelectedIndex = -1;
            comboBoxNDC.SelectedIndex = -1;
            comboBoxLevel.SelectedIndex = -1;
            comboBoxUserName.SelectedIndex = -1;
            comboBoxThread.SelectedIndex = -1;
            comboBoxMachineName.SelectedIndex = -1;
            comboBoxHostName.SelectedIndex = -1;
            comboBoxApplication.SelectedIndex = -1;
            comboBoxClass.SelectedIndex = -1;
            comboBoxMethod.SelectedIndex = -1;
            comboBoxFile.SelectedIndex = -1;
            comboLogFile.SelectedIndex = -1;
            comboLogger.SelectedIndex = -1;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
