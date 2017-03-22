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

using updater_cli.software;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace updater_test.software
{
    /// <summary>
    /// unit tests for software.All class
    /// </summary>
    [TestClass]
    public class All_Tests
    {
        /// <summary>
        /// checks whether All.get() returns some usable data
        /// </summary>
        [TestMethod]
        public void Test_get()
        {
            var result = All.get(false, true, null);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.IsNotNull(result[i]);
            } //for
        }


        /// <summary>
        /// checks whether All.get() can handle null and empty exclusion lists
        /// </summary>
        [TestMethod]
        public void Test_get_NullEmpty()
        {
            var result = All.get(false, true, null);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);

            var result2 = All.get(false, true, new List<string>());
            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.Count > 0);

            //count should be equal
            Assert.AreEqual<int>(result.Count, result2.Count);
        }


        /// <summary>
        /// checks whether All.get() respects the exclusion list
        /// </summary>
        [TestMethod]
        public void Test_get_WithExclusionList()
        {
            var result = All.get(false, false, null);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);


            var excluded = new List<string>();
            excluded.Add(new CCleaner(false).id()[0]);
            excluded.Add(new CDBurnerXP(false).id()[0]);
            excluded.Add(new Pidgin(false).id()[0]);

            var result2 = All.get(false, false, excluded);
            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.Count > 0);

            //count not should be equal
            Assert.AreNotEqual<int>(result.Count, result2.Count);
            Assert.AreEqual<int>(result.Count - excluded.Count, result2.Count);
        }

    } //class
} //namespace
