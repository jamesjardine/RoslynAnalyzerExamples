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
    class XmlTextReaderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "XmlTRXXE01";

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
            //context.RegisterSyntaxNodeAction(AnalyzeLabelNode, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeXmlTextReader, SyntaxKind.SimpleAssignmentExpression);
        }

        //private static void AnalyzeLabelNode(SyntaxNodeAnalysisContext context)
        //{
        //    if (context.Node.ToString().StartsWith("Response.Redirect") && !context.Node.ToString().Contains("Route"))
        //    {
        //        var diagnostic = Diagnostic.Create(Rule, ((InvocationExpressionSyntax)context.Node).GetLocation(), "Redirect");
        //        context.ReportDiagnostic(diagnostic);
        //    }
            
        //}

        private static void AnalyzeXmlTextReader(SyntaxNodeAnalysisContext context)
        {

            // Check for both the DtdProcessing enum (4.5+) and the ProhibitDtd property (4.0 and earlier)
            if (!(context.Node.ToString().Contains(".DtdProcessing") || context.Node.ToString().Contains(".ProhibitDtd")))
                return;

            if (((MemberAccessExpressionSyntax)((AssignmentExpressionSyntax)context.Node).Left).Name.Identifier.ValueText.Contains("Dtd"))
            {
                string leftNode = context.SemanticModel.GetSymbolInfo(((AssignmentExpressionSyntax)context.Node).Left).Symbol.ToString();

                if (leftNode == "System.Xml.XmlReaderSettings.DtdProcessing" ||
                    leftNode == "System.Xml.XmlTextReader.DtdProcessing" )
                {
                    if ((((AssignmentExpressionSyntax)context.Node).Right).GetFirstToken().ToString() != "DtdProcessing")
                        return;

                    if ((((AssignmentExpressionSyntax)context.Node).Right).GetLastToken().ToString() == "Parse")
                    {
                        var diagnostic = Diagnostic.Create(RuleProperty, ((AssignmentExpressionSyntax)context.Node).GetLocation(), "XXE");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else if(leftNode == "System.Xml.XmlReaderSettings.ProhibitDtd" ||
                    leftNode == "System.xml.XmlTextReader.ProhibitDtd")
                {
                    if ((((AssignmentExpressionSyntax)context.Node).Right).Kind().ToString() == "FalseLiteralExpression")
                    {
                        var diagnostic = Diagnostic.Create(RuleProperty, ((AssignmentExpressionSyntax)context.Node).GetLocation(), "XXE");
                        context.ReportDiagnostic(diagnostic);
                    }
                    
                }

            }


        }
    }
}
