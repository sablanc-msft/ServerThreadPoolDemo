using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;

namespace ThreadPoolDemoClient
{
    internal class MainWindowViewModel : BindableBase
    {
        static readonly HttpClient s_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        private int m_simultaneousRequests = 100;
        private bool m_isRunning;
        private Uri m_selectedEndPoint;
        private LogEntryViewModel m_selectedLogEntry;

        private readonly ObservableCollection<Uri> m_endPoints = new ObservableCollection<Uri>();
        private readonly ObservableCollection<LogEntryViewModel> m_logEntries = new ObservableCollection<LogEntryViewModel>();

        private readonly Queue<CancellationTokenSource> m_cancellationTokenSources = new Queue<CancellationTokenSource>();


        private readonly DelegateCommand m_startCommand;
        private readonly DelegateCommand m_stopCommand;


        public MainWindowViewModel()
        {
            Uri baseUri = new Uri("http://localhost:9000/api/asyncserver/");

            m_endPoints.Add(new Uri(baseUri, "hello-sync"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-blocking"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-blocking-10ms"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-blocking-exception"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-over-sync"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-over-sync-no-threadpool"));
            m_endPoints.Add(new Uri(baseUri, "hello-sync-over-async"));
            m_endPoints.Add(new Uri(baseUri, "hello-async"));
            m_endPoints.Add(new Uri(baseUri, "hello-async-exception"));

            m_selectedEndPoint = m_endPoints.FirstOrDefault();

            m_startCommand = new DelegateCommand(ExcuteStartCommand, CanExecuteStartCommand);
            m_stopCommand = new DelegateCommand(ExcuteStopCommand, CanExcuteStopCommand);
        }


        #region   #  Properties  #


        public int SimultaneousRequests
        {
            get { return m_simultaneousRequests; }
            set
            {
                if (SetProperty(ref m_simultaneousRequests, value))
                {
                    if( m_isRunning )
                        UpdateNumberOfTasks();
                }
            }
        }

        public bool IsRunning
        {
            get { return m_isRunning; }
            private set { SetProperty(ref m_isRunning, value); }
        }


        public ObservableCollection<Uri> EndPoints
        {
            get { return m_endPoints; }
        }

        public ObservableCollection<LogEntryViewModel> LogEntries
        {
            get { return m_logEntries; }
        }

        public LogEntryViewModel SelectedLogEntry
        {
            get { return m_selectedLogEntry; }
            set { SetProperty(ref m_selectedLogEntry, value); }
        }


        public Uri SelectedEndPoint
        {
            get { return m_selectedEndPoint; }
            set
            {   if( !m_isRunning )
                    SetProperty(ref m_selectedEndPoint, value);
            }
        }


        #endregion


        #region   #  Commands  #


        public ICommand StartCommand
        {
            get { return m_startCommand; }
        }

        public ICommand StopCommand
        {
            get { return m_stopCommand; }
        }


        private bool CanExecuteStartCommand()
        {
            return !m_isRunning;
        }

        private void ExcuteStartCommand()
        {
            m_isRunning = true;
            m_stopCommand.RaiseCanExecuteChanged();
            m_startCommand.RaiseCanExecuteChanged();

            UpdateNumberOfTasks();
        }

        private bool CanExcuteStopCommand()
        {
            return m_isRunning;
        }

        private void ExcuteStopCommand()
        {
            m_isRunning = false;
            m_stopCommand.RaiseCanExecuteChanged();
            m_startCommand.RaiseCanExecuteChanged();

            while(m_cancellationTokenSources.Count > 0)
            {
                CancellationTokenSource cts = m_cancellationTokenSources.Dequeue();
                cts.Cancel();
            }
        }


        #endregion


        #region   #  Methods  #


        private void UpdateNumberOfTasks()
        {
            int queueOffset = m_cancellationTokenSources.Count - m_simultaneousRequests;

            if (queueOffset < 0)  //Need to create tasks
            {
                for( int i = queueOffset; i < 0; i++ )
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    m_cancellationTokenSources.Enqueue(cts);

                    Progress<string> progress = new Progress<string>(AddLogEntry);

                    _ = Task.Factory.StartNew( () => CreateClientQueryTask( progress, cts.Token ), TaskCreationOptions.LongRunning );
                }
            }
            else if( queueOffset > 0 )
            {
                for( int i = 0; i < queueOffset; i++ )
                {
                    CancellationTokenSource cts = m_cancellationTokenSources.Dequeue();
                    cts.Cancel();
                }
            }
        }


        private async Task CreateClientQueryTask(IProgress<string> progress, CancellationToken cancellationToken )
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Debug.Assert(m_selectedEndPoint != null);

                    stopwatch.Restart();
                    //HttpResponseMessage response = await s_httpClient.GetAsync( m_selectedEndPoint, cancellationToken );
                    //response.EnsureSuccessStatusCode();
                    //string responseBody = await response.Content.ReadAsStringAsync();

                    string responseBody = await s_httpClient.GetStringAsync(m_selectedEndPoint);

                    stopwatch.Stop();

                    progress.Report($"{responseBody}   {stopwatch.Elapsed.TotalSeconds:F3} secs.");
                }
                catch (Exception ex)
                {
                    progress.Report($"{ex.GetType().ToString()} Caught! \nMessage :{ex.Message}");
                    //throw;
                }
            }
        }

        
        private void AddLogEntry( string text )
        {
            LogEntryViewModel viewModel = new LogEntryViewModel(text);
            m_logEntries.Add(viewModel);
            this.SelectedLogEntry = viewModel;
        }





        #endregion


    }
}