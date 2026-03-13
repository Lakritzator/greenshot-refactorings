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
using System.ComponentModel;
using System.Windows.Data;
using Greenshot.Translations.Core;

namespace Greenshot.Translations.Wpf
{
    /// <summary>
    /// An <see cref="INotifyPropertyChanged"/> indexer that exposes every
    /// translation key as a bindable indexed property.
    ///
    /// <para>
    /// The <see cref="TranslateExtension"/> creates a <see cref="System.Windows.Data.Binding"/>
    /// against this class using the notation <c>[key]</c> so WPF's binding
    /// engine refreshes the target whenever <see cref="ITranslationProvider.LanguageChanged"/>
    /// is raised.
    /// </para>
    /// </summary>
    /// <remarks>
    /// You do not normally need to instantiate this class directly.
    /// <see cref="TranslationManagerLocator"/> exposes the singleton instance
    /// that <see cref="TranslateExtension"/> binds to.
    /// </remarks>
    public sealed class TranslationIndexer : INotifyPropertyChanged
    {
        private readonly ITranslationProvider _provider;

        /// <summary>Initialises the indexer and subscribes to language changes.</summary>
        /// <param name="provider">The backing translation provider.</param>
        public TranslationIndexer(ITranslationProvider provider)
        {
            _provider = provider;
            _provider.LanguageChanged += OnLanguageChanged;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies WPF bindings that all indexed translations have changed.
        /// Raising <see cref="Binding.IndexerName"/> causes the binding engine
        /// to re-evaluate every <c>[key]</c> binding on this instance.
        /// </summary>
        private void OnLanguageChanged(object sender, EventArgs e)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));

        /// <summary>
        /// Returns the translated string for <paramref name="key"/>.
        /// WPF binding uses <c>[key]</c> notation to access this indexer.
        /// </summary>
        public string this[string key] => _provider.GetString(key);
    }
}
