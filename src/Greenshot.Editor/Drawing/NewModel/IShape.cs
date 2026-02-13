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
using Dapplo.Windows.Common.Structs;

namespace Greenshot.Editor.Drawing.NewModel
{
    /// <summary>
    /// Represents the pure data model for a drawable shape.
    /// Contains only the essential geometric and property data needed to define the shape.
    /// No drawing logic, no UI state (selection, adorners), no parent references.
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// Unique identifier for this shape instance
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Bounding rectangle for the shape
        /// </summary>
        NativeRect Bounds { get; set; }

        /// <summary>
        /// Style applied to this shape (can be shared across multiple shapes)
        /// </summary>
        IShapeStyle Style { get; set; }

        /// <summary>
        /// Creates a deep copy of this shape
        /// </summary>
        IShape Clone();
    }
}
