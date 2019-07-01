﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceAsset;
using RightCarryService;

namespace AgvMissionManager
{
    public class BaseAgvMissionService
    {
        private AgvMissionServiceErrorCodeEnum cur_Display_ErrorCode;
        private string cur_Display_Message;
        
        private static BaseAgvMissionService _instance = null;
        private CancellationTokenSource token = new CancellationTokenSource();

        Dictionary<AgvMissionTypeEnum, AgvMissionTypeEnum> brotherMissionType = new Dictionary<AgvMissionTypeEnum, AgvMissionTypeEnum>();

        public event Action<AgvMissionServiceState> SendAgvMissionServiceStateMessageEvent;

        #region 任务
        private List<AgvOutMisson> OutMissions { get; set; }
        private List<AgvInMisson> InMissions { get; set; }
        
        #endregion

        #region ctor
        public static BaseAgvMissionService CreateInstance()
        {
            if (_instance == null)

            {
                _instance = new BaseAgvMissionService();
            }
            return _instance;
        }

        public BaseAgvMissionService()
        {
            OutMissions = new List<AgvOutMisson>();
            InMissions = new List<AgvInMisson>();
            
            //关联任务
            brotherMissionType.Add(AgvMissionTypeEnum.RAW_IN, AgvMissionTypeEnum.EMPTY_OUT);
            brotherMissionType.Add(AgvMissionTypeEnum.EMPTY_IN, AgvMissionTypeEnum.FIN_OUT);

            #region 初始化任务

            OutMissions.Clear();
            InMissions.Clear();

            #endregion
        }

        #endregion

        #region 外部接口
        public void PushOutMission(AgvOutMisson mission)
        {
            if (OutMissions.Where(x => x.Id == mission.Id).Count() == 0)
            {
                OutMissions.Add(mission);
            }
        }

        public void PushInMission(AgvInMisson mission)
        {
            if (InMissions.Where(x => x.Id == mission.Id).Count() == 0)
            {
                InMissions.Add(mission);
            }
        }

#pragma warning disable CS0067 // 从不使用事件“BaseAgvMissionService.UpdateAgvInMissonEvent”
        public event Action<AgvInMisson> UpdateAgvInMissonEvent;
#pragma warning restore CS0067 // 从不使用事件“BaseAgvMissionService.UpdateAgvInMissonEvent”

#pragma warning disable CS0067 // 从不使用事件“BaseAgvMissionService.UpdateAgvOutMissonEvent”
        public event Action<AgvOutMisson> UpdateAgvOutMissonEvent;
#pragma warning restore CS0067 // 从不使用事件“BaseAgvMissionService.UpdateAgvOutMissonEvent”

        #endregion

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region 防冲突
                    var ret = CheckMissionConflict();
                    if (ret.Item1 == false)
                    {
                        while (ret.Item1 == false)
                        {
                            //TODO:停止所有AGV小车动作


                            //设定报警
                            var ret_alarm = SetAlarm(true);
                            //TODO:通知界面
                            SendAgvMissionServiceStateMessage(
                                new AgvMissionServiceState { State = AgvMissionServiceStateEnum.ERROR, Message = "物料调用失败,发送错误信息至设备!",ErrorCode= ret.Item2 });
                            Thread.Sleep(1000);
                        }

                        bool dev_reset = false;
                        while (dev_reset == false)
                        {
                            GetReset(ref dev_reset);
                            //TODO:通知界面
                            SendAgvMissionServiceStateMessage(
                                new AgvMissionServiceState { State = AgvMissionServiceStateEnum.WARN, Message = "物料调用失败,请复位设备!", ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL });
                            Thread.Sleep(1000);
                        }

                        SendAgvMissionServiceStateMessage(
                            new AgvMissionServiceState { State = AgvMissionServiceStateEnum.WARN, Message = "物料调用失败,设备已复位!", ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL });

                    }

                    #endregion

                    var undo_inmissions = InMissions
                        .Where(x => x.Process > AgvInMissonProcessEnum.NEW && x.Process < AgvInMissonProcessEnum.CLOSE);
                    var undo_outmissions = OutMissions
                        .Where(x => x.Process > AgvOutMissonProcessEnum.NEW && x.Process < AgvOutMissonProcessEnum.CLOSE);

