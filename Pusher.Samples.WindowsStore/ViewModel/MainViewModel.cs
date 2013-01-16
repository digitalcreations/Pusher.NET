using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Pusher.Connections.WindowsStore;
using Pusher.Events;
using Windows.UI.Popups;

namespace Pusher.Samples.WindowsStore.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		private readonly Logger _logger;
		private Pusher _pusher;

		/// <summary>
		///     Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			_logger = new Logger(this, SynchronizationContext.Current);
		}

		#region Helper classes

		private class Authenticator : IAuthenticator
		{
			private readonly Dictionary<string, string> _replacements;
			private readonly string _url;

			public Authenticator(string url, Dictionary<string, string> replacements)
			{
				_replacements = replacements;
				_url = url;
			}

			public async Task<IAuthenticationData> AuthenticateAsync(string socketId)
			{
				_replacements["socketId"] = socketId;
				using (var client = new HttpClient())
				{
					var httpResponse = await client.GetAsync(BuildUri());
					var responseText = await httpResponse.Content.ReadAsStringAsync();

					return await JsonConvert.DeserializeObjectAsync<AuthenticationData>(responseText);
				}
			}

			private Uri BuildUri()
			{
				// todo: URL encode
				return new Uri(_replacements.Aggregate(_url, (current, r) => current.Replace("{" + r.Key + "}", r.Value)));
			}
		}

		private class Logger : ILogger
		{
			private readonly SynchronizationContext _context;
			private readonly MainViewModel _vm;

			public Logger(MainViewModel vm, SynchronizationContext context)
			{
				_vm = vm;
				_context = context;
			}

			public void Debug(string text, params object[] parameters)
			{
				// marshal to UI thread before updating log
				_context.Post(
					o => _vm.Log = _vm.Log + string.Format("{0}[Debug] ", Environment.NewLine) + string.Format(text, parameters), null);
			}
		}

		#endregion

		private async void ConnectAsync()
		{
			var options = new Options {Scheme = WebServiceScheme.Secure};
			if (AuthenticationRequired)
			{
				options.Authenticator = new Authenticator(AuthenticationUrl, new Dictionary<string, string>
					{
						{"channel", Channel}
					});
			}
			_pusher = new Pusher(new WebsocketConnectionFactory(), AppKey, options);
			_pusher.Logger = _logger;

			_logger.Debug("Connecting...");
			await _pusher.ConnectAsync();
			_logger.Debug("Connected!");
			_logger.Debug("Subscribing to {0}!", Channel);
			var channel = await _pusher.SubscribeToChannelAsync(Channel);

			var synchronizationContext = SynchronizationContext.Current;

			channel.GetEventSubscription<SubscriptionSucceededEventArgs>().EventEmitted += async (sender, evt) =>
				{
					synchronizationContext.Post(async channelName =>
						{
							var d = new MessageDialog(string.Format("Subscribed to {0}!", channelName));
							await d.ShowAsync();
						}, evt.Channel);
				};
		}

		#region Commands

		private RelayCommand _connectCommand;

		/// <summary>
		///     Gets the ConnectCommand.
		/// </summary>
		public RelayCommand ConnectCommand
		{
			get
			{
				return _connectCommand
				       ?? (_connectCommand = new RelayCommand(ConnectAsync,
				                                              () =>
				                                              AppKey.Length == 20 && Channel.Length > 0 &&
				                                              (!AuthenticationRequired || AuthenticationUrl.Length > 10)));
			}
		}

		#endregion

		#region Properties

		/// <summary>
		///     The <see cref="Log" /> property's name.
		/// </summary>
		public const string LogPropertyName = "Log";

		/// <summary>
		///     The <see cref="AuthenticationUrl" /> property's name.
		/// </summary>
		public const string AuthenticationUrlPropertyName = "AuthenticationUrl";

		/// <summary>
		///     The <see cref="AuthenticationRequired" /> property's name.
		/// </summary>
		public const string AuthenticationRequiredPropertyName = "AuthenticationRequired";

		/// <summary>
		///     The <see cref="Channel" /> property's name.
		/// </summary>
		public const string ChannelPropertyName = "Channel";

		/// <summary>
		///     The <see cref="AppKey" /> property's name.
		/// </summary>
		public const string AppKeyPropertyName = "AppKey";

		private string _appKey = string.Empty;
		private bool _authenticationRequired;
		private string _authenticationUrl = string.Empty;
		private string _channel = string.Empty;
		private string _log = string.Empty;

		/// <summary>
		///     Sets and gets the Log property.
		///     Changes to that property's value raise the PropertyChanged event.
		/// </summary>
		public string Log
		{
			get { return _log; }

			set
			{
				if (_log == value)
				{
					return;
				}

				RaisePropertyChanging(LogPropertyName);
				_log = value;
				RaisePropertyChanged(LogPropertyName);
				ConnectCommand.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		///     Sets and gets the AuthenticationUrl property.
		///     Changes to that property's value raise the PropertyChanged event.
		/// </summary>
		public string AuthenticationUrl
		{
			get { return _authenticationUrl; }

			set
			{
				if (_authenticationUrl == value)
				{
					return;
				}

				RaisePropertyChanging(AuthenticationUrlPropertyName);
				_authenticationUrl = value;
				RaisePropertyChanged(AuthenticationUrlPropertyName);
				ConnectCommand.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		///     Sets and gets the AuthenticationRequired property.
		///     Changes to that property's value raise the PropertyChanged event.
		/// </summary>
		public bool AuthenticationRequired
		{
			get { return _authenticationRequired; }

			private set
			{
				if (_authenticationRequired == value)
				{
					return;
				}

				RaisePropertyChanging(AuthenticationRequiredPropertyName);
				_authenticationRequired = value;
				RaisePropertyChanged(AuthenticationRequiredPropertyName);
				ConnectCommand.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		///     Sets and gets the Channel property.
		///     Changes to that property's value raise the PropertyChanged event.
		/// </summary>
		public string Channel
		{
			get { return _channel; }

			set
			{
				if (_channel == value)
				{
					return;
				}
				AuthenticationRequired = (_channel.StartsWith("presence-") || _channel.StartsWith("private-"));

				RaisePropertyChanging(ChannelPropertyName);
				_channel = value;
				RaisePropertyChanged(ChannelPropertyName);
				ConnectCommand.RaiseCanExecuteChanged();
			}
		}

		/// <summary>
		///     Sets and gets the AppKey property.
		///     Changes to that property's value raise the PropertyChanged event.
		/// </summary>
		public string AppKey
		{
			get { return _appKey; }

			set
			{
				if (_appKey == value)
				{
					return;
				}

				RaisePropertyChanging(AppKeyPropertyName);
				_appKey = value;
				RaisePropertyChanged(AppKeyPropertyName);
				ConnectCommand.RaiseCanExecuteChanged();
			}
		}

		#endregion
	}
}