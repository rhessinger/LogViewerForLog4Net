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
    
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using log4net;

namespace LogViewer
{
    /// <summary>
    /// Manage the recent file list
    /// </summary>
    public class RecentFileList : Separator
    {
        private interface IPersist
        {
            List<string> RecentFiles(int max);
            void InsertFile(string filepath, int max);
            void RemoveFile(string filepath, int max);
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IPersist persister { get; set; }

        /// <summary>
        /// Uses the registry persister.
        /// </summary>
        public void UseRegistryPersister() { persister = new RegistryPersister(); }
        /// <summary>
        /// Uses the registry persister.
        /// </summary>
        /// <param name="key">The key.</param>
        public void UseRegistryPersister(string key) { persister = new RegistryPersister(key); }

        /// <summary>
        /// Uses the XML persister.
        /// </summary>
        public void UseXmlPersister() { persister = new XmlPersister(); }
        /// <summary>
        /// Uses the XML persister.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void UseXmlPersister(string filePath) { persister = new XmlPersister(filePath); }
        /// <summary>
        /// Uses the XML persister.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void UseXmlPersister(Stream stream) { persister = new XmlPersister(stream); }

        /// <summary>
        /// Gets or sets the max number of files.
        /// </summary>
        /// <value>The max number of files.</value>
        public int MaxNumberOfFiles { get; set; }
        /// <summary>
        /// Gets or sets the length of the max path.
        /// </summary>
        /// <value>The length of the max path.</value>
        public int MaxPathLength { get; set; }
        /// <summary>
        /// Gets or sets the file menu.
        /// </summary>
        /// <value>The file menu.</value>
        public MenuItem FileMenu { get; private set; }

        /// <summary>
        /// Used in: String.Format( MenuItemFormat, index, file path, displayPath );
        /// Default = "_{0}:  {2}"
        /// </summary>
        public string MenuItemFormatOneToNine { get; set; }

        /// <summary>
        /// Used in: String.Format( MenuItemFormat, index, file path, displayPath );
        /// Default = "{0}:  {2}"
        /// </summary>
        public string MenuItemFormatTenPlus { get; set; }

        /// <summary>
        /// Delegate
        /// </summary>
        public delegate string GetMenuItemTextDelegate(int index, string filePath);

        /// <summary>
        /// Gets or sets the get menu item text handler.
        /// </summary>
        /// <value>The get menu item text handler.</value>
        public GetMenuItemTextDelegate GetMenuItemTextHandler { get; set; }

        /// <summary>
        /// Occurs when [menu click].
        /// </summary>
        public event EventHandler<MenuClickEventArgs> MenuClick;

        Separator separator;
        private List<RecentFile> recentFiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentFileList"/> class.
        /// </summary>
        public RecentFileList()
        {
            log.Info("Initializing the registry persister");
            persister = new RegistryPersister();

            MaxNumberOfFiles = 9;
            MaxPathLength = 50;
            MenuItemFormatOneToNine = "_{0}:  {2}";
            MenuItemFormatTenPlus = "{0}:  {2}";

            Loaded += (s, e) => hookFileMenu();
        }

        private void hookFileMenu()
        {
            log.Info("Hooking up the file menu");
            var parent = Parent as MenuItem;
            if (parent == null)
            {
                log.Error("Parent must be a menuItem");
                throw new ApplicationException("Parent must be a MenuItem");
            }

            if (FileMenu == parent) return;

            if (FileMenu != null) FileMenu.SubmenuOpened -= fileMenu_SubmenuOpened;

            FileMenu = parent;
            FileMenu.SubmenuOpened += fileMenu_SubmenuOpened;
        }

        private List<string> theRecentFiles { get { return persister.RecentFiles(MaxNumberOfFiles); } }
        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="filePath">The file Path.</param>
        public void RemoveFile(string filePath) { persister.RemoveFile(filePath, MaxNumberOfFiles); }
        
        /// <summary>
        /// Inserts the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void InsertFile(string filePath) { persister.InsertFile(filePath, MaxNumberOfFiles); }

