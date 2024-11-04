namespace DTOMaker.Gentime
{
    internal sealed class NestedScope
    {
        private NestedScope? Parent { get; } = null;
        public IModelScope ModelScope { get; }
        public ILanguage Language { get; }
        public TokenReplacer GetReplacer() => new TokenReplacer(Language, ModelScope.Variables);
        public bool LocalIsActive { get; set; } = true;
        public ScopeKind Kind { get; set; } = ScopeKind.Normal;
        public int LastLineNumber { get; set; } = 0;

        public bool ParentIsActive => Parent?.IsActive ?? true;
        public bool IsActive => LocalIsActive && ParentIsActive;

        public NestedScope(NestedScope? parent, IModelScope modelScope, ILanguage language)
        {
            Parent = parent;
            ModelScope = modelScope;
            Language = language;
        }
    }
}
