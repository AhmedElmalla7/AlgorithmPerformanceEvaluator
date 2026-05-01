using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class CompilerService
    {
        private static readonly ScriptOptions _opts = ScriptOptions.Default
            .WithImports("System", "System.Linq", "System.Collections.Generic", "System.Text")
            .WithReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(List<>).Assembly);

        public async Task<Func<int[], object?>> CompileFlexibleAsync(string userCode)
        {
            if (string.IsNullOrWhiteSpace(userCode))
                throw new ArgumentException("Code cannot be empty.");

            // 1. استخراج اسم الدالة باستخدام Regex
            // يبحث عن نمط: أي نوع إرجاع ثم اسم الدالة ثم (int[] اسم_المتغير)
            string methodName = ExtractMethodName(userCode);

            // التعديل هنا: بنحط كود المستخدم وبنضيف سطر أمان في الآخر 
            // عشان لو مفيش Return جوه الدالة، الكود ميفشلش
            string wrapped = $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;

        // كود المستخدم بالكامل
        {userCode}

        // سطر الحقن الذكي
        return new Func<int[], object?>( (arr) => {{
            try {{
                return {methodName}(arr);
            }} catch {{
                return null; 
            }}
        }});
    ";

            try
            {
                var fn = await CSharpScript.EvaluateAsync<Func<int[], object?>>(wrapped, _opts);

                if (fn == null)
                    throw new InvalidOperationException("Failed to wrap the method.");

                return fn;
            }
            catch (CompilationErrorException ex)
            {
                var errors = string.Join(Environment.NewLine, ex.Diagnostics);
                throw new InvalidOperationException($"Compilation failed:{Environment.NewLine}{errors}");
            }
        }

        private string ExtractMethodName(string code)
        {
            // Regex لاكتشاف اسم الدالة التي تقبل مصفوفة int[]
            var match = Regex.Match(code, @"\b\w+\s+(\w+)\s*\(int\s*\[\s*\]");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // في حال فشل الاكتشاف، نفترض اسماً افتراضياً أو نلقي خطأ
            throw new InvalidOperationException("Could not detect a valid method signature. Ensure it looks like: public object MyMethod(int[] arr)");
        }
    }
}