﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using ScpControl.ScpCore;

namespace ScpControl.Driver
{
    [DataContract]
    public class ScpDeviceCollection : SingletonBase<ScpDeviceCollection>
    {
        #region Private fields

        private const string XmlFile = "ScpDevices.xml";
        private static readonly string XmlFilePath = Path.Combine(GlobalConfiguration.AppDirectory, XmlFile);
        private ObservableCollection<WdiUsbDevice> _collection;

        #endregion

        #region Ctor

        private ScpDeviceCollection()
        {
            ScpDevices = new List<WdiUsbDevice>();
        }

        #endregion

        #region Properties

        [DataMember(Name = "ScpDevices")]
        private List<WdiUsbDevice> ScpDevices { get; set; }

        public ObservableCollection<WdiUsbDevice> Devices
        {
            get { return Load(); }
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Fetches objects from XML file.
        /// </summary>
        /// <returns>The object collection.</returns>
        private ObservableCollection<WdiUsbDevice> Load()
        {
            _collection = new ObservableCollection<WdiUsbDevice>();

            if (File.Exists(XmlFilePath))
            {
                var serializer = new DataContractSerializer(typeof (ScpDeviceCollection));
                using (var reader = XmlReader.Create(XmlFilePath))
                {
                    ScpDevices = ((ScpDeviceCollection) serializer.ReadObject(reader)).ScpDevices;
                    _collection = new ObservableCollection<WdiUsbDevice>(ScpDevices);
                }
            }

            _collection.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        ScpDevices.AddRange(args.NewItems.Cast<WdiUsbDevice>());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in args.OldItems.Cast<WdiUsbDevice>())
                        {
                            ScpDevices.Remove(oldItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        ScpDevices.Clear();
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Save();
            };

            return _collection;
        }

        /// <summary>
        ///     Stores the current object in an XML file.
        /// </summary>
        private void Save()
        {
            // store current object in XML file
            var serializer = new DataContractSerializer(GetType());
            using (var writer = XmlWriter.Create(XmlFilePath, new XmlWriterSettings {Indent = true}))
            {
                serializer.WriteObject(writer, this);
            }
        }

        #endregion
    }
}