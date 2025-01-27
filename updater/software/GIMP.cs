﻿/*
    This file is part of the updater command line interface.
    Copyright (C) 2017, 2018, 2020, 2021  Dirk Stolle

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
    /// Handles updates for the GNU Image Manipulation Program (GIMP).
    /// </summary>
    public class GIMP : NoPreUpdateProcessSoftware
    {
        /// <summary>
        /// NLog.Logger for GIMP class
        /// </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger(typeof(GIMP).FullName);


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public GIMP(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// Gets the currently known information about the software.
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            return new AvailableSoftware("The GIMP",
                "2.10.22",
                "^GIMP [0-9]+\\.[0-9]+\\.[0-9]+$",
                "^GIMP [0-9]+\\.[0-9]+\\.[0-9]+$",
                // The GIMP uses the same installer for 32 and 64 bit.
                new InstallInfoExe(
                    "https://download.gimp.org/pub/gimp/v2.10/windows/gimp-2.10.22-setup.exe",
                    HashAlgorithm.SHA256,
                    "f7851c348584ce432dfd8e69b74a168c7dec33ebfddc29c96ad2d6b83aded083",
                    Signature.None,
                    "/VERYSILENT /NORESTART"),
                new InstallInfoExe(
                    "https://download.gimp.org/pub/gimp/v2.10/windows/gimp-2.10.22-setup.exe",
                    HashAlgorithm.SHA256,
                    "f7851c348584ce432dfd8e69b74a168c7dec33ebfddc29c96ad2d6b83aded083",
                    Signature.None,
                    "/VERYSILENT /NORESTART")
                );
        }


        /// <summary>
        /// Gets a list of IDs to identify the software.
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "gimp" };
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
            logger.Debug("Searching for newer version of GIMP...");
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://www.gimp.org/downloads/");
                }
                catch (Exception ex)
                {
                    logger.Error("Exception occurred while checking for newer version of GIMP: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } // using

            const string stableRelease = "The current stable release of GIMP is";
            int idx = htmlCode.IndexOf(stableRelease);
            if (idx < 0)
                return null;
            htmlCode = htmlCode.Remove(0, idx);

            Regex reVersion = new Regex("[0-9]+\\.[0-9]+\\.[0-9]+");
            Match matchVersion = reVersion.Match(htmlCode);
            if (!matchVersion.Success)
                return null;
            string version = matchVersion.Value;

            // SHA-256 checksum is in a file like
            // https://download.gimp.org/pub/gimp/v2.8/windows/gimp-2.8.20-setup.exe.sha256
            string shortVersion = string.Join(".", version.Split(new char[] { '.' }), 0, 2);
            htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    string sha256Url = "https://download.gimp.org/pub/gimp/v" + shortVersion + "/windows/gimp-" + version + "-setup.exe.sha256";
                    htmlCode = client.DownloadString(sha256Url);
                }
                catch (WebException webEx)
                {
                    if ((webEx.Response is HttpWebResponse)
                        && ((webEx.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound))
                    {
                        // try SHA256 file for whole directory instead
                        try
                        {
                            string sha256Url = "https://download.gimp.org/pub/gimp/v" + shortVersion + "/windows/SHA256SUMS";
                            htmlCode = client.DownloadString(sha256Url);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Exception occurred while checking for newer version of GIMP: " + ex.Message);
                            return null;
                        } // try-catch (inner)
                    } // if 404 Not Found

                    // Other web exceptions are still errors.
                    else
                    {
                        logger.Warn("Exception occurred while checking for newer version of GIMP: " + webEx.Message);
                        return null;
                    }
                } // catch WebException
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of GIMP: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } // using

            Regex reChecksum = new Regex("[0-9a-f]{64}  gimp\\-" + Regex.Escape(version) + "\\-setup\\.exe");
            Match m = reChecksum.Match(htmlCode);
            if (!m.Success)
                return null;
            string checksum = m.Value.Substring(0, 64);

            // construct new information
            var newInfo = knownInfo();
            string oldVersion = newInfo.newestVersion;
            string oldShortVersion = string.Join(".", oldVersion.Split(new char[] { '.' }), 0, 2);
            newInfo.newestVersion = version;
            // 32 bit
            newInfo.install32Bit.downloadUrl = newInfo.install32Bit.downloadUrl.Replace(oldVersion, version).Replace(oldShortVersion, shortVersion);
            newInfo.install32Bit.checksum = checksum;
            // 64 bit - same installer, same checksum
            newInfo.install64Bit.downloadUrl = newInfo.install32Bit.downloadUrl;
            newInfo.install64Bit.checksum = checksum;
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
            return new List<string>();
        }
    } // class
} // namespace
