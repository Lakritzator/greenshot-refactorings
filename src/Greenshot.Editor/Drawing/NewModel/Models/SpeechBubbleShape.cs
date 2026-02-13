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
using System.Collections.Generic;
using System.Drawing;
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Speech bubble shape with a rectangular body and a tail that can be positioned independently.
    /// Demonstrates IAdornerConfiguration for custom adorner positioning.
    /// </summary>
    public class SpeechBubbleShape : IShape, IAdornerConfiguration
    {
        public Guid Id { get; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }
        public Guid? LayerId { get; set; }

        /// <summary>
        /// Position of the tail tip (where the speech bubble points to)
        /// This is relative to the canvas, not the bubble bounds
        /// </summary>
        public Point TailPosition { get; set; }

        public SpeechBubbleShape(NativeRect bounds, Point tailPosition, IShapeStyle style = null)
        {
            Id = Guid.NewGuid();
            Bounds = bounds;
            TailPosition = tailPosition;
            Style = style ?? ShapeStyle.Default();
        }

        private SpeechBubbleShape(Guid id, NativeRect bounds, Point tailPosition, IShapeStyle style, Guid? layerId)
        {
            Id = id;
            Bounds = bounds;
            TailPosition = tailPosition;
            Style = style;
            LayerId = layerId;
        }

        public IShape Clone()
        {
            return new SpeechBubbleShape(Guid.NewGuid(), Bounds, TailPosition, Style, LayerId);
        }

        /// <summary>
        /// Provides custom adorners for the speech bubble:
        /// - Standard 8 adorners for resizing the bubble rectangle
        /// - 1 special adorner for moving the tail tip (orange color to distinguish it)
        /// </summary>
        public IEnumerable<CustomAdorner> GetAdorners(IShape shape)
        {
            if (!(shape is SpeechBubbleShape bubble))
            {
                yield break;
            }

            var bounds = bubble.Bounds;

            // Standard 8 adorners for resizing the bubble body (corners and edges)
            yield return new CustomAdorner("top-left", new Point(bounds.Left, bounds.Top), Color.White, 7);
            yield return new CustomAdorner("top-center", new Point(bounds.Left + bounds.Width / 2, bounds.Top), Color.White, 7);
            yield return new CustomAdorner("top-right", new Point(bounds.Right, bounds.Top), Color.White, 7);
            yield return new CustomAdorner("middle-left", new Point(bounds.Left, bounds.Top + bounds.Height / 2), Color.White, 7);
            yield return new CustomAdorner("middle-right", new Point(bounds.Right, bounds.Top + bounds.Height / 2), Color.White, 7);
            yield return new CustomAdorner("bottom-left", new Point(bounds.Left, bounds.Bottom), Color.White, 7);
            yield return new CustomAdorner("bottom-center", new Point(bounds.Left + bounds.Width / 2, bounds.Bottom), Color.White, 7);
            yield return new CustomAdorner("bottom-right", new Point(bounds.Right, bounds.Bottom), Color.White, 7);

            // Special adorner for the tail tip - orange color to make it visually distinct
            yield return new CustomAdorner("tail-tip", bubble.TailPosition, Color.Orange, 8);
        }
    }
}
