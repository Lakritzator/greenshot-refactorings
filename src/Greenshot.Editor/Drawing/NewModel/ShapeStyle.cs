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
    /// Immutable implementation of IShapeStyle
    /// </summary>
    public class ShapeStyle : IShapeStyle
    {
        public Color LineColor { get; }
        public int LineThickness { get; }
        public Color FillColor { get; }
        public bool Shadow { get; }

        public ShapeStyle(Color lineColor, int lineThickness, Color fillColor, bool shadow)
        {
            LineColor = lineColor;
            LineThickness = lineThickness;
            FillColor = fillColor;
            Shadow = shadow;
        }

        public IShapeStyle WithLineColor(Color color)
        {
            return new ShapeStyle(color, LineThickness, FillColor, Shadow);
        }

        public IShapeStyle WithLineThickness(int thickness)
        {
            return new ShapeStyle(LineColor, thickness, FillColor, Shadow);
        }

        public IShapeStyle WithFillColor(Color color)
        {
            return new ShapeStyle(LineColor, LineThickness, color, Shadow);
        }

        public IShapeStyle WithShadow(bool shadow)
        {
            return new ShapeStyle(LineColor, LineThickness, FillColor, shadow);
        }

        /// <summary>
        /// Creates a default style
        /// </summary>
        public static ShapeStyle Default()
        {
            return new ShapeStyle(Color.Black, 1, Color.Empty, false);
        }
    }
}
