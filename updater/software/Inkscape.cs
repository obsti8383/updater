﻿/*
    This file is part of the updater command line interface.
    Copyright (C) 2017, 2018  Dirk Stolle

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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using updater.data;
using System.Collections.Generic;

namespace updater.software
{
    public class Inkscape : AbstractSoftware
    {
        /// <summary>
        /// NLog.Logger for Inkscape class
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetLogger(typeof(Inkscape).FullName);


        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public Inkscape(bool autoGetNewer)
            : base(autoGetNewer)
        { }


        /// <summary>
        /// gets the currently known information about the software
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            return new AvailableSoftware("Inkscape",
                "0.92.4",
                "^Inkscape [0-9]\\.[0-9]+(\\.[0-9]+)?$",
                "^Inkscape [0-9]\\.[0-9]+(\\.[0-9]+)?$",
                new InstallInfoMsi(
                    "https://media.inkscape.org/dl/resources/file/inkscape-0.92.4-x86.msi",
                    HashAlgorithm.MD5,
                    "2c8016038f63398bafd3474737d21d09",
                    null,
                    "/qn /norestart"),
                new InstallInfoMsi(
                    "https://media.inkscape.org/dl/resources/file/inkscape-0.92.4-x64.msi",
                    HashAlgorithm.MD5,
                    "8a867a3ebaac8c17c3294e3aea0b61c5 ",
                    null,
                    "/qn /norestart")
                    );
        }


        /// <summary>
        /// list of IDs to identify the software
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "inkscape" };
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
            logger.Debug("Searching for newer version of Inkscape...");
            string htmlCode = null;
            using (var client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString("https://inkscape.org/release/");
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Inkscape: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            // Search for URL part like "/release/0.92.4/windows/".
            Regex reVersion = new Regex("/release/[0-9]\\.[0-9]+(\\.[0-9]+)?/windows/");
            Match matchVersion = reVersion.Match(htmlCode);
            if (!matchVersion.Success)
                return null;
            string newVersion = matchVersion.Value.Replace("/release/", "").Replace("/windows/", "").Trim();
            
            // construct new version information based on old information
            var newInfo = knownInfo();
            if (newVersion == newInfo.newestVersion)
                return newInfo;
            newInfo.newestVersion = newVersion;
            // Reset checksums to avoid working with older stuff.
            newInfo.install32Bit.checksum = null;
            newInfo.install32Bit.algorithm = HashAlgorithm.Unknown;
            newInfo.install64Bit.checksum = null;
            newInfo.install64Bit.algorithm = HashAlgorithm.Unknown;

            foreach (string bits in new string[] {"32", "64" })
            {
                // Find download URL for 32 bit version.
                // https://inkscape.org/release/inkscape-0.92.4/windows/32-bit/msi/dl/
                // 64 bit version is at an URL like
                // https://inkscape.org/release/inkscape-0.92.4/windows/64-bit/msi/dl/
                htmlCode = null;
                using (var client = new WebClient())
                {
                    try
                    {
                        htmlCode = client.DownloadString("https://inkscape.org/release/inkscape-" + newVersion + "/windows/" + bits + "-bit/msi/dl/");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Exception occurred while checking for newer version of Inkscape: " + ex.Message);
                        return null;
                    }
                    client.Dispose();
                } //using

                // Search for URL part like '<a href="/gallery/item/13322/inkscape-0.92.4-x86.msi">'.
                //     or
                // Search for URL part like '<a href="/gallery/item/13321/inkscape-0.92.4-x64.msi">'.
                string arch = (bits == "32") ? "x86" : "x64";
                {
                    Regex reUrl = new Regex("<a href=\"([a-zA-Z0-9\\/]+)/inkscape\\-" + Regex.Escape(newVersion) + "\\-" + arch + "\\.msi\">");
                    Match matchUrl = reUrl.Match(htmlCode);
                    if (!matchUrl.Success)
                        return null;
                    if (bits == "32")
                        newInfo.install32Bit.downloadUrl = "https://inkscape.org/" + matchUrl.Groups[1].Value + "inkscape-" + newVersion + "-x86.msi";
                    else
                        newInfo.install64Bit.downloadUrl = "https://inkscape.org/" + matchUrl.Groups[1].Value + "inkscape-" + newVersion + "-x64.msi";
                }

                // Signature files are given in HTML elements like
                // <a href="https://media.inkscape.org/media/resources/sigs/inkscape-0.92.4-x86.msi_v1j6SnA.md5"> or
                // <a href="https://media.inkscape.org/media/resources/sigs/inkscape-0.92.4-x64.msi_8a41Ccz.md5">.
                {
                    Regex reSigUrl = new Regex(Regex.Escape("<a href=\"https://media.inkscape.org/media/resources/sigs/inkscape-" + newVersion + "-" + arch + ".msi_") + "[A-Za-z0-9]+\\.md5\">");
                    Match matchSigUrl = reSigUrl.Match(htmlCode);
                    if (!matchSigUrl.Success)
                        return null;
                    string signatureUrl = matchSigUrl.Value.Replace("<a href=\"", "").Replace("\">", "");

                    htmlCode = null;
                    using (var client = new WebClient())
                    {
                        try
                        {
                            htmlCode = client.DownloadString(signatureUrl);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Exception occurred while checking for newer version of Inkscape: " + ex.Message);
                            return null;
                        }
                        client.Dispose();
                    } //using
                }

                Regex reHash = new Regex("[0-9a-f]{32} \\*inkscape\\-" + Regex.Escape(newVersion) + "\\-" + arch + "\\.msi");
                Match matchHash = reHash.Match(htmlCode);
                if (!matchHash.Success)
                    return null;
                string newHash = matchHash.Value.Substring(0, 32); // MD5 is 32 characters in hex.
                if (bits == "32")
                {
                    newInfo.install32Bit.checksum = newHash;
                    newInfo.install32Bit.algorithm = HashAlgorithm.MD5;
                }
                else
                {
                    newInfo.install64Bit.checksum = newHash;
                    newInfo.install64Bit.algorithm = HashAlgorithm.MD5;
                }
            } // foreach

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


        /// <summary>
        /// whether or not a separate process must be run before the update
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns true, if a separate proess returned by
        /// preUpdateProcess() needs to run in preparation of the update.
        /// Returns false, if not. Calling preUpdateProcess() may throw an
        /// exception in the later case.</returns>
        public override bool needsPreUpdateProcess(DetectedSoftware detected)
        {
            return true;
        }


        /// <summary>
        /// returns a process that must be run before the update
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns a Process ready to start that should be run before
        /// the update. May return null or may throw, if needsPreUpdateProcess()
        /// returned false.</returns>
        public override List<Process> preUpdateProcess(DetectedSoftware detected)
        {
            //The pre-update processes basically need to uninstall all older
            // versions before installing the newest version.
            //It is a requirement for the older .exe installer-based versions,
            // because they are incompatible with MSI. On the other hand the
            // MSI installers do not allow to install/update, if another MSI
            // version is already installed.
            if (string.IsNullOrWhiteSpace(detected.displayVersion))
                return null;

            var processes = new List<Process>();
            //Versions before 0.91 (i.e. exe-installers) can be uninstalled via
            // "%PROGRAMFILES%\Inkscape\uninstall.exe" /S
            string path = null;
            if (!string.IsNullOrWhiteSpace(detected.installPath))
                path = Path.Combine(detected.installPath, "uninstall.exe");
            else
                path = "C:\\Program Files (x86)\\Inkscape\\uninstall.exe";
            if (File.Exists(path))
            {
                var proc = new Process();
                proc.StartInfo.FileName = path;
                proc.StartInfo.Arguments = "/S";
                processes.Add(proc);
            }
            else if (File.Exists("C:\\Program Files\\Inkscape\\uninstall.exe"))
            {
                var proc = new Process();
                proc.StartInfo.FileName = "C:\\Program Files\\Inkscape\\uninstall.exe";
                proc.StartInfo.Arguments = "/S";
                processes.Add(proc);
            }
            else
            {
                //MSI GUIDs to uninstall older MSI builds
                string[] guids = {
                    "{81922150-317E-4BB0-A31D-FF1C14F707C5}" //0.91 MSI (x86 and x64), 0.92 MSI
                };
                foreach (var id in guids)
                {
                    var proc = new Process();
                    proc.StartInfo.FileName = "msiexec.exe";
                    proc.StartInfo.Arguments = "/qn /x" + id;
                    processes.Add(proc);
                } //foreach
            } //else
            return processes;
        }


        /// <summary>
        /// whether the detected software is older than the newest known software
        /// </summary>
        /// <param name="detected">the corresponding detected software</param>
        /// <returns>Returns true, if the detected software version is older
        /// than the newest software version, thus needing an update.
        /// Returns false, if no update is necessary.</returns>
        public override bool needsUpdate(DetectedSoftware detected)
        {
            Regex re = new Regex("[0-9]\\.[0-9]+(\\.[0-9]+)?");
            Match m = re.Match(detected.displayName);
            if (m.Success)
            {
                //Use version number from name, because it is more accurate.
                return (string.Compare(m.Value, info().newestVersion, true) < 0);
            }
            else
            {
                //Fall back to displayed version. However, this is inaccurate,
                //because versions like 0.92.1 will only show up as 0.92, thus
                //always triggering an update.
                return (string.Compare(detected.displayVersion, info().newestVersion, true) < 0);
            }
        }

    } //class
} //namespace