                    #region 小于两个在执行任务 添加一个入库任务
                    if ((undo_inmissions.Count() + undo_outmissions.Count()) < 2)
                    {
                        var new_inmission = InMissions.Where(x => x.Process == AgvInMissonProcessEnum.NEW).FirstOrDefault();
                        if (new_inmission != null) new_inmission.Process = AgvInMissonProcessEnum.START;
                    }

                    #endregion

                    #region 小于两个在执行任务 添加一个出库任务
                    if ((undo_inmissions.Count() + undo_outmissions.Count()) < 2)
                    {
                        var new_outmission = OutMissions.Where(x => x.Process == AgvOutMissonProcessEnum.NEW).FirstOrDefault();
                        if (new_outmission != null)
                        {
                            //检索出库任务之前是否有前置的入库任务
                            var brother_mission_type = brotherMissionType[new_outmission.Type];
                            var brother_inmission = OutMissions
                                .Where(x => x.ClientId == new_outmission.ClientId
                                    && x.Type == brother_mission_type
                                    && x.Process == AgvOutMissonProcessEnum.NEW)
                                .FirstOrDefault();

                            if (brother_inmission != null)
                            {
                                new_outmission.Process = AgvOutMissonProcessEnum.START;
                            }
                            
                        }
                    }

                    #endregion

                    #region 通知料库入库动作
                    var warehouse_inmission = undo_inmissions.Where(x => x.Process == AgvInMissonProcessEnum.AGVPLACEDANDLEAVE).FirstOrDefault();
                    if (warehouse_inmission != null)
                    {
                        warehouse_inmission.Process = AgvInMissonProcessEnum.WHSTART;

                        Task.Factory.StartNew(() =>
                        {
                            var ret_whinmission = WareHouseInMission(warehouse_inmission);
                            if (ret_whinmission == false)
                            {
                                warehouse_inmission.Process = AgvInMissonProcessEnum.CANCEL;

                                while (ret_whinmission == false)
                                {
                                    ret_whinmission = SetAlarm(true);
                                    SendAgvMissionServiceStateMessage(
                                        new AgvMissionServiceState {
                                            State = AgvMissionServiceStateEnum.ERROR,
                                            Message = "物料调用失败,料库入库动作错误!",
                                            ErrorCode = AgvMissionServiceErrorCodeEnum.WAREHOUSEIN
                                        });
                                    Thread.Sleep(1000);
                                }

                                bool dev_reset = false;
                                while (dev_reset == false)
                                {
                                    GetReset(ref dev_reset);
                                    SendAgvMissionServiceStateMessage(
                                        new AgvMissionServiceState{
                                            State = AgvMissionServiceStateEnum.WARN,
                                            Message = "物料调用失败,请复位设备!",
                                            ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                    });
                                    Thread.Sleep(1000);
                                }

                                SendAgvMissionServiceStateMessage(
                                    new AgvMissionServiceState
                                    {
                                        State = AgvMissionServiceStateEnum.WARN,
                                        Message = "物料调用失败,设备已复位!",
                                        ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                    });
                            }
                        });

                    }

                    #endregion

                    #region 通知料库出库动作
                    var warehouse_outmission = undo_outmissions.Where(x => x.Process == AgvOutMissonProcessEnum.START).FirstOrDefault();
                    if (warehouse_outmission != null)
                    {
                        warehouse_outmission.Process = AgvOutMissonProcessEnum.WHSTART;

                        Task.Factory.StartNew(() =>
                        {
                            var ret_whoutmission = WareHouseOutMission(warehouse_outmission);

                            if (ret_whoutmission == false)
                            {
                                warehouse_outmission.Process = AgvOutMissonProcessEnum.CANCEL;

                                while (ret_whoutmission == false)
                                {
                                    ret_whoutmission = SetAlarm(true);
                                    SendAgvMissionServiceStateMessage(
                                          new AgvMissionServiceState
                                          {
                                              State = AgvMissionServiceStateEnum.ERROR,
                                              Message = "物料调用失败,料库出库动作错误!",
                                              ErrorCode = AgvMissionServiceErrorCodeEnum.WAREHOUSEOUT
                                          });
                                    Thread.Sleep(1000);
                                }

                                bool dev_reset = false;
                                while (dev_reset == false)
                                {
                                    GetReset(ref dev_reset);
                                    SendAgvMissionServiceStateMessage(
                                        new AgvMissionServiceState
                                        {
                                            State = AgvMissionServiceStateEnum.WARN,
                                            Message = "物料调用失败,请复位设备!",
                                            ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                        });
                                    Thread.Sleep(1000);
                                }

                                SendAgvMissionServiceStateMessage(
                                    new AgvMissionServiceState
                                    {
                                        State = AgvMissionServiceStateEnum.WARN,
                                        Message = "物料调用失败,设备已复位!",
                                        ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                    });
                            }
                        });

                    }

