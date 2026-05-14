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
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Renderer for TextShape
    /// </summary>
    public class TextRenderer : IShapeRenderer
    {
        public bool CanRender(IShape shape)
        {
            return shape is TextShape;
        }

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is TextShape textShape))
            {
                return;
            }

            var bounds = shape.Bounds;
            var style = shape.Style;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            // Draw shadow if needed
            if (style.Shadow)
            {
                var shadowRect = rect;
                shadowRect.Offset(2, 2);
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                {
                    graphics.DrawString(textShape.Text, textShape.Font, shadowBrush, shadowRect);
                }
            }

            // Fill background if fill color is specified
            if (style.FillColor != Color.Empty)
            {
                using (var fillBrush = new SolidBrush(style.FillColor))
                {
                    graphics.FillRectangle(fillBrush, rect);
                }
            }

            // Draw text
            if (!string.IsNullOrEmpty(textShape.Text))
            {
                var textColor = style.LineColor != Color.Empty ? style.LineColor : Color.Black;
                using (var textBrush = new SolidBrush(textColor))
                {
                    graphics.DrawString(textShape.Text, textShape.Font, textBrush, rect);
                }
            }

            // Draw border if needed
            if (style.LineThickness > 0 && style.LineColor != Color.Empty)
            {
                using (var pen = new Pen(style.LineColor, style.LineThickness))
                {
                    graphics.DrawRectangle(pen, Rectangle.Round(rect));
                }
            }
        }
    }
}
