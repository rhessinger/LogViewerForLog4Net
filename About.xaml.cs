#region Header

#region Project
/*
 * Project     :  LogViewer
 * Description :  Viewer for Log4Net XML logs (see About box for log4Net configuration).
 * Version     :  3.0
 * Modified on :  1.0 15-3-2010 
 *                2.1 May 2010 OD
 *                2.6 26-jun-2010 OD - add quick filter on symbols, cancel filter on filter zoom/text symbol  
 *                2.7 --Jul-2010 OD - save window size, split position. Reset split.
 *                2.x -- Open Source Project on CodePlex
 *                3.0 Windows Store version OD (change from codeplex to GitHub)
 *                
 *
 * Copyrights  : (c) 2010/2017 Olivier Dahan for the enhanced version & Windows Store version - www.e-naxos.com
 *               (c) 2009 Ken C. Len for original version
 *               
 * LogViewer is a free software distributed on GitHub under the Microsoft Reciprocal License (Ms-RL)
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


#endregion

using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public About()
        {
            InitializeComponent();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var sbText = new StringBuilder();
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var attributes = assembly.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    var type = attribute.GetType();
                    if (type == typeof(AssemblyTitleAttribute))
                    {
                        var title = (AssemblyTitleAttribute)attribute;
                        LabelAssemblyName.Content = title.Title;
                    }
                    if (type == typeof(AssemblyFileVersionAttribute))
                    {
                        var version = (AssemblyFileVersionAttribute)attribute;
                        LabelAssemblyVersion.Content = version.Version;
                    }
                    if (type == typeof(AssemblyCopyrightAttribute))
                    {
                        var copyright = (AssemblyCopyrightAttribute)attribute;
                        sbText.AppendFormat("{0}\r", copyright.Copyright);
                    }
                    if (type == typeof(AssemblyCompanyAttribute))
                    {
                        var company = (AssemblyCompanyAttribute)attribute;
                        sbText.AppendFormat("{0}\r",company.Company);
                    }
                    if (type == typeof(AssemblyDescriptionAttribute))
                    {
                        var description = (AssemblyDescriptionAttribute)attribute;
                        sbText.AppendFormat("{0}\r", description.Description);
                    }
                }
                LabelAssembly.Content = sbText.ToString();
            }
// ReSharper disable VerbatimStringLiteralsWordIsNotInDictionary
            const string text = @"<log4net>
  <appender name=""RollingFileAppender"" type=""log4net.Appender.RollingFileAppender"">
    <file type=""log4net.Util.PatternString"" value=""c:\log\log.xml"" />
    <appendToFile value=""true"" />
    <datePattern value=""yyyyMMdd"" />
    <rollingStyle value=""Date"" />
    <layout type=""log4net.Layout.XmlLayoutSchemaLog4j"">
      <locationInfo value=""true"" />
    </layout>
  </appender>
  <root>
    <level value=""DEBUG"" />
    <appender-ref ref=""RollingFileAppender"" />
  </root>
</log4net>";
// ReSharper restore VerbatimStringLiteralsWordIsNotInDictionary
           
            RichTextBox1.AppendText(text);
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(((Hyperlink) sender).NavigateUri.ToString());
            e.Handled = true;
        }

      
    }
}
