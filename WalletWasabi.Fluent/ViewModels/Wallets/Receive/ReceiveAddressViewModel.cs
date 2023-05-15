using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using WalletWasabi.Fluent.Extensions;
using WalletWasabi.Fluent.Helpers;
using WalletWasabi.Fluent.Models.UI;
using WalletWasabi.Fluent.Models.Wallets;
using WalletWasabi.Fluent.ViewModels.Navigation;

namespace WalletWasabi.Fluent.ViewModels.Wallets.Receive;

[NavigationMetaData(Title = "Receive Address")]
public partial class ReceiveAddressViewModel : RoutableViewModel
{
	public ReceiveAddressViewModel(UiContext uiContext, IWalletModel wallet, IAddress model, bool isAutoCopyEnabled)
	{
		UiContext = uiContext;
		Model = model;
		Address = model.Text;
		Labels = model.Labels;
		IsHardwareWallet = wallet.IsHardwareWallet();
		IsAutoCopyEnabled = isAutoCopyEnabled;

		SetupCancel(enableCancel: false, enableCancelOnEscape: true, enableCancelOnPressed: true);

		EnableBack = true;

		CopyAddressCommand = ReactiveCommand.CreateFromTask(() => UiContext.Clipboard.SetTextAsync(Address));

		ShowOnHwWalletCommand = ReactiveCommand.CreateFromTask(ShowOnHwWalletAsync);

		NextCommand = CancelCommand;

		QrCode = UiContext.QrCodeGenerator.Generate(model.Text);

		if (IsAutoCopyEnabled)
		{
			CopyAddressCommand.Execute(null);
		}

		wallet.Addresses
			.Watch(model.Text)
			.Where(change => change.Current.IsUsed)
			.Do(_ => UiContext.Navigate(CurrentTarget).Back())
			.Subscribe();
	}

	public bool IsAutoCopyEnabled { get; }

	public ICommand CopyAddressCommand { get; }

	public ICommand ShowOnHwWalletCommand { get; }

	public string Address { get; }

	public IEnumerable<string> Labels { get; }

	public bool IsHardwareWallet { get; }

	public IObservable<bool[,]> QrCode { get; }

	private IAddress Model { get; }

	private async Task ShowOnHwWalletAsync()
	{
		try
		{
			await Model.ShowOnHwWalletAsync();
		}
		catch (Exception ex)
		{
			await ShowErrorAsync(Title, ex.ToUserFriendlyString(), "Unable to send the address to the device");
		}
	}
}
