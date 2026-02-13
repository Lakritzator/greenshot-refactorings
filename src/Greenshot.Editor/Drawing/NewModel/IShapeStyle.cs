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

using System.Drawing;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Represents styling properties that can be applied to shapes.
    /// Styles are reusable and can be shared across multiple shapes.
    /// Immutable to allow safe sharing.
    /// </summary>
    public interface IShapeStyle
    {
        /// <summary>
        /// Line/stroke color
        /// </summary>
        Color LineColor { get; }

        /// <summary>
        /// Line/stroke thickness in pixels
        /// </summary>
        int LineThickness { get; }

        /// <summary>
        /// Fill color (Color.Empty means no fill)
        /// </summary>
        Color FillColor { get; }

        /// <summary>
        /// Whether to draw a shadow
        /// </summary>
        bool Shadow { get; }

        /// <summary>
        /// Creates a copy of this style with modified properties
        /// </summary>
        IShapeStyle WithLineColor(Color color);

        /// <summary>
        /// Creates a copy of this style with modified properties
        /// </summary>
        IShapeStyle WithLineThickness(int thickness);

        /// <summary>
        /// Creates a copy of this style with modified properties
        /// </summary>
        IShapeStyle WithFillColor(Color color);

        /// <summary>
        /// Creates a copy of this style with modified properties
        /// </summary>
        IShapeStyle WithShadow(bool shadow);
    }
}
