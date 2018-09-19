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
    class RedirectAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Redirect001";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RedirectTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RedirectMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.RedirectAnalyzerDesc), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Syntax";

        public const string DiagnosticId2 = "RedirectLocation";
        private static readonly LocalizableString Title2 = new LocalizableResourceString(nameof(Resources.RedirectTitle2), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat2 = new LocalizableResourceString(nameof(Resources.RedirectMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description2 = new LocalizableResourceString(nameof(Resources.RedirectAnalyzerDesc), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);
        private static DiagnosticDescriptor RuleProperty = new DiagnosticDescriptor(DiagnosticId2, Title2, MessageFormat2, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description2);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule,RuleProperty); } }


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeRedirectNode, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeRedirectProperty, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeRedirectNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ToString().StartsWith("Response.Redirect") && !context.Node.ToString().Contains("Route"))
            {
                var diagnostic = Diagnostic.Create(Rule, ((InvocationExpressionSyntax)context.Node).GetLocation(), "Redirect");
                context.ReportDiagnostic(diagnostic);
            }
            
        }

        private static void AnalyzeRedirectProperty(SyntaxNodeAnalysisContext context)
        {
            if (!context.Node.ToString().StartsWith("Response.RedirectLocation"))
                return;

            if ((((AssignmentExpressionSyntax)context.Node).Right).Kind() == SyntaxKind.StringLiteralExpression)
                return;

            if (((MemberAccessExpressionSyntax)((AssignmentExpressionSyntax)context.Node).Left).Name.Identifier.ValueText == "RedirectLocation")
            {
                var diagnostic = Diagnostic.Create(RuleProperty, ((AssignmentExpressionSyntax)context.Node).GetLocation(), "RedirectLocation");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
