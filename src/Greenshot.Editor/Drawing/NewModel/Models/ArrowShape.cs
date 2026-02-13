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
    /// Arrow shape that represents a line with optional arrow heads at start and/or end points.
    /// Unlike other shapes, arrows use start/end points rather than bounds, but store them as bounds for consistency.
    /// </summary>
    public class ArrowShape : IShape
    {
        public enum ArrowHeadCombination
        {
            None,
            StartPoint,
            EndPoint,
            Both
        }

        public Guid Id { get; set; }
        public NativeRect Bounds { get; set; }
        public IShapeStyle Style { get; set; }
        public Guid LayerId { get; set; }
        
        /// <summary>
        /// Which arrow heads to display (none, start, end, or both)
        /// </summary>
        public ArrowHeadCombination ArrowHeads { get; set; }

        /// <summary>
        /// Start point of the arrow (stored as bounds.TopLeft)
        /// </summary>
        public Point StartPoint
        {
            get => new Point(Bounds.Left, Bounds.Top);
            set
            {
                var endPoint = EndPoint;
                Bounds = NativeRect.FromLTRB(
                    Math.Min(value.X, endPoint.X),
                    Math.Min(value.Y, endPoint.Y),
                    Math.Max(value.X, endPoint.X),
                    Math.Max(value.Y, endPoint.Y)
                );
            }
        }

        /// <summary>
        /// End point of the arrow (stored as bounds.TopLeft + bounds.Size)
        /// </summary>
        public Point EndPoint
        {
            get => new Point(Bounds.Left + Bounds.Width, Bounds.Top + Bounds.Height);
            set
            {
                var startPoint = StartPoint;
                Bounds = NativeRect.FromLTRB(
                    Math.Min(startPoint.X, value.X),
                    Math.Min(startPoint.Y, value.Y),
                    Math.Max(startPoint.X, value.X),
                    Math.Max(startPoint.Y, value.Y)
                );
            }
        }

        public ArrowShape(Point startPoint, Point endPoint, IShapeStyle style, ArrowHeadCombination arrowHeads = ArrowHeadCombination.EndPoint)
        {
            Id = Guid.NewGuid();
            Style = style;
            ArrowHeads = arrowHeads;
            
            // Store as bounds for consistency with other shapes
            Bounds = NativeRect.FromLTRB(
                Math.Min(startPoint.X, endPoint.X),
                Math.Min(startPoint.Y, endPoint.Y),
                Math.Max(startPoint.X, endPoint.X),
                Math.Max(startPoint.Y, endPoint.Y)
            );
        }

        public IShape Clone()
        {
            return new ArrowShape(StartPoint, EndPoint, Style, ArrowHeads)
            {
                Id = Guid.NewGuid(),
                LayerId = LayerId
            };
        }
    }
}