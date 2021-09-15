﻿//------------------------------------------------------------------------------
// <copyright file="ShowColumns.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.App;
using Microsoft.PowerFx.Core.Entities.QueryOptions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Microsoft.AppMagic.Authoring.Texl
{
    // ShowColumns(source:*[...], name:s, name:s, ...)
    internal sealed class ShowColumnsFunction : FunctionWithTableInput
    {
        public override bool AffectsDataSourceQueryOptions => true;
        public override bool IsSelfContained => true;

        public ShowColumnsFunction()
            : base("ShowColumns", TexlStrings.AboutShowColumns, FunctionCategories.Table, DType.EmptyTable, 0, 2, int.MaxValue, DType.EmptyTable)
        { }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new[] { TexlStrings.ShowColumnsArg1, TexlStrings.ShowColumnsArg2 };
            yield return new[] { TexlStrings.ShowColumnsArg1, TexlStrings.ShowColumnsArg2, TexlStrings.ShowColumnsArg2 };
            yield return new[] { TexlStrings.ShowColumnsArg1, TexlStrings.ShowColumnsArg2, TexlStrings.ShowColumnsArg2, TexlStrings.ShowColumnsArg2 };
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures(int arity)
        {
            if (arity > 2)
                return GetGenericSignatures(arity, TexlStrings.ShowColumnsArg1, TexlStrings.ShowColumnsArg2);
            return base.GetSignatures(arity);
        }

        public override bool CheckInvocation(TexlNode[] args, DType[] argTypes, IErrorContainer errors, out DType returnType)
        {
            Contracts.AssertValue(args);
            Contracts.AssertValue(argTypes);
            Contracts.Assert(args.Length == argTypes.Length);
            Contracts.AssertValue(errors);
            Contracts.Assert(MinArity <= args.Length && args.Length <= MaxArity);

            bool isValidInvocation = base.CheckInvocation(args, argTypes, errors, out returnType);
            Contracts.Assert(returnType.IsTable);

            if (!argTypes[0].IsTable)
            {
                isValidInvocation = false;
                errors.EnsureError(DocumentErrorSeverity.Severe, args[0], TexlStrings.ErrNeedTable_Func, Name);
            }
            else
                returnType = argTypes[0];

            DType colsToKeep = DType.EmptyTable;

            // The result type has N columns, as specified by (args[1],args[2],args[3],...)
            int count = args.Length;
            for (var i = 1; i < count; i++)
            {
                TexlNode nameArg = args[i];
                DType nameArgType = argTypes[i];

                // Verify we have a string literal for the column name. Accd to spec, we don't support
                // arbitrary expressions that evaluate to string values, because these values contribute to
                // type analysis, so they need to be known upfront (before ShowColumns executes).
                StrLitNode nameNode;
                if (nameArgType.Kind != DKind.String || (nameNode = nameArg.AsStrLit()) == null)
                {
                    isValidInvocation = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, nameArg, TexlStrings.ErrExpectedStringLiteralArg_Name, nameArg.ToString());
                    continue;
                }

                // Verify that the name is valid.
                if (!DName.IsValidDName(nameNode.Value))
                {
                    isValidInvocation = false;
                    errors.EnsureError(DocumentErrorSeverity.Severe, nameArg, TexlStrings.ErrArgNotAValidIdentifier_Name, nameNode.Value);
                    continue;
                }

                DName columnName = new DName(nameNode.Value);

                // Verify that the name exists.
                DType columnType;
                if (!returnType.TryGetType(columnName, out columnType))
                {
                    isValidInvocation = false;
                    returnType.ReportNonExistingName(FieldNameKind.Logical, errors, columnName, args[i]);
                    continue;
                }

                // Verify that the name was only specified once.
                DType existingColumnType;
                if (colsToKeep.TryGetType(columnName, out existingColumnType))
                {
                    isValidInvocation = false;
                    errors.EnsureError(DocumentErrorSeverity.Warning, nameArg, TexlStrings.WarnColumnNameSpecifiedMultipleTimes_Name, columnName);
                    continue;
                }

                // Make a note of which columns are being kept.
                Contracts.Assert(columnType.IsValid);
                colsToKeep = colsToKeep.Add(columnName, columnType);
            }

            // Drop everything but the columns that need to be kept.
            returnType = colsToKeep;

            return isValidInvocation;
        }

        public override bool UpdateDataQuerySelects(CallNode callNode, TexlBinding binding, DataSourceToQueryOptionsMap dataSourceToQueryOptionsMap)
        {
            Contracts.AssertValue(callNode);
            Contracts.AssertValue(binding);

            if (!CheckArgsCount(callNode, binding))
                return false;

            TexlNode[] args = callNode.Args.Children.VerifyValue();

            DType dsType = binding.GetType(args[0]);
            if (dsType.AssociatedDataSources == null)
                return false;

            var resultType = binding.GetType(callNode).VerifyValue();

            bool retval = false;
            foreach (var typedName in resultType.GetNames(DPath.Root))
            {
                DType columnType = typedName.Type;
                string columnName = typedName.Name.Value;

                Contracts.Assert(dsType.Contains(new DName(columnName)));

                retval |= dsType.AssociateDataSourcesToSelect(dataSourceToQueryOptionsMap, columnName, columnType, true);
            }
            return retval;
        }

        // This method returns true if there are special suggestions for a particular parameter of the function.
        public override bool HasSuggestionsForParam(int argumentIndex)
        {
            Contracts.Assert(0 <= argumentIndex);

            return argumentIndex >= 0;
        }
    }
}
