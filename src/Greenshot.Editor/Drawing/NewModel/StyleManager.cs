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
using System.Linq;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Manages a collection of reusable styles that can be applied to shapes.
    /// Supports creating, storing, and applying named styles.
    /// </summary>
    public class StyleManager
    {
        private readonly Dictionary<string, IShapeStyle> _styles = new Dictionary<string, IShapeStyle>();

        public StyleManager()
        {
            // Register default styles
            RegisterDefaultStyles();
        }

        /// <summary>
        /// Registers a style with a name
        /// </summary>
        public void RegisterStyle(string name, IShapeStyle style)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            _styles[name] = style;
        }

        /// <summary>
        /// Gets a style by name
        /// </summary>
        public IShapeStyle GetStyle(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return _styles.TryGetValue(name, out var style) ? style : null;
        }

        /// <summary>
        /// Gets all registered style names
        /// </summary>
        public IEnumerable<string> GetStyleNames()
        {
            return _styles.Keys.ToList();
        }

        /// <summary>
        /// Removes a style
        /// </summary>
        public bool RemoveStyle(string name)
        {
            return _styles.Remove(name);
        }

        /// <summary>
        /// Applies a named style to a shape
        /// </summary>
        public void ApplyStyle(IShape shape, string styleName)
        {
            var style = GetStyle(styleName);
            if (style != null && shape != null)
            {
                shape.Style = style;
            }
        }

        /// <summary>
        /// Applies a style to multiple shapes
        /// </summary>
        public void ApplyStyleToShapes(IEnumerable<IShape> shapes, string styleName)
        {
            var style = GetStyle(styleName);
            if (style == null)
            {
                return;
            }

            foreach (var shape in shapes)
            {
                shape.Style = style;
            }
        }

        private void RegisterDefaultStyles()
        {
            // Default style
            RegisterStyle("Default", ShapeStyle.Default());

            // Common predefined styles
            RegisterStyle("RedBorder", new ShapeStyle(Color.Red, 2, Color.Empty, false));
            RegisterStyle("BlueFilled", new ShapeStyle(Color.Blue, 1, Color.LightBlue, false));
            RegisterStyle("GreenHighlight", new ShapeStyle(Color.Green, 2, Color.FromArgb(100, Color.Green), false));
            RegisterStyle("YellowWithShadow", new ShapeStyle(Color.Black, 1, Color.Yellow, true));
            RegisterStyle("ThickBlackBorder", new ShapeStyle(Color.Black, 3, Color.Empty, false));
        }
    }
}
