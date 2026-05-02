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
            .WithImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Text",
                "System.Threading.Tasks")
            .WithReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(System.Collections.Generic.List<>).Assembly);

        // الـ Regex المطور: بيلقط أي ميثود بتاخد int[] مهما كان نوع الإرجاع أو الاسم أو الـ Modifiers
        private static readonly Regex _methodRegex = new(
            @"(?:public|private|protected|internal|static|async|virtual|override|\s)*\s+\w+\s+(\w+)\s*\(\s*int\s*\[\s*\]\s*\w*\s*\)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        public async Task<Func<int[], object?>> CompileFlexibleAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                throw new ArgumentException("Code cannot be empty.");

            // 1. استخراج اسم الدالة ديناميكياً
            string methodName = ExtractMethodName(userCode);

            // 2. بناء الـ Script مع معالجة ذكية للـ void والـ object
            // استخدمنا dynamic عشان نهرب من مشاكل الـ Casting لو الدالة void
            string script = $@"
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

{userCode}

return new Func<int[], object?>(arr => {{
    try 
    {{ 
        // استدعاء الدالة ديناميكياً
        dynamic result = {methodName}(arr); 
        
        // لو الدالة void الـ result هيكون null، بنرجع true كإشارة للنجاح
        return (object?)result ?? true; 
    }}
    catch (Exception ex)
    {{ 
        return null; 
    }}
}});";

            try
            {
                var fn = await CSharpScript.EvaluateAsync<Func<int[], object?>>(script, _opts);

                return fn ?? throw new InvalidOperationException("Failed to create delegate.");
            }
            catch (CompilationErrorException ex)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    ex.Diagnostics
                      .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                      .Select(d => d.GetMessage()));

                throw new InvalidOperationException($"Compilation Error:\n{errors}");
            }
        }

        // ميثود عامة (public) عشان نقدر نستخدمها في الـ MainWindow لعمل الـ Scaling
        public static string ExtractMethodName(string code)
        {
            var match = _methodRegex.Match(code);
            if (match.Success)
                return match.Groups[1].Value;

            throw new InvalidOperationException(
                "Could not detect a valid method signature.\n" +
                "Example: public object MySort(int[] arr) { ... }");
        }
    }
}