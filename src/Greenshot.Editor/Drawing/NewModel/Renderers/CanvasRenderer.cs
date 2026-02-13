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
using System.Linq;
using Dapplo.Windows.Common.Structs;
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Coordinates rendering of shapes and their editor state.
    /// Renders layer-by-layer with special handling for inverted filter shapes.
    /// </summary>
    public class CanvasRenderer
    {
        private readonly List<IShapeRenderer> _shapeRenderers = new List<IShapeRenderer>();
        private readonly AdornerRenderer _adornerRenderer = new AdornerRenderer();
        private readonly BlurFilterRenderer _blurFilterRenderer = new BlurFilterRenderer();
        private readonly HighlightFilterRenderer _highlightFilterRenderer = new HighlightFilterRenderer();

        public CanvasRenderer()
        {
            // Register default renderers
            RegisterRenderer(new RectangleRenderer());
            RegisterRenderer(new EllipseRenderer());
            RegisterRenderer(new TextRenderer());
            RegisterRenderer(new ArrowRenderer());
            RegisterRenderer(new ImageRenderer());
            RegisterRenderer(new CursorRenderer());
            RegisterRenderer(new SpeechBubbleRenderer());
            RegisterRenderer(_blurFilterRenderer);
            RegisterRenderer(_highlightFilterRenderer);
        }

        /// <summary>
        /// Registers a shape renderer
        /// </summary>
        public void RegisterRenderer(IShapeRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            _shapeRenderers.Add(renderer);
        }

        /// <summary>
        /// Renders all shapes in a canvas layer-by-layer
        /// </summary>
        public void RenderCanvas(Graphics graphics, ShapeCanvas canvas)
        {
            if (graphics == null || canvas == null)
            {
                return;
            }

            var canvasBounds = CalculateContentBounds(canvas);
            RenderLayerByLayer(graphics, canvas, canvasBounds, null);
        }

        /// <summary>
        /// Renders all shapes in a canvas with their editor states
        /// </summary>
        public void RenderCanvas(Graphics graphics, ShapeCanvas canvas, IEnumerable<ShapeEditorState> editorStates)
        {
            if (graphics == null || canvas == null)
            {
                return;
            }

            var canvasBounds = CalculateContentBounds(canvas);
            var stateMap = editorStates?.ToDictionary(s => s.Shape.Id) ?? new Dictionary<Guid, ShapeEditorState>();

            RenderLayerByLayer(graphics, canvas, canvasBounds, stateMap);

            // Render adorners for selected shapes on top of everything
            foreach (var state in stateMap.Values)
            {
                _adornerRenderer.RenderAdorners(graphics, state);
            }
        }

        /// <summary>
        /// Renders canvas layer-by-layer with special handling for inverted filters
        /// </summary>
        private void RenderLayerByLayer(Graphics graphics, ShapeCanvas canvas, NativeRect canvasBounds, Dictionary<Guid, ShapeEditorState> stateMap)
        {
            var layers = canvas.Layers.OrderBy(l => l.ZIndex).ThenBy(l => l.Id).ToList();

            foreach (var layer in layers)
            {
                if (!layer.IsVisible)
                {
                    continue;
                }

                var shapesInLayer = canvas.Shapes.Where(s => s.LayerId == layer.Id).ToList();

                // Separate inverted filters from other shapes
                var invertedBlurFilters = shapesInLayer.OfType<BlurFilterShape>().Where(f => f.IsInverted).ToList();
                var invertedHighlightFilters = shapesInLayer.OfType<HighlightFilterShape>().Where(f => f.IsInverted).ToList();
                var otherShapes = shapesInLayer.Where(s => !(s is IFilterShape f && f.IsInverted)).ToList();

                // Render non-inverted shapes first
                foreach (var shape in otherShapes)
                {
                    RenderShape(graphics, shape);
                }

                // Render inverted blur filters in batch (combined into one operation)
                if (invertedBlurFilters.Any())
                {
                    _blurFilterRenderer.RenderInvertedBatch(graphics, invertedBlurFilters, canvasBounds);
                }

                // Render inverted highlight filters in batch (combined into one operation)
                if (invertedHighlightFilters.Any())
                {
                    _highlightFilterRenderer.RenderInvertedBatch(graphics, invertedHighlightFilters, canvasBounds);
                }
            }

            // Render shapes without layer assignment
            var unassignedShapes = canvas.Shapes.Where(s => s.LayerId == null).ToList();
            foreach (var shape in unassignedShapes)
            {
                RenderShape(graphics, shape);
            }
        }

        /// <summary>
        /// Renders a single shape
        /// </summary>
        public void RenderShape(Graphics graphics, IShape shape)
        {
            if (graphics == null || shape == null)
            {
                return;
            }

            var renderer = _shapeRenderers.FirstOrDefault(r => r.CanRender(shape));
            if (renderer != null)
            {
                renderer.Render(graphics, shape);
            }
        }

        /// <summary>
        /// Renders a single shape with its editor state
        /// </summary>
        public void RenderShape(Graphics graphics, ShapeEditorState state)
        {
            if (graphics == null || state == null)
            {
                return;
            }

            RenderShape(graphics, state.Shape);
            _adornerRenderer.RenderAdorners(graphics, state);
        }

        /// <summary>
        /// Calculates the bounding rectangle for all shapes on the canvas
        /// </summary>
        private NativeRect CalculateContentBounds(ShapeCanvas canvas)
        {
            if (!canvas.Shapes.Any())
            {
                return new NativeRect(0, 0, 0, 0);
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var shape in canvas.Shapes)
            {
                var bounds = shape.Bounds;
                minX = Math.Min(minX, bounds.Left);
                minY = Math.Min(minY, bounds.Top);
                maxX = Math.Max(maxX, bounds.Right);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            return new NativeRect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
