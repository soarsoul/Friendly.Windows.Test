using System.ComponentModel;
using System.Windows;

namespace Target
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new VM();
        }

        string MyFunc(int value)
        {
            return value.ToString();
        }
    }

    class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _textData;

        public string TextData
        {
            get { return _textData; }
            set
            {
                _textData = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TextData)));
            }
        }
    }
}
