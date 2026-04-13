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

using Greenshot.Translations.Core;

namespace Greenshot.Translations.Wpf
{
    /// <summary>
    /// Application-level singleton accessor for the active
    /// <see cref="ITranslationManager"/>.
    ///
    /// <para>
    /// Set <see cref="Current"/> once at application startup before any XAML
    /// is loaded so that <see cref="TranslateExtension"/> can resolve it.
    /// Creating a <see cref="TranslationIndexer"/> from <see cref="Current"/>
    /// is done automatically the first time the property is read.
    /// </para>
    /// </summary>
    /// <example>
    /// Startup:
    /// <code>
    /// var manager = new JsonTranslationProvider();
    /// manager.AddPath(Path.Combine(AppContext.BaseDirectory, "Languages"));
    /// manager.CurrentLanguage = "en-US";
    /// TranslationManagerLocator.Current = manager;
    /// </code>
    ///
    /// XAML:
    /// <code>
    /// &lt;Button Content="{t:Translate cancel}" /&gt;
    /// </code>
    /// </example>
    public static class TranslationManagerLocator
    {
        private static ITranslationManager _current;
        private static TranslationIndexer _indexer;

        /// <summary>
        /// The global <see cref="ITranslationManager"/> used by
        /// <see cref="TranslateExtension"/> and
        /// <see cref="TranslationIndexer"/> to resolve keys.
        /// Must be set before any XAML is rendered.
        /// </summary>
        public static ITranslationManager Current
        {
            get => _current;
            set
            {
                _current = value;
                _indexer = value != null ? new TranslationIndexer(value) : null;
            }
        }

        /// <summary>
        /// The <see cref="TranslationIndexer"/> used as the binding source by
        /// <see cref="TranslateExtension"/>.  Automatically created when
        /// <see cref="Current"/> is set.
        /// </summary>
        internal static TranslationIndexer Indexer => _indexer;
    }
}
