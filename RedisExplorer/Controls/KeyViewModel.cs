﻿using System;
using System.ComponentModel.Composition;

using Caliburn.Micro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RedisExplorer.Messages;
using RedisExplorer.Models;

namespace RedisExplorer.Controls
{
    [Export(typeof(KeyViewModel))]
    public class KeyViewModel : Screen, IHandle<TreeItemSelectedMessage>, IHandle<AddKeyMessage>
    {
        #region Members

        private RedisKey item;

        private readonly IEventAggregator eventAggregator;

        private bool hasSelected;
        private string keyNameTextBox;
        private string keyValueTextBox;
        private DateTime? ttlDateTimePicker;
        private string typeLabel;

        #endregion

        #region Properties

        public bool HasSelected
        {
            get
            {
                return hasSelected;
            }
            set
            {
                hasSelected = value;
                NotifyOfPropertyChange(() => HasSelected);
            }
        }

        public string KeyNameTextBox
        {
            get
            {
                return keyNameTextBox;
            }
            set
            {
                keyNameTextBox = value;
                NotifyOfPropertyChange(() => KeyNameTextBox);
            }
        }

        public string KeyValueTextBox
        {
            get
            {
                return keyValueTextBox;
            }
            set
            {
                keyValueTextBox = value;
                NotifyOfPropertyChange(() => KeyValueTextBox);
            }
        }

        public DateTime? TTLDateTimePicker
        {
            get
            {
                return ttlDateTimePicker;
            }
            set
            {
                ttlDateTimePicker = value;
                NotifyOfPropertyChange(() => TTLDateTimePicker);
            }
        }

        public string TypeLabel
        {
            get
            {
                return typeLabel;
            }
            set
            {
                typeLabel = value;
                NotifyOfPropertyChange(() => TypeLabel);
            }
        }

        #endregion

        public KeyViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);

            SetDefault();
        }

        public void SetDefault()
        {
            TypeLabel = "String";
            TTLDateTimePicker = null;
        }

        #region Button Actions

        public void SaveButton()
        {
            item.KeyName = keyNameTextBox;
            item.KeyValue = keyValueTextBox;
            //item.TTL = TimeSpan.FromSeconds(double.Parse(TTLSecondsLabel));
            if (item.SaveValue())
            {
                
            }
        }

        public void DeleteButton()
        {
            if (item.Delete())
            {
                
            }
        }

        #endregion

        #region Message Handlers

        public void Handle(TreeItemSelectedMessage message)
        {
            if (message != null && message.SelectedItem != null && message.SelectedItem.GetType() == typeof(RedisKey) && !message.SelectedItem.HasChildren)
            {
                item = message.SelectedItem as RedisKey;

                if (item != null)
                {
                    KeyNameTextBox = item.KeyName;
                    TypeLabel = item.KeyType.ToString();

                    var ttl = item.TTL;
                    if (ttl.HasValue)
                    {
                        TTLDateTimePicker = DateTime.Now + ttl.Value;
                    } 

                    var value = item.KeyValue;

                    try
                    {
                        KeyValueTextBox = JObject.Parse(value).ToString(Formatting.Indented);
                    }
                    catch (JsonReaderException)
                    {
                        KeyValueTextBox = value;
                    }
                }
            }
        }

        public void Handle(AddKeyMessage message)
        {
            item = new RedisKey(message.ParentDatabase, eventAggregator);
            SetDefault();
            KeyNameTextBox = message.KeyBase;
            KeyValueTextBox = string.Empty;
        }

        #endregion
    }
}
