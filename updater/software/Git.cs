﻿/*
    This file is part of the updater command line interface.
    Copyright (C) 2021  Dirk Stolle

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

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using updater.data;

namespace updater.software
{
    /// <summary>
    /// Handles updates of Git for Windows.
    /// </summary>
    public class Git : NoPreUpdateProcessSoftware
    {
        /// <summary>
        /// NLog.Logger for Git class
        /// </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger(typeof(Git).FullName);


        /// <summary>
        /// publisher name for signed installers of Git for Windows
        /// </summary>
        private const string publisherX509 = "CN=Johannes Schindelin, O=Johannes Schindelin, STREET=Raderberger Str. 178, L=Köln, S=North Rhine-Westphalia, PostalCode=50968, C=DE";

        /// <summary>
        /// expiration date for the publisher certificate
        /// </summary>
        private static readonly DateTime publisherExpiryDate = new DateTime(2021, 5, 20, 0, 0, 0, DateTimeKind.Utc);


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get newer
        /// information about the software when calling the info() method</param>
        public Git(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// Gets the currently known information about the software.
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            var signature = new Signature(publisherX509, publisherExpiryDate);
            return new AvailableSoftware("Git",
                "2.31.1",
                "^Git version [0-9]+\\.[0-9]+\\.[0-9]+$",
                "^Git version [0-9]+\\.[0-9]+\\.[0-9]+$",
                new InstallInfoExe(
                    "https://github.com/git-for-windows/git/releases/download/v2.31.1.windows.1/Git-2.31.1-32-bit.exe",
                    HashAlgorithm.SHA256,
                    "6abc8c83945ee8046c7337d2376c4dcb1e4d63ed9614e161b42a30a5fe9dc6ec",
                    signature,
                    "/VERYSILENT /NORESTART"),
                new InstallInfoExe(
                    "https://github.com/git-for-windows/git/releases/download/v2.31.1.windows.1/Git-2.31.1-64-bit.exe",
                    HashAlgorithm.SHA256,
                    "c43611eb73ad1f17f5c8cc82ae51c3041a2e7279e0197ccf5f739e9129ce426e",
                    signature,
                    "/VERYSILENT /NORESTART")
                    );
        }


        /// <summary>
        /// Gets a list of IDs to identify the software.
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "git", "git-for-windows" };
        }


        /// <summary>
        /// Determines whether or not the method searchForNewer() is implemented.
        /// </summary>
        /// <returns>Returns true, if searchForNewer() is implemented for that
        /// class. Returns false, if not. Calling searchForNewer() may throw an
        /// exception in the later case.</returns>
        public override bool implementsSearchForNewer()
        {
            return true;
        }


        /// <summary>
        /// Looks for newer versions of the software than the currently known version.
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the information
        /// that was retrieved from the net.</returns>
        public override AvailableSoftware searchForNewer()
        {
            logger.Debug("Searching for newer version of Git for Windows...");
            // Just getting the latest release does not work here, because that may also be a release candidate, and we do not want that.
            string html;
            using (var client = new WebClient())
            {
                try
                {
                    html = client.DownloadString("https://github.com/git-for-windows/git/releases");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Git for Windows: " + ex.Message);
                    return null;
                }
            }

            // HTML text will contain links to releases like "https://github.com/git-for-windows/git/releases/tag/v2.30.1.windows.1".
            Regex reVersion = new Regex("git/releases/tag/v([0-9]+\\.[0-9]+\\.[0-9])\\.windows\\.[0-9]+\"");
            var matchVersion = reVersion.Match(html);
            if (!matchVersion.Success)
                return null;
            string currentVersion = matchVersion.Groups[1].Value;
            string tag = matchVersion.Value.Remove(0, "git/releases/tag/".Length).Replace("\"", "");

            // Get checksum from release page, e.g. "https://github.com/git-for-windows/git/releases/tag/v2.30.1.windows.1"
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://github.com/git-for-windows/git/releases/tag/" + tag);
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Git for Windows: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } // using

            // find SHA256 hash for 32 bit installer
            /* Hash is part of a HTML table, e. g. in
             * <td>Git-2.30.0-32-bit.exe</td>
             * <td>e41b7d0e1c88a023ecd42a1e7339c39a8e906cd51ea9ae9aefdb42c513103f57</td>
             */
            string escapedVersion = Regex.Escape(currentVersion);
            Regex reHash = new Regex("<td>Git\\-" + escapedVersion + "\\-32\\-bit\\.exe</td>\r?\n<td>([a-f0-9]{64})</td>");
            Match matchHash = reHash.Match(htmlCode);
            if (!matchHash.Success)
                return null;
            string newHash32Bit = matchHash.Groups[1].Value;
            // find SHA256 hash for 64 bit installer
            reHash = new Regex("<td>Git\\-" + escapedVersion + "\\-64\\-bit\\.exe</td>\r?\n<td>([a-f0-9]{64})</td>");
            matchHash = reHash.Match(htmlCode);
            if (!matchHash.Success)
                return null;
            string newHash64Bit = matchHash.Groups[1].Value;
            // construct new information
            var newInfo = knownInfo();
            newInfo.newestVersion = currentVersion;
            // e. g. https://github.com/git-for-windows/git/releases/download/v2.30.0.windows.1/Git-2.30.0-32-bit.exe
            newInfo.install32Bit.downloadUrl = "https://github.com/git-for-windows/git/releases/download/" + tag + "/Git-" + currentVersion + "-32-bit.exe";
            newInfo.install32Bit.checksum = newHash32Bit;
            // e. g. https://github.com/git-for-windows/git/releases/download/v2.30.0.windows.1/Git-2.30.0-64-bit.exe
            newInfo.install64Bit.downloadUrl = "https://github.com/git-for-windows/git/releases/download/" + tag + "/Git-" + currentVersion + "-64-bit.exe";
            newInfo.install64Bit.checksum = newHash64Bit;
            return newInfo;
        }


        /// <summary>
        /// Lists names of processes that might block an update, e.g. because
        /// the application cannot be updated while it is running.
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns a list of process names that block the upgrade.</returns>
        public override List<string> blockerProcesses(DetectedSoftware detected)
        {
            return new List<string>(2)
            {
                "git", // Git itself
                "bash" // Git Bash
            };
        }
    } // class
} // namespace
