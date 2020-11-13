using System.IO;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using WalletWasabi.Fluent.ViewModels.Dialogs;
using WalletWasabi.Legal;

namespace WalletWasabi.Fluent.ViewModels.AddWallet
{
	public class LegalDocumentsViewModel : RoutableViewModel
	{
		public LegalDocumentsViewModel(NavigationStateViewModel navigationState, NavigationTarget navigationTarget, string content) :
			base(navigationState, navigationTarget)
		{
			Content = content;
		}

		public ICommand NextCommand => BackCommand;

		public string Content { get; }
	}

	public class TermsAndConditionsViewModel : DialogViewModelBase<bool>
	{
		private bool _isAgreed;

		public TermsAndConditionsViewModel(NavigationStateViewModel navigationState, LegalDocuments legalDocuments) : base(navigationState, NavigationTarget.DialogScreen)
		{
			ViewTermsCommand = ReactiveCommand.CreateFromTask(
				async () =>
				{
					var content = await File.ReadAllTextAsync(legalDocuments.FilePath);

					var legalDocs = new LegalDocumentsViewModel(
						navigationState,
						NavigationTarget.DialogScreen,
						content);

					legalDocs.Navigate();
				});

			NextCommand = ReactiveCommand.Create(
				() => Close(true),
				this.WhenAnyValue(x => x.IsAgreed));
		}

		public bool IsAgreed
		{
			get => _isAgreed;
			set => this.RaiseAndSetIfChanged(ref _isAgreed, value);
		}

		public ICommand ViewTermsCommand { get; }
		protected override void OnDialogClosed()
		{

		}
	}
}