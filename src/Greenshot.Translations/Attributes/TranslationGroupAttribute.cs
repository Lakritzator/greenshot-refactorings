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

namespace Greenshot.Translations.Attributes
{
    /// <summary>
    /// Marks an interface as a translation group so that the
    /// <c>Greenshot.Translations.Generator</c> source generator emits
    /// a concrete implementation class.
    /// </summary>
    ///
    /// <example>
    /// Define a translation interface once:
    /// <code>
    /// [TranslationGroup("greenshot")]
    /// public interface IGreenshotTranslations
    /// {
    ///     string Cancel { get; }
    ///     string AboutBugs { get; }
    /// }
    /// </code>
    ///
    /// The generator creates <c>GreenshotTranslations</c> implementing
    /// <c>IGreenshotTranslations</c> and <c>INotifyPropertyChanged</c>.
    /// Property names are mapped to JSON keys using snake_case:
    /// <c>Cancel</c> → <c>"cancel"</c>, <c>AboutBugs</c> → <c>"about_bugs"</c>.
    /// </example>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class TranslationGroupAttribute : Attribute
    {
        /// <summary>
        /// Initialises the attribute with the translation group identifier.
        /// </summary>
        /// <param name="prefix">
        /// The prefix used when looking up keys, e.g. <c>"greenshot"</c>
        /// (main app, no prefix applied) or <c>"box"</c> (plugin prefix).
        /// Pass <c>null</c> or an empty string for the main application.
        /// </param>
        public TranslationGroupAttribute(string prefix = null)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// The namespace prefix applied to every key looked up through the
        /// generated class.  <c>null</c> or empty means no prefix.
        /// </summary>
        public string Prefix { get; }
    }
}
