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

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// Immutable metadata record describing a single translation file.
    /// Populated from the filename alone — no file content is read.
    /// </summary>
    public sealed class TranslationFile : IEquatable<TranslationFile>
    {
        /// <summary>
        /// IETF language tag extracted from the filename, e.g. <c>"en-US"</c>
        /// or <c>"zh-TW"</c>.
        /// </summary>
        public string Ietf { get; init; }

        /// <summary>
        /// Human-readable language name read from the file header on first
        /// load, e.g. <c>"English"</c>.  May be <c>null</c> before loading.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Plugin namespace prefix extracted from the filename, e.g. <c>"box"</c>
        /// for <c>box.en-US.json</c>.  <c>null</c> for main-application files.
        /// </summary>
        public string Prefix { get; init; }

        /// <summary>Full absolute path to the translation file.</summary>
        public string FilePath { get; init; }

        /// <summary>
        /// Version declared inside the file header, e.g. <c>1.0</c>.
        /// May be <c>null</c> before the file has been loaded.
        /// </summary>
        public Version Version { get; init; }

        /// <inheritdoc/>
        public bool Equals(TranslationFile other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as TranslationFile);

        /// <inheritdoc/>
        public override int GetHashCode()
            => FilePath is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(FilePath);

        /// <inheritdoc/>
        public override string ToString() => $"{Ietf} [{Prefix ?? "main"}] ({FilePath})";
    }
}
