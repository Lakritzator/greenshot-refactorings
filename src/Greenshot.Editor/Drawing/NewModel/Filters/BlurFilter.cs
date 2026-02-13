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
    /// Filter that blurs/pixelates an area
    /// </summary>
    public class BlurFilter : IFilter
    {
        public Guid Id { get; }
        public Guid? LayerId { get; set; }
        public NativeRect Area { get; set; }
        public bool IsInverted { get; set; }

        /// <summary>
        /// Pixelation size (larger = more blurred)
        /// </summary>
        public int PixelSize { get; set; }

        public BlurFilter(NativeRect area, int pixelSize = 5, bool isInverted = false)
        {
            Id = Guid.NewGuid();
            Area = area;
            PixelSize = pixelSize;
            IsInverted = isInverted;
        }

        public void Apply(Graphics graphics, NativeRect canvasBounds)
        {
            // This is a placeholder - actual implementation would pixelate the area
            // For now, just draw a semi-transparent overlay
            var rect = new Rectangle(Area.X, Area.Y, Area.Width, Area.Height);

            if (IsInverted)
            {
                // Apply to everything except the area
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(canvasBounds.X, canvasBounds.Y, canvasBounds.Width, canvasBounds.Height));
                    path.AddRectangle(rect);
                    using (var brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                    {
                        graphics.FillPath(brush, path);
                    }
                }
            }
            else
            {
                // Apply only to the area
                using (var brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                {
                    graphics.FillRectangle(brush, rect);
                }
            }
        }
    }
}
