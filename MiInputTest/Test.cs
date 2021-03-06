﻿////////////////////////////////////////////////////////////////////////////////
// Test.cs 
////////////////////////////////////////////////////////////////////////////////
//
// MiInput - A basic input manager for use with SFML.Net.
// Copyright (C) 2020 Michael Furlong <michaeljfurlong@outlook.com>
//
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free 
// Software Foundation, either version 3 of the License, or (at your option) 
// any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program. If not, see <https://www.gnu.org/licenses/>.
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using SFML.Graphics;
using SFML.Window;

using MiInput;
using MiCore;

using Action = MiInput.Action;

namespace MiInputTest
{
	static class Test
	{
		const string InputPath = "input.xml";

		private static int Main( string[] args )
		{
			int val = RunTests( args );
			Console.ReadLine();
			return val;
		}

		private static int RunTests( string[] _ )
		{
			if( !ActionTest() )
				return Logger.LogReturn( "Action test failed.", -1, LogType.Error );
			if( !WindowTest() )
				return Logger.LogReturn( "Window test failed.", -1, LogType.Error );

			Logger.Log( "Testing successful!" );
			Logger.Log( "Running example." );

			if( Example.RunExample() != 0 )
				return Logger.LogReturn( "Run example failed.", -1, LogType.Error );

			Logger.Log( "Press enter to exit." );
			return 0;
		}

		private static bool ActionTest()
		{
			Action act = new( "test", new InputMap( InputDevice.Joystick, InputType.Button, "0", "1" ) );

			if( !Input.Manager.Actions.Add( act ) )
				return Logger.LogReturn( "Unable to add valid action to action set.", false, LogType.Error );

			Action a = Input.Manager.Actions[ "test" ];

			if( a is null )
				return Logger.LogReturn( "Unable to retrieve previously added action from the action set.", false, LogType.Error );

			{
				bool con = Logger.LogToConsole, 
				     fil = Logger.LogToFile;

				Logger.LogToConsole = false;
				Logger.LogToFile    = false;

				bool result = Input.Manager.Actions.Add( a, false );

				Logger.LogToConsole = con;
				Logger.LogToFile    = fil;

				if( result )
					return Logger.LogReturn( "Input manager allowed adding an action that already exists when replace is false.", false, LogType.Error );
			}

			if( !Input.Manager.SaveToFile( InputPath, true ) )
				return Logger.LogReturn( "Input manager failed saving to file.", false, LogType.Error );
			
			if( !Input.Manager.LoadFromFile( InputPath ) )
				return Logger.LogReturn( "Input manager failed loading from file.", false, LogType.Error );

			if( !Input.Manager.Actions.Contains( a ) )
				return Logger.LogReturn( "Lost mapped input after loading from file.", false, LogType.Error );
			if( Input.Manager.Actions[ "test" ].Name != act.Name )
				return Logger.LogReturn( "Input manager did not load from file correctly.", false, LogType.Error );

			try
			{
				File.Delete( InputPath );
			}
			catch
			{ }

			return true;
		}
		private static bool WindowTest()
		{
			Input.Manager.Update();

			Action test = new( "test", new InputMap( InputDevice.Joystick, InputType.Button, "A", "B" ) );

			if( !Input.Manager.Actions.Add( test, true ) )
				return Logger.LogReturn( "Unable to add test action to input manager.", false, LogType.Error );

			uint press = 0;

			Logger.Log( "Press A Button." );

			using( RenderWindow window = new( new VideoMode( 640, 480 ), "MiInput Test", Styles.Close ) )
			{
				window.Closed += OnClose;

				while( window.IsOpen )
				{
					window.DispatchEvents();

					Input.Manager.Update();

					if( press is 0 && Input.Manager.Actions[ "test" ].IsPressed )
					{
						Logger.Log( "Press B button." );
						press++;
					}

					if( press is 1 && Input.Manager.Actions[ "test" ].IsNegative )
						press++;

					if( press > 1 )
						window.Close();

					window.Clear();

					window.Display();
				}
			}

			Input.Manager.Actions.Remove( "test" );
			return true;
		}

		private static void OnClose( object sender, EventArgs e )
		{
			( sender as RenderWindow ).Close();
		}
	}
}
