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
using System.Drawing.Drawing2D;
using Greenshot.Editor.Drawing.NewModel.Models;

namespace Greenshot.Editor.Drawing.NewModel.Renderers
{
    /// <summary>
    /// Renderer for arrow shapes with optional arrow heads at start and/or end points.
    /// </summary>
    public class ArrowRenderer : IShapeRenderer
    {
        private static readonly AdjustableArrowCap ARROW_CAP = new AdjustableArrowCap(4, 6);

        public void Render(Graphics graphics, IShape shape)
        {
            if (!(shape is ArrowShape arrow)) return;
            if (arrow.Style == null) return;

            var lineColor = arrow.Style.LineColor;
            var lineThickness = arrow.Style.LineThickness;
            var hasShadow = arrow.Style.HasShadow;

            if (lineThickness <= 0) return;

            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.None;

            var startPoint = arrow.StartPoint;
            var endPoint = arrow.EndPoint;

            if (hasShadow)
            {
                // Draw shadow
                int basealpha = 100;
                int alpha = basealpha;
                int steps = 5;
                int currentStep = 1;
                
                while (currentStep <= steps)
                {
                    using (Pen shadowPen = new Pen(Color.FromArgb(alpha, 100, 100, 100), lineThickness))
                    {
                        SetArrowHeads(arrow.ArrowHeads, shadowPen);
                        graphics.DrawLine(shadowPen,
                            startPoint.X + currentStep,
                            startPoint.Y + currentStep,
                            endPoint.X + currentStep,
                            endPoint.Y + currentStep);
                    }
                    currentStep++;
                    alpha -= basealpha / steps;
                }
            }

            // Draw arrow line
            using (Pen pen = new Pen(lineColor, lineThickness))
            {
                SetArrowHeads(arrow.ArrowHeads, pen);
                graphics.DrawLine(pen, startPoint, endPoint);
            }
        }

        private void SetArrowHeads(ArrowShape.ArrowHeadCombination heads, Pen pen)
        {
            if (heads == ArrowShape.ArrowHeadCombination.Both || heads == ArrowShape.ArrowHeadCombination.StartPoint)
            {
                pen.CustomStartCap = ARROW_CAP;
            }

            if (heads == ArrowShape.ArrowHeadCombination.Both || heads == ArrowShape.ArrowHeadCombination.EndPoint)
            {
                pen.CustomEndCap = ARROW_CAP;
            }
        }
    }
}