        void fileMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            setMenuItems();
        }

        void setMenuItems()
        {
            log.Info("Removing Menu Items");
            removeMenuItems();
            log.Info("Loading Recent files");
            loadRecentFiles();
            log.Info("Inserting Menu items");
            insertMenuItems();
        }

        void removeMenuItems()
        {
            log.Info("Removing the menu separators");
            if (separator != null) FileMenu.Items.Remove(separator);

            if (recentFiles != null)
            {
                log.Info("Removing recent files from the menu items");
                foreach (var r in recentFiles.Where(r => r.MenuItem != null))
                {
                    FileMenu.Items.Remove(r.MenuItem);
                }
            }
            separator = null;
            recentFiles = null;
        }

        void insertMenuItems()
        {
            if (recentFiles == null) return;
            if (recentFiles.Count == 0) return;

            var iMenuItem = FileMenu.Items.IndexOf(this);
            log.Info("Loading recent files as menu items");
            foreach (var r in recentFiles)
            {
                var header = getMenuItemText(r.Number + 1, r.Filepath, r.DisplayPath);

                r.MenuItem = new MenuItem { Header = header };
                r.MenuItem.Click += menuItem_Click;

                FileMenu.Items.Insert(++iMenuItem, r.MenuItem);
            }

            separator = new Separator();
            FileMenu.Items.Insert(++iMenuItem, separator);
        }

        string getMenuItemText(int index, string filepath, string displaypath)
        {
            var delegateGetMenuItemText = GetMenuItemTextHandler;
            if (delegateGetMenuItemText != null) return delegateGetMenuItemText(index, filepath);

            var format = (index < 10 ? MenuItemFormatOneToNine : MenuItemFormatTenPlus);
            log.Info("Format is " + format);
            var shortPath = shortenPathname(displaypath, MaxPathLength);
            log.Info("The short path is " + shortPath);
            return String.Format(format, index, filepath, shortPath);
        }

        // This method is taken from Joe Woodbury's article at: http://www.codeproject.com/KB/cs/mrutoolstripmenu.aspx

        /// <summary>
        /// Shortens a pathname for display purposes.
        /// </summary>
        /// <param name="pathName">Name of the path.</param>
        /// <param name="maxLength">Length of the max.</param>
        /// <returns></returns>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three ellipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// 	<para>If a UNC path is used or the pathname and max Length are particularly short,
        /// the resulting path may be longer than max Length.</para>
        /// 	<para>This method expects fully resolved pathnames to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        private static string shortenPathname(string pathName, int maxLength)
        {
            log.Info("Checking current pathname is less than the max length allowed");
            log.Info("Path name is " + pathName);
            if (pathName.Length <= maxLength)
                return pathName;
            //TODO: check if path is null
            var root = Path.GetPathRoot(pathName);
            if (root.Length > 3)
                root += Path.DirectorySeparatorChar;

            var elements = pathName.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var filenameIndex = elements.GetLength(0) - 1;
            log.Info("File name index is " + filenameIndex);
            if (elements.GetLength(0) == 1) // pathname is just a root and filename
            {
                if (elements[0].Length > 5) // long enough to shorten
                {
                    // if path is a UNC path, root may be rather long
                    if (root.Length + 6 >= maxLength)
                    {
                        return root + elements[0].Substring(0, 3) + "...";
                    }
                    return pathName.Substring(0, maxLength - 3) + "...";
                }
            }
            else if ((root.Length + 4 + elements[filenameIndex].Length) > maxLength) // pathname is just a root and filename
            {
                root += "...\\";

                int len = elements[filenameIndex].Length;
                if (len < 6)
                    return root + elements[filenameIndex];

                if ((root.Length + 6) >= maxLength)
                {
                    len = 3;
                }
                else
                {
                    len = maxLength - root.Length - 3;
                }
                return root + elements[filenameIndex].Substring(0, len) + "...";
            }
            else if (elements.GetLength(0) == 2)
            {
                return root + "...\\" + elements[1];
            }
            else
            {
                var len = 0;
                var begin = 0;

                for (var i = 0; i < filenameIndex; i++)
                {
                    if (elements[i].Length <= len) continue;
                    begin = i;
                    len = elements[i].Length;
                }

                var totalLength = pathName.Length - len + 3;
                var end = begin + 1;

                while (totalLength > maxLength)
                {
                    if (begin > 0)
                        totalLength -= elements[--begin].Length - 1;

                    if (totalLength <= maxLength)
                        break;

                    if (end < filenameIndex)
                        totalLength -= elements[++end].Length - 1;

                    if (begin == 0 && end == filenameIndex)
                        break;
                }

                // assemble final string

                for (var i = 0; i < begin; i++)
                {
                    root += elements[i] + '\\';
                }

                root += "...\\";

                for (int i = end; i < filenameIndex; i++)
                {
                    root += elements[i] + '\\';
                }

                return root + elements[filenameIndex];
            }
            return pathName;
        }

