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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace LogViewer
{
    public class RecentFileList : Separator
    {
        private interface IPersist
        {
            List<string> RecentFiles(int max);
            void InsertFile(string filepath, int max);
            void RemoveFile(string filepath, int max);
        }

        private IPersist Persister { get; set; }

        public void UseRegistryPersister() { Persister = new RegistryPersister(); }
        public void UseRegistryPersister(string key) { Persister = new RegistryPersister(key); }

        public void UseXmlPersister() { Persister = new XmlPersister(); }
        public void UseXmlPersister(string filepath) { Persister = new XmlPersister(filepath); }
        public void UseXmlPersister(Stream stream) { Persister = new XmlPersister(stream); }

        public int MaxNumberOfFiles { get; set; }
        public int MaxPathLength { get; set; }
        public MenuItem FileMenu { get; private set; }

        /// <summary>
        /// Used in: String.Format( MenuItemFormat, index, filepath, displayPath );
        /// Default = "_{0}:  {2}"
        /// </summary>
        public string MenuItemFormatOneToNine { get; set; }

        /// <summary>
        /// Used in: String.Format( MenuItemFormat, index, filepath, displayPath );
        /// Default = "{0}:  {2}"
        /// </summary>
        public string MenuItemFormatTenPlus { get; set; }

        public delegate string GetMenuItemTextDelegate(int index, string filepath);
        public GetMenuItemTextDelegate GetMenuItemTextHandler { get; set; }

        public event EventHandler<MenuClickEventArgs> MenuClick;

        Separator separator;
        private List<RecentFile> recentFiles;

        public RecentFileList()
        {
            Persister = new RegistryPersister();

            MaxNumberOfFiles = 9;
            MaxPathLength = 50;
            MenuItemFormatOneToNine = "_{0}:  {2}";
            MenuItemFormatTenPlus = "{0}:  {2}";

            Loaded += (s, e) => HookFileMenu();
        }

        private void HookFileMenu()
        {
            var parent = Parent as MenuItem;
            if (parent == null) throw new ApplicationException("Parent must be a MenuItem");

            if (FileMenu == parent) return;

            if (FileMenu != null) FileMenu.SubmenuOpened -= _FileMenu_SubmenuOpened;

            FileMenu = parent;
            FileMenu.SubmenuOpened += _FileMenu_SubmenuOpened;
        }

        private List<string> RecentFiles { get { return Persister.RecentFiles(MaxNumberOfFiles); } }
        public void RemoveFile(string filepath) { Persister.RemoveFile(filepath, MaxNumberOfFiles); }
        public void InsertFile(string filepath) { Persister.InsertFile(filepath, MaxNumberOfFiles); }

        void _FileMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            SetMenuItems();
        }

        void SetMenuItems()
        {
            RemoveMenuItems();

            LoadRecentFiles();

            InsertMenuItems();
        }

        void RemoveMenuItems()
        {
            if (separator != null) FileMenu.Items.Remove(separator);

            if (recentFiles != null)
                foreach (var r in recentFiles.Where(r => r.MenuItem != null))
                {
                    FileMenu.Items.Remove(r.MenuItem);
                }

            separator = null;
            recentFiles = null;
        }

        void InsertMenuItems()
        {
            if (recentFiles == null) return;
            if (recentFiles.Count == 0) return;

            var iMenuItem = FileMenu.Items.IndexOf(this);
            foreach (var r in recentFiles)
            {
                var header = GetMenuItemText(r.Number + 1, r.Filepath, r.DisplayPath);

                r.MenuItem = new MenuItem { Header = header };
                r.MenuItem.Click += MenuItem_Click;

                FileMenu.Items.Insert(++iMenuItem, r.MenuItem);
            }

            separator = new Separator();
            FileMenu.Items.Insert(++iMenuItem, separator);
        }

        string GetMenuItemText(int index, string filepath, string displaypath)
        {
            GetMenuItemTextDelegate delegateGetMenuItemText = GetMenuItemTextHandler;
            if (delegateGetMenuItemText != null) return delegateGetMenuItemText(index, filepath);

            string format = (index < 10 ? MenuItemFormatOneToNine : MenuItemFormatTenPlus);

            string shortPath = ShortenPathname(displaypath, MaxPathLength);

            return String.Format(format, index, filepath, shortPath);
        }

        // This method is taken from Joe Woodbury's article at: http://www.codeproject.com/KB/cs/mrutoolstripmenu.aspx

        /// <summary>
        /// Shortens a pathname for display purposes.
        /// </summary>
        /// <param Name="pathname">The pathname to shorten.</param>
        /// <param Name="maxLength">The maximum number of characters to be displayed.</param>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three elipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// <para>If a UNC path is used or the pathname and maxLength are particularly short,
        /// the resulting path may be longer than maxLength.</para>
        /// <para>This method expects fully resolved pathnames to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        /// <returns></returns>
        private static string ShortenPathname(string pathname, int maxLength)
        {
            if (pathname.Length <= maxLength)
                return pathname;

            var root = Path.GetPathRoot(pathname);
            if (root.Length > 3)
                root += Path.DirectorySeparatorChar;

            var elements = pathname.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var filenameIndex = elements.GetLength(0) - 1;

            if (elements.GetLength(0) == 1) // pathname is just a root and filename
            {
                if (elements[0].Length > 5) // long enough to shorten
                {
                    // if path is a UNC path, root may be rather long
                    if (root.Length + 6 >= maxLength)
                    {
                        return root + elements[0].Substring(0, 3) + "...";
                    }
                    return pathname.Substring(0, maxLength - 3) + "...";
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

                var totalLength = pathname.Length - len + 3;
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
            return pathname;
        }

        void LoadRecentFiles()
        {
            recentFiles = LoadRecentFilesCore();
        }

        List<RecentFile> LoadRecentFilesCore()
        {

            var list = RecentFiles;

            var files = new List<RecentFile>(list.Count);

            var i = 0;
            files.AddRange(list.Select(filepath => new RecentFile(i++, filepath)));

            return files;
        }

        private class RecentFile
        {
            public int Number;
            public string Filepath = "";
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

        public class MenuClickEventArgs : EventArgs
        {
            public string Filepath { get; private set; }

            public MenuClickEventArgs(string filepath)
            {
                Filepath = filepath;
            }
        }

        void MenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;

            OnMenuClick(menuItem);
        }

        protected virtual void OnMenuClick(MenuItem menuItem)
        {
            var filepath = GetFilepath(menuItem);

            if (String.IsNullOrEmpty(filepath)) return;

            var dMenuClick = MenuClick;
            if (dMenuClick != null) dMenuClick(menuItem, new MenuClickEventArgs(filepath));
        }

        string GetFilepath(MenuItem menuItem)
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

            static Version version;
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
            public string RegistryKey { get; set; }

            public RegistryPersister()
            {
                RegistryKey =
                    "Software\\" +
                    ApplicationAttributes.CompanyName + "\\" +
                    ApplicationAttributes.ProductName + "\\" +
                    "RecentFileList";
            }

            public RegistryPersister(string key)
            {
                RegistryKey = key;
            }

            static string Key(int i) { return i.ToString("00"); }

            public List<string> RecentFiles(int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey) ?? Registry.CurrentUser.CreateSubKey(RegistryKey);

                var list = new List<string>(max);

                for (var i = 0; i < max; i++)
                {
                    if (k == null) continue;
                    var filename = (string)k.GetValue(Key(i));

                    if (String.IsNullOrEmpty(filename)) break;

                    list.Add(filename);
                }

                return list;
            }

            public void InsertFile(string filepath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey);
                if (k == null) Registry.CurrentUser.CreateSubKey(RegistryKey);
                k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);

                RemoveFile(filepath, max);

                for (var i = max - 2; i >= 0; i--)
                {
                    var sThis = Key(i);
                    var sNext = Key(i + 1);

                    if (k == null) continue;
                    var oThis = k.GetValue(sThis);
                    if (oThis == null) continue;

                    k.SetValue(sNext, oThis);
                }

                if (k != null) k.SetValue(Key(0), filepath);
            }

            public void RemoveFile(string filepath, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey);
                if (k == null) return;

                for (var i = 0; i < max; i++)
                {
                again:
                    var s = (string)k.GetValue(Key(i));
                    if (s == null || !s.Equals(filepath, StringComparison.CurrentCultureIgnoreCase)) continue;
                    RemoveFile(i, max);
                    goto again;
                }
            }

            void RemoveFile(int index, int max)
            {
                var k = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
                if (k == null) return;

                k.DeleteValue(Key(index), false);

                for (var i = index; i < max - 1; i++)
                {
                    var sThis = Key(i);
                    var sNext = Key(i + 1);

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
            public string Filepath { get; set; }
            public Stream Stream { get; set; }

            public XmlPersister()
            {
                Filepath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ApplicationAttributes.CompanyName + "\\" +
                        ApplicationAttributes.ProductName + "\\" +
                        "RecentFileList.xml");
            }

            public XmlPersister(string filepath)
            {
                Filepath = filepath;
            }

            public XmlPersister(Stream stream)
            {
                Stream = stream;
            }

            public List<string> RecentFiles(int max)
            {
                return Load(max);
            }

            public void InsertFile(string filepath, int max)
            {
                Update(filepath, true, max);
            }

            public void RemoveFile(string filepath, int max)
            {
                Update(filepath, false, max);
            }

            void Update(string filepath, bool insert, int max)
            {
                var old = Load(max);

                var list = new List<string>(old.Count + 1);

                if (insert) list.Add(filepath);

                CopyExcluding(old, filepath, list, max);

                Save(list, max);
            }

            static void CopyExcluding(IEnumerable<string> source, string exclude, ICollection<string> target, int max)
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

                    Directory.CreateDirectory(Path.GetDirectoryName(filepath));

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

            SmartStream OpenStream(FileMode mode)
            {
                return !String.IsNullOrEmpty(Filepath) ? new SmartStream(Filepath, mode) : new SmartStream(Stream);
            }

            List<string> Load(int max)
            {
                var list = new List<string>(max);

                using (var ms = new MemoryStream())
                {
                    using (var ss = OpenStream(FileMode.OpenOrCreate))
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

                                        default: Debug.Assert(false); break;
                                    }
                                    break;

                                case XmlNodeType.EndElement:
                                    switch (x.Name)
                                    {
                                        case "RecentFiles": return list;
                                        default: Debug.Assert(false); break;
                                    }
                                    break;

                                default:
                                    Debug.Assert(false);
                                    break;
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

            void Save(IEnumerable<string> list, int max)
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
                        foreach (var filepath in list)
                        {
                            x.WriteStartElement("RecentFile");
                            x.WriteAttributeString("Filepath", filepath);
                            x.WriteEndElement();
                            cpt++;
                            if (cpt > max) break;
                        }

                        x.WriteEndElement();

                        x.WriteEndDocument();

                        x.Flush();

                        using (var ss = OpenStream(FileMode.Create))
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
