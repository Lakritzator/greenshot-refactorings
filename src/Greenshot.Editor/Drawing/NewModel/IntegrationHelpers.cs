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

using System.Drawing;
using System.Linq;
using Dapplo.Windows.Common.Structs;
using Greenshot.Editor.Drawing.Fields;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Integration helpers for using the new drawing system alongside the existing DrawableContainer system.
    /// Provides conversion utilities and compatibility layers.
    /// </summary>
    public static class IntegrationHelpers
    {
        /// <summary>
        /// Converts a DrawableContainer to the new IShape representation.
        /// This allows gradual migration - existing containers can be converted to new shapes.
        /// </summary>
        public static IShape ConvertToShape(DrawableContainer container)
        {
            if (container == null)
            {
                return null;
            }

            // Extract style from fields
            var style = ExtractStyleFromContainer(container);
            var bounds = container.Bounds;

            // Convert based on container type
            if (container is RectangleContainer)
            {
                return new RectangleShape(bounds, style);
            }
            else if (container is EllipseContainer)
            {
                return new EllipseShape(bounds, style);
            }
            else if (container is TextContainer textContainer)
            {
                var text = textContainer.Text ?? string.Empty;
                var font = ExtractFontFromContainer(textContainer);
                return new TextShape(bounds, style, text, font);
            }

            // For unknown types, create a generic rectangle representation
            return new RectangleShape(bounds, style);
        }

        /// <summary>
        /// Converts a new IShape back to a DrawableContainer for backward compatibility.
        /// Useful when the existing system still needs DrawableContainer instances.
        /// </summary>
        public static DrawableContainer ConvertToContainer(IShape shape, ISurface surface)
        {
            if (shape == null || surface == null)
            {
                return null;
            }

            DrawableContainer container = null;

            if (shape is RectangleShape)
            {
                container = new RectangleContainer(surface);
            }
            else if (shape is EllipseShape)
            {
                container = new EllipseContainer(surface);
            }
            else if (shape is TextShape textShape)
            {
                var textContainer = new TextContainer(surface);
                textContainer.Text = textShape.Text;
                ApplyFontToContainer(textContainer, textShape.Font);
                container = textContainer;
            }

            if (container != null)
            {
                container.Bounds = shape.Bounds;
                ApplyStyleToContainer(container, shape.Style);
            }

            return container;
        }

        /// <summary>
        /// Extracts style information from a DrawableContainer's fields
        /// </summary>
        private static IShapeStyle ExtractStyleFromContainer(DrawableContainer container)
        {
            var lineColor = Color.Black;
            var lineThickness = 1;
            var fillColor = Color.Empty;
            var shadow = false;

            // Extract from fields if available
            if (container.HasField(FieldType.LINE_COLOR))
            {
                var field = container.GetField(FieldType.LINE_COLOR);
                if (field != null && field.HasValue)
                {
                    lineColor = (Color)field.Value;
                }
            }

            if (container.HasField(FieldType.LINE_THICKNESS))
            {
                var field = container.GetField(FieldType.LINE_THICKNESS);
                if (field != null && field.HasValue)
                {
                    lineThickness = (int)field.Value;
                }
            }

            if (container.HasField(FieldType.FILL_COLOR))
            {
                var field = container.GetField(FieldType.FILL_COLOR);
                if (field != null && field.HasValue)
                {
                    fillColor = (Color)field.Value;
                }
            }

            if (container.HasField(FieldType.SHADOW))
            {
                var field = container.GetField(FieldType.SHADOW);
                if (field != null && field.HasValue)
                {
                    shadow = (bool)field.Value;
                }
            }

            return new ShapeStyle(lineColor, lineThickness, fillColor, shadow);
        }

        /// <summary>
        /// Applies style from IShapeStyle to a DrawableContainer's fields
        /// </summary>
        private static void ApplyStyleToContainer(DrawableContainer container, IShapeStyle style)
        {
            if (style == null)
            {
                return;
            }

            if (container.HasField(FieldType.LINE_COLOR))
            {
                container.GetField(FieldType.LINE_COLOR).Value = style.LineColor;
            }

            if (container.HasField(FieldType.LINE_THICKNESS))
            {
                container.GetField(FieldType.LINE_THICKNESS).Value = style.LineThickness;
            }

            if (container.HasField(FieldType.FILL_COLOR))
            {
                container.GetField(FieldType.FILL_COLOR).Value = style.FillColor;
            }

            if (container.HasField(FieldType.SHADOW))
            {
                container.GetField(FieldType.SHADOW).Value = style.Shadow;
            }
        }

        /// <summary>
        /// Extracts font from a TextContainer
        /// </summary>
        private static Font ExtractFontFromContainer(TextContainer container)
        {
            // TextContainer has Font property or field - extract it
            // This is a simplified version; actual implementation may need field access
            return container.Font ?? SystemFonts.DefaultFont;
        }

        /// <summary>
        /// Applies font to a TextContainer
        /// </summary>
        private static void ApplyFontToContainer(TextContainer container, Font font)
        {
            if (font != null)
            {
                container.Font = font;
            }
        }

        /// <summary>
        /// Creates a ShapeCanvas from a DrawableContainerList (existing surface elements)
        /// </summary>
        public static ShapeCanvas ConvertSurfaceToCanvas(DrawableContainerList containers)
        {
            var canvas = new ShapeCanvas();

            if (containers == null)
            {
                return canvas;
            }

            foreach (var container in containers)
            {
                var shape = ConvertToShape(container);
                if (shape != null)
                {
                    canvas.AddShape(shape);
                }
            }

            return canvas;
        }

        /// <summary>
        /// Converts a ShapeCanvas back to DrawableContainerList for the existing surface
        /// </summary>
        public static DrawableContainerList ConvertCanvasToSurface(ShapeCanvas canvas, ISurface surface)
        {
            var containerList = new DrawableContainerList(surface);

            if (canvas == null)
            {
                return containerList;
            }

            foreach (var shape in canvas.Shapes)
            {
                var container = ConvertToContainer(shape, surface);
                if (container != null)
                {
                    containerList.Add(container);
                }
            }

            return containerList;
        }

        /// <summary>
        /// Example: Render a canvas using the new system on a Surface's Graphics context
        /// This shows how to integrate new rendering into existing editor workflow
        /// </summary>
        public static void RenderCanvasOnSurface(Surface surface, ShapeCanvas canvas)
        {
            if (surface == null || canvas == null)
            {
                return;
            }

            // Get graphics context from surface
            using (var graphics = surface.CreateGraphics())
            {
                // Use new renderer
                var renderer = new CanvasRenderer();
                renderer.RenderCanvas(graphics, canvas);
            }
        }
    }
}
