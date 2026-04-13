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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Greenshot.Translations.Generator
{
    /// <summary>
    /// Roslyn incremental source generator that reads every interface decorated
    /// with <c>[TranslationGroup]</c> and emits a concrete implementation class.
    ///
    /// <para>
    /// <b>Input:</b> any <c>interface</c> with <c>[TranslationGroup("prefix")]</c>
    /// attribute and one or more <c>string { get; }</c> properties.
    /// </para>
    ///
    /// <para>
    /// <b>Output:</b> a <c>partial class</c> that:
    /// <list type="bullet">
    ///   <item>implements the interface</item>
    ///   <item>implements <c>INotifyPropertyChanged</c></item>
    ///   <item>accepts <c>ITranslationProvider</c> in its constructor</item>
    ///   <item>raises <c>PropertyChanged</c> for every property when
    ///         <c>LanguageChanged</c> fires</item>
    ///   <item>maps each <c>PascalCase</c> property to a <c>snake_case</c>
    ///         JSON key</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Key mapping rule:</b> insert an underscore before every uppercase letter
    /// that follows a lower-case letter, then lower-case everything.
    /// Examples:
    /// <list type="table">
    ///   <listheader><term>Interface property</term><description>JSON key</description></listheader>
    ///   <item><term>Cancel</term><description>cancel</description></item>
    ///   <item><term>AboutBugs</term><description>about_bugs</description></item>
    ///   <item><term>SettingsTooltipLanguage</term><description>settings_tooltip_language</description></item>
    /// </list>
    /// </para>
    /// </summary>
    [Generator]
    public sealed class TranslationSourceGenerator : IIncrementalGenerator
    {
        private const string AttributeFullName = "Greenshot.Translations.Attributes.TranslationGroupAttribute";
        private const string ProviderFullName = "Greenshot.Translations.Core.ITranslationProvider";

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Emit the attribute source so users don't need a separate package reference
            context.RegisterPostInitializationOutput(EmitAttributeSource);

            // Filter to interfaces decorated with [TranslationGroup]
            var interfaceDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is InterfaceDeclarationSyntax ids
                                                   && ids.AttributeLists.Count > 0,
                    transform: static (ctx, _) => GetSemanticTarget(ctx))
                .Where(static m => m is not null);

            context.RegisterSourceOutput(interfaceDeclarations, Execute);
        }

        // ------------------------------------------------------------------ //
        //  Syntax / semantic helpers
        // ------------------------------------------------------------------ //

        private static INamedTypeSymbol GetSemanticTarget(GeneratorSyntaxContext ctx)
        {
            var ids = (InterfaceDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(ids) as INamedTypeSymbol;

            if (symbol is null) return null;

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == AttributeFullName)
                    return symbol;
            }

            return null;
        }

        // ------------------------------------------------------------------ //
        //  Code emission
        // ------------------------------------------------------------------ //

        private static void Execute(
            SourceProductionContext context,
            INamedTypeSymbol interfaceSymbol)
        {
            if (interfaceSymbol is null) return;

            var namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : interfaceSymbol.ContainingNamespace.ToDisplayString();

            var interfaceName = interfaceSymbol.Name; // e.g. "IGreenshotTranslations"

            // Class name: strip leading 'I' if present
            var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
                ? interfaceName.Substring(1)
                : interfaceName + "Impl";

            // Read prefix from [TranslationGroup("prefix")]
            string prefix = null;
            foreach (var attr in interfaceSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == AttributeFullName
                    && attr.ConstructorArguments.Length > 0)
                {
                    prefix = attr.ConstructorArguments[0].Value as string;
                }
            }

            // Collect all string-returning get-only properties
            var properties = new List<(string PropName, string JsonKey)>();
            foreach (var member in interfaceSymbol.GetMembers())
            {
                if (member is IPropertySymbol prop
                    && prop.Type.SpecialType == SpecialType.System_String
                    && prop.GetMethod is not null
                    && prop.SetMethod is null)
                {
                    var jsonKey = ToSnakeCase(prop.Name);
                    if (!string.IsNullOrEmpty(prefix))
                        jsonKey = $"{prefix}.{jsonKey}";

                    properties.Add((prop.Name, jsonKey));
                }
            }

            var source = BuildSource(namespaceName, interfaceName, className, properties);
            context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static string BuildSource(
            string namespaceName,
            string interfaceName,
            string className,
            List<(string PropName, string JsonKey)> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine("using Greenshot.Translations.Core;");
            sb.AppendLine();

            var indent = "";
            if (namespaceName is not null)
            {
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
                indent = "    ";
            }

            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Auto-generated implementation of <see cref=\"{interfaceName}\"/>.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}public sealed partial class {className} : {interfaceName}, INotifyPropertyChanged");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    private readonly ITranslationProvider _provider;");
            sb.AppendLine();
            sb.AppendLine($"{indent}    /// <inheritdoc/>");
            sb.AppendLine($"{indent}    public event PropertyChangedEventHandler? PropertyChanged;");
            sb.AppendLine();

            // Constructor
            sb.AppendLine($"{indent}    /// <summary>");
            sb.AppendLine($"{indent}    /// Initialises the translations and subscribes to language changes.");
            sb.AppendLine($"{indent}    /// </summary>");
            sb.AppendLine($"{indent}    public {className}(ITranslationProvider provider)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        _provider = provider ?? throw new ArgumentNullException(nameof(provider));");
            sb.AppendLine($"{indent}        _provider.LanguageChanged += OnLanguageChanged;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // OnLanguageChanged
            sb.AppendLine($"{indent}    private void OnLanguageChanged(object? sender, EventArgs e)");
            sb.AppendLine($"{indent}    {{");
            foreach (var (propName, _) in properties)
            {
                sb.AppendLine($"{indent}        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof({propName})));");
            }
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            // Properties
            foreach (var (propName, jsonKey) in properties)
            {
                sb.AppendLine($"{indent}    /// <inheritdoc/>");
                sb.AppendLine($"{indent}    public string {propName} => _provider.GetString(\"{jsonKey}\");");
                sb.AppendLine();
            }

            sb.AppendLine($"{indent}}}");

            if (namespaceName is not null)
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        // ------------------------------------------------------------------ //
        //  Attribute source injection
        // ------------------------------------------------------------------ //

        private static void EmitAttributeSource(IncrementalGeneratorPostInitializationContext ctx)
        {
            const string source = @"// <auto-generated />
namespace Greenshot.Translations.Attributes
{
    /// <summary>
    /// Marks an interface for translation class generation.
    /// </summary>
    [global::System.AttributeUsage(global::System.AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    internal sealed class TranslationGroupAttribute : global::System.Attribute
    {
        public TranslationGroupAttribute(string prefix = null) { Prefix = prefix; }
        public string Prefix { get; }
    }
}
";
            ctx.AddSource("TranslationGroupAttribute.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        // ------------------------------------------------------------------ //
        //  Utility: PascalCase → snake_case
        // ------------------------------------------------------------------ //

        // Lookbehind (?<=[a-z0-9]) ensures we only insert an underscore after a
        // lower-case letter or digit.  Lookahead (?=[A-Z]) matches the position
        // just before an upper-case letter.  The replacement inserts "_" at that
        // position, producing e.g. "AboutBugs" → "About_Bugs", then ToLowerInvariant
        // yields "about_bugs".
        private static readonly Regex _pascalToSnake = new(
            @"(?<=[a-z0-9])(?=[A-Z])",
            RegexOptions.Compiled);

        /// <summary>
        /// Converts a <c>PascalCase</c> identifier to <c>snake_case</c>.
        /// </summary>
        internal static string ToSnakeCase(string name)
            => _pascalToSnake.Replace(name, "_").ToLowerInvariant();
    }
}
