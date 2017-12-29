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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using updater.data;

namespace updater.software
{
    public class Thunderbird : AbstractSoftware
    {
        /// <summary>
        /// NLog.Logger for Thunderbird class
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetLogger(typeof(Thunderbird).FullName);


        /// <summary>
        /// constructor with language code
        /// </summary>
        /// <param name="langCode">the language code for the Thunderbird software,
        /// e.g. "de" for German,  "en-GB" for British English, "fr" for French, etc.</param>
        /// <param name="autoGetNewer">whether to automatically get
        /// newer information about the software when calling the info() method</param>
        public Thunderbird(string langCode, bool autoGetNewer)
            : base(autoGetNewer)
        {
            if (string.IsNullOrWhiteSpace(langCode))
            {
                logger.Error("The language code must not be null, empty or whitespace!");
                throw new ArgumentNullException("langCode", "The language code must not be null, empty or whitespace!");
            }
            languageCode = langCode.Trim();
            var d = knownChecksums();
            if (!d.ContainsKey(languageCode))
            {
                logger.Error("The string '" + langCode + "' does not represent a valid language code!");
                throw new ArgumentOutOfRangeException("langCode", "The string '" + langCode + "' does not represent a valid language code!");
            }
            checksum = d[languageCode];
        }


        /// <summary>
        /// Gets a dictionary with the known checksums for the installers (key: language, value: checksum).
        /// </summary>
        /// <returns>Returns a dictionary where keys are the language codes and values are the associated checksums.</returns>
        private static Dictionary<string, string> knownChecksums()
        {
            // These are the checksums for Windows 32 bit installers from
            // https://ftp.mozilla.org/pub/thunderbird/releases/52.5.2/SHA512SUMS
            var result = new Dictionary<string, string>();
            result.Add("ar", "e8d96a772ac8390f35dd3258349e7842562f228b68b931951a93b6916c30c5f1ca7afe8f823f5bf7d49b4f43596838c032998db670fe0959db8cc478080fed57");
            result.Add("ast", "329476110d6b57e60093bce828914a33034349d97f3e593b7c2531f5fe3ea8c2e5efb3a437f2510002f65b48046adcb1c880fff92675aa98783d07333a690266");
            result.Add("be", "c2610e59967450f448ec7c6eb5013dfc4cb8f7824d77756abe150441a0ece40c75c7b10549afe8b2b1ce9a5781fdb49f46791fd3718f56816855b2fe9b71324e");
            result.Add("bg", "b1135c1bdf32864bd1f67c2302b8823c6c4b2fd9cab2e0481c05af0e8edadddc723af1abed194b6d3168bf976a97e1909fe747e3f078b7802e20257ea522df75");
            result.Add("bn-BD", "668663e644585457a4e521ddb89e8250b7fe6c3f8bfcf6f32761a8b781dd1b51d5c3e5ae65d24bab215bb28b398a3f2de3240d14313c72e2a576b0ccee329cdf");
            result.Add("br", "223183c468b2aa278bc11a7f032fbf733a7e8045d0a4c347a27738ea2554ae7ecd9e0e50e191ea2e0857a00c8a936cef290547d3ed0799deb1f6925580145d13");
            result.Add("ca", "c0c4e2296d57d03be22f4a53715125d3051b372f1aa2ab330cdcb3e74661fc31a2d91dffa4ca483cd633c85c90595e6f3b98e87598c5bb15565f24a1cef5dbbb");
            result.Add("cs", "1afc1acb0ce31831afca9653a5fe67386b892dc4202a694c9917ae9b7b57970c63f65bcb1691e1b34a2ad255a6af7b06357e4901083a087b50b26a364f692455");
            result.Add("cy", "a8ad54aefc38c56e104f9fe8aab5f3be82247ef968bd6d50c6fb88e4928cb29091f4f06ef3bce9acdecb956c3d79a702132d7a7aa64082e06935785ae7a81372");
            result.Add("da", "297a16762580bd1cd23f9991351968bf58228cf92c364fb828afc8f05b5cbf03cc9225af577cf7cf923ec45d25590da6c583f2a59d140e9c0dbc7e32da2a474f");
            result.Add("de", "60b3beb5291459b5356e2a0eda5f40ac81e40cece91636245d8d51b57012507d1bb186faacc897ab1b06919f473ff47bd22dafa2ede4bd49012874b98322b270");
            result.Add("dsb", "143a722cfb7b3ff599ed82925d9419e984df20a0f4b57879ac8610c16dcfe2fcf2503aeba5b9d37fe5d167700536b71fc0c59c5d9ecad64bea412198681ba3ea");
            result.Add("el", "cdfd2751be31d61063d6933bc70438a07462edced3b343e19d555f8be37e01c0ae92946de3595d5869d7743858cac653296d16ccece25ffcbacce444cc12acf7");
            result.Add("en-GB", "f4c82bfc68f34c53a8877d29772b4683bf6272a02f171838a4c3144816412dddc1219e3f133c9cc766471031f76f7a30da188d3e172a08875bb45852e9d2ffb0");
            result.Add("en-US", "9f2109cbaa44ece56672289401eea97d60a15f6e167c30b9151f1d1e70c5dbc594304fc0132ce910a295a89d0898f2cda4ea9ee5eb01650ad2bdf49976c83d93");
            result.Add("es-AR", "dbafde5ba7ec5cf735476e2befb810545216cee62bd69ddda2c154cda294678976f565dd7f2ea7c19417bf1b0c5367ee24c0f525af13606d6a2f85a6ebee4f25");
            result.Add("es-ES", "1dd619c2949a8e5aaa2a0f51e5407573e0d27ca8dba3b911e4307c9f706b43c0bb34b03506dd230096c01bcb7ead8a7598fb23fe72966df110546060e4df73e9");
            result.Add("et", "67fbae36fbe2dc2ac0bcf22bd2fc73f3c990502c0d85e7ce3261ec853852f852f5c918e14e25d6a548f5ab06c8f9fc745f0d449846db17faa8221daa32183c31");
            result.Add("eu", "b4f0bd57048653e7402ed38a9de31335cbfede6131f7e63fb3945bf60957e8707f43939202701fb67be2ed0136a30417e26aaf7a8fc46c72a293c1c6b23d24ab");
            result.Add("fi", "8dc10ff3ac2d4f1b59561d78e470f852562acfe2c0fb67e1b6b84cc6385f16aebca8e95f4fb96ac86346f1215c56d6b53c3a8e9700a771289ad518e8132ba505");
            result.Add("fr", "ffcde3ac70d667badd1dafbc52187a51347c88d59da7f3b9a99ae6135d79c381060a5f3fab129f1f3c8c0d1def4cc08dae0cd8f0c372c72362a8260d87a816f3");
            result.Add("fy-NL", "49d663bf1269187451619233fb2aa64e9a75836432d6e0f2dad665ddac85b646ab00cf49716bf47450ae01eaa5596872663251c4d5acd9d8ee8b83391745241c");
            result.Add("ga-IE", "e3d69cb23521c42b7e74df93f5ff7d6d587ab52247ddb247ebb5c457aba97e88a0e2e796b665a3b8a62de7715b5c1ff28a516350b918b36c806d974de31770ca");
            result.Add("gd", "7bb65a99e615e6c9c1327ddbad788d2e3684a20120b8c6eb4f2c72cc035535fbd32ea3fa2bbba187b21654c8ceb9854bd06e5c9dbe70415a80696ee18ab48e37");
            result.Add("gl", "4e5dd9f7ca90efe2bbcd552fc61376a6bd1d675b20e20cb47cb015cebc81a82ead2937215e44f42cfa077c4650d4f0d0eb2158b3d71be79de81c1b440d65485b");
            result.Add("he", "891186a2708d7fceba73cf81b8fca4f6de960a0db6dbadeec422734b930a20ba15f71807fec4b3a4916d2eafe53106f5d6c7d1cd040352dc99d6ee2a20a17132");
            result.Add("hr", "f16f3a06e1dcfae924f724d8cd3deeb394c77e851163a082cd12317a31bb1beeeac260697405fbe7831b4311f0a8877bf9989661a2d8871d9b2b6ce09f6f019d");
            result.Add("hsb", "a2ce6fc599586ee6fc9aa4f9bbf0b2be820b09fb6d4fc71b202edccaf3468e75e47c4e0fce8caf3cce70341a4556d0b57f5f6f43a1668ec3fbcdeeb558c91441");
            result.Add("hu", "7678e8999663818ca29b564adbbbae2decfcbe5aaaae98eb96a40727d00093d019a3ab61751f13a14bce5705159eceb8c38e42d32beb3f1e35ef3e06ef08edc1");
            result.Add("hy-AM", "3a2130386eb9aad9eabf2ed5fc671736d051aabeb7db38a072e92da345699d5443a3a65660ca168fdf7502a74b9c1deb3a98253256685e3758c98c49d455669c");
            result.Add("id", "572ecb45d9d8bf2f773548cf231e2b72f30a2b83986892dc0d6f89c4c70b552ce8fd98929e6e893f98740f66eebf60190077f001a4cee60d39e1d56e7fb63ba0");
            result.Add("is", "b45e93937abab7c5dc0fbffa6f4d033b99831555374ed3ab3af0576f001ce3e57eeb4bfae5fc25e43ae239f2a7b4f0af265a451bc12c604a2638843de7e2a0f2");
            result.Add("it", "c6535d00b59a6114312abb5cf148113ed85ce23c6efb3f62eadfc58a82cd260a76197ba3c955d9d51afbd6fac7b9d0ebc838fd35b32a55dd380b71f18b6ca97a");
            result.Add("ja", "20fde184e68a37a203d51c61fa3ab97869ccb58a4f2753473a8a98845ea819de0bd088e7535dfd1557d7b437209afdebf22858d3631d36109f094faaa40906b4");
            result.Add("kab", "2f2cd6aa14cc26649877b38ceecbf819b455202a0cea26b1897ae78117d128f720e86b779b76a94c085cc478cd801358cb77fc208adf47c3f1894cfb668f719a");
            result.Add("ko", "a226abe11c989200fea0b01436b32d48cfb77c3f9d0a8b54ae8cf7f29cf08c641f90e61eddf6d9442ff47e35d7cb995ee9be22ace3fe56c9c2487ee259e38c3a");
            result.Add("lt", "b97dfe07285a0deb20254955e265939f51e96018112a88f9dc1eea2353ae5b9077d850a16446e576a8f4cdc37988349f58684af5e253baa17b10f5fa2c0ae9a5");
            result.Add("nb-NO", "187b3797a1c2895f05584949f56e620f923dffadbfd2a78a4ba3f39fe04d857a17eb9976fb3ae025814f97d76e057da895473ddc7c9936595628d6bd6ffa1926");
            result.Add("nl", "12bd636c36315c1483435ddfbbd0f2d673c232799cab6292fc149a271a4db81057703bb6e64c9177a9170b35f344813d087e259b12777815757d365eccacbe89");
            result.Add("nn-NO", "d1727b3624bd3cf8cf7bb902004f774539784ea785f45430c7be532b3f08c0d59d0d35e9a85e628b957564f3674d77bf0437b8c68aeaf0d8efb61cdd70095d7f");
            result.Add("pa-IN", "8638a5700b0191fd4d2c8f56514a720325de139c7172713903cc7a2430d0dd2269682c0abdd6a3346a3749f4a17e779c61b5520886d092c43c0337133792d68c");
            result.Add("pl", "0f71b0639351c311e40895048a45897ce42951c5dd5ccfffc9024cbf97f68ce28a3cd833b57cc6d10d77dbe3951e250786823eed0c85d59170422cb3d0163809");
            result.Add("pt-BR", "14974f5eac31b69500ef89bff75ce93ab4b56331594515f388af3a2c91021bd3ad6fb8de130b65efee5f2b74f0ded24b8749e1ceddcab7d69aec8b72f598f34d");
            result.Add("pt-PT", "b67164774bfad29b291d372eb3a38170951d1a425d18e6abb5ca2ff47a8eab8439c7f590cc607bf72f3365042260e39a910b460bb144124c9f2739d3956f7db3");
            result.Add("rm", "5940a81fb60550f178563276ff3bea3ca0476c79b14c950ff77ad3f0641b80a17f99f3d7c51ee51085c6f8989a23151e5206568a41a8d5630dc8da9a61ed33b0");
            result.Add("ro", "717d83aca2b6028c16237f31465444693b51c468c6218b6405fadfee6e81891556f14406aec162c54310852c0e2657e9386d8278bc899840df75ebec0299e388");
            result.Add("ru", "7d7e00743aeeadfa246e7152bc87c1921a08f32af7ed8bdc2919697606cf88dd2d51fd20f1280da2f4760b5ea6d60bb2ede6c8b93bfd3ca0cfcb128e22321ef4");
            result.Add("si", "8c0e04e471b8f5e9059979daf7ef56302568a10295573d10bc914e2493c52445e3491cdf061efea8f9a1e8d4bfbb0c22667f92da21c0181882e4705e62e9bd31");
            result.Add("sk", "ebcf05e325bcf033ddedcde494ec53825b52094c1443d7bd9a5c50e04518577f23a9e2a3420c24eaaa60f103404bdd9e40ef19dfb3bfb18deab2905d3e06911a");
            result.Add("sl", "a5cae784165082eb2ade525a07a3e3fc559d078c4c9f5d292da168fe668caf740923500a23d3fd702620948cddb30fbda69705205ee0ba5c4475a172136ca996");
            result.Add("sq", "c1978bd5480f88a8eccd0fc6d93b00590c0260fc6a9d31b1305a6dc8a0c7e32ae4f7e5c1ab132001f32d40342aa6e9107f22f049a42c80fea806e7bcbdd26eac");
            result.Add("sr", "d98eb8fc60fbd31f97b852d9bf5eedcaedf90c969f89e50ef3a8366cf509a0f983f6625a0427b08a2b2fd29ee40a315f85eaf63645d9ceeeeb91a48af9be7190");
            result.Add("sv-SE", "16fe9194283fce93a2a1a9ae7d5b83ae05af0cd6f34a2fc0e37d14cd0457866c40ca1ae14358ce921e505ddca4125c4f159ffd2196b391093f0b1c26bfc583f5");
            result.Add("ta-LK", "8ec3d8beccdcf73fb565cfcd084b9c2355f155414a91febab4a4c65fa87d61e4fc1b6423dc546d951d14c352ebc1ed6ef3e980a7bd77c94bd3a5e22b7b8f91b8");
            result.Add("tr", "d2885b20109d146e876a5f88085a74a33b568fc94a53d1715e0347afd7ce30323de87e64be0e3c58d816ee8165c88509d146385fef7c9884a61d88490d0f9554");
            result.Add("uk", "86e3e11baab683ccd49257353b80f1caeddfc31c3a5d5c0d6371e3232ee9cf02a67e03c824a6e43266b6b0f336b127f8da7352cf9394a323eafc48ab26f4e670");
            result.Add("vi", "b18e67f7d01074c439a47410b065d6914c2585181de2fb4b422c75abb8cb88a3e645f2e8f23912995a39cdf049302d03f07f2c8a05bbd34fe996519b973d6f73");
            result.Add("zh-CN", "e3755bbe022f738fa75318cee1fead05a619cc9bcf3e41819e724d6328cb2ff2e22d51b1460282103620bddd76699148f982015438b5d19c933a2a2de43c3402");
            result.Add("zh-TW", "01106d30b858cc7dbc7f01cc6b850fa25a791d6523d7dbb3d8b97db14919cf9c492f93a0974a073bd811af5941943f029d8ebfcdff7ffd56621ca42c3bd307ec");

            return result;
        }


