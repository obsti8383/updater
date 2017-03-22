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

namespace updater_cli.operations
{
    public class IdList : IOperation
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="withAurora"></param>
        public IdList(bool withAurora)
        {
            mWithAurora = withAurora;
        }


        public int perform()
        {
            var all = software.All.get(false, mWithAurora, null);
            foreach (var software in all)
            {
                Console.WriteLine(software.info().Name + ": " + string.Join(", ", software.id()));
            } //foreach
            return 0;
        }


        /// <summary>
        /// whether Aurora is included in the list or not
        /// </summary>
        private bool mWithAurora;
    } //class
} //namespace
