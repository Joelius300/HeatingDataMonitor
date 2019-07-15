using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using DataHandler.Exceptions;
using System.CodeDom.Compiler;
using DataHandler.Enums;

namespace DataHandler.Services
{
    public class SerialDataService : DataService
    {
        private readonly SerialPort port;

        public SerialDataService(DataStorage dataStorage, Config config) : base(dataStorage)
        {
            port = CreateSerialPort(config);
        }

        private SerialPort CreateSerialPort(Config config) {
            return new SerialPort()
            {
                PortName = config.SerialPortName,       // COM1 (Win), /dev/ttyS0 (raspi)
                BaudRate = 9600,                        // def from specs (heizung-sps)
                DataBits = 8,                           // def from specs (heizung-sps)
                Parity = Parity.None,                   // def from specs (heizung-sps)
                Handshake = Handshake.None,             // def from specs (heizung-sps)
                StopBits = StopBits.One,                // def from specs (heizung-sps)
                Encoding = Encoding.ASCII,              // def from specs (heizung-sps)
                DiscardNull = true,                     // we don't need that
                ReadTimeout = config.ExpectedReadInterval * 2000, // give enough time
                NewLine = "\r\n"                        // define newline used by sps
            };
        }

        private Data GetData()
        {
            string serialData;
            try
            {
                Console.WriteLine("Now awaiting data: ");
                serialData = port.ReadLine();
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout: ");
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Canceled: ");
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("InvalidOperation: ");
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                throw new NoDataReceivedException(e);
            }

            Data newData = Convert(serialData);
            if (newData == null)
            {
                Console.WriteLine("Received invalid Data: ");
                Console.WriteLine(serialData);

                throw new FaultyDataReceivedException(serialData);
            }

            Console.WriteLine("Received valid Data: ");
            Console.WriteLine(serialData);

            return newData;
        }


