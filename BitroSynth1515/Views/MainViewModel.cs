namespace BitroSynth1515.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _channel_1_text = "";
        public string Channel1Text
        {
            get => _channel_1_text;
            set
            {
                _channel_1_text = value;
                OnPropertyChanged();
            }
        }

        private string _channel_2_text = "";
        public string Channel2Text
        {
            get => _channel_2_text;
            set
            {
                _channel_2_text = value;
                OnPropertyChanged();
            }
        }

        private string _channel_3_text = "";
        public string Channel3Text
        {
            get => _channel_3_text;
            set
            {
                _channel_3_text = value;
                OnPropertyChanged();
            }
        }

        private string _channel_4_text = "";
        public string Channel4Text
        {
            get => _channel_4_text;
            set
            {
                _channel_4_text = value;
                OnPropertyChanged();
            }
        }


    }

}