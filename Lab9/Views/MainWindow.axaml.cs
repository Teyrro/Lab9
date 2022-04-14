using Avalonia.Controls;
using System.Reactive;
using lab9.ViewModels;

namespace lab9.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.FindControl<MenuItem>("OpenFolder").Click += async delegate
            {
                var taskPath = new OpenFolderDialog()
                {
                    Title = "Choose folder..."
                };
                string? path = await taskPath.ShowAsync((Window)this.VisualRoot);

                var context = this.DataContext as MainWindowViewModel;
                if (path == null) return;
                else context.RootFolder = string.Join("\\", path);
                context.OpenDialogWindow();

            };
        }

    }
}
