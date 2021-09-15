﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AppMagic.Transport;

namespace Microsoft.AppMagic.Authoring.Texl
{
    [TransportType(TransportKind.ByValue)]
    public interface IFunction
    {
        /// <summary>
        /// The locale-specific name of the function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The function's fully qualified name, including the namespace.
        /// If the function is in the global namespace, Function.QualifiedName is the same as Function.Name.
        /// </summary>
        string QualifiedName { get; }

        /// <summary>
        /// A description associated with this function.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// External link to function help.
        /// </summary>
        string HelpLink { get; }

        /// <summary>
        /// The categories of the function.
        /// </summary>
        FunctionCategories FunctionCategoriesMask { get; }
    }
}
