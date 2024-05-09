using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GalaxyBudsClient.Generators.Utils;
using Microsoft.CodeAnalysis;

namespace GalaxyBudsClient.Generators;

[Generator]
public class LocalizationKeySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var langCodes = new List<string>();
        
        foreach (var additionalFile in context.AdditionalFiles)
        {
            if (additionalFile == null)
                continue;

            // Check if the file name is the specific file that we expect.
            if (!additionalFile.Path.EndsWith(".axaml") || !additionalFile.Path.Contains("i18n"))
                continue;

            var xaml = additionalFile.GetText();
            if (xaml == null)
                break;

            var doc = XDocument.Parse(xaml.ToString());
            var nodes = doc.Root?.Nodes();
            if(nodes == null)
                break;

            var langCode = Path.GetFileNameWithoutExtension(additionalFile.Path);
            langCodes.Add(langCode);
            
            var keyClassMembers = new List<string>();
            var stringClassMembers = new List<string>();
            
            var dictionarySource = new CodeGenerator();
            dictionarySource.AppendLines("""
                                         // <auto-generated/>
                                         namespace GalaxyBudsClient.Generated.I18N;
                                         """);
            dictionarySource.EnterScope("public static partial class LocalizationDictionaries");
            dictionarySource.EnterScope($"private static global::System.Collections.Generic.Dictionary<global::System.String, global::System.String> @{langCode} => new()");
            
            foreach (var node in nodes)
            {
                if (node is not XElement element) 
                    continue;
                
                // Example for xmlNamespace: clr-namespace:System;assembly=mscorlib
                var xmlNamespace = element.Name.NamespaceName.Split(';');
                var namespaceName = xmlNamespace.First().Split(':').Last();
                var assemblyName = xmlNamespace.Last().Split('=').Last();
                var typeName = element.Name.LocalName;
                var englishString = element.Value;

                var key = element.Attributes().First(x => x.Name.LocalName == "Key");
                if (key == null)
                    keyClassMembers.Add($"#warning {additionalFile.Path}: x:Key attribute not found for XAML element of type {namespaceName}.{typeName}");
                else
                {
                    // Generate the key/string class members based on the english translation
                    if (additionalFile.Path.EndsWith("en.axaml"))
                    {
                        // Snake case to Pascal case
                        var memberName = key.Value.Split(["_"], StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                            .Aggregate(string.Empty, (s1, s2) => s1 + s2);

                        if (memberName == null)
                            keyClassMembers.Add(
                                $"#warning {additionalFile.Path}: Failed to convert key to Pascal case for XAML element of type {namespaceName}.{typeName}");
                        else
                        {
                            keyClassMembers.Add($"/** <summary> Resolves to: '{englishString}' </summary> */");
                            keyClassMembers.Add($"public const global::System.String {memberName} = \"{key.Value}\";");
                            stringClassMembers.Add($"/** <summary> Resolves to: '{englishString}' </summary> */");
                            stringClassMembers.Add(
                                $"public static global::System.String {memberName} => Loc.Resolve(Keys.{memberName});");
                        }
                    }

                    var sanitized = element.Value
                        .Replace(@"\", @"\\")
                        .Replace("\"", "\\\"")
                        .Replace("\r\n", "\n")
                        .Replace('\r', '\n')
                        .Split('\n')
                        .Select(x => x.Trim());
                    dictionarySource.AppendLine($$"""{"{{key.Value}}", "{{string.Join("\n", sanitized).Replace("\n", "\\n")}}"},""");
                }
            }
            
            // Build up the source code.
            if (additionalFile.Path.EndsWith("en.axaml"))
            {
                var keysSource = new CodeGenerator();
                keysSource.AppendLines("""
                                       // <auto-generated/>
                                       namespace GalaxyBudsClient.Generated.I18N;
                                       """);
                keysSource.EnterScope("public static class Keys");
                keyClassMembers.ForEach(keysSource.AppendLine);
                keysSource.LeaveScope();

                var stringsSource = new CodeGenerator();
                stringsSource.AppendLines("""
                                          // <auto-generated/>
                                          using GalaxyBudsClient.Utils.Interface;

                                          namespace GalaxyBudsClient.Generated.I18N;
                                          """);
                stringsSource.EnterScope("public static class Strings");
                stringClassMembers.ForEach(stringsSource.AppendLine);
                stringsSource.LeaveScope();

                // Add the source code to the compilation.
                context.AddSource($"LocalizationKeys.g.cs", keysSource.ToString());
                context.AddSource($"LocalizationStrings.g.cs", stringsSource.ToString());
            }
            
            dictionarySource.LeaveScope(";");
            dictionarySource.LeaveScope();
            
            context.AddSource("LocalizationDictionary_" + langCode + ".g.cs", dictionarySource.ToString());
        }
        
        // Add language dict lookup function
        var lookupSource = new CodeGenerator();
        lookupSource.AppendLines("""
                                 // <auto-generated/>
                                 #nullable enable
                                 namespace GalaxyBudsClient.Generated.I18N;
                                 """);
        lookupSource.EnterScope("public static partial class LocalizationDictionaries");
        lookupSource.EnterScope("public static global::System.Collections.Generic.Dictionary<global::System.String, global::System.String>? GetByLangCode(global::System.String langCode)");
        lookupSource.EnterScope("return langCode switch");
        foreach (var langCode in langCodes)
        {
            lookupSource.AppendLine($"\"{langCode}\" => @{langCode},");
        }
        lookupSource.AppendLine("_ => null,");
        lookupSource.LeaveScope(";");
        lookupSource.LeaveScope();
        lookupSource.LeaveScope();
        
        context.AddSource($"LocalizationDictionaries.g.cs", lookupSource.ToString());
    }
}