// Core command line method
// CommandLine.cs
// Unity Version Control
//  
// Authors:
//       Josh Montoute <josh@thinksquirrel.com>
// 
// Copyright (c) 2012, Thinksquirrel Software, LLC
//
// This file is part of Unity Version Control.
//
//    Unity Version Control is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Unity Version Control is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Unity Version Control.  If not, see <http://www.gnu.org/licenses/>.
//
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;

namespace ThinksquirrelSoftware.UnityVersionControl.Core
{
	[InitializeOnLoad]
	public static class CommandLine
	{
		private static Queue<Process> processQueue = new Queue<Process>();
		private static List<Process> runningProcesses = new List<Process>();
		
		static CommandLine()
		{
			EditorApplication.update += CheckQueue;
		}
		
		static void CheckQueue()
		{
			if (processQueue.Count > 0 && runningProcesses.Count < System.Environment.ProcessorCount)
			{
				Process p = processQueue.Dequeue();
				p.Start();
				runningProcesses.Add(p);
			}
			for(int i = runningProcesses.Count - 1; i >= 0; i--)
			{
				if(runningProcesses[i].HasExited)
					runningProcesses.RemoveAt(i);
			}
		}
		
		/// <summary>
		/// Opens the default terminal.
		/// </summary>
		public static void OpenTerminal()
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				RunCommand("start", "cmd.exe", EmptyHandler);
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				RunCommand("open", "-n com.apple.Terminal", EmptyHandler);
			}
		}
		
		/// <summary>
		/// Runs a command line process asynchronously with the specified arguments.
		/// </summary>
		/// <remarks>
		/// Returns the process (with output and error streams redirected) to be handled on an exit event.
		/// If exitEventHandler is null, output and error streams are not redirected.
		/// </remarks>
		public static Process RunCommand(string fileName, string args, System.EventHandler exitEventHandler)
		{
			var process = new System.Diagnostics.Process();
			
			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = args;
			
			if (exitEventHandler != null)
			{
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.EnableRaisingEvents = true;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.Exited += new System.EventHandler(exitEventHandler);
			}
			
			processQueue.Enqueue(process);
			
			return process;
		}
		
		/// <summary>
		/// Represents an empty event handler. Used in order to get standard output and error streams.
		/// </summary>
		public static void EmptyHandler(object sender, System.EventArgs e)
		{
			// Empty process handler. Used in order to get standard output and error streams.
		}
		
		/// <summary>
		/// Opens the file in the user's specified text editor (This can be changed in the Version Control preference window).
		/// </summary>
		/// <param name='path'>
		/// Path to the file to open.
		/// </param>
		public static Process OpenFileInTextEditor(string path)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				return RunCommand(UVCPreferences.GetDefaultTextEditor(), path, null);
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				return RunCommand("open", "-a " + UVCPreferences.GetDefaultTextEditor() + " \"" + path + '"', null);	
			}
			
			return null;
		}
	}
}