                    #endregion

                    #region 发布小车搬运入库任务
                    var agv_inmission = undo_inmissions.Where(x => x.Process == AgvInMissonProcessEnum.START).FirstOrDefault();
                    if (agv_inmission != null)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            var ret_agvinmission =await AgvInMission(agv_inmission);
                            if (ret_agvinmission == false)
                            {
                                agv_inmission.Process = AgvInMissonProcessEnum.CANCEL;

                                while (ret_agvinmission == false)
                                {
                                    ret_agvinmission = SetAlarm(true);
                                    SendAgvMissionServiceStateMessage(
                                          new AgvMissionServiceState
                                          {
                                              State = AgvMissionServiceStateEnum.ERROR,
                                              Message = "物料调用失败,小车搬运入库错误!",
                                              ErrorCode = AgvMissionServiceErrorCodeEnum.WAREHOUSEOUT
                                          });
                                    Thread.Sleep(1000);
                                }

                                bool dev_reset = false;
                                while (dev_reset == false)
                                {
                                    GetReset(ref dev_reset);
                                    SendAgvMissionServiceStateMessage(
                                        new AgvMissionServiceState
                                        {
                                            State = AgvMissionServiceStateEnum.WARN,
                                            Message = "物料调用失败,请复位设备!",
                                            ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                        });
                                    Thread.Sleep(1000);
                                }

                                SendAgvMissionServiceStateMessage(
                                    new AgvMissionServiceState
                                    {
                                        State = AgvMissionServiceStateEnum.WARN,
                                        Message = "物料调用失败,设备已复位!",
                                        ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                    });
                            }
                        });
                    }

                    #endregion

                    #region 发布小车搬运出库任务
                    var agv_outmission = undo_outmissions.Where(x => x.Process == AgvOutMissonProcessEnum.START).FirstOrDefault();
                    if (agv_outmission != null)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            var ret_agvoutmission = await AgvOutMission(agv_outmission);
                            if (ret_agvoutmission == false)
                            {
                                agv_outmission.Process = AgvOutMissonProcessEnum.CANCEL;

                                while (ret_agvoutmission == false)
                                {
                                    ret_agvoutmission = SetAlarm(true);
                                    SendAgvMissionServiceStateMessage(
                                          new AgvMissionServiceState
                                          {
                                              State = AgvMissionServiceStateEnum.ERROR,
                                              Message = "物料调用失败,小车搬运出库错误!",
                                              ErrorCode = AgvMissionServiceErrorCodeEnum.WAREHOUSEOUT
                                          });
                                    Thread.Sleep(1000);
                                }

                                bool dev_reset = false;
                                while (dev_reset == false)
                                {
                                    GetReset(ref dev_reset);
                                    SendAgvMissionServiceStateMessage(
                                        new AgvMissionServiceState
                                        {
                                            State = AgvMissionServiceStateEnum.WARN,
                                            Message = "物料调用失败,请复位设备!",
                                            ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                        });
                                    Thread.Sleep(1000);
                                }

                                SendAgvMissionServiceStateMessage(
                                    new AgvMissionServiceState
                                    {
                                        State = AgvMissionServiceStateEnum.WARN,
                                        Message = "物料调用失败,设备已复位!",
                                        ErrorCode = AgvMissionServiceErrorCodeEnum.NORMAL
                                    });
                            }
                        });
                    }

                    #endregion

                    #region 入库完工处理
                    var finished_inmission = InMissions.Where(x => x.Process == AgvInMissonProcessEnum.FINISHED).FirstOrDefault();
                    if (finished_inmission != null)
                    {
                        finished_inmission.Process = AgvInMissonProcessEnum.CLOSE;
                    }

                    #endregion

                    #region 出库完工处理
                    var finished_outmission = OutMissions.Where(x => x.Process == AgvOutMissonProcessEnum.FINISHED).FirstOrDefault();
                    if (finished_outmission != null)
                    {
                        finished_outmission.Process = AgvOutMissonProcessEnum.CLOSE;
                    }

                    #endregion

                    #region 入库异常处理
                    var cancel_inmission = InMissions.Where(x => x.Process == AgvInMissonProcessEnum.CANCEL).FirstOrDefault();
                    if (cancel_inmission != null)
                    {
                        cancel_inmission.Process = AgvInMissonProcessEnum.CANCELED;
                        AgvInMissionCancel(cancel_inmission);
                    }

                    #endregion

                    #region 出库异常处理
                    var cancel_outmission = OutMissions.Where(x => x.Process == AgvOutMissonProcessEnum.CANCELED).FirstOrDefault();
                    if (cancel_outmission != null)
                    {
                        cancel_outmission.Process = AgvOutMissonProcessEnum.CANCELED;
                        AgvOutMissionCancel(cancel_outmission);
                    }

                    #endregion
                }
            }, token.Token);
        }

        //TODO:
        public void Stop()
        {

        }

        //TODO:
        public void Suspend()
        {

        }

        public bool CloseOutMission(string missionId)
        {
            var finished_outmission = OutMissions.Where(x => x.Id==missionId && x.Process == AgvOutMissonProcessEnum.FINISHED).FirstOrDefault();
            if (finished_outmission != null)
            {
                finished_outmission.Process = AgvOutMissonProcessEnum.CLOSE;
            }

            return true;
        }

        public bool CloseInMission(string missionId)
        {
            var finished_inmission = InMissions.Where(x => x.Id == missionId && x.Process == AgvInMissonProcessEnum.FINISHED).FirstOrDefault();
            if (finished_inmission != null)
            {
                finished_inmission.Process = AgvInMissonProcessEnum.CLOSE;
            }

            return true;
        }

        //TODO:小车入库任务取消
        private bool AgvInMissionCancel(AgvInMisson mission)
        {
            //TODO:添加异常处理

            mission.Process = AgvInMissonProcessEnum.CLOSE;
            return true;
        }

        //TODO:小车出库任务取消
        private bool AgvOutMissionCancel(AgvOutMisson mission)
        {
            //TODO:添加异常处理

            mission.Process = AgvOutMissonProcessEnum.CLOSE;
            return true;
        }
        

        private void SendAgvMissionServiceStateMessage(AgvMissionServiceState state)
        {
            if (cur_Display_ErrorCode != state.ErrorCode || state.Message != cur_Display_Message)
            {
                SendAgvMissionServiceStateMessageEvent?.Invoke(state);

                cur_Display_ErrorCode = state.ErrorCode;
                cur_Display_Message = state.Message;
            }

            if (cur_Display_ErrorCode != state.ErrorCode)
            {
                Console.WriteLine($"【RIGHT MATERIAL MISSION】【ERROR CODE】: {state.ErrorCode}     【MESSAGE】:{state.Message}");

                cur_Display_ErrorCode = state.ErrorCode;
            }
        }

        private void UpdataAgvInMisson()
        {

        }

        private void AgvOrderStateEvent()
        {

        }

        private Tuple<bool, AgvMissionServiceErrorCodeEnum> CheckMissionConflict()
        {
            var undo_inmissions = InMissions
                .Where(x => x.Process > AgvInMissonProcessEnum.NEW && x.Process < AgvInMissonProcessEnum.CLOSE);
            var undo_outmissions = OutMissions
                .Where(x => x.Process > AgvOutMissonProcessEnum.NEW && x.Process < AgvOutMissonProcessEnum.CLOSE);

            //存在相同ID的任务
            var max_inmission = undo_inmissions.GroupBy(x => x.Id).Select(x => new { num = x.Count() }).Max() ?? new { num = 0 };
            var max_outmission = undo_outmissions.GroupBy(x => x.Id).Select(x => new { num = x.Count() }).Max() ?? new { num = 0 };
            if (max_inmission.num > 1 || max_outmission.num > 1)
            {
                return new Tuple<bool, AgvMissionServiceErrorCodeEnum>
                    (false, AgvMissionServiceErrorCodeEnum.IDCONFLICT);
            }

            //多于2个任务正在执行
            var count_inmissions = undo_inmissions.Count();
            var count_outmissions = undo_outmissions.Count();
            if (count_inmissions + count_outmissions > 2)
            {
                return new Tuple<bool, AgvMissionServiceErrorCodeEnum>
                    (false, AgvMissionServiceErrorCodeEnum.QUANTITYLIMIT);
            }

            //存在两个出料库任务，已经达到AGVSTART， 但是小于AGVLEAVEPICK
            //var count_outmission_conflict = undo_outmissions
            //    .Where(x => x.Process >= AgvOutMissonProcessEnum.AGVSTART && x.Process < AgvOutMissonProcessEnum.AGVLEAVEPICK).Count();
            //if (count_outmission_conflict > 1)
            //{
            //    return new Tuple<bool, AgvMissionServiceErrorCodeEnum>
            //        (false, AgvMissionServiceErrorCodeEnum.IDCONFLICT);
            //}

            //        //只能有一个入料库任务
            //        var count_inmission_conflict = undo_inmissions.Count();
            //        if (count_inmission_conflict > 1)
            //        {
            //            return new Tuple<bool, AgvMissionServiceErrorCodeEnum>
            //(false, AgvMissionServiceErrorCodeEnum.IDCONFLICT);
            //}

            return new Tuple<bool, AgvMissionServiceErrorCodeEnum>
                (true, AgvMissionServiceErrorCodeEnum.NORMAL);
        }

        //TODO:发送入库任务给小车调度中心
        private void SendInMissionToAgvRoboRoute(AgvInMisson mission)
        {


        }

        //TODO:发送错误消息给小车任务管理系统
        private bool SetAlarm(bool alarm)
        {
            return true;
        }

        //TODO:从小车任务管理系统获得复位信号
        private bool GetReset(ref bool reset)
        {
            return true;
        }

        //料库执行入库
        private bool WareHouseInMission(AgvInMisson mission)
        {
            TestRightCarryService carry = new TestRightCarryService();
            
            var ret = carry.CarryIn(mission.ProductId, mission.MaterialId);

            if (ret == false)
            {
                mission.Process = AgvInMissonProcessEnum.CANCEL;
                return false;
            }

            mission.Process = AgvInMissonProcessEnum.FINISHED;
            return true;
        }

        //料库执行出库
        private bool WareHouseOutMission(AgvOutMisson mission)
        {

            TestRightCarryService carry = new TestRightCarryService();

            int quantity = 0;
            var ret = carry.CarryOut(mission.ProductId,mission.MaterialId,ref quantity);

            if(ret==false)
            {
                mission.Process = AgvOutMissonProcessEnum.CANCEL;
                return false;
            }

            mission.Process = AgvOutMissonProcessEnum.WHPICKED;
            return true;
        }
        
        //TODO:异步小车入库搬运
        private async Task<bool> AgvInMission(AgvInMisson mission)
        {
            var agv_order_id = mission.Id + "_" + mission.TimeId;
            IClient client = new Client();
            await client.TransportOrders2Async(agv_order_id, new TransportOrder()
            {
                Deadline = DateTime.Now.AddMinutes(20),
                Destinations = new List<DestinationOrder>()
                {
                    new DestinationOrder(){ LocationName=mission.PickStationId,Operation="JackLoad"}
                }
            });

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(2000);

                    var order = await client.TransportOrdersAsync(agv_order_id);
                    if (order.State == TransportOrderStateState.FINISHED)
                    {
                        mission.Process = AgvInMissonProcessEnum.FINISHED;
                        break;
                    }
                    else if (order.State == TransportOrderStateState.WITHDRAWN || order.State == TransportOrderStateState.FAILED)
                    {
                        mission.Process = AgvInMissonProcessEnum.CANCEL;
                        break;
                    }
                }
            });
            return true;
        }

        //TODO:异步小车出库搬运
#pragma warning disable CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        private async Task<bool> AgvOutMission(AgvOutMisson mission)
#pragma warning restore CS1998 // 此异步方法缺少 "await" 运算符，将以同步方式运行。请考虑使用 "await" 运算符等待非阻止的 API 调用，或者使用 "await Task.Run(...)" 在后台线程上执行占用大量 CPU 的工作。
        {
            return true;
        }


    }
}
