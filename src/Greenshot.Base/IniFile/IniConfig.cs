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
using System.IO;
using System.Reflection;
using Dapplo.Ini;
using Dapplo.Ini.Interfaces;
using Greenshot.Base.Core;
using log4net;

namespace Greenshot.Base.IniFile
{
    /// <summary>
    /// Thin wrapper around <see cref="IniConfigRegistry"/> / <see cref="Dapplo.Ini.IniConfig"/>
    /// that preserves the Greenshot-specific file-location logic (portable mode, AppData,
    /// custom directory override) while delegating all INI serialisation to Dapplo.Ini.
    ///
    /// <para>Three-phase plugin loading pattern:</para>
    /// <list type="number">
    ///   <item><description>Call <see cref="Init()"/> — creates the registry and registers core sections.  No file I/O yet.</description></item>
    ///   <item><description>Each plugin's <c>RegisterConfiguration()</c> adds its own sections.</description></item>
    ///   <item><description>Call <see cref="Load()"/> once — reads every registered section in a single pass.</description></item>
    /// </list>
    /// </summary>
    public static class IniConfig
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IniConfig));

        private const string IniExtension = ".ini";
        private const string DefaultsPostfix = "-defaults";
        private const string FixedPostfix = "-fixed";

        private static string _applicationName;
        private static string _configName;
        private static bool _portableCheckMade;
        private static bool _loaded;

        /// <summary>
        /// The underlying Dapplo.Ini config object; non-null after <see cref="Init()"/>.
        /// </summary>
        private static Dapplo.Ini.IniConfig _iniConfig;

        // ── Public surface ───────────────────────────────────────────────────

        /// <summary>Is Greenshot running in PortableApp (PAF) mode?</summary>
        public static bool IsPortable { get; private set; }

        /// <summary>
        /// Optional override directory for the config file.
        /// Must be set before <see cref="Load()"/> is called (e.g. from command-line arguments).
        /// </summary>
        public static string IniDirectory { get; set; }

        /// <summary>Full path of the active greenshot.ini file.</summary>
        public static string ConfigLocation => CreateIniLocation(_configName + IniExtension, false);

        /// <summary>Returns <c>true</c> once <see cref="Init()"/> has been called.</summary>
        public static bool IsInitialized => _iniConfig != null;

        // ── Initialisation ───────────────────────────────────────────────────

        /// <summary>
        /// Initialises with the application name taken from the entry-assembly's
        /// <see cref="AssemblyProductAttribute"/>.
        /// Builds the <see cref="Dapplo.Ini.IniConfig"/> (no file I/O yet).
        /// </summary>
        public static void Init()
        {
            var productAttributes = Assembly.GetEntryAssembly()?
                .GetCustomAttributes(typeof(AssemblyProductAttribute), false) as AssemblyProductAttribute[];

            if (productAttributes is { Length: > 0 })
            {
                Init(productAttributes[0].Product, productAttributes[0].Product);
            }
            else
            {
                throw new InvalidOperationException("Assembly ProductName not set.");
            }
        }

        /// <summary>
        /// Initialises with explicit application and configuration names.
        /// Builds the <see cref="Dapplo.Ini.IniConfig"/> (no file I/O yet).
        /// The host's own <see cref="Greenshot.Base.Core.CoreConfigurationImpl"/> is registered here
        /// so that it is always part of the config before plugins add their own sections.
        /// </summary>
        public static void Init(string appName, string configName)
        {
            _applicationName = appName;
            _configName = configName;
            _loaded = false;

            // Register custom value converters before building any IniConfig instance.
            // This ensures types not natively supported by Dapplo.Ini (NativeRect, Color, etc.)
            // can be serialised/deserialised correctly.
            IniValueConverters.Register();

            var writeLocation = CreateIniLocation(configName + IniExtension, false);
            var writeDir = Path.GetDirectoryName(writeLocation) ?? ".";
            var fileName = Path.GetFileName(writeLocation);

            // Fixed-values file (admin-controlled, read-only — overrides user and defaults).
            var fixedFile = CreateIniLocation(configName + FixedPostfix + IniExtension, true);

            var builder = IniConfigRegistry.ForFile(fileName)
                .AddSearchPath(writeDir);

            // Add the defaults file (read-only) so default values from the admin-supplied
            // greenshot-defaults.ini are applied before user values.
            var defaultsFile = CreateIniLocation(configName + DefaultsPostfix + IniExtension, true);
            if (File.Exists(defaultsFile))
            {
                builder.AddDefaultsFile(defaultsFile);
            }

            if (File.Exists(fixedFile))
            {
                builder.AddConstantsFile(fixedFile);
            }

            // Register the core host section so it is always present when plugins add theirs.
            _iniConfig = builder
                .RegisterSection(new CoreConfigurationImpl())
                .Create();
        }

        /// <summary>
        /// Forces the configuration to be stored in the application start-up directory
        /// rather than in AppData (converts to portable mode).
        /// Must be called before <see cref="Load()"/>.
        /// </summary>
        public static void ForceIniInStartupPath()
        {
            if (_portableCheckMade)
            {
                throw new InvalidOperationException("ForceIniInStartupPath must be called before any file is read.");
            }

            IsPortable = false;
            _portableCheckMade = true;

            string startupPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".";
            if (_applicationName == null || _configName == null)
            {
                Init();
            }

            string forcedIni = Path.Combine(startupPath, _applicationName + IniExtension);
            if (!File.Exists(forcedIni))
            {
                using (File.Create(forcedIni)) { }
            }
        }

        // ── File I/O ─────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the INI file and populates all registered sections.
        /// Should be called once, after all sections (including plugin sections) have been
        /// registered via <see cref="GetIniSection{T}"/> or
        /// <see cref="Dapplo.Ini.IniConfig.AddSection{T}"/>.
        /// </summary>
        public static void Load()
        {
            EnsureInitialized();
            _iniConfig.Load();
            _loaded = true;
        }

        /// <summary>Re-reads the INI file and refreshes all registered sections.</summary>
        public static void Reload()
        {
            EnsureInitialized();
            _iniConfig.Reload();
            _loaded = true;
        }

        /// <summary>Persists all registered sections to the INI file.</summary>
        public static void Save()
        {
            EnsureInitialized();
            try
            {
                _iniConfig.Save();
            }
            catch (Exception ex)
            {
                Log.Error("A problem occurred while saving the configuration.", ex);
            }
        }

        // ── Section access ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the singleton section instance of type <typeparamref name="T"/>.
        /// If the section has not yet been registered it is auto-registered using the
        /// Dapplo.Ini naming convention (<c>I{Name}</c> → <c>{Name}Impl</c>).
        /// </summary>
        /// <typeparam name="T">The section interface (must implement <see cref="IIniSection"/>).</typeparam>
        public static T GetIniSection<T>() where T : class, IIniSection
        {
            EnsureInitialized();

            try
            {
                return IniConfigRegistry.GetSection<T>();
            }
            catch (InvalidOperationException)
            {
                // Section not yet registered — auto-register using the generated Impl class.
                var implType = FindImplType<T>();
                if (implType == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot find the generated implementation class for {typeof(T).FullName}. " +
                        "Ensure Dapplo.Ini.Generator has been run and the *Impl class is in the same namespace.");
                }

                var impl = (T)Activator.CreateInstance(implType);
                _iniConfig.AddSection(impl);

                if (_loaded)
                {
                    // Re-read so the newly added section gets its values.
                    _iniConfig.Reload();
                }

                return IniConfigRegistry.GetSection<T>();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Exposes the underlying <see cref="Dapplo.Ini.IniConfig"/> so that
        /// plugin registration code can call <c>AddSection&lt;T&gt;</c> directly.
        /// </summary>
        public static Dapplo.Ini.IniConfig GetActiveConfig()
        {
            EnsureInitialized();
            return _iniConfig;
        }

        private static void EnsureInitialized()
        {
            if (_iniConfig == null)
            {
                throw new InvalidOperationException("IniConfig.Init() has not been called.");
            }
        }

        /// <summary>
        /// Uses Dapplo.Ini's naming convention to locate the generated implementation class
        /// for a given section interface.
        /// <para>Convention: <c>IFooSection</c> → <c>FooSectionImpl</c> (same namespace + assembly).</para>
        /// </summary>
        private static Type FindImplType<T>() where T : class, IIniSection
        {
            var t = typeof(T);
            var baseName = t.Name.StartsWith("I") ? t.Name.Substring(1) : t.Name;
            return t.Assembly.GetType(t.Namespace + "." + baseName + "Impl");
        }

        /// <summary>
        /// Determines the full path of the configuration file, applying the same
        /// portable-mode / AppData / IniDirectory logic as the original implementation.
        /// </summary>
        private static string CreateIniLocation(string configFilename, bool isReadOnly)
        {
            if (_applicationName == null || _configName == null)
            {
                throw new InvalidOperationException("IniConfig.Init() has not been called.");
            }

            string iniFilePath = null;

            // 1. Honour the externally supplied IniDirectory (e.g. from command-line).
            try
            {
                if (IniDirectory != null && Directory.Exists(IniDirectory))
                {
                    if (!isReadOnly)
                    {
                        return Path.Combine(IniDirectory, configFilename);
                    }

                    iniFilePath = Path.Combine(IniDirectory, configFilename);
                    if (File.Exists(iniFilePath))
                    {
                        return iniFilePath;
                    }

                    iniFilePath = null;
                }
            }
            catch (Exception ex)
            {
                Log.WarnFormat("The ini-directory {0} cannot be used: {1}", IniDirectory, ex.Message);
            }

            string applicationStartupPath;
            try
            {
                applicationStartupPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Problem retrieving the assembly location: {0}", ex.Message);
                applicationStartupPath = ".";
            }

            // 2. Check for PortableApp (PAF) structure.
            if (applicationStartupPath != null)
            {
                string pafPath = Path.Combine(applicationStartupPath, @"App\" + _applicationName);

                if (IsPortable || !_portableCheckMade)
                {
                    if (!IsPortable)
                    {
                        Log.Info("Checking for portable mode.");
                        _portableCheckMade = true;
                        if (Directory.Exists(pafPath))
                        {
                            IsPortable = true;
                            Log.Info("Portable mode active!");
                        }
                    }

                    if (IsPortable)
                    {
                        string pafConfigPath = Path.Combine(applicationStartupPath, @"Data\Settings");
                        try
                        {
                            if (!Directory.Exists(pafConfigPath))
                            {
                                Directory.CreateDirectory(pafConfigPath);
                            }

                            iniFilePath = Path.Combine(pafConfigPath, configFilename);
                        }
                        catch (Exception ex)
                        {
                            Log.InfoFormat("Portable mode not possible, couldn't create '{0}': {1}", pafConfigPath, ex.Message);
                        }
                    }
                }
            }

            // 3. Fall back to application directory (local portable copy) or AppData.
            if (iniFilePath == null)
            {
                if (applicationStartupPath != null)
                {
                    iniFilePath = Path.Combine(applicationStartupPath, configFilename);
                }

                if (!File.Exists(iniFilePath))
                {
                    string iniDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        _applicationName);

                    if (!Directory.Exists(iniDirectory))
                    {
                        Directory.CreateDirectory(iniDirectory);
                    }

                    iniFilePath = Path.Combine(iniDirectory, configFilename);
                }
            }

            Log.InfoFormat("Using ini file {0}", iniFilePath);
            return iniFilePath;
        }
    }
}
