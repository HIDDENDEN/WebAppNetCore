using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ViewModel;

namespace View
{
    public class MainWindow : Window
    {
        MyAppUIServices uiservices;
        ViewModel.ViewModel _viewModel;
        TextBlock textBlock;
        public MainWindow()
        {
            InitializeComponent();

            uiservices = new MyAppUIServices(this,
                                                this.FindControl<ListBox>("statisticImagesListBox"),

                                                 this.FindControl<ListBox>("recognizedImagesListBox"),
                                                 this.FindControl<ListBox>("chosenTypeImagesListBox"),

                                                 this.FindControl<ComboBox>("possibleResultsComboBox"),

                                                 this.FindControl<ProgressBar>("progressBar"),
                                                 this.FindControl<TextBlock>("progressBarTextBlock"),

                                                 this.FindControl<TextBlock>("recognizedImagesTextBlock"),
                                                 this.FindControl<TextBlock>("chosenTypeImagesTextBlock"),
                                                 this.FindControl<TextBlock>("possibleResultsTextBlock"),

                                                 this.FindControl<TextBlock>("serverCondition")
                                                 );
            _viewModel = new ViewModel.ViewModel(uiservices);
            DataContext = _viewModel;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    class MyAppUIServices : IUIServices
    {
        Window window;
        ListBox statisticImagesListBox;
        ListBox recognizedImagesListBox;
        ListBox chosenTypeImagesListBox;
        ComboBox possibleResultsComboBox;
        ProgressBar progressBar;
        TextBlock progressBarTextBlock;

        TextBlock recognizedImagesTextBlock;
        TextBlock chosenTypeImagesTextBlock;
        TextBlock possibleResultsTextBlock;
        TextBlock serverCondition;

        public MyAppUIServices(Window window,
                               ListBox statisticImagesListBox,
                               ListBox recognizedImagesListBox, ListBox chosenTypeImagesListBox,
                               ComboBox possibleResultsComboBox,
                               ProgressBar progressBar, TextBlock progressBarTextBlock,
                               TextBlock recognizedImagesTextBlock, TextBlock chosenTypeImagesTextBlock, TextBlock possibleResultsTextBlock,
                               TextBlock serverCondition
                               )
        {
            this.window = window;
            this.statisticImagesListBox = statisticImagesListBox;
            this.recognizedImagesListBox = recognizedImagesListBox;
            this.chosenTypeImagesListBox = chosenTypeImagesListBox;
            this.possibleResultsComboBox = possibleResultsComboBox;
            this.progressBar = progressBar;
            this.progressBarTextBlock = progressBarTextBlock;
            this.possibleResultsTextBlock = possibleResultsTextBlock;
            this.recognizedImagesTextBlock = recognizedImagesTextBlock;
            this.chosenTypeImagesTextBlock = chosenTypeImagesTextBlock;

            this.serverCondition = serverCondition;
        }

        public async Task<string> OpenDialog()
        {


            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Directory = @"/Users/denis/Downloads/ImageSet";
            return await openFolderDialog.ShowAsync(window);
        }
        public void SetServerOnline()
        {
            serverCondition.IsVisible = false;
        }
        public void SetServerOffline()
        {
            serverCondition.IsVisible = true;

        }

    }
}