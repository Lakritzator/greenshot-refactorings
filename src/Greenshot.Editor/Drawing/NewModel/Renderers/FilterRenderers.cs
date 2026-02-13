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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Dapplo.Windows.Common.Structs;
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Renderer for BlurFilterShape
    /// Can render multiple inverted filters in a single operation by combining their areas
    /// </summary>
    public class BlurFilterRenderer : IShapeRenderer
    {
        public bool CanRender(IShape shape)
        {
            return shape is BlurFilterShape;
        }

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is BlurFilterShape blurFilter))
            {
                return;
            }

            RenderSingle(graphics, blurFilter, NativeRect.Empty);
        }

        /// <summary>
        /// Renders multiple inverted blur filters in one operation by combining their areas
        /// </summary>
        public void RenderInvertedBatch(Graphics graphics, IEnumerable<BlurFilterShape> filters, NativeRect canvasBounds)
        {
            var filterList = filters.ToList();
            if (!filterList.Any())
            {
                return;
            }

            // Combine all inverted filter areas into a single path
            using (var path = new GraphicsPath())
            {
                // Add the entire canvas as the base
                path.AddRectangle(new Rectangle(canvasBounds.X, canvasBounds.Y, canvasBounds.Width, canvasBounds.Height));

                // Cut out each filter's area
                foreach (var filter in filterList)
                {
                    var rect = new Rectangle(filter.Bounds.X, filter.Bounds.Y, filter.Bounds.Width, filter.Bounds.Height);
                    path.AddRectangle(rect);
                }

                // Use the average pixel size if multiple filters
                int avgPixelSize = (int)filterList.Average(f => f.PixelSize);

                // Apply blur effect (placeholder - real implementation would pixelate)
                using (var brush = new SolidBrush(Color.FromArgb(128, Color.Gray)))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void RenderSingle(Graphics graphics, BlurFilterShape filter, NativeRect canvasBounds)
        {
            var rect = new Rectangle(filter.Bounds.X, filter.Bounds.Y, filter.Bounds.Width, filter.Bounds.Height);

            if (filter.IsInverted && canvasBounds != NativeRect.Empty)
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

    /// <summary>
    /// Renderer for HighlightFilterShape
    /// Can render multiple inverted filters in a single operation by combining their areas
    /// </summary>
    public class HighlightFilterRenderer : IShapeRenderer
    {
        public bool CanRender(IShape shape)
        {
            return shape is HighlightFilterShape;
        }

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is HighlightFilterShape highlightFilter))
            {
                return;
            }

            RenderSingle(graphics, highlightFilter, NativeRect.Empty);
        }

        /// <summary>
        /// Renders multiple inverted highlight filters in one operation by combining their areas
        /// </summary>
        public void RenderInvertedBatch(Graphics graphics, IEnumerable<HighlightFilterShape> filters, NativeRect canvasBounds)
        {
            var filterList = filters.ToList();
            if (!filterList.Any())
            {
                return;
            }

            // Combine all inverted filter areas into a single path
            using (var path = new GraphicsPath())
            {
                // Add the entire canvas as the base
                path.AddRectangle(new Rectangle(canvasBounds.X, canvasBounds.Y, canvasBounds.Width, canvasBounds.Height));

                // Cut out each filter's area
                foreach (var filter in filterList)
                {
                    var rect = new Rectangle(filter.Bounds.X, filter.Bounds.Y, filter.Bounds.Width, filter.Bounds.Height);
                    path.AddRectangle(rect);
                }

                // Use the first filter's color and average opacity
                var firstFilter = filterList.First();
                int avgOpacity = (int)filterList.Average(f => f.Opacity);

                using (var brush = new SolidBrush(Color.FromArgb(avgOpacity, firstFilter.HighlightColor)))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }

        private void RenderSingle(Graphics graphics, HighlightFilterShape filter, NativeRect canvasBounds)
        {
            var rect = new Rectangle(filter.Bounds.X, filter.Bounds.Y, filter.Bounds.Width, filter.Bounds.Height);

            if (filter.IsInverted && canvasBounds != NativeRect.Empty)
            {
                // Dim everything except the area
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(canvasBounds.X, canvasBounds.Y, canvasBounds.Width, canvasBounds.Height));
                    path.AddRectangle(rect);
                    using (var brush = new SolidBrush(Color.FromArgb(filter.Opacity, filter.HighlightColor)))
                    {
                        graphics.FillPath(brush, path);
                    }
                }
            }
            else
            {
                // Highlight only the area
                using (var brush = new SolidBrush(Color.FromArgb(filter.Opacity, filter.HighlightColor)))
                {
                    graphics.FillRectangle(brush, rect);
                }
            }
        }
    }
}
