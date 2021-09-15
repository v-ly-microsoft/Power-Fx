﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AppMagic.Authoring.Texl;
using System.Collections.Generic;

namespace Microsoft.PowerFx.Core.Utils
{
    internal static class GetTokensUtils
    {
        internal static IReadOnlyDictionary<string, TokenResultType> GetTokens(TexlBinding binding, GetTokensFlags flags)
        {
            var tokens = new Dictionary<string, TokenResultType>();

            if (binding == null)
            {
                return tokens;
            }

            if (flags.HasFlag(GetTokensFlags.UsedInExpression))
            {
                foreach (var item in binding.GetCalls())
                {
                    if (item.Function != null)
                    {
                        tokens[item.Function.QualifiedName] = TokenResultType.Function;
                    }
                }

                foreach (var item in binding.GetFirstNames())
                {
                    switch (item.Kind)
                    {
                        case BindKind.Control:
                            tokens[item.Name] = TokenResultType.HostSymbol;
                            break;
                        case BindKind.PowerFxResolvedObject:
                            tokens[item.Name] = TokenResultType.Variable;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (flags.HasFlag(GetTokensFlags.AllFunctions))
            {
                foreach (var item in binding.NameResolver.Functions)
                {
                    tokens[item.QualifiedName] = TokenResultType.Function;
                }
            }

            return tokens;
        }
    }
}