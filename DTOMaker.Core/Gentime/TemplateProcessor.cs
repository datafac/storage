using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DTOMaker.Gentime
{
    public sealed class TemplateProcessor : ITemplateProcessor
    {
        private (TemplateCommand command, ReadOnlyMemory<char> remaining) GetCommandAndArgs(ReadOnlyMemory<char> line)
        {
            ReadOnlySpan<char> firstToken;
            ReadOnlyMemory<char> remaining;
            int indexOfFirstSpace = line.Span.IndexOf(' ');
            if (indexOfFirstSpace < 0)
            {
                firstToken = line.Span;
                remaining = ReadOnlyMemory<char>.Empty;
            }
            else
            {
                firstToken = line.Span.Slice(0, indexOfFirstSpace);
                remaining = line.Slice(indexOfFirstSpace).TrimStart();
            }
            Span<char> command = stackalloc char[firstToken.Length];
            firstToken.ToLowerInvariant(command);
            if (command.SequenceEqual("eval")) return (TemplateCommand.Eval, remaining);
            if (command.SequenceEqual("if")) return (TemplateCommand.If, remaining);
            if (command.SequenceEqual("elif")) return (TemplateCommand.Elif, remaining);
            if (command.SequenceEqual("else")) return (TemplateCommand.Else, remaining);
            if (command.SequenceEqual("endif")) return (TemplateCommand.EndIf, remaining);
            if (command.SequenceEqual("foreach")) return (TemplateCommand.ForEach, remaining);
            if (command.SequenceEqual("endfor")) return (TemplateCommand.EndFor, remaining);
            return (TemplateCommand.Unknown, remaining);
        }
        private (TemplateCommand command, ReadOnlyMemory<char> remaining) GetCommand(SourceLine source, ILanguage options)
        {
            var trimmed = source.Text.AsMemory().Trim();
            if (!trimmed.Span.StartsWith(options.CommentPrefix.AsSpan())) return (TemplateCommand.None, ReadOnlyMemory<char>.Empty);
            trimmed = trimmed.Slice(options.CommentPrefix.Length).Trim();
            if (!trimmed.Span.StartsWith(options.CommandPrefix.AsSpan())) return (TemplateCommand.None, ReadOnlyMemory<char>.Empty);
            trimmed = trimmed.Slice(options.CommandPrefix.Length).Trim();
            var (command, remaining) = GetCommandAndArgs(trimmed);
            return (command, remaining);
        }
        private string[] ParseArguments(ReadOnlySpan<char> text)
        {
            var args = new List<string>();
            bool inSpace = true;
            var arg = new StringBuilder();
            foreach (char ch in text)
            {
                if (inSpace)
                {
                    if (!Char.IsWhiteSpace(ch))
                    {
                        inSpace = false;
                        arg.Append(ch);
                    }
                }
                else
                {
                    if (Char.IsWhiteSpace(ch))
                    {
                        // end
                        args.Add(arg.ToString());
                        inSpace = true;
                        arg = new StringBuilder();
                    }
                    else
                    {
                        arg.Append(ch);
                    }
                }
            }
            if (arg.Length > 0)
            {
                args.Add(arg.ToString());
            }
            return args.ToArray();
        }
        private string GetArg(SourceLine source, string[] args, int argn)
        {
            if (argn >= args.Length)
                throw new TemplateException($"Command arg[{argn}] missing", source);
            return args[argn];
        }
        private string[] ProcessTemplateScope(ReadOnlySpan<SourceLine> template, int startLine, NestedScope scope, ILanguage options)
        {
            List<string> output = new List<string>();
            var parser = new ExprParser();
            SourceLine lastLine = SourceLine.Empty;
            int lineNumber = startLine;
            while (lineNumber < template.Length)
            {
                var line = template[lineNumber];
                lastLine = line;
                // template commands
                var (command, remaining) = GetCommand(line, options);
                if (command != TemplateCommand.None)
                {
                    // command - don't emit
                    var args = ParseArguments(remaining.Span);
                    switch (command)
                    {
                        case TemplateCommand.Eval:
                            if (scope.IsActive)
                            {
                                // parse and evaluate expression
                                var parsed = parser.Parse(remaining);
                                if (parsed is null) throw new TemplateException("Unknown error", line);
                                if (parsed is ErrorNode error) throw new TemplateException(error.Message ?? "", line);
                                parsed.Evaluate(scope.ModelScope.Variables);
                                // todo check for eval error
                            }
                            break;
                        case TemplateCommand.If:
                            {
                                var innerScope = new NestedScope(scope, scope.ModelScope, scope.Language);
                                // parse and evaluate expression as boolean
                                var parsed = parser.Parse(remaining);
                                if (parsed is null) throw new TemplateException("Unknown error", line);
                                if (parsed is ErrorNode error) throw new TemplateException(error.Message ?? "", line);
                                var result = parsed.Evaluate(innerScope.ModelScope.Variables);
                                innerScope.LocalIsActive = result is bool boolResult ? boolResult : false;
                                innerScope.Kind = ScopeKind.InIfBlock;
                                var innerOutput = ProcessTemplateScope(template, lineNumber + 1, innerScope, options);
                                output.AddRange(innerOutput);
                                lineNumber = innerScope.LastLineNumber;
                            }
                            break;
                        case TemplateCommand.Elif:
                            // skip if already handled
                            if (!scope.LocalIsActive)
                            {
                                // not already handled - parse and evaluate expression as boolean
                                var parsed = parser.Parse(remaining);
                                if (parsed is null) throw new TemplateException("Unknown error", line);
                                if (parsed is ErrorNode error) throw new TemplateException(error.Message ?? "", line);
                                var result = parsed.Evaluate(scope.ModelScope.Variables);
                                scope.LocalIsActive = result is bool boolResult ? boolResult : false;
                            }
                            break;
                        case TemplateCommand.Else:
                            // skip if already handled
                            scope.LocalIsActive = !scope.LocalIsActive;
                            break;
                        case TemplateCommand.EndIf:
                            if (scope.Kind != ScopeKind.InIfBlock)
                                throw new TemplateException("If/EndIf mismatch", line);
                            scope.LastLineNumber = lineNumber;
                            return output.ToArray();
                        case TemplateCommand.ForEach:
                            {
                                string iteratorName = GetArg(line, args, 0);
                                var (hasIterations, innerModelScopes) = scope.ModelScope.GetInnerScopes(iteratorName);
                                if (hasIterations is null)
                                    throw new TemplateException("Unknown iterator name", line);
                                int newLineNumber = lineNumber;
                                foreach (var innerModelScope in innerModelScopes)
                                {
                                    var innerScope = new NestedScope(scope, innerModelScope, scope.Language);
                                    innerScope.Kind = ScopeKind.InForEach;
                                    innerScope.LocalIsActive = hasIterations.Value;
                                    var innerOutput = ProcessTemplateScope(template, lineNumber + 1, innerScope, options);
                                    output.AddRange(innerOutput);
                                    newLineNumber = innerScope.LastLineNumber;
                                }
                                lineNumber = newLineNumber;
                            }
                            break;
                        case TemplateCommand.EndFor:
                            if (scope.Kind != ScopeKind.InForEach)
                                throw new TemplateException("ForEach/EndFor mismatch", line);
                            scope.LastLineNumber = lineNumber;
                            return output.ToArray();
                        default:
                            throw new TemplateException("Unknown command", line);
                    }
                }
                else
                {
                    if (scope.IsActive)
                    {
                        // output enabled - replace tokens and emit
                        string outputLine
                            = (string.IsNullOrWhiteSpace(line.Text)
                            || line.Text.IndexOf(options.TokenPrefix) < 0)
                            ? line.Text
                            : scope.GetReplacer().ReplaceTokens(line.Text);
                        output.Add(outputLine);
                    }
                }
                lineNumber++;
            }
            if (scope.Kind == ScopeKind.InIfBlock)
                throw new TemplateException("If/EndIf mismatch", lastLine);
            if (scope.Kind == ScopeKind.InForEach)
                throw new TemplateException("ForEach/EndFor mismatch", lastLine);
            scope.LastLineNumber = lineNumber;
            return output.ToArray();
        }

        private static ReadOnlyMemory<SourceLine> EnumerateSource(ReadOnlySpan<string> source)
        {
            var result = new SourceLine[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = new SourceLine(i, source[i]);
            }
            return result;
        }

        public string[] ProcessTemplate(ReadOnlySpan<string> source, ILanguage language, IModelScope outerScope)
        {
            ReadOnlyMemory<SourceLine> template = EnumerateSource(source);
            var scope = new NestedScope(null, outerScope, language);
            var output = ProcessTemplateScope(template.Span, 0, scope, language);
            return output;
        }
    }
}
