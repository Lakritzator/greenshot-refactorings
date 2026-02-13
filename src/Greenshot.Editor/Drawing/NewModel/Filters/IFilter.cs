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

namespace Greenshot.Editor.Drawing.NewModel.Filters
{
    /// <summary>
    /// Base interface for filters that can be applied to areas of the canvas
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Unique identifier for this filter instance
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Layer this filter belongs to
        /// </summary>
        Guid? LayerId { get; set; }

        /// <summary>
        /// The area this filter applies to
        /// </summary>
        NativeRect Area { get; set; }

        /// <summary>
        /// Whether this is an inverted filter (applies everywhere EXCEPT the area)
        /// </summary>
        bool IsInverted { get; set; }

        /// <summary>
        /// Applies the filter to the graphics context
        /// </summary>
        void Apply(Graphics graphics, NativeRect canvasBounds);
    }
}
