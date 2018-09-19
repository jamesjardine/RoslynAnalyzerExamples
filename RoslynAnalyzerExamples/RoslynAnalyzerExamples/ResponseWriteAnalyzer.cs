using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzerExamples
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class ResponseWriteAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "XSS-001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ResponseTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.ResponseMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.ResponseAnalyzerDesc), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Syntax";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeResponseNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeResponseNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ToString().Equals("Response.Write"))
            {
                var diagnostic = Diagnostic.Create(Rule, ((InvocationExpressionSyntax)context.Node).GetLocation(), "XSS");
                context.ReportDiagnostic(diagnostic);
            }

        }

        
    }
}
