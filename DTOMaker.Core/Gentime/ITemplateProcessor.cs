using System;

namespace DTOMaker.Gentime
{
    public interface ITemplateProcessor
    {
        string[] ProcessTemplate(ReadOnlySpan<string> source, ILanguage language, IModelScope outerScope);
    }
}
