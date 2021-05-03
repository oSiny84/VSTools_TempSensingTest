using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eGBI_TempSensingTest
{
    public class TempSensorSpec
    {
        public double startTempValue;
        public double startVntc;
        public double endTempValue;
        public double endVntc;
    };

    public class Sensor
    {
        TempSensorSpec[] tableHeatSinkNTC = new TempSensorSpec[4];
        TempSensorSpec[] tableFF600RNTC  = new TempSensorSpec[4];

        int TEMP_CALC_DIV_END = 3;

        public  Sensor()
        {
            tableHeatSinkNTC[0] = new TempSensorSpec();
            tableHeatSinkNTC[1] = new TempSensorSpec();
            tableHeatSinkNTC[2] = new TempSensorSpec();
            tableHeatSinkNTC[3] = new TempSensorSpec();
            tableFF600RNTC[0] = new TempSensorSpec();
            tableFF600RNTC[1] = new TempSensorSpec();
            tableFF600RNTC[2] = new TempSensorSpec();
            tableFF600RNTC[3] = new TempSensorSpec();


            writeHeatSpec(0, 10.0, 3.001048, 32.0, 2.629285);
            writeHeatSpec(1, 33, 2.607912, 55, 2.057949);
            writeHeatSpec(2, 56, 2.041439, 78, 1.46984);
            writeHeatSpec(3, 79, 1.445537, 100, 0.995552);

            writeFFSpec(0, 50, 3.136607, 71, 2.130837);
            writeFFSpec(1, 72, 2.08965, 93, 1.373112);
            writeFFSpec(2, 94, 1.345658, 115, 0.88406);
            writeFFSpec(3, 116, 0.866843, 135, 0.601933);
        }

        void writeHeatSpec(int i, double s, double sv, double e, double ev)
        {
            tableHeatSinkNTC[i].startTempValue = s;
            tableHeatSinkNTC[i].startVntc = sv;
            tableHeatSinkNTC[i].endTempValue = e;
            tableHeatSinkNTC[i].endVntc = ev;
        }

        void writeFFSpec(int i, double s, double sv, double e, double ev)
        {
            tableFF600RNTC[i].startTempValue = s;
            tableFF600RNTC[i].startVntc = sv;
            tableFF600RNTC[i].endTempValue = e;
            tableFF600RNTC[i].endVntc = ev;
        }


        double ADC_ChangeRawValToVoltage(UInt16 adcRawVal)
        {
            // 0~3.3V => 0~4095
            // adc raw val 1 : 0.0008058V

            return adcRawVal * 8.0586080e-4;
        }

        public double ADC_GetTempHeat(UInt16 adcRawVal)
        {
            double fVin = 0;
            double fDeltaV = 0;
            int i = 0;

            fVin = ADC_ChangeRawValToVoltage(adcRawVal);

            // range check
            if (fVin >= tableHeatSinkNTC[0].startVntc)
                return tableHeatSinkNTC[0].startTempValue;
            if (fVin <= tableHeatSinkNTC[TEMP_CALC_DIV_END].endVntc)
                return tableHeatSinkNTC[TEMP_CALC_DIV_END].endTempValue;

            // search table
            for (i = 0; i <= TEMP_CALC_DIV_END; i++)
            {
                if (tableHeatSinkNTC[i].endVntc <= fVin)
                    break;
            }

            // calc delta V per temp
            fDeltaV = (tableHeatSinkNTC[i].startVntc - tableHeatSinkNTC[i].endVntc) / (tableHeatSinkNTC[i].endTempValue - tableHeatSinkNTC[i].startTempValue);

            // vIn position
            fVin = tableHeatSinkNTC[i].startVntc - fVin;

            // calc Temp
            return (fVin / fDeltaV) + tableHeatSinkNTC[i].startTempValue /* + offset */;
        }

        public double ADC_GetTempIgbt(UInt16 adcRawVal)
        {
            double fVin = 0;
            double fDeltaV = 0;
            int i = 0;

            fVin = ADC_ChangeRawValToVoltage(adcRawVal);

            // range check
            if (fVin >= tableFF600RNTC[0].startVntc)
                return tableFF600RNTC[0].startTempValue;
            if (fVin <= tableFF600RNTC[TEMP_CALC_DIV_END].endVntc)
                return tableFF600RNTC[TEMP_CALC_DIV_END].endTempValue;

            // search table
            for (i = 0; i <= TEMP_CALC_DIV_END; i++)
            {
                if (tableFF600RNTC[i].endVntc <= fVin)
                    break;
            }

            // calc delta V per temp
            fDeltaV = (tableFF600RNTC[i].startVntc - tableFF600RNTC[i].endVntc) / (tableFF600RNTC[i].endTempValue - tableFF600RNTC[i].startTempValue);

            // vIn position
            fVin = tableFF600RNTC[i].startVntc - fVin;

            // calc Temp
            return (fVin / fDeltaV) + tableFF600RNTC[i].startTempValue /* + offset */;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            UInt16 i = 0;
            Sensor sensor = new Sensor();

            Console.WriteLine("HEAT SINK ===================");
            for (i = 4095; i > 0; i--)
            {
                if ( (i % 100) == 0 )
                {
                    Console.WriteLine("adc:{0} V:{1} temp:{2}", i, i * 8.0586080e-4, sensor.ADC_GetTempHeat(i));
                }

                if ((i % 1000) == 0)
                {
                    Console.ReadKey();
                }
            }

            Console.WriteLine("IGBT ===================");
            for (i = 4095; i > 0; i--)
            {
                if ((i % 100) == 0)
                {
                    Console.WriteLine("adc:{0} V:{1} temp:{2}", i, i * 8.0586080e-4, sensor.ADC_GetTempIgbt(i));
                }

                if ((i % 1000) == 0)
                {
                    Console.ReadKey();
                }
            }

        }
    }
}
