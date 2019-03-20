/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 
 * The contents of this file are subject to the GNU General Public License
 * v3.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy * of the License at 
 * https://github.com/k3ldar/FbReplicationEngine/blob/master/LICENSE
 *
 * Software distributed under the License is distributed on an
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
 * or implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2011 - 2019 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  Generator Information
 *
 */

using System;

namespace Replication.Service
{
    public class Generators
    {
        public Generators(DatabaseConnection connection, string name, long currentValue, bool setRemote, bool bigint)
        {
            Name = name;
            CurrentValue = currentValue;

            if (setRemote)
            {
                NewValue = Int32.MinValue;
            }
            else
            {
                long multiplier = bigint ? (long)100000000 : (long)1000000;
                long expectedValue = multiplier * connection.SiteID;

                if (currentValue > expectedValue)
                    expectedValue = currentValue;

                if (expectedValue >= currentValue)
                    NewValue = expectedValue;
                else
                    NewValue = currentValue;
            }
        }

        public string Name { get; private set; }

        public long CurrentValue { get; private set; }

        public long NewValue { get; set; }
    }
}
