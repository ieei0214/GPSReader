using FluentAssertions;
using GPSReader.EventArgs;
using GPSReader.Interfaces;
using GPSReader.Models;
using GPSReader.Parsers;
using System.Globalization;
using System.Reflection.PortableExecutable;
using NSubstitute;

namespace GPSReader.Tests
{
    public class Tests
    {
        private INMEAInput mockInputSource;
        private Microsoft.Extensions.Logging.ILogger<GPSReaderService> logger;
        private GPSReaderService gpsReader;

        public Tests()
        {
            mockInputSource = Substitute.For<INMEAInput>();
            logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<GPSReaderService>>();
            gpsReader = new GPSReaderService(logger, mockInputSource);
        }

        [Theory]
        [InlineData("$GPGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*42")]
        public void Test_OnGPGGAUpdated(string inputNemm)
        {
            gpsReader.StartReading();
            var GPGGAUpdated = false;

            gpsReader.OnGPGGAUpdated += (sender, e) => GPGGAUpdated = true;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            GPGGAUpdated.Should().BeTrue();
        }

        [Theory]
        [InlineData("$GPGSA,A,1,22,17,14,24,,,,,,,,,57.14,57.13,1.00*03")]
        public void Test_OnGPGSAUpdated(string inputNemm)
        {
            gpsReader.StartReading();
            var GPGSAUpdated = false;

            gpsReader.OnGPGSAUpdated += (sender, e) => GPGSAUpdated = true;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            GPGSAUpdated.Should().BeTrue();
        }

        [Theory]
        [InlineData("$GPGLL,,,,,211123.00,V,N*48")]
        public void Test_OnGPGLLUpdated(string inputNemm)
        {
            gpsReader.StartReading();
            var GPGLLUpdated = false;

            gpsReader.OnGPGLLUpdated += (sender, e) => GPGLLUpdated = true;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            GPGLLUpdated.Should().BeTrue();
        }

        [Theory]
        [InlineData("$GPGSV,4,1,13,05,04,174,37,06,51,046,35,11,80,138,34,12,53,324,25*7D")]
        public void Test_OnGPGSVUpdated(string inputNemm)
        {
            gpsReader.StartReading();
            var GPGSVUpdated = false;

            gpsReader.OnGPGSVUpdated += (sender, e) => GPGSVUpdated = true;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            GPGSVUpdated.Should().BeTrue();
        }

        [Theory]
        [InlineData("$GPGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*42")]
        public void Test_GPGGAEventArgs(string inputNemm)
        {
            gpsReader.StartReading();
            GPGGAEventArgs gpggaEventArgs = null;

            gpsReader.OnGPGGAUpdated += (sender, e) => gpggaEventArgs = e;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            // Assert
            gpggaEventArgs.Should().NotBeNull();
            gpggaEventArgs.GPGGAData.UTC.Should().Be("123519");
            double parsedLatitude = double.Parse(gpggaEventArgs.GPGGAData.Latitude, CultureInfo.InvariantCulture);
            double parsedLongitude = double.Parse(gpggaEventArgs.GPGGAData.Longitude, CultureInfo.InvariantCulture);
            parsedLatitude.Should().BeApproximately(48.1173, 0.0001);
            parsedLongitude.Should().BeApproximately(11.5221, 0.0001);
            gpggaEventArgs.GPGGAData.Quality.Should().Be(1);
            gpggaEventArgs.GPGGAData.Satellites.Should().Be(8);
            gpggaEventArgs.GPGGAData.HDOP.Should().BeApproximately(0.9, 0.001);
            gpggaEventArgs.GPGGAData.Altitude.Should().BeApproximately(545.4, 0.1);
            gpggaEventArgs.GPGGAData.AltitudeUnits.Should().Be("M");
            gpggaEventArgs.GPGGAData.GeoidHeight.Should().BeApproximately(46.9, 0.1);
            gpggaEventArgs.GPGGAData.GeoidHeightUnits.Should().Be("M");
            gpggaEventArgs.GPGGAData.DGPSDataAge.Should().BeNull();
            gpggaEventArgs.GPGGAData.Checksum.Should().Be("42");
        }

        [Theory]
        [InlineData("$GPGSA,A,1,22,17,14,,,,,,,,,,57.18,57.17,1.00*0D")]
        public void Test_GPGSAEventArgs(string inputNemm)
        {
            gpsReader.StartReading();
            GPGSAEventArgs gpgsaEventArgs = null;

            gpsReader.OnGPGSAUpdated += (sender, e) => gpgsaEventArgs = e;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            // Assert
            gpgsaEventArgs.Should().NotBeNull();
            var value = gpgsaEventArgs.GPGSAData;
            value.Mode.Should().Be("A");
            value.FixStatus.Should().Be("1");
            value.Satellites.Should().BeEquivalentTo("22", "17", "14");
            value.PDOP.Should().Be("57.18");
            value.HDOP.Should().Be("57.17");
            value.VDOP.Should().Be("1.00");
            value.Checksum.Should().Be("0D");
        }

