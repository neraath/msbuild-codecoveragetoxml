// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertCodeCoverateToXmlTask.cs" company="Improving Enterprises">
//   Copyright (c) Improving Enterprises. All rights reserved.
// </copyright>
// <author>Christopher Weldon - chris.weldon@improvingenterprises.com</author>
// <summary>
//   Contains the MSBuild task to convert the MSTest binary code coverage into an XML file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Improving.MSBuildTasks.CodeCoverageToXml
{
    using System;
    using System.IO;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.VisualStudio.Coverage.Analysis;

    /// <summary>
    /// Contains the MSBuild task to convert the MSTest binary code coverage into an XML file.
    /// </summary>
    /// <remarks>
    /// This was inspired by Peter Huene's article 
    /// <see href="http://blogs.msdn.com/b/phuene/archive/2009/12/01/programmatic-coverage-analysis-in-visual-studio-2010.aspx">Programmatic Coverage Analysis in Visual Studio 2010</see>. 
    /// However, when working on a customer project in which we were attempting to tightly integrate the build process
    /// using MSBuild thru Hudson, it was important that we didn't create another executable to run on command-line. 
    /// </remarks>
    public class ConvertCodeCoverateToXmlTask : Task
    {
        #region Properties

        /// <summary>
        /// Gets or sets the path to the binary code coverage output file.
        /// </summary>
        [Required]
        public string CoverageFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the instrumented binaries (libraries, executables, etc.) that were analyzed in the <seealso cref="CoverageFile"/>.
        /// </summary>
        public string[] BinarySearchPaths { get; set; }

        /// <summary>
        /// Gets or sets the path to the debug symbols for the corresponding <seealso cref="BinarySearchPaths">binary files</seealso>.
        /// </summary>
        public string[] SymbolSearchPaths { get; set; }

        /// <summary>
        /// Gets or sets the path where the XML file should be output to.
        /// </summary>
        [Required]
        public string OutputReportFile { get; set; }

        #endregion

        /// <summary>
        /// Executes the conversion task.
        /// </summary>
        /// <remarks>
        /// The task opens the code coverage file, corresponding binaries, and symbols and delivers an XML report of the
        /// same information to the file name specified in <seealso cref="OutputReportFile"/>.
        /// </remarks>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                // Validate the CoverageFile.
                if (!File.Exists(this.CoverageFile))
                {
                    throw new FileNotFoundException("Could not locate the code coverage file.", this.CoverageFile);
                }

                // Validate the binary search paths.
                foreach (var binarySearchPath in BinarySearchPaths)
                {
                    if (!Directory.Exists(binarySearchPath))
                    {
                        throw new DirectoryNotFoundException(string.Format("Could not find the binary search path directory {0}.", binarySearchPath));
                    }
                }

                // Validate the symbol search paths.
                foreach (var symbolSearchPath in this.SymbolSearchPaths)
                {
                    if (!Directory.Exists(symbolSearchPath))
                    {
                        throw new DirectoryNotFoundException(string.Format("Could not find the symbol search path directory {0}.", symbolSearchPath));
                    }
                }

                // Validate the destination file.
                if (string.IsNullOrEmpty(this.OutputReportFile))
                {
                    throw new ArgumentNullException("OutputReportFile", "OutputReportFile must be a valid path.");
                }

                // Create our CoverageInfo object from the parameters passed to this task.
                using (CoverageInfo info = CoverageInfo.CreateFromFile(
                        this.CoverageFile, this.BinarySearchPaths, this.SymbolSearchPaths))
                {
                    // Create the data set from our info we generated.
                    CoverageDS dataSet = info.BuildDataSet(null);

                    // Write to the OutputReportFile.
                    dataSet.WriteXml(this.OutputReportFile);
                }
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true);
                return false;
            }

            return true;
        }
    }
}
