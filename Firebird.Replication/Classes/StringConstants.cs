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
 *  Copyright (c) 2011 - 2017 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  Global String Constants
 *
 */

namespace Replication.Engine
{
    public class Constants
    {
        internal const string ERROR_FB_READING_CONNECTION = "Error reading data from the connection";
        internal const string ERROR_FB_CONNECTION_SHUTDOWN = "connection shutdown";

        public const string DEFAULT_SERVER = "127.0.0.1";
        public const int DEFAULT_PORT = 3424;
    }
}
