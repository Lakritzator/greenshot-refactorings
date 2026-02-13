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
    /// Shape representing the background/screenshot image.
    /// Always resides in its own implicit background layer.
    /// </summary>
    public class BackgroundShape : ImageShape
    {
        public BackgroundShape(NativeRect bounds, IImageData backgroundImage)
            : base(bounds, backgroundImage, ShapeStyle.Default())
        {
            // Background always in the background layer
            LayerId = null; // Will be set to background layer by canvas
        }

        private BackgroundShape(Guid id, NativeRect bounds, IImageData imageData, IShapeStyle style, Guid? layerId)
            : base(bounds, imageData, style)
        {
            LayerId = layerId;
        }

        public override IShape Clone()
        {
            return new BackgroundShape(Guid.NewGuid(), Bounds, ImageData, Style, LayerId);
        }
    }
}
