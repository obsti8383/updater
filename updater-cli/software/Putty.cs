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

using updater_cli.data;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace updater_cli.software
{
    public class Putty : ISoftware
    {
        /// <summary>
        /// gets the currently known information about the software
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public AvailableSoftware info()
        {
            return new AvailableSoftware("PuTTY", "0.68",
                "^PuTTY release [0-9]\\.[0-9]+$",
                "^PuTTY release [0-9]\\.[0-9]+ \\(64\\-bit\\)$",
                //32 bit installer
                new InstallInfoMsi(
                    "https://the.earth.li/~sgtatham/putty/0.68/w32/putty-0.68-installer.msi",
                    HashAlgorithm.SHA512,
                    "2b6b1acc51fc9cfe8a08e17afc4ac7d962af4575d55806456aa1a42c563f14457cc81d7e625191fd856b4be9a0e885cef78da4018d542010eba18718bb73fecd",
                    "/qn /norestart",
                    "C:\\Program Files\\PuTTY",
                    "C:\\Program Files (x86)\\PuTTY"),
                //64 bit installer
                new InstallInfoMsi(
                    "https://the.earth.li/~sgtatham/putty/0.68/w64/putty-64bit-0.68-installer.msi",
                    HashAlgorithm.SHA512,
                    "8262c133e3569dcc188e5ac2360ebd3cc09d9edd2f78d6eaaf0e2762fba511de07ce614ba4fedf5f62fe64dd17d5a626dc63a579ee1fc1ef90f45e9e1f0c3d06",
                    "/qn /norestart",
                    null,
                    "C:\\Program Files\\PuTTY")
                );
        }


        /// <summary>
        /// whether or not the method searchForNewer() is implemented
        /// </summary>
        /// <returns>Returns true, if searchForNewer() is implemented for that
        /// class. Returns false, if not. Calling searchForNewer() may throw an
        /// exception in the later case.</returns>
        public bool implementsSearchForNewer()
        {
            return true;
        }


        /// <summary>
        /// looks for newer versions of the software than the currently known version
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the information
        /// that was retrieved from the net.</returns>
        public AvailableSoftware searchForNewer()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://the.earth.li/~sgtatham/putty/latest/");
            request.Method = WebRequestMethods.Http.Head;
            request.AllowAutoRedirect = false;
            string newLocation = null;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.Found)
                    return null;
                newLocation = response.Headers[HttpResponseHeader.Location];
                request = null;
                response = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while looking for newer PuTTY version: " + ex.Message);
                return null;
            }

            Regex reVersion = new Regex("/[0-9]+\\.[0-9]+/");
            Match matchVersion = reVersion.Match(newLocation);
            if (!matchVersion.Success)
                return null;
            string newVersion = matchVersion.Value.Replace("/", "");

            //Checksums are in a file like https://the.earth.li/~sgtatham/putty/0.68/sha512sums
            string sha512sums = null;
            using (var client = new WebClient())
            {
                try
                {
                    sha512sums = client.DownloadString("https://the.earth.li/~sgtatham/putty/" + newVersion + "/sha512sums");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occurred while checking for newer version of PuTTY: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using

            Regex reHash32 = new Regex("[0-9a-f]{128}  w32/putty\\-" + Regex.Escape(newVersion) + "\\-installer\\.msi");
            Match matchHash32 = reHash32.Match(sha512sums);
            if (!matchHash32.Success)
                return null;
            string hash32 = matchHash32.Value.Substring(0, 128);

            Regex reHash64 = new Regex("[0-9a-f]{128}  w64/putty\\-64bit\\-" + Regex.Escape(newVersion) + "\\-installer\\.msi");
            Match matchHash64 = reHash64.Match(sha512sums);
            if (!matchHash64.Success)
                return null;
            string hash64 = matchHash64.Value.Substring(0, 128);

            //construct new version information
            var newInfo = info();
            //replace version number - both as newest version and in URL for download
            string oldVersion = newInfo.newestVersion;
            newInfo.newestVersion = newVersion;
            newInfo.install32Bit.downloadUrl = newInfo.install32Bit.downloadUrl.Replace(oldVersion, newVersion);
            newInfo.install32Bit.checksum = hash32;
            newInfo.install64Bit.downloadUrl = newInfo.install64Bit.downloadUrl.Replace(oldVersion, newVersion);
            newInfo.install64Bit.checksum = hash64;
            return newInfo;
        }
    } //class
} //namesoace