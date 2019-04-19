using System;
using System.Collections.Generic;

namespace MultiMod.Editor
{
    /// <summary>
    /// Class that stores data during the exporting process.
    /// </summary>
    [Serializable]
    public class ExportData
    {
        public List<Asset> scriptAssemblies = new List<Asset>();

        public List<Asset> asmDefs = new List<Asset>();

        public List<Asset> assets = new List<Asset>();

        public List<Asset> scenes = new List<Asset>();

        public string prefix;
    }
}
