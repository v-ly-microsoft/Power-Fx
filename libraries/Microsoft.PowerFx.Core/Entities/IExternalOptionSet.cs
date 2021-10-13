//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace PowerApps.Language.Entities
{
    internal interface IExternalOptionSet<T> : IExternalEntity, IDisplayMapped<int>
    {
        string Name { get; }
        bool IsBooleanValued { get; }
        
        string RelatedEntityName { get; }
        string RelatedColumnInvariantName { get; }
        bool IsGlobal { get; }
    }
}