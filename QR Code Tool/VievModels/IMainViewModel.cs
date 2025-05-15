using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.VievModels
{
    public interface IMainViewModel
    {
        ICommand ClickBack { get; }
        ICommand ClickGoUp { get; }
        ICommand ClickRefresh { get; }
        ICommand ClickHome { get; }
        ICommand ClickPrintQR { get; }
        ICommand ClickPublish { get; }
        ICommand ClickUnPublish { get; }
        ICommand ClickCopiLink { get; }
        ICommand ClickUpLoadFile { get; }
        ICommand ClickDeleteFile { get; }
        ICommand ClickLogOut { get; }
        ICommand ClickLogIn { get; }
        Visibility IsProressVisibility { get; set; }
        ObservableCollection<Resource> FolderItems { get; set; }

        void GridItems_SelectionChanged(SelectionChangedEventArgs e);
        void Row_DoubleClick();
    }
}
