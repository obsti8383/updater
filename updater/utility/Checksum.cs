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

using System.IO;
using System.Security.Cryptography;

namespace updater.utility
{
    /// <summary>
    /// utility class to handle checksums
    /// </summary>
    public class Checksum
    {
        /// <summary>
        /// calculates the checksum of a string
        /// </summary>
        /// <param name="fileName">name of the file for which the checksum will be calculated</param>
        /// <param name="algorithm">hashing/checksum algorithm</param>
        /// <returns>Returns a hexadecimal string representation of the checksum.
        /// Returns null, if an error occurred.</returns>
        public static string calculate(string fileName, data.HashAlgorithm algorithm)
        {
            if (string.IsNullOrEmpty(fileName) || (!File.Exists(fileName))
                || (algorithm == data.HashAlgorithm.Unknown))
                return null;

            HashAlgorithm hasher = null;
            switch (algorithm)
            {
                case data.HashAlgorithm.MD5:
                    hasher = new MD5Cng();
                    break;
                case data.HashAlgorithm.SHA1:
                    hasher = new SHA1Managed();
                    break;
                case data.HashAlgorithm.SHA256:
                    hasher = new SHA256Managed();
                    break;
                case data.HashAlgorithm.SHA384:
                    hasher = new SHA384Managed();
                    break;
                case data.HashAlgorithm.SHA512:
                    hasher = new SHA512Managed();
                    break;
                case data.HashAlgorithm.Unknown:
                default:
                    return null;
            } //switch


            byte[] hash = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                fs.Position = 0;
                hash = hasher.ComputeHash(fs);
                fs.Close();
                fs.Dispose();
            } //using

            return hashToString(hash);
        }


        /// <summary>
        /// gets the hexadecimal string representation of a hash value
        /// </summary>
        /// <param name="hash">the hash value</param>
        /// <returns>Returns the hexadecimal string representation of the hash,
        /// if successful. Returns null, if an error occurred.</returns>
        public static string hashToString(byte[] hash)
        {
            if ((null == hash) || (hash.Length == 0))
                return null;

            string result = "";
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            for (int i = 0; i < hash.Length; i++)
            {
                result += digits[hash[i] / 16];
                result += digits[hash[i] % 16];
            } //for
            return result;
        }


        /// <summary>
        /// normalises a hexadecimal checksum string
        /// </summary>
        /// <param name="cs">hexadecimal string representation of a checksum</param>
        /// <returns>Returns normalised checksum representation.</returns>
        public static string normalise(string cs)
        {
            if (string.IsNullOrWhiteSpace(cs))
                return null;

            cs = cs.Trim().ToLower();
            for (int i = cs.Length - 1; i >= 0; i--)
            {
                switch (cs[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        break;
                    default:
                        //removes invalid character
                        cs = cs.Remove(i, 1);
                        break;
                } //switch
            } //for

            return cs;
        }


        /// <summary>
        /// compares two checksum values
        /// </summary>
        /// <param name="checksum1">first checksum</param>
        /// <param name="checksum2">second checksum</param>
        /// <returns>Returns true, if both checksums are equal.</returns>
        public static bool areEqual(string checksum1, string checksum2)
        {
            checksum1 = normalise(checksum1);
            checksum2 = normalise(checksum2);

            // Null or empty values are never a match.
            if (string.IsNullOrWhiteSpace(checksum1) || string.IsNullOrWhiteSpace(checksum2))
                return false;
            //Simple equality checks will do after normalisation.
            return (checksum1 == checksum2);
        }

    } //class
} //namespace