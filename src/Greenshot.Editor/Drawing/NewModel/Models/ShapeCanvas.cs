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

namespace Greenshot.Editor.Drawing.NewModel.Models
{
    /// <summary>
    /// Manages a collection of shapes and layers on a canvas.
    /// Pure data container - no rendering or UI logic.
    /// Filters are now shapes (IFilterShape) and are managed just like other shapes.
    /// </summary>
    public class ShapeCanvas
    {
        private readonly List<IShape> _shapes = new List<IShape>();
        private readonly List<Layer> _layers = new List<Layer>();
        private Layer _backgroundLayer;
        private Layer _defaultLayer;

        public ShapeCanvas()
        {
            // Create implicit background layer
            _backgroundLayer = Layer.CreateBackgroundLayer();
            _layers.Add(_backgroundLayer);

            // Create default layer for shapes
            _defaultLayer = Layer.CreateDefaultLayer();
            _layers.Add(_defaultLayer);
        }

        /// <summary>
        /// All shapes on the canvas (includes filter shapes)
        /// </summary>
        public IReadOnlyList<IShape> Shapes => _shapes.AsReadOnly();

        /// <summary>
        /// All layers on the canvas
        /// </summary>
        public IReadOnlyList<Layer> Layers => _layers.AsReadOnly();

        /// <summary>
        /// The background layer (implicit, always exists)
        /// </summary>
        public Layer BackgroundLayer => _backgroundLayer;

        /// <summary>
        /// The default layer for shapes
        /// </summary>
        public Layer DefaultLayer => _defaultLayer;

        /// <summary>
        /// Adds a shape to the canvas
        /// </summary>
        public void AddShape(IShape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            // Assign to default layer if not assigned
            if (shape.LayerId == null)
            {
                if (shape is BackgroundShape)
                {
                    shape.LayerId = _backgroundLayer.Id;
                }
                else
                {
                    shape.LayerId = _defaultLayer.Id;
                }
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
        /// Gets all shapes in a specific layer
        /// </summary>
        public IEnumerable<IShape> GetShapesInLayer(Guid layerId)
        {
            return _shapes.Where(s => s.LayerId == layerId);
        }

        /// <summary>
        /// Adds a layer to the canvas
        /// </summary>
        public void AddLayer(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException(nameof(layer));
            }

            _layers.Add(layer);
        }

        /// <summary>
        /// Removes a layer (shapes in that layer will have their LayerId set to null)
        /// </summary>
        public bool RemoveLayer(Layer layer)
        {
            if (layer == _backgroundLayer || layer == _defaultLayer)
            {
                throw new InvalidOperationException("Cannot remove background or default layer");
            }

            // Clear layer assignment from shapes
            foreach (var shape in _shapes.Where(s => s.LayerId == layer.Id))
            {
                shape.LayerId = null;
            }

            return _layers.Remove(layer);
        }

        /// <summary>
        /// Gets a layer by ID
        /// </summary>
        public Layer GetLayerById(Guid id)
        {
            return _layers.FirstOrDefault(l => l.Id == id);
        }

        /// <summary>
        /// Clears all shapes from the canvas
        /// </summary>
        public void Clear()
        {
            _shapes.Clear();
        }

        /// <summary>
        /// Brings a shape to the front within its layer
        /// </summary>
        public void BringToFront(IShape shape)
        {
            if (_shapes.Remove(shape))
            {
                _shapes.Add(shape);
            }
        }

        /// <summary>
        /// Sends a shape to the back within its layer
        /// </summary>
        public void SendToBack(IShape shape)
        {
            if (_shapes.Remove(shape))
            {
                _shapes.Insert(0, shape);
            }
        }

        /// <summary>
        /// Gets shapes ordered by layer Z-index and position within layer
        /// </summary>
        public IEnumerable<IShape> GetShapesOrderedByLayer()
        {
            var layerOrder = _layers.OrderBy(l => l.ZIndex).ThenBy(l => l.Id).ToList();
            var orderedShapes = new List<IShape>();

            foreach (var layer in layerOrder)
            {
                if (layer.IsVisible)
                {
                    orderedShapes.AddRange(_shapes.Where(s => s.LayerId == layer.Id));
                }
            }

            // Add shapes without layer assignment
            orderedShapes.AddRange(_shapes.Where(s => s.LayerId == null));

            return orderedShapes;
        }
    }
}
