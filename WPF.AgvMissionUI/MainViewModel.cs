﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Agv.Common;
using AgvMissionManager;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;

namespace WPF.AgvMissionUI
{

    public class InMissionItem : ViewModelBase
    {
        private string _Id;
        public string Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    RaisePropertyChanged(() => _Id);
                }
            }
        }

        private string _TimeId;
        public string TimeId
        {
            get { return _TimeId; }
            set
            {
                if (_TimeId != value)
                {
                    _TimeId = value;
                    RaisePropertyChanged(() => TimeId);
                }
            }
        }

        private AgvStationEnum _ClientId;
        public AgvStationEnum ClientId
        {
            get { return _ClientId; }
            set
            {
                if (_ClientId != value)
                {
                    _ClientId = value;
                    RaisePropertyChanged(() => ClientId);
                }
            }
        }
        
        private AgvMissionTypeEnum _Type;
        public AgvMissionTypeEnum Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    _Type = value;
                    RaisePropertyChanged(() => Type);
                }
            }
        }
        
        private AgvInMissonProcessEnum _Process;
        public AgvInMissonProcessEnum Process
        {
            get { return _Process; }
            set
            {
                if (_Process != value)
                {
                    _Process = value;
                    RaisePropertyChanged(() => Process);
                }
            }
        }

        private DateTime _CreateDateTime;
        public DateTime CreateDateTime
        {
            get { return _CreateDateTime; }
            set
            {
                if (_CreateDateTime != value)
                {
                    _CreateDateTime = value;
                    RaisePropertyChanged(() => CreateDateTime);
                }
            }
        }

    }

    public class OutMissionItem : ViewModelBase
    {
        private string _Id;
        public string Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;
                    RaisePropertyChanged(() => _Id);
                }
            }
        }

        private AgvStationEnum _ClientId;
        public AgvStationEnum ClientId
        {
            get { return _ClientId; }
            set
            {
                if (_ClientId != value)
                {
                    _ClientId = value;
                    RaisePropertyChanged(() => ClientId);
                }
            }
        }

        private AgvMissionTypeEnum _Type;
        public AgvMissionTypeEnum Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    _Type = value;
                    RaisePropertyChanged(() => Type);
                }
            }
        }

        private AgvOutMissonProcessEnum _Process;
        public AgvOutMissonProcessEnum Process
        {
            get { return _Process; }
            set
            {
                if (_Process != value)
                {
                    _Process = value;
                    RaisePropertyChanged(() => Process);
                }
            }
        }

        private DateTime _CreateDateTime;
        public DateTime CreateDateTime
        {
            get { return _CreateDateTime; }
            set
            {
                if (_CreateDateTime != value)
                {
                    _CreateDateTime = value;
                    RaisePropertyChanged(() => CreateDateTime);
                }
            }
        }

    }

    public class MainViewModel : ViewModelBase
    {
        BaseAgvMissionService agvMissionSrv = new BaseAgvMissionService();

        private ObservableCollection<InMissionItem> _InMissions = new ObservableCollection<InMissionItem>();
        public ObservableCollection<InMissionItem> InMissions
        {
            get { return _InMissions; }
            set
            {
                if (_InMissions != value)
                {
                    _InMissions = value;
                    //RaisePropertyChanged(() => InMissions);
                }
            }
        }

        private ObservableCollection<OutMissionItem> _OutMissions = new ObservableCollection<OutMissionItem>();
        public ObservableCollection<OutMissionItem> OutMissions
        {
            get { return _OutMissions; }
            set
            {
                if (_OutMissions != value)
                {
                    _OutMissions = value;
                    //RaisePropertyChanged(() => OutMissions);
                }
            }
        }

        public ICommand Start_Command { get; set; }

        private BackgroundWorker m_static_BackgroundWorker = new BackgroundWorker();

        public MainViewModel()
        {
            Start_Command = new RelayCommand(OnStart_Command);

            m_static_BackgroundWorker.WorkerReportsProgress = false;
            m_static_BackgroundWorker.WorkerSupportsCancellation = true;
            m_static_BackgroundWorker.DoWork += ScannerStaticFunc;
            m_static_BackgroundWorker.RunWorkerAsync();


        }

        private void OnStart_Command()
        {
            agvMissionSrv.Start();
        }

        private void ScannerStaticFunc(object sender, DoWorkEventArgs e)
        {
            while (m_static_BackgroundWorker.CancellationPending == false)
            {

                try
                {
                    foreach (var mission in agvMissionSrv.InMissions)
                    {
                        var item = InMissions.Where(x => x.Id == mission.Id && x.Process != AgvInMissonProcessEnum.CLOSE).FirstOrDefault();
                        if (item != null)
                        {
                            item.Process = mission.Process;
                            item.CreateDateTime = mission.CreateDateTime;
                        }
                        else
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                InMissions.Add(new InMissionItem
                                {
                                    Id = mission.Id,
                                    ClientId = mission.ClientId,
                                    Type = mission.Type,
                                    Process = mission.Process,
                                    CreateDateTime = mission.CreateDateTime,
                                });
                            });
                        }
                    }

                    foreach (var mission in agvMissionSrv.OutMissions)
                    {
                        var item = OutMissions.Where(x => x.Id == mission.Id && x.Process != AgvOutMissonProcessEnum.CLOSE).FirstOrDefault();
                        if (item != null)
                        {
                            item.Process = mission.Process;
                            item.CreateDateTime = mission.CreateDateTime;
                        }
                        else
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                OutMissions.Add(new OutMissionItem
                                {
                                    Id = mission.Id,
                                    ClientId = mission.ClientId,
                                    Type = mission.Type,
                                    Process = mission.Process,
                                    CreateDateTime = mission.CreateDateTime,
                                });
                            });
                        }
                    }
                }
                catch { }

                

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}