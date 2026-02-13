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
using System.Linq;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Manages a collection of shapes on a canvas.
    /// Pure data container - no rendering or UI logic.
    /// </summary>
    public class ShapeCanvas
    {
        private readonly List<IShape> _shapes = new List<IShape>();

        /// <summary>
        /// All shapes on the canvas
        /// </summary>
        public IReadOnlyList<IShape> Shapes => _shapes.AsReadOnly();

        /// <summary>
        /// Adds a shape to the canvas
        /// </summary>
        public void AddShape(IShape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            _shapes.Add(shape);
        }

        /// <summary>
        /// Removes a shape from the canvas
        /// </summary>
        public bool RemoveShape(IShape shape)
        {
            return _shapes.Remove(shape);
        }

        /// <summary>
        /// Removes a shape by its ID
        /// </summary>
        public bool RemoveShapeById(Guid id)
        {
            var shape = _shapes.FirstOrDefault(s => s.Id == id);
            if (shape != null)
            {
                return _shapes.Remove(shape);
            }

            return false;
        }

        /// <summary>
        /// Gets a shape by its ID
        /// </summary>
        public IShape GetShapeById(Guid id)
        {
            return _shapes.FirstOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Clears all shapes from the canvas
        /// </summary>
        public void Clear()
        {
            _shapes.Clear();
        }

        /// <summary>
        /// Brings a shape to the front (end of list = drawn last = on top)
        /// </summary>
        public void BringToFront(IShape shape)
        {
            if (_shapes.Remove(shape))
            {
                _shapes.Add(shape);
            }
        }

        /// <summary>
        /// Sends a shape to the back (start of list = drawn first = on bottom)
        /// </summary>
        public void SendToBack(IShape shape)
        {
            if (_shapes.Remove(shape))
            {
                _shapes.Insert(0, shape);
            }
        }
    }
}
