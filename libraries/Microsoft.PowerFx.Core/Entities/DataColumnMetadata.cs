﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AppMagic.Authoring.DataToControls;

namespace Microsoft.AppMagic.Authoring
{
    internal sealed class DataColumnMetadata : IDataColumnMetadata
    {
        private readonly ColumnMetadata _columnMetadata;

        public DataColumnMetadata(ColumnMetadata columnMetadata, DataTableMetadata tableMetadata)
        {
            Contracts.AssertValue(columnMetadata);
            Contracts.AssertValue(tableMetadata);

            _columnMetadata = columnMetadata;
            ParentTableMetadata = tableMetadata;
            IsSearchable = _columnMetadata.LookupMetadata.HasValue && _columnMetadata.LookupMetadata.Value.IsSearchable;
            IsSearchRequired = _columnMetadata.LookupMetadata.HasValue && _columnMetadata.LookupMetadata.Value.IsSearchRequired;
            Type = columnMetadata.Type;
            Name = columnMetadata.Name;
            IsExpandEntity = false;
        }

        public DataColumnMetadata(string name, DType type, DataTableMetadata tableMetadata)
        {
            Contracts.AssertValue(name);
            Contracts.AssertValid(type);
            Contracts.AssertValue(tableMetadata);

            Name = name;
            Type = type;
            ParentTableMetadata = tableMetadata;
            IsSearchable = false;
            IsSearchRequired = false;
            IsExpandEntity = true;
        }

        public bool IsSearchable { get; }

        public bool IsExpandEntity { get; }

        public bool IsSearchRequired { get; }

        public string Name { get; }

        public DataTableMetadata ParentTableMetadata { get; }

        public DType Type { get; }
    }

    internal sealed class DataTableMetadata
    {
        public string DisplayName { get; }

        public string Name { get; }

        public DataTableMetadata(string name, string DisplayName)
        {
            Contracts.AssertNonEmpty(name);
            Contracts.AssertNonEmpty(DisplayName);

            Name = name;
            this.DisplayName = DisplayName;
        }
    }
}
