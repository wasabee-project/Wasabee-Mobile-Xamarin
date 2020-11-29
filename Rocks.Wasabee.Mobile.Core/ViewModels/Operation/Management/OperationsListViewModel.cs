using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management
{
    public class OperationsListViewModel : BaseViewModel
    {
        private readonly OperationsDatabase _operationsDatabase;
        private readonly IMvxMessenger _messenger;

        public OperationsListViewModel(OperationsDatabase operationsDatabase, IMvxMessenger messenger)
        {
            _operationsDatabase = operationsDatabase;
            _messenger = messenger;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            RefreshCommand.Execute(false);
        }

        #region Properties

        public MvxObservableCollection<Operation> OperationsCollection { get; set; } = new MvxObservableCollection<Operation>();
        public bool IsRefreshing { get; set; }

        private bool _showHiddenOps = false;
        public bool ShowHiddenOps
        {
            get => _showHiddenOps;
            set
            {
                if (SetProperty(ref _showHiddenOps, value))
                    RefreshCommand.Execute(_showHiddenOps);
            }
        }

        #endregion

        #region Commands

        public IMvxCommand RefreshCommand => new MvxCommand(async () => await RefreshExecuted());
        public async Task RefreshExecuted()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;

            var ops = await _operationsDatabase.GetOperationModels();
            if (ops.Any())
            {
                if (!ShowHiddenOps)
                    ops = ops.Where(x => !x.IsHiddenLocally).ToList();

                OperationsCollection = new MvxObservableCollection<Operation>(ops.Select(x => new Operation(x.Id, x.Name) { IsHiddenLocally = x.IsHiddenLocally }));
            }

            IsRefreshing = false;
        }

        public IMvxAsyncCommand<Operation> HideOperationCommand => new MvxAsyncCommand<Operation>(HideOperationExecuted);
        private async Task HideOperationExecuted(Operation op)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            var hideOp = !op.IsHiddenLocally;
            var result = await _operationsDatabase.HideLocalOperation(op.Id, hideOp);
            if (result)
            {
                var rowToUpdate = OperationsCollection.First(x => x.Id.Equals(op.Id));
                var rowIndex = OperationsCollection.IndexOf(rowToUpdate);
                if (ShowHiddenOps)
                {
                    OperationsCollection[rowIndex].IsHiddenLocally = hideOp;
                }
                else
                {
                    if (hideOp)
                        OperationsCollection.Remove(op);
                }
            }

            _messenger.Publish(new MessageFrom<OperationsListViewModel>(this));
            IsBusy = false;
        }

        #endregion


        #region Private Methods



        #endregion
    }

    public class Operation : MvxViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public Operation(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool IsHiddenLocally { get; set; } = false;
    }
}