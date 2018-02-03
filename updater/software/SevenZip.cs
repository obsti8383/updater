﻿/*
    This file is part of the updater command line interface.
    Copyright (C) 2017  Dirk Stolle

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
    public class SevenZip : NoPreUpdateProcessSoftware
    {
        /// <summary>
        /// NLog.Logger for SevenZip class
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetLogger(typeof(SevenZip).FullName);


        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public SevenZip(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// gets the currently known information about the software
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            return new AvailableSoftware("7-Zip",
                "18.01",
                "^7\\-Zip [0-9]+\\.[0-9]{2}$",
                "^7\\-Zip [0-9]+\\.[0-9]{2} \\(x64\\)$",
                new InstallInfoExe(
                    "http://www.7-zip.org/a/7z1801.exe",
                    HashAlgorithm.SHA256,
                    "c55c60a674114be26ce470f43109d405a5adcd2bd38e346d4a35c98727174eb0",
                    null,
                    "/S",
                    "C:\\Program Files\\7-Zip",
                    "C:\\Program Files (x86)\\7-Zip"),
                new InstallInfoExe(
                    "http://www.7-zip.org/a/7z1801-x64.exe",
                    HashAlgorithm.SHA256,
                    "86670d63429281a4a65c36919ca0f3099e3f803e3096c3a9722d61b3d31e4a9f",
                    null,
                    "/S",
                    null,
                    "C:\\Program Files\\7-Zip")
                );
        }


        /// <summary>
        /// list of IDs to identify the software
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "7zip", "7-zip", "sevenzip" };
        }


        /// <summary>
        /// whether or not the method searchForNewer() is implemented
        /// </summary>
        /// <returns>Returns true, if searchForNewer() is implemented for that
        /// class. Returns false, if not. Calling searchForNewer() may throw an
        /// exception in the later case.</returns>
        public override bool implementsSearchForNewer()
        {
            return true;
        }


        /// <summary>
        /// looks for newer versions of the software than the currently known version
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the information
        /// that was retrieved from the net.</returns>
        public override AvailableSoftware searchForNewer()
        {
            logger.Debug("Searching for newer version of 7-Zip...");
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("http://www.7-zip.org/");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of 7-Zip: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            Regex reVersion = new Regex("Download 7\\-Zip [0-9]+\\.[0-9]{2} \\([0-9]{4}\\-[0-9]{2}\\-[0-9]{2}\\) for Windows");
            Match matchVersion = reVersion.Match(htmlCode);
            if (!matchVersion.Success)
                return null;

            string version = matchVersion.Value.Replace("Download 7-Zip", "").Trim();
            int idx = version.IndexOf(' ');
            if (idx < 0)
                return null;
            version = version.Remove(idx);
            if (string.IsNullOrWhiteSpace(version))
                return null;

            //construct new information
            var newInfo = knownInfo();
            newInfo.newestVersion = version;
            string newVersionWithoutDot = version.Replace(".", "");
            //32 bit
            newInfo.install32Bit.downloadUrl = "http://www.7-zip.org/a/7z" + newVersionWithoutDot + ".exe";
            // The official 7-zip.org site does not provide any checksums,
            // so we have to do without.
            newInfo.install32Bit.algorithm = HashAlgorithm.Unknown;
            newInfo.install32Bit.checksum = null;
            // 64 bit
            newInfo.install64Bit.downloadUrl = "http://www.7-zip.org/a/7z" + newVersionWithoutDot + "-x64.exe";
            // The official 7-zip.org site does not provide any checksums,
            // so we have to do without.
            newInfo.install64Bit.algorithm = HashAlgorithm.Unknown;
            newInfo.install64Bit.checksum = null;
            return newInfo;
        }


        /// <summary>
        /// lists names of processes that might block an update, e.g. because
        /// the application cannot be update while it is running
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns a list of process names that block the upgrade.</returns>
        public override List<string> blockerProcesses(DetectedSoftware detected)
        {
            return new List<string>();
        }
    } //class
} //namespace
