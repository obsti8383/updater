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

using updater.data;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using updater.versions;

namespace updater.software
{
    public class VLC : NoPreUpdateProcessSoftware
    {
        /// <summary>
        /// NLog.Logger for VLC class
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetLogger(typeof(VLC).FullName);


        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public VLC(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// gets the currently known information about the software
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            return new AvailableSoftware("VLC media player",
                "2.2.5.1",
                "^VLC media player$",
                "^VLC media player$",
                //32 bit installer
                new InstallInfoExe(
                    "https://get.videolan.org/vlc/2.2.5.1/win32/vlc-2.2.5.1-win32.exe",
                    HashAlgorithm.SHA256,
                    "37e857b0322a6676c4d01310b2927c109720a6d89cc149d1a35852b649b53387",
                    "/S",
                    "C:\\Program Files\\VideoLAN\\VLC",
                    "C:\\Program Files (x86)\\VideoLAN\\VLC"),
                //64 bit installer
                new InstallInfoExe(
                    "http://get.videolan.org/vlc/2.2.5.1/win64/vlc-2.2.5.1-win64.exe",
                    HashAlgorithm.SHA256,
                    "41a69a5f329315ea9b5ae6d3da8f4900f9f7ba4fb3d3c07e1adb844e60208159",
                    "/S",
                    null,
                    "C:\\Program Files\\VideoLAN\\VLC")
                );
        }


        /// <summary>
        /// list of IDs to identify the software
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "vlc" };
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
        /// determines the last version of VLC media player, which is usually
        /// the latest version, too
        /// </summary>
        /// <returns>Returns a version number, if successful.
        /// Returns null, if an error occurred.</returns>
        private string getLastVersion()
        {
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://get.videolan.org/vlc/last/");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for latest version of VLC: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            Regex reTarXz = new Regex("vlc\\-[0-9]+\\.[0-9]+\\.[0-9]+(\\.[0-9]+)?\\.tar\\.xz");
            Match matchTarXz = reTarXz.Match(htmlCode);
            if (!matchTarXz.Success)
                return null;
            //extract new version number
            string newVersion = matchTarXz.Value.Replace("vlc-", "").Replace(".tar.xz", "");
            return newVersion;
        }


        /// <summary>
        /// gets the latest available version from download site directory listing
        /// </summary>
        /// <returns>Returns latest available version number, if successful.
        /// Returns null, if an error occurred.</returns>
        private string getLatestAvailableVersion()
        {
            //See http://get.videolan.org/vlc/ for available versions.
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://get.videolan.org/vlc/");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for available versions of VLC: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            Regex reVersion = new Regex("\"[0-9]+\\.[0-9]+\\.[0-9]+(\\.[0-9]+)?/\"");
            var matches = reVersion.Matches(htmlCode);
            var latestVersion = new Quartet("0.0.0.0");
            string latestVersionString = null;
            foreach (Match m in matches)
            {
                if (!m.Success)
                    continue;
                var possibleVersionString = m.Value.Replace("\"", "").Replace("/", "");
                var possibleVersion = new Quartet(possibleVersionString);
                if (string.IsNullOrWhiteSpace(latestVersionString) || possibleVersion > latestVersion)
                {
                    latestVersion = possibleVersion;
                    latestVersionString = possibleVersionString;
                }
            } //foreach
            return latestVersionString;
        }

        /// <summary>
        /// looks for newer versions of the software than the currently known version
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the information
        /// that was retrieved from the net.</returns>
        public override AvailableSoftware searchForNewer()
        {
            logger.Debug("Searching for newer version of VLC media player...");
            //get new version number
            string lastVersion = getLastVersion();
            string availableVersion = getLatestAvailableVersion();
            string newVersion = lastVersion;
            if (string.Compare(lastVersion, availableVersion) < 0)
                newVersion = availableVersion;
            //should not be lesser than known newest version
            if (string.Compare(newVersion, knownInfo().newestVersion) < 0)
                return null;
            //version number should match usual scheme, e.g. 5.x.y, where x and y are digits
            Regex version = new Regex("^[1-9]+\\.[0-9]+\\.[0-9]+(\\.[0-9]+)?$");
            if (!version.IsMatch(newVersion))
                return null;

            //There are extra files for hashes:
            // https://get.videolan.org/vlc/last/win32/vlc-2.2.4-win32.exe.sha256 for 32 bit
            // and https://get.videolan.org/vlc/last/win64/vlc-2.2.4-win64.exe.sha256 for 64 bit.
            var newHashes = new List<string>();
            foreach (var bits in new string[] { "32", "64" })
            {
                string htmlCode = null;
                using (var client = new WebClient())
                {
                    try
                    {
                        htmlCode = client.DownloadString("https://get.videolan.org/vlc/" + newVersion + "/win" + bits + "/vlc-" + newVersion + "-win" + bits + ".exe.sha256");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Exception occurred while checking for newer version of VLC: " + ex.Message);
                        return null;
                    }
                    client.Dispose();
                } //using

                //extract hash
                Regex reHash = new Regex("^[0-9a-f]{64} [\\* ]vlc\\-" + Regex.Escape(newVersion) + "\\-win" + bits + ".exe");
                Match matchHash = reHash.Match(htmlCode);
                if (!matchHash.Success)
                    return null;
                string newHash = matchHash.Value.Substring(0, 64).Trim();
                newHashes.Add(newHash);
            } //foreach

            //construct new version information
            var newInfo = knownInfo();
            //replace version number - both as newest version and in URL for download
            string oldVersion = newInfo.newestVersion;
            newInfo.newestVersion = newVersion;
            newInfo.install32Bit.downloadUrl = newInfo.install32Bit.downloadUrl.Replace(oldVersion, newVersion);
            newInfo.install32Bit.checksum = newHashes[0];
            newInfo.install64Bit.downloadUrl = newInfo.install64Bit.downloadUrl.Replace(oldVersion, newVersion);
            newInfo.install64Bit.checksum = newHashes[1];
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
} //namesoace
