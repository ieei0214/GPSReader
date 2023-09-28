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
        public void Test_OnGPGGAUpdated(string gpggaNemm)
        {
            gpsReader.StartReading();
            var GPGGAUpdated = false;

            gpsReader.OnGPGGAUpdated += (sender, e) => GPGGAUpdated = true;
            mockInputSource.DataReceived += Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(gpggaNemm));

            gpsReader.StopReading();

            GPGGAUpdated.Should().BeTrue();
        }

        public static bool AreEquivalent(GPGGAData expected, GPGGAData actual)
        {
            return expected.UTC == actual.UTC &&
                   expected.Latitude == actual.Latitude &&
                   expected.Longitude == actual.Longitude &&
                   expected.Quality == actual.Quality &&
                   expected.Satellites == actual.Satellites &&
                   expected.HDOP == actual.HDOP &&
                   expected.Altitude == actual.Altitude &&
                   expected.AltitudeUnits == actual.AltitudeUnits &&
                   expected.GeoidHeight == actual.GeoidHeight &&
                   expected.GeoidHeightUnits == actual.GeoidHeightUnits &&
                   expected.DGPSDataAge == actual.DGPSDataAge &&
                   expected.Checksum == actual.Checksum;
        }

        public static IEnumerable<object[]> GPGGADataTestData()
        {
            yield return new object[]
            {
                "$GPGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*42",
                new GPGGAData
                {
                    UTC = "123519",
                    Latitude = "48.1173",
                    Longitude = "11.5221",
                    Quality = 1,
                    Satellites = 8,
                    HDOP = 0.9,
                    Altitude = 545.4,
                    AltitudeUnits = "M",
                    GeoidHeight = 46.9,
                    GeoidHeightUnits = "M",
                    DGPSDataAge = "",
                    Checksum = "*42"
                }
            };
            
        }

        // [Theory]
        // [MemberData(nameof(GPGGADataTestData))]
        // public void Test_GPGGAEventArgs_B(string gpggaNmea, GPGGAData expectedGPGGAData)
        // {
        //     var mockInputSource = new MockInputSource();
        //     var gpsReader = new GPSReaderService(mockInputSource);  // Provide parsers argument
        //     gpsReader.StartReading();
        //     GPGGAEventArgs gpggaEventArgs = null;
        //
        //     gpsReader.OnGPGGAUpdated += (sender, e) => gpggaEventArgs = e;
        //     mockInputSource.SimulateDataReceived(gpggaNmea);
        //
        //     gpsReader.StopReading();
        //
        //     // Assert
        //     gpggaEventArgs.Should().NotBeNull();
        //     AreEquivalent(expectedGPGGAData, gpggaEventArgs.GPGGAData).Should().BeTrue();
        // }
        //

        [Theory]
        [InlineData("$GPGGA,123519,4807.038,N,01131.324,E,1,08,0.9,545.4,M,46.9,M,,*42")]
        public void Test_GPGGAEventArgs(string gpggaNemm)
        {
            gpsReader.StartReading();
            GPGGAEventArgs gpggaEventArgs = null;
        
            gpsReader.OnGPGGAUpdated += (sender, e) => gpggaEventArgs = e;
            mockInputSource.DataReceived += Raise.Event<EventHandler<InputReceivedEventArgs>>(this, new InputReceivedEventArgs(gpggaNemm));

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
            gpggaEventArgs.GPGGAData.Checksum.Should().Be("*42");
        }


    }
}