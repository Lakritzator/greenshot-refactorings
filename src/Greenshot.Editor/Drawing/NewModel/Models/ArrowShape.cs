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
using Greenshot.Editor.Drawing.NewModel.Renderers;

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Arrow shape that represents a line with optional arrow heads at start and/or end points.
    /// Unlike other shapes, arrows use start/end points rather than bounds, but store them as bounds for consistency.
    /// Implements IAdornerConfiguration to provide only 2 adorners (start and end points).
    /// </summary>
    public class ArrowShape : IShape, IAdornerConfiguration
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
        /// Actual start point of the arrow (not normalized to bounds)
        /// </summary>
        private Point _actualStartPoint;
        
        /// <summary>
        /// Actual end point of the arrow (not normalized to bounds)
        /// </summary>
        private Point _actualEndPoint;

        /// <summary>
        /// Start point of the arrow
        /// </summary>
        public Point StartPoint
        {
            get => _actualStartPoint;
            set
            {
                _actualStartPoint = value;
                UpdateBounds();
            }
        }

        /// <summary>
        /// End point of the arrow
        /// </summary>
        public Point EndPoint
        {
            get => _actualEndPoint;
            set
            {
                _actualEndPoint = value;
                UpdateBounds();
            }
        }

        private void UpdateBounds()
        {
            Bounds = NativeRect.FromLTRB(
                Math.Min(_actualStartPoint.X, _actualEndPoint.X),
                Math.Min(_actualStartPoint.Y, _actualEndPoint.Y),
                Math.Max(_actualStartPoint.X, _actualEndPoint.X),
                Math.Max(_actualStartPoint.Y, _actualEndPoint.Y)
            );
        }

        public ArrowShape(Point startPoint, Point endPoint, IShapeStyle style, ArrowHeadCombination arrowHeads = ArrowHeadCombination.EndPoint)
        {
            Id = Guid.NewGuid();
            Style = style;
            ArrowHeads = arrowHeads;
            _actualStartPoint = startPoint;
            _actualEndPoint = endPoint;
            UpdateBounds();
        }

        public IShape Clone()
        {
            return new ArrowShape(StartPoint, EndPoint, Style, ArrowHeads)
            {
                Id = Guid.NewGuid(),
                LayerId = LayerId
            };
        }

        /// <summary>
        /// Arrows only need 2 adorners: start point and end point
        /// </summary>
        public IEnumerable<CustomAdorner> GetAdorners(IShape shape)
        {
            var arrow = shape as ArrowShape;
            if (arrow == null) yield break;

            // Start point adorner (index 0)
            yield return new CustomAdorner("start", arrow.StartPoint, Color.White, 8);
            
            // End point adorner (index 1)
            yield return new CustomAdorner("end", arrow.EndPoint, Color.White, 8);
        }
    }
}
