using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn2
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] projectLocations = new string[]
            {
               @"C:\Users\wreitz\Source\Repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.Provider.Host"
            };

            string projectDirectoryLocation = @"C:\Users\wreitz\Source\Repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.PredictionDelivery.Core";

            string uml = string.Empty;
              projectLocations.ToList().ForEach((a)=> uml += GetProjectClasses(a));
                       
            int f = 0;

        }

        static string GetProjectClasses(string projectDirectoryLocation)
        {
            // string[] filePaths = Directory.GetFiles(@"C:\Users\wreitz\source\repos\ShelterSigns\Source\Projects", "*.cs", SearchOption.AllDirectories);
           
            string[] projectFileNames = Directory.GetFiles(projectDirectoryLocation);
            var dir = Directory.GetDirectoryRoot(projectFileNames?.FirstOrDefault());
      //      var startClass = @"C:\Users\wreitz\source\repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.PredictionDelivery.Core\DeliveryManager.cs";
      //      string source = @"C:\Users\wreitz\source\repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.PredictionDelivery.Core\DeliveryManager.cs";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@startUml");
            sb.AppendLine("package \"" + Path.GetFileName(@"C:\Users\wreitz\Source\Repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.PredictionDelivery.Core") + "\" #DDDDDD {");
            foreach (var fileString in projectFileNames)
            {
                var code = new StreamReader(fileString).ReadToEnd();


                SyntaxTree node = CSharpSyntaxTree.ParseText(code);
                var root = node.GetRoot();

                var members = node.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>().ToList();
                var publicMembers = node.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().ToList();
                var fields = node.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax>().ToList();
                var descNodes = node.GetRoot().DescendantNodes().ToList();
                var myAssembly = node.GetRoot().Ancestors().OfType<Microsoft.CodeAnalysis.AssemblyIdentity>().ToList();
                var myParents = node.GetRoot().AncestorsAndSelf().ToList();
                var className = node.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
                if (className != null)
                {

                    sb.AppendLine(@$"class {node.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text}" + "{");
                    foreach (var field in fields.Where(a => a.Parent.SyntaxTree == node))
                    {

                      //  sb.AppendLine(@$" {string.Join(" ", field.Modifiers.Select(a => a.Text).ToList()).Replace("private", "-").Replace("public", "+")  } ");
                        
                        sb.AppendLine(@$"{string.Join(" ", field.Modifiers.Select(a => a.Text).ToList()).Replace("private", "-").Replace("public", "+")  } {field.Declaration.ToString()  } ");
                        //  field.Declaration.Variables.OfType<VariableDeclaratorSyntax>().FirstOrDefault().Identifier.Text

                    }


                    foreach (var item in publicMembers)
                    {

                        sb.AppendLine("+" + item.Identifier.Text +
                            item.ParameterList.OpenParenToken.Text +
                           (item.ParameterList.Parameters.Count > 0 ? item.ParameterList?.Parameters.ToString() : string.Empty) +
                            item.ParameterList.CloseParenToken.Text);
                    }
                    sb.AppendLine("}");
                }
            }
            sb.AppendLine("}");
            sb.AppendLine("@enduml");
            Console.WriteLine(sb.ToString());



            int f = 0;
            return sb.ToString();
        }


    }

    public class CustomWalker : CSharpSyntaxWalker
    {
        static int Tabs = 0;
        public override void Visit(SyntaxNode node)
        {
            Tabs++;
            var indents = new String('\t', Tabs);
            Console.WriteLine(indents + node.Kind());
            base.Visit(node);
            Tabs--;
        }
    }
    internal class MyWalker : CSharpSyntaxWalker
    {
        public int MethodCount { get; private set; }

        public MyWalker() : base(Microsoft.CodeAnalysis.SyntaxWalkerDepth.Trivia)
        { }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            //  MyMethodCount++;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            base.VisitClassDeclaration(node); // this was missin
                                              // Class++;
                                              // Complexity++;
        }
        public readonly List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToString() != "System" &&
                !node.Name.ToString().StartsWith("System."))
            {
                this.Usings.Add(node);
            }
        }
    }
}
