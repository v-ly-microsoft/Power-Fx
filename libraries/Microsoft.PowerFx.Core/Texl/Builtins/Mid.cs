﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.AppMagic.Authoring.Texl
{
    // Mid(source:s, start:n, [count:n])
    internal sealed class MidFunction : BuiltinFunction
    {
        public override bool RequiresErrorContext => true;
        public override bool IsSelfContained => true;

        public MidFunction()
            : base("Mid", TexlStrings.AboutMid, FunctionCategories.Text, DType.String, 0, 2, 3, DType.String, DType.Number, DType.Number)
        { }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new [] { TexlStrings.StringFuncArg1, TexlStrings.StringFuncArg2 };
            yield return new [] { TexlStrings.StringFuncArg1, TexlStrings.StringFuncArg2, TexlStrings.StringFuncArg3 };
        }
    }

    // Mid(source:s|*[s], start:n|*[n], [count:n|*[n]])
    internal sealed class MidTFunction : BuiltinFunction
    {
        public override bool RequiresErrorContext => true;
        public override bool IsSelfContained => true;

        public MidTFunction()
            : base("Mid", TexlStrings.AboutMidT, FunctionCategories.Table, DType.EmptyTable, 0, 2, 3)
        { }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new [] { TexlStrings.StringTFuncArg1, TexlStrings.StringFuncArg2 };
            yield return new [] { TexlStrings.StringTFuncArg1, TexlStrings.StringFuncArg2, TexlStrings.StringFuncArg3 };
        }

        public override string GetUniqueTexlRuntimeName(bool isPrefetching = false)
        {
            return GetUniqueTexlRuntimeName(suffix: "_T");
        }

        public override bool CheckInvocation(TexlNode[] args, DType[] argTypes, IErrorContainer errors, out DType returnType)
        {
            Contracts.AssertValue(args);
            Contracts.AssertAllValues(args);
            Contracts.AssertValue(argTypes);
            Contracts.Assert(args.Length == argTypes.Length);
            Contracts.AssertValue(errors);
            Contracts.Assert(MinArity <= args.Length && args.Length <= MaxArity);

            bool fValid = base.CheckInvocation(args, argTypes, errors, out returnType);

            DType type0 = argTypes[0];
            DType type1 = argTypes[1];

            // Arg0 should be either a string or a column of strings.
            // Its type dictates the function return type.
            if (type0.IsTable)
            {
                // Ensure we have a one-column table of strings
                fValid &= CheckStringColumnType(type0, args[0], errors);
                // Borrow the return type from the 1st arg
                returnType = type0;
            }
            else
            {
                returnType = DType.CreateTable(new TypedName(DType.String, OneColumnTableResultName));
                if (!DType.String.Accepts(type0))
                {
                    fValid = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[0], TexlStrings.ErrStringExpected);
                }
            }

            // Arg1 should be either a number or a column of numbers.
            if (type1.IsTable)
                fValid &= CheckNumericColumnType(type1, args[1], errors);
            else if (!DType.Number.Accepts(type1))
            {
                fValid = false;
                errors.EnsureError(DocumentErrorSeverity.Severe, args[1], TexlStrings.ErrNumberExpected);
            }

            // Arg2 should be either a number or a column of numbers, if it exists.
            if (argTypes.Length > 2)
            {
                DType type2 = argTypes[2];
                if (type2.IsTable)
                    fValid &= CheckNumericColumnType(type2, args[2], errors);
                else if (!DType.Number.Accepts(type2))
                {
                    fValid = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[2], TexlStrings.ErrNumberExpected);
                }
            }

            // At least one arg has to be a table.
            if (!type0.IsTable && !type1.IsTable && (argTypes.Length <= 2 || !argTypes[2].IsTable))
            {
                fValid = false;
                errors.EnsureError(DocumentErrorSeverity.Severe, args[0], TexlStrings.ErrTypeError);
                errors.EnsureError(DocumentErrorSeverity.Severe, args[1], TexlStrings.ErrTypeError);
                if (args.Length > 2)
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[2], TexlStrings.ErrTypeError);
            }

            return fValid;
        }
    }
}
