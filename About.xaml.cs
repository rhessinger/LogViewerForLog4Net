using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Documents;
using System.Windows.Navigation;
using WPF.Themes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ApplyTheme("ExpressionDark");
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
                        labelAssemblyName.Content = title.Title;
                    }
                    if (type == typeof(AssemblyFileVersionAttribute))
                    {
                        var version = (AssemblyFileVersionAttribute)attribute;
                        labelAssemblyVersion.Content = version.Version;
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
                labelAssembly.Content = sbText.ToString();
            }
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
           
            RichTextBox1.AppendText(text);
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(((Hyperlink) sender).NavigateUri.ToString());
            e.Handled = true;
        }

      
    }
}
