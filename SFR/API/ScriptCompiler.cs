using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ScriptEngine;
using SFD;
using SFDGameScriptInterface;

namespace SFR.API;

[HarmonyPatch]
internal static class ScriptHandler
{
    private static readonly string RuntimePath = RuntimeEnvironment.GetRuntimeDirectory();
    // private static string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";
    //
    // private static readonly IEnumerable<MetadataReference> DefaultReferences = new[]
    // {
    //     MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
    //     MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
    //     MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
    // };

    // private static readonly CSharpCompilationOptions DefaultCompilationOptions =
    //     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    //         .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
    //         .WithUsings(DefaultNamespaces);

    private static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
    {
        var stringText = SourceText.From(text, Encoding.UTF8);
        return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.GetScriptTypes))]
    private static bool GetScriptTypes(ref Type[] __result)
    {
        __result = new[]
        {
            typeof(GameScriptInterface),
            typeof(GameWorld),
            typeof(BitConverter),
            typeof(EnumerableQuery),
            typeof(ArrayList),
            typeof(List<int>),
            typeof(StringBuilder),
            typeof(Regex)
        };

        return false;
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences(List<string> referencedAssemblies)
    {
        var references = new List<MetadataReference>();
        foreach (string reference in referencedAssemblies)
        {
            references.Add(MetadataReference.CreateFromFile(reference));
        }

        return references;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScriptCompiler), nameof(ScriptCompiler.CompileCode))]
    private static bool ChangeCompilerVersion(string script, string outputAssemblyPath, List<string> referencedAssemblies, bool generateDebugInformation, ref ScriptCompilerResult __result)
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release);

        bool flag = string.IsNullOrEmpty(outputAssemblyPath);
        if (!flag)
        {
            // if (generateDebugInformation)
            // {
            //     text = Path.ChangeExtension(outputAssemblyPath, ".cs");
            //     File.WriteAllText(text, script);
            //     compilerParameters.IncludeDebugInformation = true;
            // }

            string fileName = Path.GetFileNameWithoutExtension(outputAssemblyPath);
            var parsedSyntaxTree = Parse(script, string.Empty, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Default));
            var compilation = CSharpCompilation.Create(fileName, new[] { parsedSyntaxTree }, GetMetadataReferences(referencedAssemblies), compilationOptions);

            var result = compilation.Emit(outputAssemblyPath);

            CompilerResults compilerResults;
            if (result.Success)
            {
                compilerResults = new CompilerResults(null)
                {
                    PathToAssembly = outputAssemblyPath
                };
            }
            else
            {
                var compilerErrors = new CompilerErrorCollection();
                foreach (var diagnostic in result.Diagnostics)
                {
                    string[] parts = diagnostic.ToString().Replace("(", "").Replace(")", "").Split(',');
                    compilerErrors.Add(new CompilerError(fileName, int.Parse(parts[0]), 0, null, diagnostic.GetMessage()));
                }

                compilerResults = new CompilerResults(null)
                {
                    PathToAssembly = outputAssemblyPath
                };

                typeof(CompilerResults).GetField("errors", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(compilerResults, compilerErrors);
            }

            __result = new ScriptCompilerResult(outputAssemblyPath, null, compilerResults);
            return false;
        }

        __result = null;
        return false;
    }
}