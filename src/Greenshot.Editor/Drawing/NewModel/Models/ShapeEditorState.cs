/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Represents the editor state for a shape (selection, adorners, etc.)
    /// Completely separate from the shape data itself.
    /// </summary>
    public class ShapeEditorState
    {
        /// <summary>
        /// The shape this state applies to
        /// </summary>
        public IShape Shape { get; }

        /// <summary>
        /// Whether the shape is currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether adorners (resize handles) should be shown
        /// </summary>
        public bool ShowAdorners { get; set; }

        /// <summary>
        /// Whether the shape is being edited (e.g., text editing mode)
        /// </summary>
        public bool IsEditing { get; set; }

        /// <summary>
        /// Temporary bounds used during resize operations
        /// </summary>
        public NativeRect? ResizingBounds { get; set; }

        public ShapeEditorState(IShape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            IsSelected = false;
            ShowAdorners = false;
            IsEditing = false;
            ResizingBounds = null;
        }
    }
}
