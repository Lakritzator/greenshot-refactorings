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

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Coordinates rendering of shapes and their editor state.
    /// Uses registered renderers to draw shapes and adorners.
    /// </summary>
    public class CanvasRenderer
    {
        private readonly List<IShapeRenderer> _shapeRenderers = new List<IShapeRenderer>();
        private readonly AdornerRenderer _adornerRenderer = new AdornerRenderer();

        public CanvasRenderer()
        {
            // Register default renderers
            RegisterRenderer(new RectangleRenderer());
            RegisterRenderer(new EllipseRenderer());
            RegisterRenderer(new TextRenderer());
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
        /// Renders all shapes in a canvas
        /// </summary>
        public void RenderCanvas(Graphics graphics, ShapeCanvas canvas)
        {
            if (graphics == null || canvas == null)
            {
                return;
            }

            foreach (var shape in canvas.Shapes)
            {
                RenderShape(graphics, shape);
            }
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

            // Create a lookup for editor states by shape ID
            var stateMap = editorStates?.ToDictionary(s => s.Shape.Id) ?? new Dictionary<Guid, ShapeEditorState>();

            // Render shapes
            foreach (var shape in canvas.Shapes)
            {
                RenderShape(graphics, shape);
            }

            // Render adorners for selected shapes on top
            foreach (var state in stateMap.Values)
            {
                _adornerRenderer.RenderAdorners(graphics, state);
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
    }
}
