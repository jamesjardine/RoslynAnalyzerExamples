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
    class XmlDocumentAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "XmlDocLoadXXE01";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RedirectTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RedirectMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.RedirectAnalyzerDesc), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Syntax";

        public const string DiagnosticId2 = "XmlDocLoadXML";
        private static readonly LocalizableString Title2 = new LocalizableResourceString(nameof(Resources.RedirectTitle2), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat2 = new LocalizableResourceString(nameof(Resources.RedirectMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description2 = new LocalizableResourceString(nameof(Resources.RedirectAnalyzerDesc), Resources.ResourceManager, typeof(Resources));

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);
        private static DiagnosticDescriptor RuleProperty = new DiagnosticDescriptor(DiagnosticId2, Title2, MessageFormat2, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description2);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule,RuleProperty); } }


        public override void Initialize(AnalysisContext context)
        {
            //context.RegisterSyntaxNodeAction(AnalyzeLabelNode, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeXmlDoc, SyntaxKind.InvocationExpression);
        }

        //private static void AnalyzeLabelNode(SyntaxNodeAnalysisContext context)
        //{
        //    if (context.Node.ToString().StartsWith("Response.Redirect") && !context.Node.ToString().Contains("Route"))
        //    {
        //        var diagnostic = Diagnostic.Create(Rule, ((InvocationExpressionSyntax)context.Node).GetLocation(), "Redirect");
        //        context.ReportDiagnostic(diagnostic);
        //    }
            
        //}

        private static void AnalyzeXmlDoc(SyntaxNodeAnalysisContext context)
        {
            // Looking for both XmlDocument.Load and XmlDocument.LoadXml
            if (!context.Node.ToString().Contains(".Load"))
                return;

            if (!context.SemanticModel.GetSymbolInfo((InvocationExpressionSyntax)context.Node).Symbol.ContainingType.ToString().StartsWith("System.Xml.XmlDocument"))
                return;

            //Need to check to see if XmlResolver is set or not.. That will come later. In 4.6, this was changed ot default to Null and isn't needed.  At this point.. lets just highlight the code.
            var diagnostic = Diagnostic.Create(Rule, ((InvocationExpressionSyntax)context.Node).GetLocation(), "XXE");
            context.ReportDiagnostic(diagnostic);


        }
    }
}
