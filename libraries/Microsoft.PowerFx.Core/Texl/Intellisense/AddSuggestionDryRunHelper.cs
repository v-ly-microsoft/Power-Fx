﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx;

namespace Microsoft.AppMagic.Authoring.Texl
{
    internal class AddSuggestionDryRunHelper : AddSuggestionHelper
    {
        protected override bool CheckAndAddSuggestion(IntellisenseSuggestionList suggestions, IntellisenseSuggestion candidate)
        {
            return !suggestions.ContainsSuggestion(candidate.DisplayText.Text);
        }

        protected override UIString ConstructUIString(SuggestionKind suggestionKind, DType type, IntellisenseSuggestionList suggestions, string valueToSuggest, int highlightStart, int highlightEnd)
        {
            return new UIString(valueToSuggest, highlightStart, highlightEnd);
        }
    }
}