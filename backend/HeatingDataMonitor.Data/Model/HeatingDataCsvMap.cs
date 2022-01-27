using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using NodaTime;
using NodaTime.Text;

namespace HeatingDataMonitor.Data.Model;

internal sealed class HeatingDataCsvMap : ClassMap<HeatingData>
{
    private static readonly IPattern<LocalDateTime> s_dateTimePattern =
        LocalDateTimePattern.Create("dd.MM.yyHH:mm:ss", CultureInfo.InvariantCulture);

    public HeatingDataCsvMap()
    {
        Map(d => d.SPS_Zeit).Convert(ParseHeatingDataTime);
        Map(d => d.Kessel).Index(2);
        Map(d => d.Ruecklauf).Index(3);
        Map(d => d.Abgas).Index(4);
        // Map(d => d.Brennkammer).Index(5);
        Map(d => d.CO2_Soll).Index(6);
        Map(d => d.CO2_Ist).Index(7);
        Map(d => d.Saugzug_Ist).Index(8);
        Map(d => d.Puffer_Oben).Index(9);
        Map(d => d.Puffer_Unten).Index(10);
        Map(d => d.Platine).Index(11);
        Map(d => d.Betriebsphase_Kessel).Index(12);
        Map(d => d.Aussen).Index(13);
        Map(d => d.Vorlauf_HK1_Ist).Index(14);
        Map(d => d.Vorlauf_HK1_Soll).Index(15);
        Map(d => d.Betriebsphase_HK1).Index(16);
        // Map(d => d.Betriebsart_Fern_HK1).Index(17);
        // Map(d => d.Verschiebung_Fern_HK1).Index(18);
        // Map(d => d.Freigabekontakt_HK1).Index(19);
        Map(d => d.Vorlauf_HK2_Ist).Index(20);
        Map(d => d.Vorlauf_HK2_Soll).Index(21);
        Map(d => d.Betriebsphase_HK2).Index(22);
        // Map(d => d.Betriebsart_Fern_HK2).Index(23);
        // Map(d => d.Verschiebung_Fern_HK2).Index(24);
        // Map(d => d.Freigabekontakt_HK2).Index(25);
        // Map(d => d.Vorlauf_HK3_Ist).Index(26);
        // Map(d => d.Vorlauf_HK3_Soll).Index(27);
        // Map(d => d.Betriebsphase_HK3).Index(28);
        // Map(d => d.Betriebsart_Fern_HK3).Index(29);
        // Map(d => d.Verschiebung_Fern_HK3).Index(30);
        // Map(d => d.Freigabekontakt_HK3).Index(31);
        // Map(d => d.Vorlauf_HK4_Ist).Index(32);
        // Map(d => d.Vorlauf_HK4_Soll).Index(33);
        // Map(d => d.Betriebsphase_HK4).Index(34);
        // Map(d => d.Betriebsart_Fern_HK4).Index(35);
        // Map(d => d.Verschiebung_Fern_HK4).Index(36);
        // Map(d => d.Freigabekontakt_HK4).Index(37);
        Map(d => d.Boiler_1).Index(38);
        // Map(d => d.Boiler_2).Index(39);
        Map(d => d.DI_0).Index(40);
        Map(d => d.DI_1).Index(41);
        Map(d => d.DI_2).Index(42);
        Map(d => d.DI_3).Index(43);
        Map(d => d.A_W_0).Index(44);
        Map(d => d.A_W_1).Index(45);
        Map(d => d.A_W_2).Index(46);
        Map(d => d.A_W_3).Index(47);
        Map(d => d.A_EA_0).Index(48);
        Map(d => d.A_EA_1).Index(49);
        Map(d => d.A_EA_2).Index(50);
        Map(d => d.A_EA_3).Index(51);
        Map(d => d.A_EA_4).Index(52);
        Map(d => d.A_PHASE_0).Index(53);
        Map(d => d.A_PHASE_1).Index(54);
        Map(d => d.A_PHASE_2).Index(55);
        Map(d => d.A_PHASE_3).Index(56);
        Map(d => d.A_PHASE_4).Index(57);
    }

    // https://github.com/JoshClose/CsvHelper/issues/1927
    // Cannot be static because of a bug in CsvHelper
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private LocalDateTime ParseHeatingDataTime(ConvertFromStringArgs args)
    {
        string datePart = args.Row.GetField<string>(0);
        string timePart = args.Row.GetField<string>(1);

        return s_dateTimePattern.Parse(datePart + timePart).Value;
    }
}
