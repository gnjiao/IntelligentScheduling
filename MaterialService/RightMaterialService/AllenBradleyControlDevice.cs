﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeviceAsset;

namespace RightMaterialService
{
    public class AllenBradleyControlDevice : IControlDevice
    {

        private AllenBradleyDataConfig m_RHouseProductTypeConfig;
        private AllenBradleyDataConfig m_RHouseMaterialTypeConfig;
        private AllenBradleyDataConfig m_RHouseOutConfig;
        private AllenBradleyDataConfig m_RHouseInConfig;
        private AllenBradleyDataConfig m_RHouseRequestConfig;
        private AllenBradleyDataConfig m_RHouseFinConfig;
        private AllenBradleyDataConfig m_RHouseAlarmConfig;
        private AllenBradleyDataConfig m_RHouseResetConfig;

        private AllenBradleyDataConfig m_HouseRequestFCSConfig;
        private AllenBradleyDataConfig m_HouseRequestFCSFinConfig;
        private AllenBradleyDataConfig m_HouseRequestInfoConfig;
        private AllenBradleyDataConfig m_HouseRequestInfoFCSFinConfig;
        private AllenBradleyDataConfig m_HouseFCSAlarmConfig;
        private AllenBradleyDataConfig m_HouseFCSResetConfig;
        private AllenBradleyDataConfig m_HouseProductTypeConfig;
        private AllenBradleyDataConfig m_HouseMaterialTypeConfig;
        private AllenBradleyDataConfig m_HouseOutConfig;
        private AllenBradleyDataConfig m_HouseInConfig;
        private AllenBradleyDataConfig m_HouseProductPostionConfig;
        private AllenBradleyDataConfig m_HouseTrayPostionConfig;
        private AllenBradleyDataConfig m_HouseQuantityConfig;
        private AllenBradleyDataConfig m_HouseConfirmMaterialTypeConfig;
        private AllenBradleyDataConfig m_HouseTrayOutInpositionConfig;


        private AllenBradley m_ABDevice;

        public AllenBradleyControlDevice()
        {
            m_ABDevice = new AllenBradley("192.168.1.10", 44818);

            m_RHouseProductTypeConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_IN.Product_Type" };
            m_RHouseMaterialTypeConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_IN.Material_Type" };
            m_RHouseOutConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_OUT.Storage_Out" };
            m_RHouseInConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_OUT.Storage_In" };
            m_RHouseRequestConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_IN.Act_Req" };
            m_RHouseFinConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_OUT.Act_Finish" };
            m_RHouseAlarmConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_IN.Alarm" };
            m_RHouseResetConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_OUT.Reset" };
            //m_RHouseQuantityConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.AGV_Fix_Robot_IN.Current_Quantity_In_Box" };

            m_HouseRequestFCSConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Act_REQ" };
            m_HouseRequestFCSFinConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Act_Req_Over" };
            m_HouseRequestInfoConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.DoneInf_Write_Req" };
            m_HouseRequestInfoFCSFinConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Inf_Write_Req_Over" };
            m_HouseFCSAlarmConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Alarm" };
            m_HouseFCSResetConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Reset" };
            m_HouseProductTypeConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Product_Type" };
            m_HouseMaterialTypeConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Material_Type" };
            m_HouseOutConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Storage_Out" };
            m_HouseInConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Storage_In" };
            m_HouseProductPostionConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Product_Pos" };
            m_HouseTrayPostionConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Tray_Number" };
            m_HouseQuantityConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Current_Quantity_In_Box" };
            m_HouseConfirmMaterialTypeConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.SHORT, DataAdr = "PLC_MES_COMM.Fix_Robot_IN.Material_Type" };
            m_HouseTrayOutInpositionConfig = new AllenBradleyDataConfig { DataType = AllenBradleyDataTypeEnum.BOOL, DataAdr = "PLC_MES_COMM.Fix_Robot_OUT.Tray_Inposition" };
        }

        //public bool SetRHouseProductType(int prod_type)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseProductTypeConfig, prod_type.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool SetRHouseMaterialType(int material_type)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseMaterialTypeConfig, material_type.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool GetRHouseRequest(ref bool req)
        //{
        //    var ret = m_ABDevice.Read(m_RHouseRequestConfig);
        //    if (ret.IsSuccess == false) return false;

        //    bool temp = false;
        //    var pret = bool.TryParse(ret.Content, out temp);
        //    if (pret == false) return false;

        //    req = temp;
        //    return true;
        //}
        
        //public bool SetRHouseRequest(bool req)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseRequestConfig, req.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool GetRHouseFin(ref bool fin)
        //{
        //    var ret = m_ABDevice.Read(m_RHouseFinConfig);
        //    if (ret.IsSuccess == false) return false;

        //    bool temp = false;
        //    var pret = bool.TryParse(ret.Content, out temp);
        //    if (pret == false) return false;

        //    fin = temp;
        //    return true;
        //}

