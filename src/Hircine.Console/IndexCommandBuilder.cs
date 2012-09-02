﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hircine.Core;
using Mono.Options;

namespace Hircine.Console
{
    /// <summary>
    /// Static helper class used for validating the command-line tool's interface
    /// </summary>
    public static class IndexCommandBuilder
    {
        /// <summary>
        /// Yields an initialized IndexBuildCommand object from a set of commandline arguments
        /// </summary>
        /// <param name="args">Command line parameters</param>
        /// <param name="showHelp">A flag that indicates whether or not the user simply asked for help</param>
        /// <param name="p">The optionset used to parse the command line</param>
        /// <returns>An IndexBuildCommand object</returns>
        public static IndexBuildCommand ParseIndexBuildCommand(string[] args, out bool showHelp, out OptionSet p)
        {
            var buildCommand = new IndexBuildCommand() { };
            showHelp = false;

            var showHelpClosure = false;

            var connectionStrings = new List<string>() { };
            var assemblies = new List<string> { };

            p = new OptionSet()
                        {
                            {
                                "c|connectionstr=",
                                "a RavenDB {CONNECTIONSTRING} to be used in the index building process.",
                                connectionStrings.Add
                            },
                            {
                                "a|assembly=", "a path to a .NET {ASSEMBLY} which contains RavenDB indexes", assemblies.Add
                            },
                            {
                                "e|emebedded", "Run this job only against an embedded database (useful for testing purposes)", 
                                v => {if (v != null) buildCommand.UseEmbedded = true; }},
                            {
                                "s|sequential", "Run this job sequentially against all of the specified databases, rather than in parallel",
                                v => {if (v != null) buildCommand.ExecuteJobsSequentially = true; }},
                            {
                                "f|continueonfailure", "If this job is being run sequentially, continue running all jobs even if there are failures",
                                v => {if (v != null) buildCommand.ContinueJobOnFailure = true; }},
                            {
                                "h|help", "show this message and exit", v => showHelpClosure = v != null
                            }
                        };

            p.Parse(args);

            //Assign our collected assemblies and connection strings to the command object
            buildCommand.AssemblyPaths = assemblies.ToArray();
            buildCommand.ConnectionStrings = connectionStrings.ToArray();

            showHelp = showHelpClosure;

            return buildCommand;
        }
    }
}