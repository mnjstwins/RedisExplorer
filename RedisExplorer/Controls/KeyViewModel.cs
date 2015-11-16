﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedisExplorer.Messages;
using StackExchange.Redis;
using RedisKey = RedisExplorer.Models.RedisKey;

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
        private RedisType selectedType;

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

        public RedisType SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                NotifyOfPropertyChange(() => SelectedType);
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

        public IEnumerable<RedisType> RedisTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(RedisType)).Cast<RedisType>();
            }
        }

        #region Button Actions

        public void SaveButton()
        {
            if (item == null)
            {
                return;
            }
            item.KeyName = keyNameTextBox;
            item.KeyValue = keyValueTextBox;
            if (TTLDateTimePicker.HasValue)
            {
                item.TTL = new TimeSpan((TTLDateTimePicker.Value - DateTime.Now).Ticks);
            }
            item.Save();
        }

        public void DeleteButton()
        {
            if (item == null)
            {
                return;
            }
            item.Delete();
        }

        public void ReloadButton()
        {
            if (item == null)
            {
                return;
            }
            item.Reload();
            DisplayItem();
        }

        public void ClearButton()
        {
            TTLDateTimePicker = null;
        }

        public void OneHourButton()
        {
            TTLDateTimePicker = DateTime.Now.AddHours(1);
        }

        public void TwentyFourHoursButton()
        {
            TTLDateTimePicker = DateTime.Now.AddHours(24);
        }

        #endregion

        #region Message Handlers

        public void Handle(TreeItemSelectedMessage message)
        {
            if (message != null && message.SelectedItem != null && message.SelectedItem.GetType() == typeof(RedisKey) && !message.SelectedItem.HasChildren)
            {
                item = message.SelectedItem as RedisKey;

                DisplayItem();
            }
        }

        private void DisplayItem()
        {
            if (item != null)
            {
                KeyNameTextBox = item.KeyName;
                TypeLabel = item.KeyType.ToString();

                var ttl = item.TTL;
                if (ttl.HasValue)
                {
                    TTLDateTimePicker = DateTime.Now + ttl.Value;
                }
                else
                {
                    TTLDateTimePicker = null;
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
