using System;
using Compal.FMS.Component;
using System.Reflection;
using Compal.FMS.Kernel.Beans;
using log4net;
using FMSCommon.Compal.FMS.Kernel.Operations;
using FMSCommon.Compal.FMS.Kernel.Utils;

namespace Compal.FMS.Kernel.Threading
{
    public class FileMonitor
    {
        //protected System.Timers.Timer monitorRunner;
        public SrvInfo tempSrvInfo;
        private ILog platformLog;//@JC03A
        private ILog mesLog = LogManager.GetLogger(FMSLog.PLATFORM);

        public FileMonitor(SrvInfo vSrvInfo)
        {
            this.tempSrvInfo = vSrvInfo;
        }

        ~FileMonitor()
        {
            this.Dispose();
        }

        public void Start()
        {
            try
            {
                //this.monitorRunner.Enabled = true;
                this.ScheduleService();
            }
            catch (Exception ex)
            {
                if (platformLog.IsErrorEnabled)
                {
                    FMSLOG.Platform(MethodBase.GetCurrentMethod().Name + "[" + tempSrvInfo.Operation + "]Thread Process Begin Exception. Msg: " + ex.Message, tempSrvInfo.Operation);
                    //platformLog.Error(ex.StackTrace);
                }
            }
        }


        //stop timer monitor
        public void Stop()
        {
            //this.monitorRunner.Enabled = false;
            FMSLOG.Platform("DB to SAP Post Data Stopped", tempSrvInfo.Operation);
            this.Schedular.Dispose();

        }
        private System.Threading.Timer Schedular;
        public void ScheduleService()
        {
            try
            {
                Schedular = new System.Threading.Timer(new System.Threading.TimerCallback(SchedularCallback));

                FMSLOG.Platform("DB to SAP Post Data Service Mode : " + tempSrvInfo.SyncType, tempSrvInfo.Operation);
                //Set the Default Time.
                DateTime scheduledTime = DateTime.MinValue;

                if (tempSrvInfo.SyncType == "Daily")
                {
                    //Get the Scheduled Time from AppSettings.
                    scheduledTime = DateTime.Parse(tempSrvInfo.Interval);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                }
                else if (tempSrvInfo.SyncType == "Interval")
                {
                    //Get the Interval in Minutes from AppSettings.
                    int intervalMinutes = Convert.ToInt32(tempSrvInfo.Interval);

                    //Set the Scheduled Time by adding the Interval to Current Time.
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next Interval.
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }
                else if (tempSrvInfo.SyncType == "Monthly")
                {
                    scheduledTime = DateTime.Parse(tempSrvInfo.Interval);
                    if (DateTime.Now > scheduledTime)
                    {
                        //If Scheduled Time is passed set Schedule for the next day.
                        scheduledTime = scheduledTime.AddMonths(1);
                    }
                }
                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minute(s) {3} seconds(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                FMSLOG.Platform("Sync Schedule will Run After: " + schedule, tempSrvInfo.Operation);
                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);

                //Change the Timer's Due Time.
                Schedular.Change(dueTime, System.Threading.Timeout.Infinite);
            }
            catch (Exception ex)
            {
                FMSLOG.Platform("ERP to MES Data Copy Service Error" + ex.Message, tempSrvInfo.Operation);
            }
        }

        //dispose instance monitor
        public void Dispose()
        {
            this.tempSrvInfo = null;
            this.platformLog = null;
            //this.monitorRunner = null;
            GC.Collect();
        }