        /// <summary>
        /// Gets an enumerable collection of valid language codes.
        /// </summary>
        /// <returns>Returns an enumerable collection of valid language codes.</returns>
        public static IEnumerable<string> validLanguageCodes()
        {
            var d = knownChecksums();
            return d.Keys;
        }


        /// <summary>
        /// Gets the currently known information about the software.
        /// </summary>
        /// <returns>Returns an AvailableSoftware instance with the known
        /// details about the software.</returns>
        public override AvailableSoftware knownInfo()
        {
            const string version = "52.5.2";
            return new AvailableSoftware("Mozilla Thunderbird (" + languageCode + ")",
                version,
                "^Mozilla Thunderbird [0-9]{2}\\.[0-9]\\.[0-9] \\(x86 " + Regex.Escape(languageCode) + "\\)$",
                null,
                new InstallInfoExe(
                    "https://ftp.mozilla.org/pub/thunderbird/releases/" + version + "/win32/" + languageCode + "/Thunderbird%20Setup%20" + version + ".exe",
                    HashAlgorithm.SHA512,
                    checksum,
                    "CN=Mozilla Corporation, O=Mozilla Corporation, L=Mountain View, S=California, C=US",
                    "-ms -ma",
                    "C:\\Program Files\\Mozilla Thunderbird",
                    "C:\\Program Files (x86)\\Mozilla Thunderbird"),
                // There is no 64 bit installer yet.
                null);
        }


