namespace GPSReader.Models;

public class GPGSVData : NMEAData
{
    public string? DataCount { get; }
    public string? MessageNumber { get; }
    public string? SatellitesInView { get; }

    public List<Satellite>? Satellites;

    public GPGSVData(string? dataCount, string? messageNumber, string? satellitesInView, List<Satellite>? satellites)
    {
        DataCount = dataCount;
        MessageNumber = messageNumber;
        SatellitesInView = satellitesInView;
        Satellites = satellites;
    }

    public static GPGSVData CreateFromFields(string[] fields)
    {
        /*
GPGSV
GPS Satellites in View（GSV）可见卫星信息

 $GPGSV, <1>,<2>,<3>,<4>,<5>,<6>,<7>,?<4>,<5>,<6>,<7>,<8>
各字段描述如下：

<1> GSV语句的总数
<2> 本句GSV的编号
<3> 可见卫星的总数，00 至 12。
<4> 卫星编号， 01 至 32。
<5> 卫星仰角， 00 至 90 度。
<6> 卫星方位角， 000 至 359 度。实际值。
<7> 讯号噪声比（C/No）， 00 至 99 dB；无表未接收到讯号。
<8> Checksum（检查位）
注意：第 <4>,<5>,<6>,<7> 项个别卫星会重复出现，每行最多有四颗卫星。其余卫星信息会于次一行出现，若未使用，这些字段会空白。
         */

        var DataCount = fields[1];
        var DataNumber = fields[2];
        var SatellitesInView = fields[3];
        var Satellites = new List<Satellite>();
        for (int i = 4; i < fields.Length; i += 4)
        {
            var satellite = new Satellite
            {
                SatelliteNumber = fields[i],
                Elevation = fields[i + 1],
                Azimuth = fields[i + 2],
                SNR = fields[i + 3]
            };
            Satellites.Add(satellite);
        }

        return new GPGSVData(DataCount, DataNumber, SatellitesInView, Satellites);
    }
}

public class Satellite
{
    public string? SatelliteNumber { get; set; }
    public string? Elevation { get; set; }
    public string? Azimuth { get; set; }
    public string? SNR { get; set; }
}