        [Theory]
        [InlineData("$GPGLL,,,,,211123.00,V,N*48")]
        public void Test_GPGLLEventArgs(string inputNemm)
        {
            gpsReader.StartReading();
            GPGLLEventArgs gpgllEventArgs = null;

            gpsReader.OnGPGLLUpdated += (sender, e) => gpgllEventArgs = e;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            // Assert
            gpgllEventArgs.Should().NotBeNull();
            var value = gpgllEventArgs.GPGLLData;
            value.Latitude.Should().BeNull();
            value.LatitudeHemisphere.Should().BeNull();
            value.Longitude.Should().BeNull();
            value.LongitudeHemisphere.Should().BeNull();
            value.UTC.Should().Be("211123.00");
            value.PositionStatus.Should().Be("V");
            value.ModeIndicator.Should().Be("N");
            value.Checksum.Should().Be("48");
        }

        [Theory]
        [InlineData("$GPGSV,3,1,12,05,22,165,31,06,31,041,27,11,69,043,33,12,76,332,36*74",
            "3", "1", "12", 4,
            "05", "22", "165", "31",
            "06", "31", "041", "27",
            "11", "69", "043", "33",
            "12", "76", "332", "36")]
        [InlineData("$GPGSV,3,2,12,19,12,074,28,20,46,130,27,24,24,210,16,25,42,316,29*74",
            "3", "2", "12", 4,
            "19", "12", "074", "28",
            "20", "46", "130", "27",
            "24", "24", "210", "16",
            "25", "42", "316", "29")]
        [InlineData("$GPGSV,3,3,12,28,04,325,,29,18,280,09,46,49,200,29,48,50,193,38*7E",
            "3", "3", "12", 4,
            "28", "04", "325", "",
            "29", "18", "280", "09",
            "46", "49", "200", "29",
            "48", "50", "193", "38")]
        public void Test_GPGSVEventArgs(string inputNemm, string DataCount, string MessageNumber,
            string SatellitesInView, int SatellitesCount,
            string SatelliteNumber1, string Elevation1, string Azimuth1, string SNR1,
            string SatelliteNumber2, string Elevation2, string Azimuth2, string SNR2,
            string SatelliteNumber3, string Elevation3, string Azimuth3, string SNR3,
            string SatelliteNumber4, string Elevation4, string Azimuth4, string SNR4)
        {
            gpsReader.StartReading();
            GPGSVEventArgs gpgsvEventArgs = null;

            gpsReader.OnGPGSVUpdated += (sender, e) => gpgsvEventArgs = e;
            mockInputSource.DataReceived +=
                Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(inputNemm));

            gpsReader.StopReading();

            // Assert
            gpgsvEventArgs.Should().NotBeNull();
            var value = gpgsvEventArgs.GPGSVData;
            value.DataCount.Should().Be(DataCount);
            value.MessageNumber.Should().Be(MessageNumber);
            value.SatellitesInView.Should().Be(SatellitesInView);
            value.Satellites.Count.Should().Be(SatellitesCount);
            value.Satellites[0].SatelliteNumber.Should().Be(SatelliteNumber1);
            value.Satellites[0].Elevation.Should().Be(Elevation1);
            value.Satellites[0].Azimuth.Should().Be(Azimuth1);
            value.Satellites[0].SNR.Should().Be(SNR1);
            value.Satellites[1].SatelliteNumber.Should().Be(SatelliteNumber2);
            value.Satellites[1].Elevation.Should().Be(Elevation2);
            value.Satellites[1].Azimuth.Should().Be(Azimuth2);
            value.Satellites[1].SNR.Should().Be(SNR2);
            value.Satellites[2].SatelliteNumber.Should().Be(SatelliteNumber3);
            value.Satellites[2].Elevation.Should().Be(Elevation3);
            value.Satellites[2].Azimuth.Should().Be(Azimuth3);
            value.Satellites[2].SNR.Should().Be(SNR3);
            value.Satellites[3].SatelliteNumber.Should().Be(SatelliteNumber4);
            value.Satellites[3].Elevation.Should().Be(Elevation4);
            value.Satellites[3].Azimuth.Should().Be(Azimuth4);
            value.Satellites[3].SNR.Should().Be(SNR4);
        }
    }
} 