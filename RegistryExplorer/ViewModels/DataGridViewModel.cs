﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Prism.Commands;
using Prism.Mvvm;
using Zodiacon.WPF;

namespace RegistryExplorer.ViewModels {
	class DataGridViewModel : ViewModelBase, IDisposable {
		public MainViewModel MainViewModel { get; }
		PropertyFollower<MainViewModel, DataGridViewModel> _follower;

		public DelegateCommand ClearFilterCommand { get; }

		public DataGridViewModel() {
			MainViewModel = App.MainViewModel;

			_follower = new PropertyFollower<MainViewModel, DataGridViewModel>(MainViewModel, this, nameof(IsReadOnlyMode));
			_follower.Add(nameof(MainViewModel.SelectedItem), _ => {
				FilterText = string.Empty;
				RaisePropertyChanged(nameof(Values));
			});

			ClearFilterCommand = new DelegateCommand(() => FilterText = string.Empty);
		}

		IEnumerable<RegistryValue> _values;

		public IEnumerable<RegistryValue> Values {
			get {
				var regItem = MainViewModel.SelectedItem as RegistryKeyItem;
				return (_values = (regItem != null ? regItem.Values : null));
			}
		}

		public bool IsReadOnlyMode {
			get { return MainViewModel.IsReadOnlyMode; }
		}

		public void Refresh() {
			RaisePropertyChanged(nameof(Values));
		}

		private string _filterText;

		public string FilterText {
			get { return _filterText; }
			set {
				if(SetProperty(ref _filterText, value)) {
					if(_values != null && string.IsNullOrEmpty(value)) {
						CollectionViewSource.GetDefaultView(_values).Filter = null;
					}
					else if(_values != null) {
						CollectionViewSource.GetDefaultView(_values).Filter = obj => {
							var theValue = (RegistryValue)obj;
							var lvalue = value.ToLower();
							return theValue.Name.ToLower().Contains(lvalue) || theValue.ValueAsString.ToLower().Contains(lvalue);
						};
					}
				}
			}
		}

		public void Dispose() {
			_follower.Dispose();
		}

		protected override void OnIsActiveChanged() {
			base.OnIsActiveChanged();

			if(IsActive)
				ActiveView = this;
		}
	}
}