        // timer monitor event.
        private void SchedularCallback(object e)
        {
            try
           {
                if (tempSrvInfo.SyncType == "Daily")
                {
                    tempSrvInfo.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                }
                else if (tempSrvInfo.SyncType == "Interval")
                {
                    int interval = Convert.ToInt32(tempSrvInfo.Interval) + 10;
                    tempSrvInfo.StartDate = DateTime.Now.AddMinutes(-interval).ToString("yyyy-MM-dd HH:mm:ss");
                    tempSrvInfo.EndDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (tempSrvInfo.SyncType == "Monthly")
                {
                    tempSrvInfo.StartDate = DateTime.Now.ToString("yyyy-MM-dd"); 
                }
                Run_AEQS_Pivot88_Operations rAEQS = new Run_AEQS_Pivot88_Operations();
                Run_AEQS_Middle_Operations rAEQS_Middle = new Run_AEQS_Middle_Operations();
                Run_TempAndHumid_Data_Transfer TempandHumid = new Run_TempAndHumid_Data_Transfer();
                Run_Emp_Attandance_Transfer emp_attandance = new Run_Emp_Attandance_Transfer();
                Run_ManDay_Input_Data manday = new Run_ManDay_Input_Data();
                Not_Sync_PO_Alerts po_alerts = new Not_Sync_PO_Alerts();
                Run_TSM_Operations tsm = new Run_TSM_Operations();
                Run_KPI_Data_Calc_Operations kpi = new Run_KPI_Data_Calc_Operations();
                Digital_Board DBoard = new Digital_Board();
                Run_AEQS_Operations aeqs = new Run_AEQS_Operations();
                DelayPOS etoe = new DelayPOS();
                Run_MES_Operations mes = new Run_MES_Operations();
                Lack_Of_Material lom = new Lack_Of_Material();
                WH7000 wh = new WH7000();

                if (tempSrvInfo.Operation == "AQL Outbound")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (today.TimeOfDay >= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)) && today.TimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(45)))
                        {
                            rAEQS.PostRequestAsync_AQL(tempSrvInfo);
                        }
                    }
                }

                else if (tempSrvInfo.Operation == "Inline")
                    rAEQS.PostRequestAsync_Inline(tempSrvInfo);
                else if (tempSrvInfo.Operation == "EndOfLine")
                    rAEQS.PostRequestAsync_EndOfLine(tempSrvInfo);
                else if (tempSrvInfo.Operation == "EndOfLine_Rework")
                    rAEQS.PostRequestAsync_EndOfLine_ReWork(tempSrvInfo);
                else if (tempSrvInfo.Operation == "TQC_Middle")
                    rAEQS_Middle.PostRequestAsync_TQC_Middle(tempSrvInfo);
                else if (tempSrvInfo.Operation == "TQC_Rework_Middle")
                    rAEQS_Middle.PostRequestAsync_TQCRework_Middle(tempSrvInfo);
                else if (tempSrvInfo.Operation == "TempandHumid")
                    TempandHumid.TempAndHumid_Data_Transfer(tempSrvInfo);
                else if (tempSrvInfo.Operation == "TempandHumidRangeExceedAlert")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (today.TimeOfDay >= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)) && today.TimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
                        {
                            TempandHumid.TempAndHumid_RangeExceedAlert(tempSrvInfo);
                        }
                    }
                }

                else if (tempSrvInfo.Operation == "New_Emp_Data_Transfer")
                    emp_attandance.New_Emp_Data_Transfer(tempSrvInfo);
                else if (tempSrvInfo.Operation == "Missing_Manday")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        manday.CheckForMissingInput(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "PO_Count_Assembly")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        mes.Plant_PO_Count_Assembly(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "PO_Count_Stitching")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        mes.Plant_PO_Count_Stitching(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "Not_Sync_PO_Alerts")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (today.TimeOfDay >= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)) && today.TimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(45)))
                        {
                            po_alerts.Send_Not_Sync_PO_List(tempSrvInfo);
                        }

                    }
                }
                else if (tempSrvInfo.Operation == "Data_Lock")
                    kpi.KPI_Data_Lock(tempSrvInfo);
                else if (tempSrvInfo.Operation == "TSM_Reg_Status_Update")
                    tsm.Update_TSM_Registration_Status(tempSrvInfo);
                else if (tempSrvInfo.Operation == "BGrade_Download")
                    kpi.Get_BGrade_DataAsync(tempSrvInfo);
                else if (tempSrvInfo.Operation == "daily_KPI")
                    kpi.Daily_KPI_Calculation(tempSrvInfo);
                else if (tempSrvInfo.Operation == "Daily_IE")
                    kpi.Daily_IE_Calculation(tempSrvInfo);
                else if (tempSrvInfo.Operation == "C2B_C2S")
                    kpi.C2B_C2S_IE_Calculation(tempSrvInfo);
                else if (tempSrvInfo.Operation == "KPI_Alerts")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        kpi.Send_KPI_DataEntry_Alerts(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "Digital_Board")
                {
                    DateTime today = DateTime.Now;
                    TimeSpan startTime = new TimeSpan(8, 30, 0);
                    TimeSpan endTime = new TimeSpan(19, 15, 0);
                    if (today.DayOfWeek != DayOfWeek.Sunday && today.TimeOfDay >= startTime && today.TimeOfDay <= endTime)
                    {
                        DBoard.Digital_board_Data_Sync(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "MES_To_AEQS_CompareData")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        aeqs.MES_To_AEQS_CompareData(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "Quality_Bonus")
                {
                    aeqs.Calculate_Quality_Bonus(tempSrvInfo);
                }
                else if (tempSrvInfo.Operation == "Line_RFT")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        aeqs.Send_Line_RFT(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "FGT_Digitalization_Report")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        aeqs.FGT_Digitalization_Report(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "BGrade_Report")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        mes.Send_BGrade_Report(tempSrvInfo);
                    }
                }

                else if (tempSrvInfo.Operation == "Unfinished_POs_List")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        mes.Send_Unfinished_POs_List(tempSrvInfo);
                    }
                }

                else if (tempSrvInfo.Operation == "Delay_PO")
                {
                    etoe.GetDelayPOSMethods(tempSrvInfo);
                }
                else if (tempSrvInfo.Operation == "Auto_Schedule_Report")
                {
                    mes.Send_Auto_Schedule_Insert_Report(tempSrvInfo);
                }
                else if (tempSrvInfo.Operation == "LackofMaterial")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        lom.GetLackofMaterialPos(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "AdvanceAbesntReport")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        tsm.Send_Employee_Absent_Report(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "EmployeeExcessReport")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        tsm.Send_Employee_Excess_Report(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "AQL_PO_Download")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        aeqs.Insert_Walmart_POS_Data(tempSrvInfo);
                    }
                }

                else if (tempSrvInfo.Operation == "AQL_Inspection_Result")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (today.TimeOfDay >= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)) && today.TimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
                        {
                            aeqs.Send_AQL_Inspection_Alert(tempSrvInfo);
                        }
                    }
                }
                else if (tempSrvInfo.Operation == "Supplementary_Report")
                {
                    mes.Get_SupplementaryData_From_ClientAPIAsync(tempSrvInfo);
                }
                else if (tempSrvInfo.Operation == "Daily_Attendance_Report")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        tsm.Send_Daily_Attendance_Report(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "WH7000")
                {
                    DateTime today = DateTime.Now;
                    if (today.DayOfWeek != DayOfWeek.Sunday)
                    {
                        wh.GetWH7000Materials(tempSrvInfo);
                    }
                }
                else if (tempSrvInfo.Operation == "AQL_PO_Receive_Alert")
                {
                   aeqs.Send_AQL_PO_Receive_Alert(tempSrvInfo);
                }

            }
            catch (Exception ex)
            {
                FMSLOG.Platform("Sync Error" + ex.Message, tempSrvInfo.Operation);
            }
            finally
            {
                this.ScheduleService();
            }

        }


    }
}