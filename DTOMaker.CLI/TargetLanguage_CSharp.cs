namespace DTOMaker.CLI
{
    internal sealed class TargetLanguage_CSharp : ITargetLanguage
    {
        private static readonly TargetLanguage_CSharp _instance = new TargetLanguage_CSharp();
        public static ITargetLanguage Instance => _instance;

        private static readonly string _header =
            """
            using System;
            using System.Linq;
            using DTOMaker.Gentime;
            namespace _targetNamespace_;
            #pragma warning disable CS0162 // Unreachable code detected
            public sealed class EntityGenerator : EntityGeneratorBase
            {
                public EntityGenerator(ILanguage language) : base(language) { }
                protected override void OnGenerate(ModelScopeEntity entity)
                {
            """;

        private static readonly string _footer =
            """
                }
            }
            """;

        public string Name => "CSharp";
        public string PrefixComment => "//";
        public string PrefixMetaCode => "##";
        public string EmitCodePrefix => "Emit(\"";
        public string EmitCodeSuffix => "\");";

        public string EmitFileHeader => _header;
        public string EmitFileFooter => _footer;

        private TargetLanguage_CSharp() { }
    }
}
