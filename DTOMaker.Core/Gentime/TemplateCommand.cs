namespace DTOMaker.Gentime
{
    internal enum TemplateCommand
    {
        None,
        Eval,
        If,
        Elif,
        Else,
        EndIf,
        ForEach,
        EndFor,
        Unknown,
    }
}
