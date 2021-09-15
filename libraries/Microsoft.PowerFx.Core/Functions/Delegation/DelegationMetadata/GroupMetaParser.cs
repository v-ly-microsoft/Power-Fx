﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AppMagic.Common;
using Microsoft.PowerFx.Core.Delegation;

namespace Microsoft.AppMagic.Authoring
{
    internal sealed partial class DelegationMetadata : IDelegationMetadata
    {
        private sealed class GroupMetaParser : MetaParser
        {
            public override OperationCapabilityMetadata Parse(JsonElement dataServiceCapabilitiesJsonObject, DType schema)
            {
                Contracts.AssertValid(schema);

                Dictionary<DPath, DelegationCapability> columnRestrictions = new Dictionary<DPath, DelegationCapability>();
                if (!dataServiceCapabilitiesJsonObject.TryGetProperty(CapabilitiesConstants.Group_Restriction, out var groupRestrictionJsonObject ))
                {
                    return null;
                }

                if (groupRestrictionJsonObject.TryGetProperty(CapabilitiesConstants.Group_UngroupableProperties, out var ungroupablePropertiesJsonArray ))
                {
                    foreach (var prop in ungroupablePropertiesJsonArray.EnumerateArray())
                    {
                        var columnName = new DName(prop.GetString());
                        var columnPath = DPath.Root.Append(columnName);

                        if (!columnRestrictions.ContainsKey(columnPath))
                        {
                            columnRestrictions.Add(columnPath, new DelegationCapability(DelegationCapability.Group));
                        }
                    }
                }

                return new GroupOpMetadata(schema, columnRestrictions);
            }
        }
    }
}