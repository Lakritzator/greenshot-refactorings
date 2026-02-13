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

using System.Collections.Generic;
using System.Drawing;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Defines custom adorner configuration for shapes that need special adorner behavior
    /// (e.g., speech bubble with tail control point)
    /// </summary>
    public interface IAdornerConfiguration
    {
        /// <summary>
        /// Gets the custom adorner positions for this shape
        /// </summary>
        IEnumerable<CustomAdorner> GetAdorners(IShape shape);
    }

    /// <summary>
    /// Represents a custom adorner with position and color
    /// </summary>
    public class CustomAdorner
    {
        public string Id { get; set; }
        public Point Position { get; set; }
        public Color Color { get; set; }
        public int Size { get; set; }

        public CustomAdorner(string id, Point position, Color? color = null, int size = 7)
        {
            Id = id;
            Position = position;
            Color = color ?? Color.White;
            Size = size;
        }
    }
}
