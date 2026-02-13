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
using System.Drawing.Drawing2D;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel.Filters
{
    /// <summary>
    /// Filter that highlights an area (dims everything else)
    /// </summary>
    public class HighlightFilter : IFilter
    {
        public Guid Id { get; }
        public Guid? LayerId { get; set; }
        public NativeRect Area { get; set; }
        public bool IsInverted { get; set; }

        /// <summary>
        /// Color of the highlight overlay
        /// </summary>
        public Color HighlightColor { get; set; }

        /// <summary>
        /// Opacity of the highlight (0-255)
        /// </summary>
        public int Opacity { get; set; }

        public HighlightFilter(NativeRect area, Color? highlightColor = null, int opacity = 100, bool isInverted = true)
        {
            Id = Guid.NewGuid();
            Area = area;
            HighlightColor = highlightColor ?? Color.Black;
            Opacity = Math.Max(0, Math.Min(255, opacity));
            IsInverted = isInverted; // Typically inverted (dims everything except the area)
        }

        public void Apply(Graphics graphics, NativeRect canvasBounds)
        {
            var rect = new Rectangle(Area.X, Area.Y, Area.Width, Area.Height);

            if (IsInverted)
            {
                // Dim everything except the area
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(canvasBounds.X, canvasBounds.Y, canvasBounds.Width, canvasBounds.Height));
                    path.AddRectangle(rect);
                    using (var brush = new SolidBrush(Color.FromArgb(Opacity, HighlightColor)))
                    {
                        graphics.FillPath(brush, path);
                    }
                }
            }
            else
            {
                // Highlight only the area
                using (var brush = new SolidBrush(Color.FromArgb(Opacity, HighlightColor)))
                {
                    graphics.FillRectangle(brush, rect);
                }
            }
        }
    }
}
