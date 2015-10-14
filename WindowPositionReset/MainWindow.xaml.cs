﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Drawing;
using Microsoft.Win32;
using WindowScrape.Types;
using Application = System.Windows.Forms.Application;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace WindowPositionReset
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int DefaultDelay = 1;
		private const int DefaultInterval = 5;
		private object syncLock = new object();
		private readonly Dictionary<Tuple<int, int, int>, Dictionary<IntPtr, WindowPlacement>> resolutionDictionary = new Dictionary<Tuple<int, int, int>, Dictionary<IntPtr, WindowPlacement>>();
		private DispatcherTimer _timer = new DispatcherTimer();
		private bool changing = false;
		private Timer _restoreTimer = new Timer();
		private NotifyIcon mynotifyicon = new NotifyIcon();
		private bool workstationLocked;

		public MainWindow()
		{
			int delay;
			int interval;

			InitializeComponent();
			this.WindowState = System.Windows.WindowState.Minimized;
			if (!int.TryParse(ConfigurationManager.AppSettings["DelayBeforeRestoration"], out delay))
			{
				delay = DefaultDelay;
			}

			if (!int.TryParse(ConfigurationManager.AppSettings["RecordInterval"], out interval))
			{
				interval = DefaultInterval;
			}

			_timer.Interval = TimeSpan.FromSeconds(interval);
			_timer.Tick += TimerOnTick;
			_timer.Start();

			_restoreTimer.AutoReset = false;
			_restoreTimer.Elapsed += (sender, args) => Dispatcher.InvokeAsync(async () => await RestorePositionsAsync());
			_restoreTimer.Interval = 1000 * delay;

			var bitmap = new Bitmap("./tray.png"); // or get it from resource
			var iconHandle = bitmap.GetHicon();
			mynotifyicon.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			mynotifyicon.Text = mynotifyicon.BalloonTipTitle = "Window Position Monitor";
			this.StateChanged += OnStateChanged ;

			var exitMenuItem = new MenuItem
			{
				Enabled = true,
				Text = "Exit"
			};

			exitMenuItem.Click += (sender, args) =>
			{
				Debug.WriteLine("Clicked Exit");
				this.Close();
			};


			mynotifyicon.ContextMenu = new ContextMenu();
			mynotifyicon.ContextMenu.MenuItems.Add(exitMenuItem);
			
			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
			SystemEvents.DisplaySettingsChanging += SystemEvents_DisplaySettingsChanging;
			SystemEvents.SessionSwitch += OnSystemEventsOnSessionSwitch;

			this.Closed += OnClosed;
			this.OnStateChanged(null,null);
		}

		private void OnSystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs args)
		{
			if (args.Reason == SessionSwitchReason.SessionLock)
			{
				Debug.WriteLine("Station Locked");
				workstationLocked = true;
			}
			else if (args.Reason == SessionSwitchReason.SessionUnlock)
			{
				Debug.WriteLine("Station Unlocked");
				workstationLocked = false;
			}
		}

		private void OnStateChanged(object sender, EventArgs eventArgs)
		{
			mynotifyicon.Visible = true;
			mynotifyicon.BalloonTipText = "Monitoring window positions";
			mynotifyicon.ShowBalloonTip(500);

			this.Hide();			
		}

		private void OnClosed(object sender, EventArgs eventArgs)
		{
			SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
			SystemEvents.DisplaySettingsChanging -= SystemEvents_DisplaySettingsChanging;
			SystemEvents.SessionSwitch -= OnSystemEventsOnSessionSwitch;
		}

		private void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e)
		{
			changing = true;
			Debug.WriteLine("Display Settings Changing");
		}

		private void TimerOnTick(object sender, EventArgs eventArgs)
		{
			Task.Run(() => RecordPositions());
		}

		private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
		{
			Debug.WriteLine("Display Settings Changed");

			var newTuple = GetCurrentScreenConfig();

			if (resolutionDictionary.ContainsKey(newTuple) && !workstationLocked)
			{
				_restoreTimer.Start();
			}
			else
			{
				changing = false;
			}
		}

		private static Tuple<int, int, int> GetCurrentScreenConfig()
		{
			var config = Tuple.Create(Screen.AllScreens.Length, Screen.AllScreens.Sum(x => x.Bounds.Width),
				Screen.AllScreens.Sum(y => y.Bounds.Height));

			Debug.WriteLine("Screen config: " + config);

			return config;
		}

		private void RecordPositions()
		{
			lock (syncLock)
			{
				if (changing || workstationLocked)
					return;

				Debug.WriteLine("Recording Positions");
				var windowPositions = new Dictionary<IntPtr, WindowPlacement>();

				var openWindowProcesses = System.Diagnostics.Process.GetProcesses()
					.Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer");

				var currentConfig = GetCurrentScreenConfig();

				foreach (var window in HwndObject.GetWindows())
				{
					Debug.WriteLine("Saving position of " + window.Hwnd + " as " + window.WindowPlacement);
					var state = window.WindowPlacement;

					windowPositions[window.Hwnd] = state;
				}

				var newConfig = GetCurrentScreenConfig();

				//Make sure resolution didn't change mid-record
				if (newConfig.Equals(currentConfig))
					resolutionDictionary[currentConfig] = windowPositions;
				else
					Debug.WriteLine("Resolution changed mid-record, dropping record");
			}
		}

		private async Task RestorePositionsAsync()
		{
			Debug.WriteLine("Restoring Positions");

			mynotifyicon.BalloonTipText = "Restoring Window Positions";
			mynotifyicon.ShowBalloonTip(500);
			await Task.Run(() => RestorePositions());
			changing = false;
		}

		private void RestorePositions()
		{
			lock (syncLock)
			{
				var openWindowProcesses = System.Diagnostics.Process.GetProcesses()
					.Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName != "explorer").ToArray();

				var currentConfig = GetCurrentScreenConfig();

				if (!resolutionDictionary.ContainsKey(currentConfig))
					return;

				var windowPositions = resolutionDictionary[currentConfig];

				foreach (var window in HwndObject.GetWindows())
				{
					if (windowPositions.ContainsKey(window.Hwnd))
					{
						Debug.WriteLine("Restoring position of " + window.Hwnd + " to " + windowPositions[window.Hwnd]);
						window.WindowPlacement = windowPositions[window.Hwnd];
					}
				}
			}
		}
	}
}