        //public bool SetRHouseFin(bool fin)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseFinConfig, fin.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool SetRHouseInOut(bool in_out)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseOutConfig, in_out == true ? "true" : "false");
        //    if (ret.IsSuccess == false) return false;

        //    ret = m_ABDevice.Write(m_RHouseOutConfig, in_out == true ? "false" : "true");
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool SetRHouseAlarm(bool alarm)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseAlarmConfig, alarm.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        //public bool GetRHouseReset(ref bool reset)
        //{
        //    var ret = m_ABDevice.Read(m_RHouseResetConfig);
        //    if (ret.IsSuccess == false) return false;

        //    bool temp = false;
        //    var pret = bool.TryParse(ret.Content, out temp);
        //    if (pret == false) return false;

        //    reset = temp;
        //    return true;
        //}

        //public bool SetRHouseReset(bool reset)
        //{
        //    var ret = m_ABDevice.Write(m_RHouseResetConfig, reset.ToString());
        //    if (ret.IsSuccess == false) return false;

        //    return true;
        //}

        #region 右侧料库数据服务
        public bool Temp_S_House_RequestFCS_Last { get; set; }

        public bool GetHouseRequestFCS(ref bool req)
        {
            var ret = m_ABDevice.Read(m_HouseRequestFCSConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            req = temp;
            return true;
        }

        public bool GetHouseRequestFCSFin(ref bool req_fin)
        {
            var ret = m_ABDevice.Read(m_HouseRequestFCSFinConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            req_fin = temp;
            return true;
        }

        public bool GetHouseRequestInfoFCS(ref bool req_info)
        {
            var ret = m_ABDevice.Read(m_HouseRequestInfoConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            req_info = temp;
            return true;
        }

        public bool GetHouseRequestInfoFCSFin(ref bool req_info_fin)
        {
            var ret = m_ABDevice.Read(m_HouseRequestInfoFCSFinConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            req_info_fin = temp;
            return true;
        }

        public bool GetHouseFCSAlarm(ref bool alarm)
        {
            var ret = m_ABDevice.Read(m_HouseFCSAlarmConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            alarm = temp;
            return true;
        }

        public bool GetHouseFCSReset(ref bool reset)
        {
            var ret = m_ABDevice.Read(m_HouseFCSResetConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            reset = temp;
            return true;
        }

        public bool GetHouseProductType(ref int product)
        {
            var ret = m_ABDevice.Read(m_HouseProductTypeConfig);
            if (ret.IsSuccess == false) return false;

            int temp = 0;
            var pret = int.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            product = temp;
            return true;
        }

        public bool GetHouseMaterialType(ref int material)
        {
            var ret = m_ABDevice.Read(m_HouseMaterialTypeConfig);
            if (ret.IsSuccess == false) return false;

            int temp = 0;
            var pret = int.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            material = temp;
            return true;
        }

        public bool GetHouseInOut(ref bool in_out)
        {
            var temp_in = false;
            var temp_out = false;

            var ret = m_ABDevice.Read(m_HouseInConfig);
            if (ret.IsSuccess == false) return false;

            var pret = bool.TryParse(ret.Content, out temp_in);
            if (pret == false) return false;

            ret = m_ABDevice.Read(m_HouseOutConfig);
            if (ret.IsSuccess == false) return false;

            pret = bool.TryParse(ret.Content, out temp_out);
            if (pret == false) return false;

            if (temp_in == temp_out)
            {
                return false;
            }

            in_out = temp_out == true ? true : false;

            return true;
        }

        public bool SetHouseProductPostion(int product_pos)
        {
            var ret = m_ABDevice.Write(m_HouseProductPostionConfig, product_pos.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseTrayPostion(int tray_pos)
        {
            var ret = m_ABDevice.Write(m_HouseTrayPostionConfig, tray_pos.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseQuantity(int quantity)
        {
            var ret = m_ABDevice.Write(m_HouseQuantityConfig, quantity.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseConfirmMaterialType(int type)
        {
            var ret = m_ABDevice.Write(m_HouseConfirmMaterialTypeConfig, type.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseFCSAlarm(bool alarm)
        {
            var ret = m_ABDevice.Write(m_HouseFCSAlarmConfig, alarm.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseFCSReset(bool alarm)
        {
            var ret = m_ABDevice.Write(m_HouseFCSResetConfig, alarm.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseRequestFCSFin(bool req_fin)
        {
            var ret = m_ABDevice.Write(m_HouseRequestFCSFinConfig, req_fin.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool SetHouseRequestInfoFCSFin(bool req_info_fin)
        {
            var ret = m_ABDevice.Write(m_HouseRequestInfoFCSFinConfig, req_info_fin.ToString());
            if (ret.IsSuccess == false) return false;

            return true;
        }

        public bool GetHouseTrayInposition(ref bool inposition)
        {
            var ret = m_ABDevice.Read(m_HouseTrayOutInpositionConfig);
            if (ret.IsSuccess == false) return false;

            bool temp = false;
            var pret = bool.TryParse(ret.Content, out temp);
            if (pret == false) return false;

            inposition = temp;
            return true;
        }

        #endregion
    }
}
