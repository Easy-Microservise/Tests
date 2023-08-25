using CodeReviewer.Engine;
using CodeReviewer.Structures;
using CodeReviewer.Tests;
using System;
using System.IO;
using System.Linq;

namespace EasyMicroservices.Tests
{
    public class CodeReviewerTests : MainCodeReviewer
    {
        static CodeReviewerTests()
        {
            //enum types to check has "Type" suffix like GenderType
            CustomCodeReviewerManager.AddCustomTypeSuffixNamingCodeReviewer(x => x.IsEnum, CheckType.TypeName, System.StringComparison.Ordinal, "Type");
            //check properties and methods and fields that has "Type" suffix like GetGenderType()
            CustomCodeReviewerManager.AddCustomInsideOfTypeSuffixNamingCodeReviewer(x => x.IsClass, x => x.IsEnum, CheckType.PropertyName | CheckType.MethodName | CheckType.FieldName, System.StringComparison.Ordinal, "Type");
            //check properties and methods and fields that has some boolean suffix like HasPassport
            CustomCodeReviewerManager.AddCustomInsideOfTypePrefixNamingCodeReviewer(x => x.IsClass, x => x == typeof(bool), CheckType.PropertyName | CheckType.MethodName | CheckType.FieldName, System.StringComparison.Ordinal, "Has", "Have", "Is", "Can");
            //check names and values and indexes of enum
            CustomCodeReviewerManager.AddCustomEnumValuesCodeReviewer((Data) => Data.Index != 0 || Data.Name == "None");
            //check a type details very fast with a reviewer
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Contract.") || type.Namespace.Contains(".Entity.") || type.Namespace.Contains(".Document.") ? ("NameSpace of type", "is not a valid namespace!") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Entities.") && !type.Name.EndsWith("Entity") ? ("Suffix name of type", "is not a valid name! suffix have to be 'Entity'") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Documents.") && !type.Name.EndsWith("Document") ? ("Suffix name of type", "is not a valid name! suffix have to be 'Document'") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Contracts.") && !type.Name.EndsWith("Contract") ? ("Suffix name of type", "is not a valid name! suffix have to be 'Contract'") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Contracts.Requests") && !type.Name.EndsWith("RequestContract") ? ("Suffix name of type", "is not a valid name! suffix have to be 'RequestContract'") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Contracts.Responses") && !type.Name.EndsWith("ResponseContract") ? ("Suffix name of type", "is not a valid name! suffix have to be 'ResponseContract'") : default);
            CustomCodeReviewerManager.AddFastCustomCodeReviewer(type => type.Namespace.Contains(".Contracts.Common") && (type.Name.EndsWith("ResponseContract") || type.Name.EndsWith("RequestContract")) ? ("Suffix name of type", "is not a valid name! suffix have to be 'Contract' not 'Request' or 'Response'") : default);
            //check stream
            CustomCodeReviewerManager.AddStreamCustomCodeReviewer(stream =>
            {
                stream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                if (text.ToLower().Contains("password="))
                    return "Text 'Password=' found in your source files! please remove it!";
                return null;
            });

            foreach (var filePath in GetCheckFiles())
            {
                var file = new FileInfo(filePath);
                if (file.Length > 1024 * 1024 * 2)
                    continue;
                AssemblyManager.AddStreamsToReview(new MemoryStream(File.ReadAllBytes(filePath)));
            }
        }

        static string GetSolutionPath()
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory;
            string file = default;
            do
            {
                directory = Path.GetDirectoryName(directory);
                file = Directory.GetFiles(directory).FirstOrDefault(x => x.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));
            }
            while (file == null);
            return directory;
        }

        static string[] GetCheckFiles()
        {
            var path = GetSolutionPath();
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            var extensions = new string[]
            {
                "sln",
                "csproj",
                "cs",
                "txt",
                "json"
            };
            var excludes = new string[]
            {
                "bin",
                "obj",
                "debug",
                "release"
            };

            return files.Where(x => !excludes.Any(e => x.Contains($"{Path.DirectorySeparatorChar}{e}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)) && extensions.Any(e => x.EndsWith(e, StringComparison.OrdinalIgnoreCase))).ToArray();
        }
    }
}