        private void loadRecentFiles()
        {
            recentFiles = loadRecentFilesCore();
        }

        private List<RecentFile> loadRecentFilesCore()
        {

            var list = theRecentFiles;

            var files = new List<RecentFile>(list.Count);

            var i = 0;
            files.AddRange(list.Select(filepath => new RecentFile(i++, filepath)));

            return files;
        }

        private class RecentFile
        {
            public readonly int Number;
            public readonly string Filepath = "";
            public MenuItem MenuItem;

            public string DisplayPath
            {
                get
                {
                    return Path.Combine(
                        Path.GetDirectoryName(Filepath),
                        Path.GetFileNameWithoutExtension(Filepath));
                }
            }

            public RecentFile(int number, string filepath)
            {
                Number = number;
                Filepath = filepath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class MenuClickEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the filePath.
            /// </summary>
            /// <value>The filePath.</value>
            public string FilePath { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="theFilePath"></param>
            public MenuClickEventArgs(string theFilePath)
            {
                FilePath = theFilePath;
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;

            OnMenuClick(menuItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuItem"></param>
        protected virtual void OnMenuClick(MenuItem menuItem)
        {
            var filepath = getFilepath(menuItem);

            if (String.IsNullOrEmpty(filepath)) return;

            var dMenuClick = MenuClick;
            if (dMenuClick != null) dMenuClick(menuItem, new MenuClickEventArgs(filepath));
        }

        private string getFilepath(MenuItem menuItem)
        {
            foreach (var r in recentFiles.Where(r => r.MenuItem == menuItem))
            {
                return r.Filepath;
            }

            return String.Empty;
        }

        //-----------------------------------------------------------------------------------------

        static class ApplicationAttributes
        {
            static readonly Assembly assembly;

            static readonly AssemblyTitleAttribute title;
            static readonly AssemblyCompanyAttribute company;
            static readonly AssemblyCopyrightAttribute copyright;
            static readonly AssemblyProductAttribute product;

            public static string Title { get; private set; }
            public static string CompanyName { get; private set; }
            public static string Copyright { get; private set; }
            public static string ProductName { get; private set; }

            static readonly Version version;
            public static string Version { get; private set; }

            static ApplicationAttributes()
            {
                try
                {
                    Title = String.Empty;
                    CompanyName = String.Empty;
                    Copyright = String.Empty;
                    ProductName = String.Empty;
                    Version = String.Empty;

                    assembly = Assembly.GetEntryAssembly();

                    if (assembly != null)
                    {
                        var attributes = assembly.GetCustomAttributes(false);

                        foreach (var attribute in attributes)
                        {
                            var type = attribute.GetType();

                            if (type == typeof(AssemblyTitleAttribute)) title = (AssemblyTitleAttribute)attribute;
                            if (type == typeof(AssemblyCompanyAttribute)) company = (AssemblyCompanyAttribute)attribute;
                            if (type == typeof(AssemblyCopyrightAttribute)) copyright = (AssemblyCopyrightAttribute)attribute;
                            if (type == typeof(AssemblyProductAttribute)) product = (AssemblyProductAttribute)attribute;
                        }

                        version = assembly.GetName().Version;
                    }

                    if (title != null) Title = title.Title;
                    if (company != null) CompanyName = company.Company;
                    if (copyright != null) Copyright = copyright.Copyright;
                    if (product != null) ProductName = product.Product;
                    if (version != null) Version = version.ToString();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        //-----------------------------------------------------------------------------------------

        private class RegistryPersister : IPersist
        {
            private string registryKey { get; set; }

            public RegistryPersister()
            {
                registryKey =
                    "Software\\" +
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList";
            }

            public RegistryPersister(string key)
            {
                registryKey = key;
            }

            private static string key(int i) { return i.ToString("00"); }

            public List<string> RecentFiles(int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(registryKey) ?? Registry.CurrentUser.CreateSubKey(registryKey);

                var list = new List<string>(max);

                for (var i = 0; i < max; i++)
                {
                    if (k == null) continue;
                    var filename = (string)k.GetValue(key(i));

                    if (String.IsNullOrEmpty(filename)) break;

                    list.Add(filename);
                }

                return list;
            }

            public void InsertFile(string filepath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(registryKey);
                if (k == null) Registry.CurrentUser.CreateSubKey(registryKey);
                k = Registry.CurrentUser.OpenSubKey(registryKey, true);

                RemoveFile(filepath, max);

                for (var i = max - 2; i >= 0; i--)
                {
                    var sThis = key(i);
                    var sNext = key(i + 1);

                    if (k == null) continue;
                    var oThis = k.GetValue(sThis);
                    if (oThis == null) continue;

                    k.SetValue(sNext, oThis);
                }

                if (k != null) k.SetValue(key(0), filepath);
            }

            public void RemoveFile(string theFilePath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(registryKey);
                if (k == null) return;

                for (var i = 0; i < max; i++)
                {
                again:
                    var s = (string)k.GetValue(key(i));
                    if (s == null || !s.Equals(theFilePath, StringComparison.CurrentCultureIgnoreCase)) continue;
                    removeFile(i, max);
                    goto again;
                }
            }

            private void removeFile(int index, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(registryKey, true);
                if (k == null) return;

                k.DeleteValue(key(index), false);

                for (var i = index; i < max - 1; i++)
                {
                    var sThis = key(i);
                    var sNext = key(i + 1);

                    var oNext = k.GetValue(sNext);
                    if (oNext == null) break;

                    k.SetValue(sThis, oNext);
                    k.DeleteValue(sNext);
                }
            }
        }

        //-----------------------------------------------------------------------------------------

        private class XmlPersister : IPersist
        {
            private string filepath { get; set; }
            private Stream stream { get; set; }

            public XmlPersister()
            {
                filepath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ApplicationAttributes.CompanyName + "\\" +
                        ApplicationAttributes.ProductName + "\\" +
                        "RecentFileList.xml");
            }

            public XmlPersister(string filepath)
            {
                this.filepath = filepath;
            }

            public XmlPersister(Stream stream)
            {
                this.stream = stream;
            }

            public List<string> RecentFiles(int max)
            {
                return load(max);
            }

            public void InsertFile(string theFilePath, int max)
            {
                update(theFilePath, true, max);
            }

            public void RemoveFile(string theFilePath, int max)
            {
                update(theFilePath, false, max);
            }

            private void update(string theFilePath, bool insert, int max)
            {
                var old = load(max);

                var list = new List<string>(old.Count + 1);

                if (insert) list.Add(theFilePath);

                copyExcluding(old, theFilePath, list, max);

                save(list, max);
            }

            private static void copyExcluding(IEnumerable<string> source, string exclude, ICollection<string> target, int max)
            {
                foreach (var s in from s in source
                                  where !String.IsNullOrEmpty(s)
                                  where !s.Equals(exclude, StringComparison.OrdinalIgnoreCase)
                                  where target.Count < max
                                  select s)
                {
                    target.Add(s);
                }
            }

            class SmartStream : IDisposable
            {
                readonly bool isStreamOwned = true;

                public Stream Stream { get; private set; }

                public static implicit operator Stream(SmartStream me) { return me.Stream; }

                public SmartStream(string filepath, FileMode mode)
                {
                    isStreamOwned = true;

// ReSharper disable AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(filepath));
// ReSharper restore AssignNullToNotNullAttribute

                    Stream = File.Open(filepath, mode);
                }

                public SmartStream(Stream stream)
                {
                    isStreamOwned = false;
                    Stream = stream;
                }

                public void Dispose()
                {
                    if (isStreamOwned && Stream != null) Stream.Dispose();

                    Stream = null;
                }
            }

            private SmartStream openStream(FileMode mode)
            {
                return !String.IsNullOrEmpty(filepath) ? new SmartStream(filepath, mode) : new SmartStream(stream);
            }

            private List<string> load(int max)
            {
                var list = new List<string>(max);

                using (var ms = new MemoryStream())
                {
                    using (var ss = openStream(FileMode.OpenOrCreate))
                    {
                        if (ss.Stream.Length == 0) return list;

                        ss.Stream.Position = 0;

                        var buffer = new byte[1 << 20];
                        for (; ; )
                        {
                            var bytes = ss.Stream.Read(buffer, 0, buffer.Length);
                            if (bytes == 0) break;
                            ms.Write(buffer, 0, bytes);
                        }

                        ms.Position = 0;
                    }

                    XmlTextReader x = null;

                    try
                    {
                        x = new XmlTextReader(ms);

                        while (x.Read())
                        {
                            switch (x.NodeType)
                            {
                                case XmlNodeType.XmlDeclaration:
                                case XmlNodeType.Whitespace:
                                    break;

                                case XmlNodeType.Element:
                                    switch (x.Name)
                                    {
                                        case "RecentFiles": break;

                                        case "RecentFile":
                                            if (list.Count < max) list.Add(x.GetAttribute(0));
                                            break;

// ReSharper disable HeuristicUnreachableCode
                                        default: Debug.Assert(false); break;
// ReSharper restore HeuristicUnreachableCode
                                    }
                                    break;

                                case XmlNodeType.EndElement:
                                    switch (x.Name)
                                    {
                                        case "RecentFiles": return list;
// ReSharper disable HeuristicUnreachableCode
                                        default: Debug.Assert(false); break;
// ReSharper restore HeuristicUnreachableCode
                                    }
// ReSharper disable HeuristicUnreachableCode
                                    break;
// ReSharper restore HeuristicUnreachableCode

                                default:
                                    Debug.Assert(false);
// ReSharper disable HeuristicUnreachableCode
                                    break;
// ReSharper restore HeuristicUnreachableCode
                            }
                        }
                    }
                    finally
                    {
                        if (x != null) x.Close();
                    }
                }
                return list;
            }

            private void save(IEnumerable<string> list, int max)
            {
                using (var ms = new MemoryStream())
                {
                    XmlTextWriter x = null;

                    try
                    {
                        x = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented };

                        x.WriteStartDocument();

                        x.WriteStartElement("RecentFiles");

                        var cpt = 0;
                        foreach (var loopFilePath in list)
                        {
                            x.WriteStartElement("RecentFile");
// ReSharper disable StringLiteralsWordIsNotInDictionary
                            x.WriteAttributeString("Filepath", loopFilePath);
// ReSharper restore StringLiteralsWordIsNotInDictionary
                            x.WriteEndElement();
                            cpt++;
                            if (cpt > max) break;
                        }

                        x.WriteEndElement();

                        x.WriteEndDocument();

                        x.Flush();

                        using (var ss = openStream(FileMode.Create))
                        {
                            ss.Stream.SetLength(0);

                            ms.Position = 0;

                            var buffer = new byte[1 << 20];
                            for (; ; )
                            {
                                int bytes = ms.Read(buffer, 0, buffer.Length);
                                if (bytes == 0) break;
                                ss.Stream.Write(buffer, 0, bytes);
                            }
                        }
                    }
                    finally
                    {
                        if (x != null) x.Close();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------

    }
}
