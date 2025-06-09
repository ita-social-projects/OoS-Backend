using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using OutOfSchool.QuartzJobs.Api.Configuration;
using OutOfSchool.QuartzJobs.Api.Extensions;
using OutOfSchool.QuartzJobs.Api.Models;
using OutOfSchool.QuartzJobs.Api.Services;

namespace OutOfSchool.WebApi.Tests.QuartzJobs.Monitoring;

[TestFixture]
public class QuartzMonitoringEndpointsTests
{
    private HttpClient _client;
    private TestServer _server;
    private IHost _host;
    private Mock<IQuartzMonitoringService> _monitoringServiceMock;

    [SetUp]
    public void Setup()
    {
        _monitoringServiceMock = new Mock<IQuartzMonitoringService>();

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton(_monitoringServiceMock.Object);

                    services.AddRouting();
                    services.AddEndpointsApiExplorer();

                    services.Configure<QuartzMonitoringOptions>(opt =>
                    {
                        opt.Enabled = true;
                        opt.Redis = new QuartzMonitoringOptions.RedisConfig
                        {
                            Server = "localhost",
                            Port = 6379,
                            Password = "",
                            HistoryLength = 100
                        };
                    });
                });

                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapQuartzMonitoringApi();
                    });
                });
            });

        _host = hostBuilder.Start();
        _server = _host.GetTestServer();
        _client = _server.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _host?.Dispose();
    }

    [Test]
    public async Task GetAllJobs_ReturnsOkWithJobs()
    {
        // Arrange
        var expectedJobs = CreateSampleJobInfoList();
        _monitoringServiceMock.Setup(s => s.GetAllJobsAsync())
            .ReturnsAsync(expectedJobs);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<JobInfoDto>>("/quartz/jobs");

        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0].Name, Is.EqualTo("TestJob1"));
        Assert.That(actual[1].Name, Is.EqualTo("TestJob2"));
    }

    [Test]
    public async Task GetRunningJobs_ReturnsOkWithRunningJobs()
    {
        // Arrange
        var expectedJobs = CreateSampleRunningJobsList();
        _monitoringServiceMock.Setup(s => s.GetRunningJobsAsync())
            .ReturnsAsync(expectedJobs);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<RunningJobDto>>("/quartz/jobs/running");

        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual[0].JobName, Is.EqualTo("TestJob1"));
    }

    [Test]
    public async Task GetJobDetails_WithValidJobName_ReturnsOkWithJobDetails()
    {
        // Arrange
        var jobName = "TestJob1";
        var jobDetails = CreateSampleJobDetails(jobName);
        _monitoringServiceMock.Setup(s => s.GetJobDetailsAsync(jobName))
            .ReturnsAsync(jobDetails);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<JobDetailsDto>($"/quartz/jobs/{jobName}");

        Assert.That(actual.Name, Is.EqualTo(jobName));
        Assert.That(actual.Triggers.Count, Is.EqualTo(1));
    }

    [Test]
    public Task GetJobDetails_WithInvalidJobName_ReturnsNotFound()
    {
        // Arrange
        var jobName = "NonExistentJob";
        _monitoringServiceMock.Setup(s => s.GetJobDetailsAsync(jobName))
            .ReturnsAsync((JobDetailsDto)null);

        // Act & Assert
        return VerifyNotFoundResponseAsync($"/quartz/jobs/{jobName}");
    }

    [Test]
    public async Task GetJobHistory_WithValidJobName_ReturnsOkWithHistory()
    {
        // Arrange
        var jobName = "TestJob1";
        var history = CreateSampleJobHistory(jobName);
        _monitoringServiceMock.Setup(s => s.GetJobHistoryAsync(jobName))
            .ReturnsAsync(history);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<JobExecutionInfo>>($"/quartz/jobs/{jobName}/history");

        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0].JobName, Is.EqualTo(jobName));
        Assert.That(actual[0].WasSuccessful, Is.True);
        Assert.That(actual[1].WasSuccessful, Is.False);
    }

    [Test]
    public Task GetJobHistory_WithNoHistory_ReturnsNotFound()
    {
        // Arrange
        var jobName = "JobWithNoHistory";
        _monitoringServiceMock.Setup(s => s.GetJobHistoryAsync(jobName))
            .ReturnsAsync(new List<JobExecutionInfo>());

        // Act & Assert
        return VerifyNotFoundResponseAsync($"/quartz/jobs/{jobName}/history");
    }

    [Test]
    public async Task GetJobErrors_WithValidJobName_ReturnsOkWithErrors()
    {
        // Arrange
        var jobName = "TestJob1";
        var errors = CreateSampleJobErrors(jobName);
        _monitoringServiceMock.Setup(s => s.GetFailedJobsAsync(jobName))
            .ReturnsAsync(errors);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<JobExecutionInfo>>($"/quartz/jobs/{jobName}/errors");

        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0].JobName, Is.EqualTo(jobName));
        Assert.That(actual[0].WasSuccessful, Is.False);
        Assert.That(actual[1].WasSuccessful, Is.False);
    }

    [Test]
    public Task GetJobErrors_WithNoErrors_ReturnsNotFound()
    {
        // Arrange
        var jobName = "JobWithNoErrors";
        _monitoringServiceMock.Setup(s => s.GetFailedJobsAsync(jobName))
            .ReturnsAsync(new List<JobExecutionInfo>());

        // Act & Assert
        return VerifyNotFoundResponseAsync($"/quartz/jobs/{jobName}/errors");
    }

    [Test]
    public async Task GetScheduledJobs_ReturnsOkWithScheduledJobs()
    {
        // Arrange
        var scheduledJobs = CreateSampleScheduledJobs();
        _monitoringServiceMock.Setup(s => s.GetNextExecutionsForAllJobsAsync())
            .ReturnsAsync(scheduledJobs);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<JobNextExecutionDto>>("/quartz/jobs/scheduled");

        Assert.That(actual.Count, Is.EqualTo(2));
        Assert.That(actual[0].JobName, Is.EqualTo("TestJob1"));
        Assert.That(actual[1].JobName, Is.EqualTo("TestJob2"));
    }

    [Test]
    public async Task GetJobSchedule_WithValidJobName_ReturnsOkWithSchedule()
    {
        // Arrange
        var jobName = "TestJob1";
        var scheduledJobs = CreateSampleJobSchedule(jobName);
        _monitoringServiceMock.Setup(s => s.GetNextExecutionsForJobAsync(jobName))
            .ReturnsAsync(scheduledJobs);

        // Act & Assert
        var actual = await GetAndVerifySuccessResponseAsync<List<JobNextExecutionDto>>($"/quartz/jobs/{jobName}/scheduled");

        Assert.That(actual.Count, Is.EqualTo(1));
        Assert.That(actual[0].JobName, Is.EqualTo(jobName));
    }

    [Test]
    public Task GetJobSchedule_WithInvalidJobName_ReturnsNotFound()
    {
        // Arrange
        var jobName = "NonExistentJob";
        _monitoringServiceMock.Setup(s => s.GetNextExecutionsForJobAsync(jobName))
            .ReturnsAsync(new List<JobNextExecutionDto>());

        // Act & Assert
        return VerifyNotFoundResponseAsync($"/quartz/jobs/{jobName}/scheduled");
    }

    #region Helper Methods

    private async Task<T> GetAndVerifySuccessResponseAsync<T>(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content.ReadFromJsonAsync<T>();
        Assert.That(result, Is.Not.Null);

        return result;
    }

    private async Task VerifyNotFoundResponseAsync(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private List<JobInfoDto> CreateSampleJobInfoList()
    {
        return new List<JobInfoDto>
        {
            new JobInfoDto
            {
                Name = "TestJob1",
                Group = "DefaultGroup",
                JobType = "TestType1",
                Status = "Scheduled",
                Description = "Test job 1"
            },
            new JobInfoDto
            {
                Name = "TestJob2",
                Group = "DefaultGroup",
                JobType = "TestType2",
                Status = "Paused",
                Description = "Test job 2"
            }
        };
    }

    private List<RunningJobDto> CreateSampleRunningJobsList()
    {
        return new List<RunningJobDto>
        {
            new RunningJobDto
            {
                JobName = "TestJob1",
                Group = "DefaultGroup",
                StartedAt = DateTimeOffset.Now.AddMinutes(-5),
                Trigger = "TestTrigger1",
                TriggerGroup = "DefaultGroup"
            }
        };
    }

    private JobDetailsDto CreateSampleJobDetails(string jobName)
    {
        return new JobDetailsDto
        {
            Name = jobName,
            Group = "DefaultGroup",
            JobType = "TestType",
            Status = "Scheduled",
            Description = "Test job description",
            Triggers = new List<JobTriggerDto>
            {
                new JobTriggerDto
                {
                    TriggerKey = "TestTrigger.DefaultGroup",
                    NextExecutions = new List<DateTimeOffset>
                    {
                        DateTimeOffset.Now.AddMinutes(10),
                        DateTimeOffset.Now.AddMinutes(20)
                    }
                }
            }
        };
    }

    private List<JobExecutionInfo> CreateSampleJobHistory(string jobName)
    {
        return new List<JobExecutionInfo>
        {
            new JobExecutionInfo
            {
                JobName = jobName,
                JobGroup = "DefaultGroup",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(-1).AddMinutes(5),
                Duration = TimeSpan.FromMinutes(5),
                WasSuccessful = true
            },
            new JobExecutionInfo
            {
                JobName = jobName,
                JobGroup = "DefaultGroup",
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-2).AddMinutes(3),
                Duration = TimeSpan.FromMinutes(3),
                WasSuccessful = false,
                ErrorMessage = "Test error message"
            }
        };
    }

    private List<JobExecutionInfo> CreateSampleJobErrors(string jobName)
    {
        return new List<JobExecutionInfo>
        {
            new JobExecutionInfo
            {
                JobName = jobName,
                JobGroup = "DefaultGroup",
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = DateTime.Now.AddHours(-1).AddMinutes(5),
                Duration = TimeSpan.FromMinutes(5),
                WasSuccessful = false,
                ErrorMessage = "Test error 1",
                StackTrace = "Stack trace 1"
            },
            new JobExecutionInfo
            {
                JobName = jobName,
                JobGroup = "DefaultGroup",
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-2).AddMinutes(3),
                Duration = TimeSpan.FromMinutes(3),
                WasSuccessful = false,
                ErrorMessage = "Test error 2",
                StackTrace = "Stack trace 2"
            }
        };
    }

    private List<JobNextExecutionDto> CreateSampleScheduledJobs()
    {
        return new List<JobNextExecutionDto>
        {
            new JobNextExecutionDto
            {
                JobName = "TestJob1",
                JobGroup = "DefaultGroup",
                NextExecutions = new List<TriggerExecutionDto>
                {
                    new TriggerExecutionDto
                    {
                        TriggerName = "Trigger1",
                        TriggerGroup = "DefaultGroup",
                        NextExecutions = new List<DateTimeOffset>
                        {
                            DateTimeOffset.Now.AddMinutes(10),
                            DateTimeOffset.Now.AddMinutes(20)
                        }
                    }
                }
            },
            new JobNextExecutionDto
            {
                JobName = "TestJob2",
                JobGroup = "DefaultGroup",
                NextExecutions = new List<TriggerExecutionDto>
                {
                    new TriggerExecutionDto
                    {
                        TriggerName = "Trigger2",
                        TriggerGroup = "DefaultGroup",
                        NextExecutions = new List<DateTimeOffset>
                        {
                            DateTimeOffset.Now.AddMinutes(5),
                            DateTimeOffset.Now.AddMinutes(15)
                        }
                    }
                }
            }
        };
    }

    private List<JobNextExecutionDto> CreateSampleJobSchedule(string jobName)
    {
        return new List<JobNextExecutionDto>
        {
            new JobNextExecutionDto
            {
                JobName = jobName,
                JobGroup = "DefaultGroup",
                NextExecutions = new List<TriggerExecutionDto>
                {
                    new TriggerExecutionDto
                    {
                        TriggerName = "Trigger1",
                        TriggerGroup = "DefaultGroup",
                        NextExecutions = new List<DateTimeOffset>
                        {
                            DateTimeOffset.Now.AddMinutes(10),
                            DateTimeOffset.Now.AddMinutes(20)
                        }
                    }
                }
            }
        };
    }

    #endregion
}