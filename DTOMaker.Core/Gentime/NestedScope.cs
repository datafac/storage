namespace DTOMaker.Gentime
{
    internal sealed class NestedScope
    {
        private NestedScope? Parent { get; } = null;
        public IModelScope ModelScope { get; }
        public TokenReplacer Replacer { get; }
        public bool LocalIsActive { get; set; } = true;
        public ScopeKind Kind { get; set; } = ScopeKind.Normal;
        public int LastLineNumber { get; set; } = 0;

        public bool ParentIsActive => Parent?.IsActive ?? true;
        public bool IsActive => LocalIsActive && ParentIsActive;

        public NestedScope(NestedScope? parent, IModelScope modelScope, TokenReplacer replacer)
        {
            Parent = parent;
            ModelScope = modelScope;
            Replacer = replacer;
        }
    }
}
