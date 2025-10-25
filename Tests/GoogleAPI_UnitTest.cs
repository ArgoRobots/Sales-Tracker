using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Sales_Tracker.Classes;
using Sales_Tracker.Encryption;

namespace Tests
{
    [TestClass]
    public class GoogleAPI_UnitTest
    {
        private static string? _testSpreadsheetId;

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            Directories.SetUniversalDirectories();
            ArgoCompany.InitCacheFiles();
            EncryptionManager.Initialize();
            DotEnv.Load();
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static async Task ClassCleanup()
        {
            if (!string.IsNullOrEmpty(_testSpreadsheetId))
            {
                await DeleteTestSpreadsheet(_testSpreadsheetId);
            }
        }

        [TestMethod]
        public void TestCredentialsLoading()
        {
            // Test passes if credentials load without throwing an exception
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            // Verify we can create a service with the credentials
            _ = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });
        }

        [TestMethod]
        public async Task TestGoogleSheetsAccess()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Access" }
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            Assert.IsNotNull(result.SpreadsheetId, "Spreadsheet should be created");
        }

        [TestMethod]
        public async Task TestBasicOperations()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService sheetsService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create spreadsheet
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Operations" }
            };

            Spreadsheet result = await sheetsService.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            Assert.IsNotNull(result.SpreadsheetId);

            // Set permissions
            DriveService driveService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            Permission permission = new()
            {
                Type = "anyone",
                Role = "writer"
            };

            await driveService.Permissions.Create(permission, result.SpreadsheetId).ExecuteAsync();

            _testSpreadsheetId = result.SpreadsheetId;
            Assert.IsNotNull(_testSpreadsheetId);
        }

        [TestMethod]
        public async Task TestSpreadsheetCreation()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties
                {
                    Title = $"Unit Test - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                }
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            Assert.IsNotNull(result.SpreadsheetId);
            Assert.IsNotNull(result.Properties);
            Assert.IsTrue(result.Properties.Title.Contains("Unit Test"));
        }

        [TestMethod]
        public async Task TestWriteData()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create spreadsheet
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Write Data" }
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            // Write data
            ValueRange valueRange = new()
            {
                Values =
                [
                    ["Header1", "Header2"],
                    ["Value1", "Value2"]
                ]
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
                service.Spreadsheets.Values.Update(valueRange, result.SpreadsheetId, "A1:B2");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            UpdateValuesResponse updateResponse = await updateRequest.ExecuteAsync();

            Assert.AreEqual(2, updateResponse.UpdatedRows);
        }

        [TestMethod]
        public async Task TestReadData()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create and write data
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Read Data" }
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            ValueRange valueRange = new()
            {
                Values =
                [
                    ["Test", "Data"]
                ]
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest =
                service.Spreadsheets.Values.Update(valueRange, result.SpreadsheetId, "A1:B1");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateRequest.ExecuteAsync();

            // Read data
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                service.Spreadsheets.Values.Get(result.SpreadsheetId, "A1:B1");

            ValueRange readResponse = await getRequest.ExecuteAsync();

            Assert.IsNotNull(readResponse.Values);
            Assert.AreEqual(1, readResponse.Values.Count);
            Assert.AreEqual("Test", readResponse.Values[0][0]);
            Assert.AreEqual("Data", readResponse.Values[0][1]);
        }

        [TestMethod]
        public async Task TestDrivePermissions()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService sheetsService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            DriveService driveService = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create spreadsheet
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Permissions" }
            };

            Spreadsheet result = await sheetsService.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            // Set public permission
            Permission permission = new()
            {
                Type = "anyone",
                Role = "reader"
            };

            Permission permissionResult = await driveService.Permissions
                .Create(permission, result.SpreadsheetId)
                .ExecuteAsync();

            Assert.AreEqual("anyone", permissionResult.Type);
            Assert.AreEqual("reader", permissionResult.Role);
        }

        [TestMethod]
        public async Task TestMultipleSheets()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create spreadsheet with multiple sheets
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Multiple Sheets" },
                Sheets =
                [
                    new() { Properties = new SheetProperties { Title = "Sheet1" } },
                    new() { Properties = new SheetProperties { Title = "Sheet2" } }
                ]
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            Assert.IsNotNull(result.Sheets);
            Assert.AreEqual(2, result.Sheets.Count);
            Assert.AreEqual("Sheet1", result.Sheets[0].Properties.Title);
            Assert.AreEqual("Sheet2", result.Sheets[1].Properties.Title);
        }

        [TestMethod]
        public async Task TestBatchUpdate()
        {
            GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

            SheetsService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Argo Sales Tracker"
            });

            // Create spreadsheet
            Spreadsheet spreadsheet = new()
            {
                Properties = new SpreadsheetProperties { Title = "Test Batch Update" }
            };

            Spreadsheet result = await service.Spreadsheets.Create(spreadsheet).ExecuteAsync();
            _testSpreadsheetId = result.SpreadsheetId;

            // Batch update - format header row
            BatchUpdateSpreadsheetRequest batchRequest = new()
            {
                Requests =
                [
                    new()
                    {
                        RepeatCell = new RepeatCellRequest
                        {
                            Range = new GridRange
                            {
                                SheetId = 0,
                                StartRowIndex = 0,
                                EndRowIndex = 1,
                                StartColumnIndex = 0,
                                EndColumnIndex = 2
                            },
                            Cell = new CellData
                            {
                                UserEnteredFormat = new CellFormat
                                {
                                    BackgroundColor = new Color { Red = 0.8f, Green = 0.8f, Blue = 0.8f },
                                    TextFormat = new TextFormat { Bold = true }
                                }
                            },
                            Fields = "userEnteredFormat(backgroundColor,textFormat)"
                        }
                    }
                ]
            };

            BatchUpdateSpreadsheetResponse batchResponse = await service.Spreadsheets
                .BatchUpdate(batchRequest, result.SpreadsheetId)
                .ExecuteAsync();

            Assert.IsNotNull(batchResponse.Replies);
            Assert.AreEqual(1, batchResponse.Replies.Count);
        }

        private static async Task DeleteTestSpreadsheet(string spreadsheetId)
        {
            try
            {
                GoogleCredential credential = GoogleCredentialsManager.GetCredentialsFromEnvironment();

                DriveService driveService = new(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Argo Sales Tracker"
                });

                await driveService.Files.Delete(spreadsheetId).ExecuteAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}