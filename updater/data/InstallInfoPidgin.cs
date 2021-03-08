﻿/*
    This file is part of the updater command line interface.
    Copyright (C) 2017, 2018, 2021  Dirk Stolle

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Diagnostics;

namespace updater.data
{
    /// <summary>
    /// Special case of .exe installation for Pidgin.
    /// </summary>
    public class InstallInfoPidgin : InstallInfoExe
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public InstallInfoPidgin()
            : base()
        {
            // Base class constructor does initialization.
        }


        /// <summary>
        /// Constructor with initial value parameters.
        /// </summary>
        /// <param name="_downloadUrl">URL where the installer can be downloaded</param>
        /// <param name="_algo">hash algorithm that was used to create or verify the checksum</param>
        /// <param name="_check">checksum for the installer - hexadecimal representation</param>
        /// <param name="_sig">signature information</param>
        /// <param name="_silent">switches for silent installation</param>
        public InstallInfoPidgin(string _downloadUrl, HashAlgorithm _algo, string _check, Signature _sig, string _silent)
            : base(_downloadUrl, _algo, _check, _sig, _silent)
        {
            // Base class constructor does initialization.
        }


        /// <summary>
        /// Creates a process instance that can be used to perform the update.
        /// </summary>
        /// <param name="downloadedFile">path to the downloaded installer file</param>
        /// <param name="detected">info about detected software</param>
        /// <returns>Returns a process instance ready to start, if successful.
        /// Returns null, if an error occurred.</returns>
        public override Process createInstallProccess(string downloadedFile, DetectedSoftware detected)
        {
            if (string.IsNullOrWhiteSpace(downloadedFile))
                return null;

            var proc = new Process();
            proc.StartInfo.FileName = downloadedFile;
            proc.StartInfo.Arguments = silentSwitches;
            if (!string.IsNullOrWhiteSpace(detected.installPath))
                proc.StartInfo.Arguments += " /D " + utility.Strings.removeTrailingBackslash(detected.installPath);
            return proc;
        }
    } // class
} // namespace
