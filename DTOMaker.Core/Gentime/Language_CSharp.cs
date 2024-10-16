namespace DTOMaker.Gentime
{
    public class Language_CSharp : ILanguage
    {
        private static readonly Language_CSharp _instance = new Language_CSharp();
        public static Language_CSharp Instance => _instance;

        private Language_CSharp()
        {
            CommentPrefix = "//";
            CommandPrefix = "##";
            TokenPrefix = "T_";
            TokenSuffix = "_";
        }

        public string TokenPrefix { get; } = "";
        public string TokenSuffix { get; } = "";
        public string CommentPrefix { get; } = "";
        public string CommandPrefix { get; } = "";

        //private string NativeTypeRefToToken(NativeTypeRef ntr)
        //{
        //    return ntr.InnerType switch
        //    {
        //        // todo? fullnames
        //        NativeType.Boolean => "bool",
        //        NativeType.Int32 => "int",
        //        NativeType.UInt32 => "uint",
        //        NativeType.Int64 => "long",
        //        NativeType.UInt64 => "ulong",
        //        NativeType.Int128 => "System.Int128",
        //        NativeType.UInt128 => "System.UInt128",
        //        NativeType.String => "string",
        //        NativeType.Binary => "Octets",
        //        _ => $"Unknown<{ntr.InnerType}>"
        //    };
        //}

        //private string NativeTypeRefDefaultValue(NativeTypeRef ntr)
        //{
        //    return ntr.InnerType switch
        //    {
        //        NativeType.String => "string.Empty",
        //        NativeType.Binary => "Octets.Empty",
        //        _ => $"default"
        //    };
        //}

        //private string ModelEnumRefToToken(ModelEnumRef modelEnumRef)
        //{
        //    var result = new StringBuilder();
        //    result.Append(modelEnumRef.DomainName);
        //    result.Append('.');
        //    result.Append(modelEnumRef.ItemName);
        //    return result.ToString();
        //}

        //private string OtherEnumRefToToken(OtherEnumRef otherEnumRef)
        //{
        //    var result = new StringBuilder();
        //    result.Append(otherEnumRef.FullName);
        //    return result.ToString();
        //}

        //private string EntityRefToToken(EntityRef er, bool defaultValue)
        //{
        //    var result = new StringBuilder();
        //    result.Append(er.DomainName);
        //    result.Append('.');
        //    result.Append(er.ItemName);
        //    if (defaultValue)
        //    {
        //        result.Append(".Empty");
        //    }
        //    return result.ToString();
        //}

        //private string GenericTypeRefToToken(GenericTypeRef gtr, bool defaultValue)
        //{
        //    StringBuilder result = new StringBuilder();
        //    result.Append(gtr.DomainName);
        //    result.Append('.');
        //    result.Append(gtr.ItemName);
        //    result.Append('<');
        //    if (gtr.GenericTypeArgs is not null)
        //    {
        //        result.Append(string.Join(',', gtr.GenericTypeArgs.NonNull().Select(GetDataTypeToken)));
        //    }
        //    result.Append('>');
        //    if (defaultValue)
        //    {
        //        result.Append(".Empty");
        //    }
        //    return result.ToString();
        //}

        public string GetDataTypeToken(string dataTypeName)
        {
            return dataTypeName;
            //return dataTypeRef switch
            //{
            //    null => throw new ArgumentNullException(nameof(dataTypeRef)),
            //    NativeTypeRef ntr => NativeTypeRefToToken(ntr),
            //    ModelEnumRef etr => ModelEnumRefToToken(etr),
            //    OtherEnumRef etr => OtherEnumRefToToken(etr),
            //    EntityRef ent => EntityRefToToken(ent, false),
            //    GenericTypeRef gtr => GenericTypeRefToToken(gtr, false),
            //    _ => throw new NotSupportedException($"DataTypeRef: {dataTypeRef}")
            //};
        }

        public string GetDefaultValue(string dataTypeName)
        {
            return dataTypeName switch
            {
                "String" => "string.Empty",
                //NativeType.Binary => "Octets.Empty",
                _ => $"default"
            };
            //return dataTypeRef switch
            //{
            //    null => throw new ArgumentNullException(nameof(dataTypeRef)),
            //    NativeTypeRef ntr => NativeTypeRefDefaultValue(ntr),
            //    ModelEnumRef => "default",
            //    OtherEnumRef => "default",
            //    EntityRef er => EntityRefToToken(er, true),
            //    GenericTypeRef gtr => GenericTypeRefToToken(gtr, true),
            //    _ => throw new NotSupportedException($"DataTypeRef: {dataTypeRef}")
            //};
        }
    }
}