        /// <summary>
        /// Gets a list of IDs to identify the software.
        /// </summary>
        /// <returns>Returns a non-empty array of IDs, where at least one entry is unique to the software.</returns>
        public override string[] id()
        {
            return new string[] { "thunderbird-" + languageCode.ToLower(), "thunderbird" };
        }


        /// <summary>
        /// Tries to find the newest version number of Thunderbird.
        /// </summary>
        /// <returns>Returns a string containing the newest version number on success.
        /// Returns null, if an error occurred.</returns>
        public string determineNewestVersion()
        {
            string url = "https://download.mozilla.org/?product=thunderbird-latest&os=win&lang=" + languageCode;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Head;
            request.AllowAutoRedirect = false;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.Found)
                    return null;
                string newLocation = response.Headers[HttpResponseHeader.Location];
                request = null;
                response = null;
                Regex reVersion = new Regex("[0-9]{2}\\.[0-9]\\.[0-9]");
                Match matchVersion = reVersion.Match(newLocation);
                if (!matchVersion.Success)
                    return null;
                string currentVersion = matchVersion.Value;
                
                return currentVersion;
            }
            catch (Exception ex)
            {
                logger.Warn("Error while looking for newer Thunderbird version: " + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Tries to get the checksum of the newer version.
        /// </summary>
        /// <returns>Returns a string containing the checksum, if successfull.
        /// Returns null, if an error occurred.</returns>
        private string determineNewestChecksum(string newerVersion)
        {
            if (string.IsNullOrWhiteSpace(newerVersion))
                return null;
            /* Checksums are found in a file like
             * https://ftp.mozilla.org/pub/thunderbird/releases/45.7.1/SHA512SUMS
             * Common lines look like
             * "69d11924...7eff  win32/en-GB/Thunderbird Setup 45.7.1.exe"
             */

            string url = "https://ftp.mozilla.org/pub/thunderbird/releases/" + newerVersion + "/SHA512SUMS";
            string sha512SumsContent = null;
            using (var client = new WebClient())
            {
                try
                {
                    sha512SumsContent = client.DownloadString(url);
                }
                catch (Exception ex)
                {
                    logger.Warn("Exception occurred while checking for newer version of Thunderbird: " + ex.Message);
                    return null;
                }
                client.Dispose();
            } //using
            //look for line with the correct language code and version
            Regex reChecksum = new Regex("[0-9a-f]{128}  win32/" + languageCode.Replace("-", "\\-")
                + "/Thunderbird Setup " + Regex.Escape(newerVersion) + "\\.exe");
            Match matchChecksum = reChecksum.Match(sha512SumsContent);
            if (!matchChecksum.Success)
                return null;
            // checksum is the first 128 characters of the match
            return matchChecksum.Value.Substring(0, 128);
        }


        /// <summary>
        /// Indicates whether or not the method searchForNewer() is implemented.
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
            logger.Debug("Searching for newer version of Thunderbird (" + languageCode + ")...");
            string newerVersion = determineNewestVersion();
            if (string.IsNullOrWhiteSpace(newerVersion))
                return null;
            var currentInfo = knownInfo();
            if (newerVersion == currentInfo.newestVersion)
                // fallback to known information
                return currentInfo;
            string newerChecksum = determineNewestChecksum(newerVersion);
            if (string.IsNullOrWhiteSpace(newerChecksum))
                return null;
            // replace all stuff
            string oldVersion = currentInfo.newestVersion;
            currentInfo.newestVersion = newerVersion;
            currentInfo.install32Bit.downloadUrl = currentInfo.install32Bit.downloadUrl.Replace(oldVersion, newerVersion);
            currentInfo.install32Bit.checksum = newerChecksum;
            return currentInfo;
        }


        /// <summary>
        /// Lists names of processes that might block an update, e.g. because
        /// the application cannot be update while it is running.
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns a list of process names that block the upgrade.</returns>
        public override List<string> blockerProcesses(DetectedSoftware detected)
        {
            var p = new List<string>();
            p.Add("thunderbird");
            return p;
        }


        /// <summary>
        /// Determines whether or not a separate process must be run before the update.
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
        /// Returns a process that must be run before the update.
        /// </summary>
        /// <param name="detected">currently installed / detected software version</param>
        /// <returns>Returns a Process ready to start that should be run before
        /// the update. May return null or may throw, if needsPreUpdateProcess()
        /// returned false.</returns>
        public override List<Process> preUpdateProcess(DetectedSoftware detected)
        {
            if (string.IsNullOrWhiteSpace(detected.installPath))
                return null;
            var processes = new List<Process>();
            // Uninstall previous version to avoid having two Thunderbird entries in control panel.
            var proc = new Process();
            proc.StartInfo.FileName = Path.Combine(detected.installPath, "uninstall", "helper.exe");
            proc.StartInfo.Arguments = "/SILENT";
            processes.Add(proc);
            return processes;
        }


        /// <summary>
        /// language code for the Thunderbird version
        /// </summary>
        private string languageCode;


        /// <summary>
        /// checksum for the installer
        /// </summary>
        private string checksum;

    } // class
} // namespace