        public static Data Convert(string serialData)
        {
            // not real data
            if (String.IsNullOrWhiteSpace(serialData))
            {
                return null;
            }

            // split csv into fields
            string[] list = serialData.Split(';');

            // there should be 58 fields + one empty at the end because of the trailing ;
            if (list.Length != 59)
            {
                return null;
            }

            Data data;
            try
            {
                data = new Data
                {
                    DatumZeit = DateTime.TryParse($"{list[0]} {list[1]}", out DateTime vDatum) ? vDatum : throw new InvalidCastException("DateTime received from SerialPort is not valid"),
                    Kessel = float.TryParse(list[2], out float vKessel) ? (float?)vKessel : null,
                    Ruecklauf = float.TryParse(list[3], out float vRuecklauf) ? (float?)vRuecklauf : null,
                    Abgas = float.TryParse(list[4], out float vAbgas) ? (float?)vAbgas : null,
                    Brennkammer = float.TryParse(list[5], out float vBrennkammer) ? (float?)vBrennkammer : null,
                    CO2_Soll = float.TryParse(list[6], out float vCO2_Soll) ? (float?)vCO2_Soll : null,
                    CO2_Ist = float.TryParse(list[7], out float vCO2_Ist) ? (float?)vCO2_Ist : null,
                    Saugzug_Ist = float.TryParse(list[8], out float vSaugzug_Ist) ? (float?)vSaugzug_Ist : null,
                    Puffer_Oben = float.TryParse(list[9], out float vPuffer_Oben) ? (float?)vPuffer_Oben : null,
                    Puffer_Unten = float.TryParse(list[10], out float vPuffer_Unten) ? (float?)vPuffer_Unten : null,
                    Platine = float.TryParse(list[11], out float vPlatine) ? (float?)vPlatine : null,
                    Betriebsphase_Kessel = int.TryParse(list[12], out int vBetriebsphase_Kessel) ? (BetriebsPhaseKessel?)vBetriebsphase_Kessel : null,
                    Aussen = float.TryParse(list[13], out float vAussen) ? (float?)vAussen : null,
                    Vorlauf_HK1_Ist = float.TryParse(list[14], out float vVorlauf_HK1_Ist) ? (float?)vVorlauf_HK1_Ist : null,
                    Vorlauf_HK1_Soll = float.TryParse(list[15], out float vVorlauf_HK1_Soll) ? (float?)vVorlauf_HK1_Soll : null,
                    Betriebsphase_HK1 = int.TryParse(list[16], out int vBetriebsphase_HK1) ? (BetriebsPhaseHK?)vBetriebsphase_HK1 : null,
                    Betriebsart_Fern_HK1 = int.TryParse(list[17], out int vBetriebsart_Fern_HK1) ? (int?)vBetriebsart_Fern_HK1 : null,
                    Verschiebung_Fern_HK1 = float.TryParse(list[18], out float vVerschiebung_Fern_HK1) ? (float?)vVerschiebung_Fern_HK1 : null,
                    Freigabekontakt_HK1 = float.TryParse(list[19], out float vFreigabekontakt_HK1) ? (float?)vFreigabekontakt_HK1 : null,
                    Vorlauf_HK2_Ist = float.TryParse(list[20], out float vVorlauf_HK2_Ist) ? (float?)vVorlauf_HK2_Ist : null,
                    Vorlauf_HK2_Soll = float.TryParse(list[21], out float vVorlauf_HK2_Soll) ? (float?)vVorlauf_HK2_Soll : null,
                    Betriebsphase_HK2 = int.TryParse(list[22], out int vBetriebsphase_HK2) ? (BetriebsPhaseHK?)vBetriebsphase_HK2 : null,
                    Betriebsart_Fern_HK2 = int.TryParse(list[23], out int vBetriebsart_Fern_HK2) ? (int?)vBetriebsart_Fern_HK2 : null,
                    Verschiebung_Fern_HK2 = float.TryParse(list[24], out float vVerschiebung_Fern_HK2) ? (float?)vVerschiebung_Fern_HK2 : null,
                    Freigabekontakt_HK2 = float.TryParse(list[25], out float vFreigabekontakt_HK2) ? (float?)vFreigabekontakt_HK2 : null,
                    Vorlauf_HK3_Ist = float.TryParse(list[26], out float vVorlauf_HK3_Ist) ? (float?)vVorlauf_HK3_Ist : null,
                    Vorlauf_HK3_Soll = float.TryParse(list[27], out float vVorlauf_HK3_Soll) ? (float?)vVorlauf_HK3_Soll : null,
                    Betriebsphase_HK3 = int.TryParse(list[28], out int vBetriebsphase_HK3) ? (BetriebsPhaseHK?)vBetriebsphase_HK3 : null,
                    Betriebsart_Fern_HK3 = int.TryParse(list[29], out int vBetriebsart_Fern_HK3) ? (int?)vBetriebsart_Fern_HK3 : null,
                    Verschiebung_Fern_HK3 = float.TryParse(list[30], out float vVerschiebung_Fern_HK3) ? (float?)vVerschiebung_Fern_HK3 : null,
                    Freigabekontakt_HK3 = float.TryParse(list[31], out float vFreigabekontakt_HK3) ? (float?)vFreigabekontakt_HK3 : null,
                    Vorlauf_HK4_Ist = float.TryParse(list[32], out float vVorlauf_HK4_Ist) ? (float?)vVorlauf_HK4_Ist : null,
                    Vorlauf_HK4_Soll = float.TryParse(list[33], out float vVorlauf_HK4_Soll) ? (float?)vVorlauf_HK4_Soll : null,
                    Betriebsphase_HK4 = int.TryParse(list[34], out int vBetriebsphase_HK4) ? (BetriebsPhaseHK?)vBetriebsphase_HK4 : null,
                    Betriebsart_Fern_HK4 = int.TryParse(list[35], out int vBetriebsart_Fern_HK4) ? (int?)vBetriebsart_Fern_HK4 : null,
                    Verschiebung_Fern_HK4 = float.TryParse(list[36], out float vVerschiebung_Fern_HK4) ? (float?)vVerschiebung_Fern_HK4 : null,
                    Freigabekontakt_HK4 = float.TryParse(list[37], out float vFreigabekontakt_HK4) ? (float?)vFreigabekontakt_HK4 : null,
                    Boiler_1 = float.TryParse(list[38], out float vBoiler_1) ? (float?)vBoiler_1 : null,
                    Boiler_2 = float.TryParse(list[39], out float vBoiler_2) ? (float?)vBoiler_2 : null,
                    DI_0 = int.TryParse(list[40], out int vDI_0) ? (int?)vDI_0 : null,
                    DI_1 = int.TryParse(list[41], out int vDI_1) ? (int?)vDI_1 : null,
                    DI_2 = int.TryParse(list[42], out int vDI_2) ? (int?)vDI_2 : null,
                    DI_3 = int.TryParse(list[43], out int vDI_3) ? (int?)vDI_3 : null,
                    A_W_0 = int.TryParse(list[44], out int vA_W_0) ? (int?)vA_W_0 : null,
                    A_W_1 = int.TryParse(list[45], out int vA_W_1) ? (int?)vA_W_1 : null,
                    A_W_2 = int.TryParse(list[46], out int vA_W_2) ? (int?)vA_W_2 : null,
                    A_W_3 = int.TryParse(list[47], out int vA_W_3) ? (int?)vA_W_3 : null,
                    A_EA_0 = int.TryParse(list[48], out int vA_EA_0) ? (int?)vA_EA_0 : null,
                    A_EA_1 = int.TryParse(list[49], out int vA_EA_1) ? (int?)vA_EA_1 : null,
                    A_EA_2 = int.TryParse(list[50], out int vA_EA_2) ? (int?)vA_EA_2 : null,
                    A_EA_3 = int.TryParse(list[51], out int vA_EA_3) ? (int?)vA_EA_3 : null,
                    A_EA_4 = int.TryParse(list[52], out int vA_EA_4) ? (int?)vA_EA_4 : null,
                    A_PHASE_0 = int.TryParse(list[53], out int vA_PHASE_0) ? (int?)vA_PHASE_0 : null,
                    A_PHASE_1 = int.TryParse(list[54], out int vA_PHASE_1) ? (int?)vA_PHASE_1 : null,
                    A_PHASE_2 = int.TryParse(list[55], out int vA_PHASE_2) ? (int?)vA_PHASE_2 : null,
                    A_PHASE_3 = int.TryParse(list[56], out int vA_PHASE_3) ? (int?)vA_PHASE_3 : null,
                    A_PHASE_4 = int.TryParse(list[57], out int vA_PHASE_4) ? (int?)vA_PHASE_4 : null
                };

                data.SetDisplayableValues();
            }
            catch
            {
                return null;
            }

            return data;
        }

        protected override async Task<Data> GetNewData(CancellationToken cancellationToken)
        {
            // make async call
            return await Task.Run(() => GetData());
        }

        protected override Task BeforeLoopStart()
        {
            port.Open();
            return Task.CompletedTask;
        }

        protected override async Task CleanupOnApplicationShutdown()
        {
            port.Close();           // this would raise an OperationCanceledException if the port is still reading
            await Task.Delay(250);  // give the loop one last chance to end (gracefully)
        }

        public override void Dispose()
        {
            port.Dispose();
            base.Dispose();
        }
    }
}
