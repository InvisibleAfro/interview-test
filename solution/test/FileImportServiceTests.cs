using HrmFileImport.Mappers;
using HrmFileImport.Models.Csv;
using HrmFileImport.Models.HrmApi;
using HrmFileImport.Ports;
using HrmFileImport.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using System.Text;
using Xunit;

namespace HrmFileImport.Tests
{
    public class FileImportServiceTests
    {
        [Fact]
        public async Task Import_AllRowsSucceed_ReturnsSuccess()
        {
            // Arrange
            var csv = BuildCsv(Row("E1", "NO"), Row("E2", "NO"));

            var employeeSource = new Mock<IEmployeeSource>();
            employeeSource
                .Setup(x => x.DownloadEmployeeSourceAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var authClient = new Mock<IHrmAuthClient>();
            authClient
                .Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("token");

            var mapper = new Mock<IEmploymentRequestMapperMapper>();
            mapper
                .Setup(x => x.Map(It.IsAny<EmployeeCsvRow>()))
                .Returns(new CreateEmploymentRequestDto());

            var apiClient = new Mock<IHrmApiClient>();
            apiClient
                .Setup(x => x.PostEmployee(
                    It.IsAny<CreateEmploymentRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<FileImportService>>();

            var importService = new FileImportService(
                logger.Object,
                authClient.Object,
                employeeSource.Object,
                mapper.Object,
                apiClient.Object);

            // Act
            var result = await importService.Import();

            // Assert
            Assert.True(result.IsSuccess);
            apiClient.Verify(x =>
                x.PostEmployee(It.IsAny<CreateEmploymentRequestDto>(), "token", It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Import_BadRequest_ContinuesImport()
        {
            //arrange
            var csv = BuildCsv(Row("E1", "NO"), Row("E2", "NO"), Row("E3", "NO"));

            var employeeSource = new Mock<IEmployeeSource>();
            employeeSource.Setup(x => x.DownloadEmployeeSourceAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var authClient = new Mock<IHrmAuthClient>();
            authClient.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("token");

            var mapper = new Mock<IEmploymentRequestMapperMapper>();
            mapper.Setup(x => x.Map(It.IsAny<EmployeeCsvRow>()))
                .Returns(new CreateEmploymentRequestDto());

            var apiClient = new Mock<IHrmApiClient>();

            var callCount = 0;
            apiClient.Setup(x => x.PostEmployee(It.IsAny<CreateEmploymentRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 2)
                        throw new HttpRequestException("400", null, System.Net.HttpStatusCode.BadRequest);

                    return Task.CompletedTask;
                });

            var logger = new Mock<ILogger<FileImportService>>();

            var importService = new FileImportService(
                logger.Object,
                authClient.Object,
                employeeSource.Object,
                mapper.Object,
                apiClient.Object);

            //act
            var result = await importService.Import();

            //assert
            Assert.True(result.IsSuccess);
            apiClient.Verify(x =>
                x.PostEmployee(It.IsAny<CreateEmploymentRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }


        [Fact]
        public async Task Import_Unauthorized_StopsImport()
        {
            //arrange
            var csv = BuildCsv(Row("E1", "NO"), Row("E2", "NO"), Row("E3", "NO"));

            var employeeSource = new Mock<IEmployeeSource>();
            employeeSource.Setup(x => x.DownloadEmployeeSourceAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var authClient = new Mock<IHrmAuthClient>();
            authClient.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("token");

            var mapper = new Mock<IEmploymentRequestMapperMapper>();
            mapper.Setup(x => x.Map(It.IsAny<EmployeeCsvRow>()))
                .Returns(new CreateEmploymentRequestDto());

            var apiClient = new Mock<IHrmApiClient>();
            apiClient.Setup(x => x.PostEmployee(It.IsAny<CreateEmploymentRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException("401", null, System.Net.HttpStatusCode.Unauthorized));

            var logger = new Mock<ILogger<FileImportService>>();

            var importService = new FileImportService(
                logger.Object,
                authClient.Object,
                employeeSource.Object,
                mapper.Object,
                apiClient.Object);

            var result = await importService.Import();

            //act
            Assert.False(result.IsSuccess);

            //assert
            apiClient.Verify(x =>
                x.PostEmployee(It.IsAny<CreateEmploymentRequestDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Import_WhenTransientHttpErrorOccurs_DoesNotAbort_AndContinuesWithNextRows()
        {
            // Arrange
            var csv = BuildCsv(Row("E1", "NO"), Row("E2", "NO"), Row("E3", "NO"));

            var employeeSource = new Mock<IEmployeeSource>();
            employeeSource
                .Setup(x => x.DownloadEmployeeSourceAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

            var authClient = new Mock<IHrmAuthClient>();
            authClient.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("token");

            var mapper = new Mock<IEmploymentRequestMapperMapper>();
            mapper.Setup(x => x.Map(It.IsAny<EmployeeCsvRow>()))
                .Returns(new CreateEmploymentRequestDto());

            var api = new Mock<IHrmApiClient>();

            api.SetupSequence(x => x.PostEmployee(
                    It.IsAny<CreateEmploymentRequestDto>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new HttpRequestException("503", null, HttpStatusCode.ServiceUnavailable))
                .Returns(Task.CompletedTask)
                .Returns(Task.CompletedTask);

            var logger = new Mock<ILogger<FileImportService>>();

            var importService = new FileImportService(
                logger.Object,
                authClient.Object,
                employeeSource.Object,
                mapper.Object,
                api.Object);

            // Act
            var result = await importService.Import(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            //verify three posts were called.
            api.Verify(x => x.PostEmployee(
                    It.IsAny<CreateEmploymentRequestDto>(),
                    "token",
                    It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task Import_WhenMappingFailsForOneRow_DoesNotAbort_AndContinues()
        {
            // Arrange: row2 throws mapping exception.
            var csv = BuildCsv(
                Row(employmentId: "E1", country: "NO"),
                Row(employmentId: "E2", country: "XX"),
                Row(employmentId: "E3", country: "NO"));

            IEmployeeSource employeeSource = new InMemoryEmployeeSource(csv);
            IHrmAuthClient authClient = new FixedTokenAuthClient("token");

            IEmploymentRequestMapperMapper mapper =
                new EmploymentRequestMapper(new CountryCodeMapper(), new DateFormatter());

            var apiClient = new RecordingHrmApiClient();
            var logger = new NullLogger<FileImportService>();

            var importService = new FileImportService(logger, authClient, employeeSource, mapper, apiClient);

            // Act
            var result = await importService.Import();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, apiClient.CallCount);
        }

        #region test helpers
        private static string BuildCsv(params string[] rows)
        {
            var header =
                "EmployeeID;FirstName;LastName;OrgUnitId;EmailWork;EmailPrivate;MobilePhone;StreetAddress;ZipCode;City;Country;StartDate;EndDate;FTEFactor;Location;JobID;IdNumber;IdType;IdCountry;ActiveDirectoryLogin;ManagerID;Gender;DateOfBirth;EmploymentID";

            return header + "\n" + string.Join("\n", rows) + "\n";
        }

        private static string Row(string employmentId, string country)
        {
            // Keep everything else valid so the only failure is the country mapping.
            return $"E1;Ola;Nordmann;123;ola@work.com;ola@private.com;912 34 567;Gate 1;0123;Oslo;{country};01.01.2020;;1.0;HQ;42;12345678901;NIN;NO;ola.ad;;M;01.01.1990;{employmentId}";
        }

        private sealed class InMemoryEmployeeSource : IEmployeeSource
        {
            private readonly string _csv;
            public InMemoryEmployeeSource(string csv) => _csv = csv;

            public Task<Stream> DownloadEmployeeSourceAsync(CancellationToken ct)
                => Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes(_csv)));
        }

        private sealed class FixedTokenAuthClient : IHrmAuthClient
        {
            private readonly string _token;
            public FixedTokenAuthClient(string token) => _token = token;
            public Task<string> GetAccessTokenAsync(CancellationToken ct) => Task.FromResult(_token);
        }

        private sealed class RecordingHrmApiClient : IHrmApiClient
        {
            public int CallCount { get; private set; }

            public Task PostEmployee(CreateEmploymentRequestDto employmentRequestDto, string authToken, CancellationToken ct)
            {
                CallCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class NullLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
        #endregion
    }
}
