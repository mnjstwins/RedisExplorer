﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Windows;

using Caliburn.Micro;

using Newtonsoft.Json.Linq;

using RedisExplorer.Controls;
using RedisExplorer.Interface;
using RedisExplorer.Messages;
using RedisExplorer.Models;
using RedisExplorer.Properties;

namespace RedisExplorer
{
    [Export(typeof(AppViewModel))]
    public sealed class AppViewModel : Conductor<ITabItem>.Collection.OneActive, IApp, IHandle<TreeItemSelectedMessage>, IHandle<AddConnectionMessage>
    {
        #region Private members

        private readonly IEventAggregator eventAggregator;

        private readonly IWindowManager windowManager;

        private string statusBarTextBlock;

        private BindableCollection<RedisServer> servers; 

        #endregion

        #region Properties

        public BindableCollection<RedisServer> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        #endregion

        public AppViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            this.DisplayName = "Redis Explorer";

            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            Servers = new BindableCollection<RedisServer>();

            KeyViewModel = new KeyViewModel(eventAggregator);
            KeyViewModel.ConductWith(this);

            LoadServers();
        }

        private void LoadServers()
        {
            if (Settings.Default.Servers != null)
            {
                foreach (var connection in Settings.Default.Servers)
                {
                    var server = new RedisConnection(connection);
                    Servers.Add(new RedisServer(server.Name, server.Address + ",keepAlive = 180,allowAdmin=true", eventAggregator));
                }
            }
        }

        #region Properties

        public KeyViewModel KeyViewModel { get; set; }

        public string StatusBarTextBlock
        {
            get
            {
                return statusBarTextBlock;
            }
            set
            {
                statusBarTextBlock = value;
                NotifyOfPropertyChange(() => StatusBarTextBlock);
            }
        }

        #endregion

        #region Menu

        public void Exit()
        {
            Application.Current.Shutdown();
        }


        public void AddServer()
        {
            dynamic settings = new ExpandoObject();
            settings.Width = 300;
            settings.Height = 250;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            settings.Title = "Add Server";

            windowManager.ShowWindow(new AddConnectionViewModel(eventAggregator), null, settings);    
        }

        #endregion

        public void Handle(TreeItemSelectedMessage message)
        {
            StatusBarTextBlock = message.SelectedItem.Display;
        }

        public void Handle(AddConnectionMessage message)
        {
            StringCollection connections = Settings.Default.Servers ?? new StringCollection();

            var newconn = new RedisConnection
            {
                Name = message.Connection.Name,
                Address = message.Connection.Address,
                Port = message.Connection.Port
            };

            connections.Add(newconn.ToString());

            Settings.Default.Servers = connections;
            Settings.Default.Save();

            Servers.Clear();

            LoadServers();
        }
    }
}
