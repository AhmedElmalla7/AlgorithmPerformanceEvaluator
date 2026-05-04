using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class CompilerService
    {
        private static readonly ScriptOptions _opts = ScriptOptions.Default
            .WithImports("System", "System.Linq", "System.Collections.Generic")
            .WithReferences(typeof(Enumerable).Assembly);

        // Regex to identify the method name from user input
        private static readonly Regex _methodRegex = new(
            @"\w+\s+(\w+)\s*\(\s*int\s*\[\s*\]\s*\w*\s*\)",
            RegexOptions.Compiled);

        public async Task<Func<int[], object?>> CompileFlexibleAsync(string userCode)
        {
            string methodName = ExtractMethodName(userCode);

            // Wrap user code and return it as a delegate
            // Blank lines used to separate logic from return statement
            string script = $@"
                {userCode}

                new Func<int[], object?>(arr => {{
                    try {{ 
                        return {methodName}(arr) ?? true; 
                    }}
                    catch {{ 
                        return null; 
                    }}
                }})";

            try
            {
                // Evaluate the script and return the executable function
                return await CSharpScript.EvaluateAsync<Func<int[], object?>>(script, _opts);
            }
            catch (CompilationErrorException ex)
            {
                var errors = string.Join("\n", ex.Diagnostics.Select(d => d.GetMessage()));
                throw new InvalidOperationException($"Syntax Error:\n{errors}");
            }
        }

        public static string ExtractMethodName(string code)
        {
            var match = _methodRegex.Match(code);
            if (!match.Success)
                throw new InvalidOperationException("No valid method found. Use: public void MyFunc(int[] arr)");

            return match.Groups[1].Value;
        }
    }
}