using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class CompilerService
    {
        private static readonly ScriptOptions _opts = ScriptOptions.Default
            .WithImports("System", "System.Linq", "System.Collections.Generic")
            .WithReferences(typeof(object).Assembly);

        public async Task<Func<int[], object?>> CompileAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                throw new ArgumentException("User code cannot be empty.");

            string wrapped = $@"
                using System;
                using System.Linq;
                using System.Collections.Generic;

                Func<int[], object?> fn = (int[] arr) =>
                {{
                    {userCode}
                }};
                return fn;
            ";

            try
            {
                var fn = await CSharpScript
                    .EvaluateAsync<Func<int[], object?>>(wrapped, _opts);

                if (fn == null)
                    throw new InvalidOperationException("Compilation succeeded but function is null.");

                return fn;
            }
            catch (CompilationErrorException ex)
            {
                var errors = string.Join(Environment.NewLine, ex.Diagnostics);
                throw new InvalidOperationException($"Compilation failed:{Environment.NewLine}{errors}");
            }
        }
    }
}