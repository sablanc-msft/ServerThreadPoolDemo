using Prism.Mvvm;

namespace ThreadPoolDemoClient
{
    internal class LogEntryViewModel : BindableBase
    {
        public string Text { get; }

        public LogEntryViewModel(string text)
        {
            this.Text = text;
        }
    }
}