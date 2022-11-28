using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using WalletWasabi.Blockchain.Analysis.Clustering;
using WalletWasabi.Fluent.Helpers;

namespace WalletWasabi.Fluent.ViewModels.CoinControl.Core;

public abstract partial class CoinControlItemViewModelBase : ViewModelBase, IHierarchicallySelectable
{
	[AutoNotify] private bool? _isSelected;

	public CoinControlItemViewModelBase(IEnumerable<IHierarchicallySelectable> children)
	{
		Selectables = children.ToList();
		Children = Selectables.Cast<CoinControlItemViewModelBase>().ToList();
		HierarchicalSelection = new HierarchicalSelection(this);
	}

	public bool IsPrivate => Labels == CoinPocketHelper.PrivateFundsText;

	public bool IsSemiPrivate => Labels == CoinPocketHelper.SemiPrivateFundsText;

	public bool IsNonPrivate => !IsSemiPrivate && !IsPrivate;

	public IEnumerable<CoinControlItemViewModelBase> Children { get; }

	public bool IsConfirmed { get; protected set; }

	public bool IsCoinjoining { get; protected set; }

	public bool IsBanned { get; protected set; }

	public string ConfirmationStatus { get; protected set; } = "";

	public Money Amount { get; protected set; } = Money.Zero;

	public string? BannedUntilUtcToolTip { get; protected set; }

	public int AnonymityScore { get; protected set; }

	public SmartLabel Labels { get; protected set; } = SmartLabel.Empty;

	public DateTimeOffset? BannedUntilUtc { get; protected set; }

	public bool IsExpanded { get; set; } = true;

	public HierarchicalSelection HierarchicalSelection { get; }

	public IEnumerable<IHierarchicallySelectable> Selectables { get; }
}
