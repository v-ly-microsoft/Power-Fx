//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AppMagic.Authoring.Texl
{
    internal sealed class Identifier
    {
        public readonly Token AtToken; // The "@" token, if any. May be null.
        public readonly IdentToken Token;
        public readonly DName Name;
        public readonly DPath Namespace;

        public Identifier(DPath theNamespace, Token atToken, IdentToken tok)
        {
            Contracts.Assert(theNamespace.IsValid);
            Contracts.AssertValueOrNull(atToken);
            Contracts.AssertValue(tok);
            Contracts.Assert(tok.Name.IsValid);

            Namespace = theNamespace;
            AtToken = atToken;
            Token = tok;
            Name = tok.Name;
        }

        public Identifier Clone(Span ts)
        {
            return new Identifier(
                Namespace,
                AtToken == null ? null : AtToken.Clone(ts),
                Token.Clone(ts).As<IdentToken>());
        }

        public Identifier(IdentToken token)
            : this(DPath.Root, null, token)
        { }

        public Identifier(Token atToken, IdentToken token)
            : this(DPath.Root, atToken, token)
        { }
    }
}