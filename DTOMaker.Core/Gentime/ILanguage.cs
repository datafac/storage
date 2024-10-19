namespace DTOMaker.Gentime
{
    public interface ILanguage
    {
        string CommentPrefix { get; }
        string CommandPrefix { get; }
        string TokenPrefix { get; }
        string TokenSuffix { get; }
        string GetDataTypeToken(string dataTypeName);
        string GetDefaultValue(string dataTypeName);
        string GetValueAsCode(object? value);
    }
}
