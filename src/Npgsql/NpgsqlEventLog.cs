// created on 07/06/2002 at 09:34

// Npgsql.NpgsqlConnection.cs
// 
// Author:
//	Dave Page (dpage@postgresql.org)
//
//	Copyright (C) 2002 Dave Page
//

// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System.IO;
using System.Text;
using System.Diagnostics;
using System;

namespace Npgsql
{
	/// <summary>
	/// This class handles all the Npgsql event & debug logging
	/// </summary>
	public class NpgsqlEventLog
	{
	  
    // Logging related values
    private static readonly String CLASSNAME = "NpgsqlEventLog";
    private static   String    logfile;
    private static   Int32     level;

    ///<summary>
    /// Sets/Returns the level of information to log to the logfile.
    /// 0 - None
    /// 1 - Normal
    /// 2 - Complete
    /// </summary>	
    public static Int32 Level
    {
      get
      {
        return level;
      }
      set
      {
        level = value;
        LogMsg("Set " + CLASSNAME + ".Level = " + value, 1);
      }
    }
    
    ///<summary>
    /// Sets/Returns the filename to use for logging.
    /// </summary>	
    public static String LogName
    {
      get
      {
        return logfile;
      }
      set
      {
        logfile = value;
        LogMsg("Set " + CLASSNAME + ".LogFile = " + value, 1);
      }
    }
    
    // Event/Debug Logging
    public static void LogMsg(String message, Int32 msglevel) 
    {
      if (msglevel > level)
        return;
        
      Process proc = Process.GetCurrentProcess();
      
      if (logfile != null)
      {
        if (logfile != "")
        {
          
          StreamWriter writer = new StreamWriter(logfile, true);
          
          // The format of the logfile is
          // [Date] [Time]  [PID]  [Level]  [Message]
          writer.WriteLine(System.DateTime.Now + "  " + proc.Id + "  " + msglevel + "  " + message);
          writer.Close(); 
        }
      }
    }
    
	}
}
