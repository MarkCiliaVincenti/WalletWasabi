using Moq;
using WalletWasabi.Fluent.Models.UI;
using WalletWasabi.Fluent.Models.Wallets;
using WalletWasabi.Fluent.ViewModels.Dialogs.Base;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Tests.UnitTests.ViewModels.TestDoubles;

namespace WalletWasabi.Tests.UnitTests.ViewModels;

public class UiContextBuilder
{
	public INavigate Navigate { get; private set; } = Mock.Of<INavigate>();
	public IQrCodeGenerator QrGenerator { get; } = Mock.Of<IQrCodeGenerator>();
	public IQrCodeReader QrReader { get; } = Mock.Of<IQrCodeReader>();
	public IUiClipboard Clipboard { get; private set; } = Mock.Of<IUiClipboard>();
	public IWalletRepository WalletRepository { get; private set; } = new NullWalletRepository();
	public IHardwareWalletInterface HardwareWalletInterface { get; private set; } = new NullHardwareWalletInterface();

	public UiContextBuilder WithDialogThatReturns(object value)
	{
		Navigate = new NavigationMock((value, DialogResultKind.Normal));
		return this;
	}

	public UiContextBuilder WithClipboard(IUiClipboard clipboard)
	{
		Clipboard = clipboard;
		return this;
	}

	public UiContext Build()
	{
		var uiContext = new UiContext(QrGenerator, QrReader, Clipboard, WalletRepository, HardwareWalletInterface);
		uiContext.RegisterNavigation(Navigate);
		return uiContext;
	}
}
