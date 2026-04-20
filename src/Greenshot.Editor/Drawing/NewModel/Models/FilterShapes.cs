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
    /// Base interface for filter shapes.
    /// Filters are shapes that apply visual effects to areas of the canvas.
    /// They don't have styles but can have adorners for positioning.
    /// </summary>
    public interface IFilterShape : IShape
    {
        /// <summary>
        /// Whether this filter is inverted (applies everywhere EXCEPT the area)
        /// </summary>
        bool IsInverted { get; set; }
    }

    /// <summary>
    /// Filter shape that blurs/pixelates an area
    /// </summary>
    public class BlurFilterShape : IFilterShape
    {
        public Guid Id { get; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }
        public Guid? LayerId { get; set; }
        public bool IsInverted { get; set; }

        /// <summary>
        /// Pixelation size (larger = more blurred)
        /// </summary>
        public int PixelSize { get; set; }

        public BlurFilterShape(NativeRect bounds, int pixelSize = 5, bool isInverted = false)
        {
            Id = Guid.NewGuid();
            Bounds = bounds;
            PixelSize = pixelSize;
            IsInverted = isInverted;
            Style = null; // Filters don't use styles
        }

        private BlurFilterShape(Guid id, NativeRect bounds, int pixelSize, bool isInverted, Guid? layerId)
        {
            Id = id;
            Bounds = bounds;
            PixelSize = pixelSize;
            IsInverted = isInverted;
            LayerId = layerId;
            Style = null;
        }

        public IShape Clone()
        {
            return new BlurFilterShape(Guid.NewGuid(), Bounds, PixelSize, IsInverted, LayerId);
        }
    }

    /// <summary>
    /// Filter shape that highlights an area (dims everything else)
    /// </summary>
    public class HighlightFilterShape : IFilterShape
    {
        public Guid Id { get; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }
        public Guid? LayerId { get; set; }
        public bool IsInverted { get; set; }

        /// <summary>
        /// Color of the highlight overlay
        /// </summary>
        public System.Drawing.Color HighlightColor { get; set; }

        /// <summary>
        /// Opacity of the highlight (0-255)
        /// </summary>
        public int Opacity { get; set; }

        public HighlightFilterShape(NativeRect bounds, System.Drawing.Color? highlightColor = null, int opacity = 100, bool isInverted = true)
        {
            Id = Guid.NewGuid();
            Bounds = bounds;
            HighlightColor = highlightColor ?? System.Drawing.Color.Black;
            Opacity = Math.Max(0, Math.Min(255, opacity));
            IsInverted = isInverted; // Typically inverted (dims everything except the area)
            Style = null; // Filters don't use styles
        }

        private HighlightFilterShape(Guid id, NativeRect bounds, System.Drawing.Color highlightColor, int opacity, bool isInverted, Guid? layerId)
        {
            Id = id;
            Bounds = bounds;
            HighlightColor = highlightColor;
            Opacity = opacity;
            IsInverted = isInverted;
            LayerId = layerId;
            Style = null;
        }

        public IShape Clone()
        {
            return new HighlightFilterShape(Guid.NewGuid(), Bounds, HighlightColor, Opacity, IsInverted, LayerId);
        }
    }
}
