﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.Delegation;
using System.Collections.Generic;

namespace Microsoft.AppMagic.Authoring.Texl
{
    // Filter(source:*, predicate1:b, [predicate2:b, ...])
    // Corresponding DAX function: Filter
    internal sealed class FilterFunction : FilterFunctionBase
    {
        public override bool RequiresErrorContext { get { return true; } }

        public FilterFunction()
            : base("Filter", TexlStrings.AboutFilter, FunctionCategories.Table, DType.EmptyTable, -2, 2, int.MaxValue, DType.EmptyTable)
        {
            ScopeInfo = new FunctionScopeInfo(this, acceptsLiteralPredicates: false);
        }

        public override bool SupportsParamCoercion { get { return true; } }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            // Enumerate just the base overloads (the first 3 possibilities).
            yield return new[] { TexlStrings.FilterArg1, TexlStrings.FilterArg2 };
            yield return new[] { TexlStrings.FilterArg1, TexlStrings.FilterArg2, TexlStrings.FilterArg2 };
            yield return new[] { TexlStrings.FilterArg1, TexlStrings.FilterArg2, TexlStrings.FilterArg2, TexlStrings.FilterArg2 };
        }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures(int arity)
        {
            if (arity > 2)
                return GetGenericSignatures(arity, TexlStrings.FilterArg1, TexlStrings.FilterArg2);
            return base.GetSignatures(arity);
        }

        public override bool CheckInvocation(TexlBinding binding, TexlNode[] args, DType[] argTypes, IErrorContainer errors, out DType returnType, out Dictionary<TexlNode, DType> nodeToCoercedTypeMap)
        {
            Contracts.AssertValue(args);
            Contracts.AssertValue(argTypes);
            Contracts.Assert(args.Length == argTypes.Length);
            Contracts.AssertValue(errors);
            nodeToCoercedTypeMap = null;
            int viewCount = 0;

            bool fArgsValid = base.CheckInvocation(args, argTypes, errors, out returnType, out nodeToCoercedTypeMap);

            var dataSourceVisitor = new ViewFilterDataSourceVisitor(binding);
            // Ensure that all the args starting at index 1 are booleans or view
            for (int i = 1; i < args.Length; i++)
            {
                if (argTypes[i].Kind == DKind.ViewValue)
                {
                    if (++viewCount > 1)
                    {
                        // Only one view expected
                        errors.EnsureError(DocumentErrorSeverity.Severe, args[i], TexlStrings.ErrOnlyOneViewExpected);
                        fArgsValid = false;
                        continue;
                    }

                    // Use the visitor to get the datasource info and if a view was already used anywhere in the node tree.
                    args[0].Accept(dataSourceVisitor);
                    var dataSourceInfo = dataSourceVisitor.cdsDataSourceInfo;

                    if (dataSourceVisitor.ContainsViewFilter)
                    {
                        // Only one view expected
                        errors.EnsureError(DocumentErrorSeverity.Severe, args[i], TexlStrings.ErrOnlyOneViewExpected);
                        fArgsValid = false;
                        continue;
                    }

                    if (dataSourceInfo != null)
                    {
                        // Verify the view belongs to the same datasource
                        var viewInfo = argTypes[i].ViewInfo.VerifyValue();
                        if (viewInfo.RelatedEntityName != dataSourceInfo.Name)
                        {
                            errors.EnsureError(DocumentErrorSeverity.Severe, args[i], TexlStrings.ErrViewFromCurrentTableExpected, dataSourceInfo.Name);
                            fArgsValid = false;
                        }
                    }
                    else
                    {
                        errors.EnsureError(DocumentErrorSeverity.Severe, args[i], TexlStrings.ErrBooleanExpected);
                        fArgsValid = false;
                    }

                    continue;
                }
                else if (DType.Boolean.Accepts(argTypes[i]))
                {
                    continue;
                }
                else if (!argTypes[i].CoercesTo(DType.Boolean))
                {
                    errors.EnsureError(DocumentErrorSeverity.Severe, args[i], TexlStrings.ErrBooleanExpected);
                    fArgsValid = false;
                    continue;
                }
            }

            // The first Texl function arg determines the cursor type, the scope type for the lambda params, and the return type.
            DType typeScope;
            fArgsValid &= ScopeInfo.CheckInput(args[0], argTypes[0], errors, out typeScope);

            Contracts.Assert(typeScope.IsRecord);
            returnType = typeScope.ToTable();

            return fArgsValid;
        }

        // Verifies if given callnode can be server delegatable or not.
        // Return true if
        //        - Arg0 is delegatable ds and supports filter operation.
        //        - All predicates to filter are delegatable if each firstname/binary/unary/dottedname/call node in each predicate satisfies delegation criteria set by delegation strategy for each node.
        public override bool IsServerDelegatable(CallNode callNode, TexlBinding binding)
        {
            Contracts.AssertValue(callNode);
            Contracts.AssertValue(binding);

            if (!CheckArgsCount(callNode, binding))
                return false;

            IExternalDataSource dataSource;
            FilterOpMetadata metadata = null;
            IDelegationMetadata delegationMetadata = null;
            if (TryGetEntityMetadata(callNode, binding, out delegationMetadata))
            {
                if (!binding.Document.Properties.EnabledFeatures.IsEnhancedDelegationEnabled ||
                    !TryGetValidDataSourceForDelegation(callNode, binding, DelegationCapability.ArrayLookup, out _))
                {
                    SuggestDelegationHint(callNode, binding);
                    return false;
                }

                metadata = delegationMetadata.FilterDelegationMetadata.VerifyValue();
            }
            else
            {
                if (!TryGetValidDataSourceForDelegation(callNode, binding, FunctionDelegationCapability, out dataSource))
                    return false;

                metadata = dataSource.DelegationMetadata.FilterDelegationMetadata;
            }

            TexlNode[] args = callNode.Args.Children.VerifyValue();
            // Validate for each predicate node.
            for (int i = 1; i < args.Length; i++)
            {
                if (!IsValidDelegatableFilterPredicateNode(args[i], binding, metadata))
                    return false;
            }

            return true;
        }

        public override bool IsEcsExcemptedLambda(int index)
        {
            // All lambdas in filter can be excluded from ECS.
            return IsLambdaParam(index);
        }
    }
}
