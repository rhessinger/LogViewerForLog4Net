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


using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;


namespace LogViewer
{
    /// <summary>
    /// Sort Adorner is displayed on each column to indicate sort direction.
    /// </summary>
    public class SortAdorner : Adorner
    {
        private static readonly Geometry AscGeometry =
            Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

        private static readonly Geometry DescGeometry =
            Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public ListSortDirection Direction { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortAdorner"/> class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="dir">The directory.</param>
        public SortAdorner(UIElement element, ListSortDirection dir)
            : base(element)
        { Direction = dir; }

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            drawingContext.PushTransform(
                new TranslateTransform(
                  AdornedElement.RenderSize.Width - 15,
                  (AdornedElement.RenderSize.Height - 5) / 2));

// ReSharper disable AssignNullToNotNullAttribute
            drawingContext.DrawGeometry(Brushes.Red, null,
// ReSharper restore AssignNullToNotNullAttribute
                Direction == ListSortDirection.Ascending ?
                  AscGeometry : DescGeometry);

            drawingContext.Pop();
        }
    }
}
