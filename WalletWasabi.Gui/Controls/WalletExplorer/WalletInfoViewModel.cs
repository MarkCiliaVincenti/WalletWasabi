﻿using Avalonia.Threading;
using NBitcoin;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using WalletWasabi.Helpers;
using WalletWasabi.KeyManagement;
using WalletWasabi.Services;

namespace WalletWasabi.Gui.Controls.WalletExplorer
{
	public class WalletInfoViewModel : WalletActionViewModel
	{
		private string _password;
		private string _extendedMasterPrivateKey;
		private string _extendedMasterZprv;
		private string _extendedAccountPrivateKey;
		private string _extendedAccountZprv;
		private string _warningMessage;
		private CompositeDisposable _disposables;

		public WalletInfoViewModel(WalletViewModel walletViewModel) : base(walletViewModel.Name, walletViewModel)
		{
			ClearSensitiveData(true);
			_warningMessage = "";
			Closing = new CancellationTokenSource();

			this.WhenAnyValue(x => x.Password).Subscribe(x =>
			{
				try
				{
					if (x.NotNullAndNotEmpty())
					{
						char lastChar = x.Last();
						if (lastChar == '\r' || lastChar == '\n') // If the last character is cr or lf then act like it'd be a sign to do the job.
						{
							Password = x.TrimEnd('\r', '\n');
						}
					}
				}
				catch (Exception ex)
				{
					Logging.Logger.LogTrace(ex);
				}
			});

			ShowSensitiveKeysCommand = ReactiveCommand.Create(() =>
			{
				try
				{
					Password = Guard.Correct(Password);
					var secret = KeyManager.GetMasterExtKey(Password);
					Password = "";

					string master = secret.GetWif(Global.Network).ToWif();
					string account = secret.Derive(KeyManager.AccountKeyPath).GetWif(Global.Network).ToWif();
					string masterZ = secret.ToZPrv(Global.Network);
					string accountZ = secret.Derive(KeyManager.AccountKeyPath).ToZPrv(Global.Network);
					SetSensitiveData(master, account, masterZ, accountZ);
				}
				catch (Exception ex)
				{
					SetWarningMessage(ex.ToTypeMessageString());
				}
			});
		}

		private void ClearSensitiveData(bool passwordToo)
		{
			ExtendedMasterPrivateKey = "";
			ExtendedMasterZprv = "";
			ExtendedAccountPrivateKey = "";
			ExtendedAccountZprv = "";

			if (passwordToo)
			{
				Password = "";
			}
		}

		public CancellationTokenSource Closing { get; }

		public WalletService WalletService => Wallet.WalletService;
		public KeyManager KeyManager => WalletService.KeyManager;

		public string ExtendedAccountPublicKey => KeyManager.ExtPubKey.ToString(Global.Network);
		public string ExtendedAccountZpub => KeyManager.ExtPubKey.ToZpub(Global.Network);
		public string EncryptedExtendedMasterPrivateKey => KeyManager.EncryptedSecret.ToWif();
		public string AccountKeyPath => $"m/{KeyManager.AccountKeyPath.ToString()}";

		public ReactiveCommand ShowSensitiveKeysCommand { get; }

		public string Password
		{
			get => _password;
			set => this.RaiseAndSetIfChanged(ref _password, value);
		}

		public string ExtendedMasterPrivateKey
		{
			get => _extendedMasterPrivateKey;
			set => this.RaiseAndSetIfChanged(ref _extendedMasterPrivateKey, value);
		}

		public string ExtendedAccountPrivateKey
		{
			get => _extendedAccountPrivateKey;
			set => this.RaiseAndSetIfChanged(ref _extendedAccountPrivateKey, value);
		}

		public string ExtendedMasterZprv
		{
			get => _extendedMasterZprv;
			set => this.RaiseAndSetIfChanged(ref _extendedMasterZprv, value);
		}

		public string ExtendedAccountZprv
		{
			get => _extendedAccountZprv;
			set => this.RaiseAndSetIfChanged(ref _extendedAccountZprv, value);
		}

		public string WarningMessage
		{
			get => _warningMessage;
			set => this.RaiseAndSetIfChanged(ref _warningMessage, value);
		}

		private void SetSensitiveData(string extendedMasterPrivateKey, string extendedAccountPrivateKey, string extendedMasterZprv, string extendedAccountZprv)
		{
			ExtendedMasterPrivateKey = extendedMasterPrivateKey;
			ExtendedAccountPrivateKey = extendedAccountPrivateKey;
			ExtendedMasterZprv = extendedMasterZprv;
			ExtendedAccountZprv = extendedAccountZprv;

			Dispatcher.UIThread.PostLogException(async () =>
			{
				try
				{
					await Task.Delay(21000, Closing.Token);
				}
				catch (TaskCanceledException)
				{
					// Ignore
				}
				finally
				{
					ClearSensitiveData(false);
				}
			});
		}

		public override void OnOpen()
		{
			if (_disposables != null)
			{
				throw new Exception("WalletInfo was opened before it was closed.");
			}

			_disposables = new CompositeDisposable();

			Global.UiConfig.WhenAnyValue(x => x.LurkingWifeMode).Subscribe(_ =>
			{
				this.RaisePropertyChanged(nameof(EncryptedExtendedMasterPrivateKey));
				this.RaisePropertyChanged(nameof(ExtendedAccountPublicKey));
				this.RaisePropertyChanged(nameof(ExtendedAccountZpub));
			}).DisposeWith(_disposables);

			base.OnOpen();
		}

		public override void OnDeselected()
		{
			ClearSensitiveData(true);
			base.OnDeselected();
		}

		public override void OnSelected()
		{
			ClearSensitiveData(true);
			base.OnSelected();
		}

		public override bool OnClose()
		{
			ClearSensitiveData(true);
			return base.OnClose();
		}

		private void SetWarningMessage(string message)
		{
			WarningMessage = message;

			Dispatcher.UIThread.PostLogException(async () =>
			{
				try
				{
					await Task.Delay(7000, Closing.Token);
				}
				catch (TaskCanceledException)
				{
					// Ignore
				}
				finally
				{
					if (WarningMessage == message)
					{
						WarningMessage = "";
					}
				}
			});
		}
	}
}
