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
using System.Drawing;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Pure data model for a text shape
    /// </summary>
    public class TextShape : IShape
    {
        public Guid Id { get; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }

        /// <summary>
        /// The text content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Font to use for rendering text
        /// </summary>
        public Font Font { get; set; }

        public TextShape(NativeRect bounds, IShapeStyle style, string text, Font font)
        {
            Id = Guid.NewGuid();
            Bounds = bounds;
            Style = style ?? ShapeStyle.Default();
            Text = text ?? string.Empty;
            Font = font ?? SystemFonts.DefaultFont;
        }

        private TextShape(Guid id, NativeRect bounds, IShapeStyle style, string text, Font font)
        {
            Id = id;
            Bounds = bounds;
            Style = style;
            Text = text;
            Font = font;
        }

        public IShape Clone()
        {
            return new TextShape(Guid.NewGuid(), Bounds, Style, Text, (Font)Font.Clone());
        }
    }
}
