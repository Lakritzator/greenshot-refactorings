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

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Shape representing a mouse cursor with a hotspot indicating where the cursor points
    /// </summary>
    public class CursorShape : ImageShape
    {
        /// <summary>
        /// The hotspot point where the cursor actually points (relative to top-left of image)
        /// </summary>
        public Point Hotspot { get; set; }

        public CursorShape(NativeRect bounds, IImageData cursorImage, Point hotspot, IShapeStyle style = null)
            : base(bounds, cursorImage, style)
        {
            Hotspot = hotspot;
        }

        private CursorShape(Guid id, NativeRect bounds, IImageData imageData, Point hotspot, IShapeStyle style, Guid? layerId)
            : base(id, bounds, imageData, style, layerId)
        {
            Hotspot = hotspot;
        }

        public override IShape Clone()
        {
            return new CursorShape(Guid.NewGuid(), Bounds, ImageData, Hotspot, Style, LayerId);
        }
    }
}
