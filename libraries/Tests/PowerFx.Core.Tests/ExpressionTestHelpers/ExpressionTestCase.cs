//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

namespace Microsoft.PowerFx.Core.Tests
{
    // Describe a test case in the file. 
    public class ExpressionTestCase
    {
        // Formula string to run 
        public string Input;

        // Expected Result, indexed by runner name
        public Dictionary<string, string> _expected = new Dictionary<string, string>();

        // Location from source file. 
        public string SourceFile;
        public int SourceLine;

        public override string ToString()
        {
            return $"{Path.GetFileName(this.SourceFile)}:{this.SourceLine}: {Input}";
        }

        public void SetExpected(string expected, string engineName = null)
        {
            if (engineName == null)
            {
                engineName = "-";
            }
            _expected[engineName] = expected;
        }

        public string GetExpected(string engineName)
        {
            if (!_expected.TryGetValue(engineName, out var expected))
            {
                return _expected["-"];
            }
            return expected;
        }
    }
}