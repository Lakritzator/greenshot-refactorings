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
    /// Shape that contains an image (bitmap or vector).
    /// Multiple ImageShapes can share the same IImageData instance for memory efficiency.
    /// </summary>
    public class ImageShape : IShape
    {
        public Guid Id { get; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }
        public Guid? LayerId { get; set; }

        /// <summary>
        /// The image data (can be shared across multiple shapes)
        /// </summary>
        public IImageData ImageData { get; set; }

        public ImageShape(NativeRect bounds, IImageData imageData, IShapeStyle style = null)
        {
            Id = Guid.NewGuid();
            Bounds = bounds;
            ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
            Style = style ?? ShapeStyle.Default();
        }

        private ImageShape(Guid id, NativeRect bounds, IImageData imageData, IShapeStyle style, Guid? layerId)
        {
            Id = id;
            Bounds = bounds;
            ImageData = imageData;
            Style = style;
            LayerId = layerId;
        }

        public virtual IShape Clone()
        {
            // Share the same image data for memory efficiency
            // Caller can replace ImageData if deep copy is needed
            return new ImageShape(Guid.NewGuid(), Bounds, ImageData, Style, LayerId);
        }
    }
}
