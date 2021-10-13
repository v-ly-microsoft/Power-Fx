﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AppMagic.Authoring.Texl
{
    // Concat(source:*[...], expr, [separator:s])
    // This is an aggregate function that folds over the specified table (first arg) with respect to
    // the '&' operator and the specified row projection (2nd arg).
    internal sealed class ConcatFunction : FunctionWithTableInput
    {
        public override bool IsSelfContained => true;
        public override bool SupportsParamCoercion => false;

        public ConcatFunction()
            : base("Concat", TexlStrings.AboutConcat, FunctionCategories.Table, DType.String, 0x02, 2, 3, DType.EmptyTable, DType.String)
        {
            ScopeInfo = new FunctionScopeInfo(this, usesAllFieldsInScope: false);
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new [] { TexlStrings.ConcatArg1, TexlStrings.ConcatArg2 };
            yield return new [] { TexlStrings.ConcatArg1, TexlStrings.ConcatArg2, TexlStrings.ConcatArg3 };
        }
    }
}