using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

            // gets all Dirs foe a given project
            //var dirs = Directory.GetDirectories(@"C:\Users\wreitz\source\repos\ShelterSigns\Source\Projects")
            //    .Where(a=>a.Contains("Luminator")
            //    &&  !a.EndsWith("Host")
            //    &&  a.Contains("TransitPredictions")
            //    &&  a.Contains("Projects\\Luminator.TransitPredictions.Delivery.Core")
            //    && !a.Contains("Test")).ToArray();

            string[] dirs = new string[]
            {
              @"C:\Users\wreitz\Source\Repos\ShelterSigns\Source\Projects\Luminator.TransitPredictions.PredictionDelivery.Core"
           };

            string uml = StartUml();
            foreach (var dir in dirs.ToList())
            {
                uml += GetProjectClasses(dir);
            }

            uml += EndUml();
            Console.WriteLine(uml);



            var strCompressed = Cipher.Compress(uml);
            var s = $"http://www.plantuml.com/plantuml/uml/ {strCompressed}";


            var psi = new ProcessStartInfo
            {
                FileName = "http://www.plantuml.com/plantuml/uml/Aov9B2hXil98pSd9LoZFByf9iUOgBial0000 ",
                UseShellExecute = true
            };
            Process.Start(psi);
          

            int f = 0;
            Console.ReadKey();
        }
        

        private static string EndUml()
        {
            return "\n @enduml";
        }

        private static string StartUml()
        {
            return "\n @startUml \n";
        }

        static string GetProjectClasses(string projectDirectoryLocation)
        {

           var projectFileNames = Directory.GetFiles(projectDirectoryLocation).ToArray();  //.Where(a=>a.EndsWith(".cs")).ToArray();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("package \"" + Path.GetFileName(projectDirectoryLocation) + "\" #DDDDDD {");
            foreach (var fileString in projectFileNames)
            {
                var code = new StreamReader(fileString).ReadToEnd();
                SyntaxTree node = CSharpSyntaxTree.ParseText(code);

                var publicMembers = node.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().ToList();
                var fields = node.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax>().ToList();
                var className = node.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;
                if (className != null)
                {

                    sb.AppendLine(@$"class {node.GetRoot().DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text}" + "{");
                    foreach (var field in fields.Where(a => a.Parent.SyntaxTree == node))
                    {
                        sb.AppendLine(@$"{string.Join(" ", field.Modifiers.Select(a => a.Text).ToList())
                            .Replace("private", "-")
                            .Replace("public", "+")  } {field.Declaration.ToString()  } ");
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
