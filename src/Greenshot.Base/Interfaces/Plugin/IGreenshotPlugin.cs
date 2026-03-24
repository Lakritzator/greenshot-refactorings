using System;
using Dapplo.Ini;
using Greenshot.Base.Interfaces;

namespace Greenshot.Base.Interfaces.Plugin
{
    /// <summary>
    /// This defines the plugin
    /// </summary>
    public interface IGreenshotPlugin : IDisposable
    {
        /// <summary>
        /// Phase 1 — called before the INI file is read.
        /// The plugin receives the shared <see cref="Dapplo.Ini.IniConfig"/> and must register
        /// its configuration section(s) by calling <c>iniConfig.AddSection(new XxxImpl())</c>.
        /// Translations may also be registered here.
        /// No file I/O has occurred at this point.
        /// </summary>
        /// <param name="iniConfig">The application-wide Dapplo.Ini config object.</param>
        void RegisterConfiguration(Dapplo.Ini.IniConfig iniConfig);

        /// <summary>
        /// Phase 2 — called after the INI file has been loaded.
        /// The plugin should register its services into the supplied DI container.
        /// Configuration values are safe to read at this point.
        /// </summary>
        /// <param name="serviceLocator">The application-wide service locator.</param>
        void RegisterServices(IServiceLocator serviceLocator);

        /// <summary>
        /// Phase 3 — called after all services have been registered.
        /// The plugin may perform its remaining start-up work here; both configuration
        /// and all registered services are guaranteed to be available.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the plugin started successfully and should be shown;
        /// <c>false</c> to indicate that the plugin is not active.
        /// </returns>
        bool Start();

        /// <summary>
        /// Unload of the plugin
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Open the Configuration Form, will/should not be called before handshaking is done
        /// </summary>
        void Configure();

        /// <summary>
        /// Define the name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Specifies if the plugin can be configured
        /// </summary>
        bool IsConfigurable { get; }
    }
}