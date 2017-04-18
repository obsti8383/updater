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

namespace updater.software
{
    public class NotepadPlusPlus : NoPreUpdateProcessSoftware
    {
        /// <summary>
        /// NLog.Logger for NotepadPlusPlus class
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetLogger(typeof(NotepadPlusPlus).FullName);


        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public NotepadPlusPlus(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// gets the currently known information about the software
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            return new AvailableSoftware("Notepad++", "7.3.3",
                "^Notepad\\+\\+ \\(32\\-bit x86\\)$",
                "^Notepad\\+\\+ \\(64\\-bit x64\\)$",
                new InstallInfoExe(
                    "https://notepad-plus-plus.org/repository/7.x/7.3.3/npp.7.3.3.Installer.exe",
                    HashAlgorithm.SHA1,
                    "0a55e43108b289e6d9aff14d5ed0204e5eca6468",
                    "/S",
                    "C:\\Program Files\\Notepad++",
                    "C:\\Program Files (x86)\\Notepad++"),
                new InstallInfoExe(
                    "https://notepad-plus-plus.org/repository/7.x/7.3.3/npp.7.3.3.Installer.x64.exe",
                    HashAlgorithm.SHA1,
                    "a5912c2c2546a6f2f3b8dfdbea880a08d8c5dccb",
                    "/S",
                    null,
                    "C:\\Program Files\\Notepad++")
                );
        }


        /// <summary>
        /// list iof IDs to identify the software
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "notepad++" };
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
            logger.Debug("Searching for newer version of Notepad++...");
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://notepad-plus-plus.org/repository/?C=N;O=D");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Notepad++: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            Regex reVersionDir = new Regex("<a href=\"[0-9]+\\.x/\">[0-9]+\\.x/</a>");
            Match matchDirectory = reVersionDir.Match(htmlCode);
            if (!matchDirectory.Success)
                return null;

            string directoryMajor = matchDirectory.Value.Replace("<a href=\"", "");
            int idx = directoryMajor.IndexOf('/');
            if (idx < 0)
                return null;
            directoryMajor = directoryMajor.Remove(idx);

            //get directory listing again "https://notepad-plus-plus.org/repository/7.x/?C=M;O=D"
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://notepad-plus-plus.org/repository/" + directoryMajor + "/?C=M;O=D");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Notepad++: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using
            //search for link like <a href="7.3.1/">7.3.1/</a>
            reVersionDir = new Regex("<a href=\"[0-9]+\\.[0-9]+(\\.[0-9]+)?/\">[0-9]+\\.[0-9]+(\\.[0-9]+)?/</a>");
            matchDirectory = reVersionDir.Match(htmlCode);
            if (!matchDirectory.Success)
                return null;
            string directoryDetailed = matchDirectory.Value.Replace("<a href=\"", "");
            idx = directoryDetailed.IndexOf('/');
            if (idx < 0)
                return null;
            directoryDetailed = directoryDetailed.Remove(idx);
            if (string.Compare(directoryDetailed, knownInfo().newestVersion) < 0)
                return null;

            //download checksum file, e.g. "https://notepad-plus-plus.org/repository/7.x/7.3.1/npp.7.3.1.sha1.md5.digest.txt"
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://notepad-plus-plus.org/repository/" + directoryMajor + "/" + directoryDetailed + "/npp." + directoryDetailed + ".sha1.md5.digest.txt");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Notepad++: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            //find SHA1 hash for 32 bit installer
            Regex reHash = new Regex("[a-f0-9]{40}    npp.+Installer\\.exe");
            Match matchHash = reHash.Match(htmlCode);
            if (!matchHash.Success)
                return null;
            string newHash32Bit = matchHash.Value.Substring(0, 40);
            //find SHA1 hash for 64 bit installer
            reHash = new Regex("[a-f0-9]{40}    npp.+Installer\\.x64\\.exe");
            matchHash = reHash.Match(htmlCode);
            if (!matchHash.Success)
                return null;
            string newHash64Bit = matchHash.Value.Substring(0, 40);
            //construct new information
            var newInfo = knownInfo();
            string oldVersion = newInfo.newestVersion;
            newInfo.newestVersion = directoryDetailed;
            newInfo.install32Bit.downloadUrl = "https://notepad-plus-plus.org/repository/" + directoryMajor + "/" + directoryDetailed + "/npp." + directoryDetailed + ".Installer.exe";
            newInfo.install32Bit.checksum = newHash32Bit;
            newInfo.install64Bit.downloadUrl = "https://notepad-plus-plus.org/repository/" + directoryMajor + "/" + directoryDetailed + "/npp." + directoryDetailed + ".Installer.x64.exe";
            newInfo.install64Bit.checksum = newHash64Bit;
            return newInfo;
        }

    } //class
} //namespace