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
using System.Globalization;

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// Immutable metadata record describing a single translation file.
    /// All information is derived from the filename alone — no file content
    /// is read during discovery.
    ///
    /// <para>
    /// The human-readable language name (<see cref="DisplayName"/>) is
    /// resolved at runtime from <see cref="CultureInfo"/> and does not need
    /// to be stored inside the file.  Versioning is handled alongside the
    /// source code, not inside individual translation files.
    /// </para>
    /// </summary>
    public sealed class TranslationFile : IEquatable<TranslationFile>
    {
        /// <summary>
        /// IETF language tag extracted from the filename, e.g. <c>"en-US"</c>
        /// or <c>"zh-TW"</c>.
        /// </summary>
        public string Ietf { get; init; }

        /// <summary>
        /// Plugin namespace prefix extracted from the filename, e.g. <c>"box"</c>
        /// for <c>box.en-US.ini</c>.  <c>null</c> for main-application files.
        /// </summary>
        public string Prefix { get; init; }

        /// <summary>Full absolute path to the translation file.</summary>
        public string FilePath { get; init; }

        /// <summary>
        /// Native display name for the language, resolved from
        /// <see cref="CultureInfo"/> using <see cref="Ietf"/>.
        /// For example <c>"English (United States)"</c> for <c>"en-US"</c>.
        /// Falls back to the raw <see cref="Ietf"/> tag when the culture is
        /// not recognised by the runtime.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (Ietf is null) return string.Empty;
                try
                {
                    return CultureInfo.GetCultureInfo(Ietf).NativeName;
                }
                catch (CultureNotFoundException)
                {
                    return Ietf;
                }
            }
        }

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

