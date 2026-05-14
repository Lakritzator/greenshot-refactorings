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

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Abstraction for image data that can be implemented by different image libraries
    /// (System.Drawing, ImageSharp, etc.)
    /// </summary>
    public interface IImageData : IDisposable
    {
        /// <summary>
        /// Width of the image in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the image in pixels
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Type of image (Bitmap, Vector, etc.)
        /// </summary>
        ImageType Type { get; }

        /// <summary>
        /// Creates a copy of this image data
        /// </summary>
        IImageData Clone();
    }

    /// <summary>
    /// Type of image
    /// </summary>
    public enum ImageType
    {
        /// <summary>
        /// Raster/Bitmap image (PNG, JPG, etc.)
        /// </summary>
        Bitmap,

        /// <summary>
        /// Vector image (SVG, etc.)
        /// </summary>
        Vector
    }
}
