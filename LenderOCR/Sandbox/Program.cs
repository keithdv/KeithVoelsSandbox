using Google;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.DocumentAI.V1Beta2;
using Google.Cloud.Storage.V1;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoogleObject = Google.Apis.Storage.v1.Data.Object;

namespace Sandbox
{
    class Program
    {
        const string serviceAccountJsonFile = @"pk.json";
        private static HttpClient httpClient = new HttpClient();
        internal static List<object> Log = new List<object>();

        static async Task Main(string[] args)
        {
            string importBucket = @"doc-processing-import-poc";
            string exportBucket = @"documentprocessing_export_poc";


            using (httpClient)
            {

                //httpClient.BaseAddress = new Uri("https://us-documentai.googleapis.com/v1beta3");
                httpClient.DefaultRequestHeaders.Add("X-Goog-User-Project", "docsprocessing-poc");
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true, NoStore = true };

                Console.WriteLine("Hello World!");

                //await GoogleServiceAccountToken();

                var timestamp = Regex.Replace($"{DateTime.Now.ToShortDateString()}_{DateTime.Now.Hour}_{DateTime.Now.Minute}", @"[^\d]", "_");
                var dName = $@"C:\2021_Local\OCR\Google Alpha\Beta3\Single Page Image Quality\{timestamp}";
                if (!Directory.Exists(dName)) { Directory.CreateDirectory(dName); }

                string TempBucketFolder = $@"KeithVoelsTest/{timestamp}";

                // Explicitly use service account credentials by specifying 
                // the private key file.
                var credential = await GoogleCredential.FromFileAsync(serviceAccountJsonFile, CancellationToken.None);

                var storage = await StorageClient.CreateAsync(credential);

                //---------------------------------------------------------------------------------------------------------------
                // Upload files

                //await DeleteDocumentObjects(storage, importBucket, "Mock"); // Delete the files that already exist in temp folder
                //await DeleteDocumentObjects(storage, exportBucket, "Mock"); // Delete the files that already exist in temp folder

                //var files = Directory.GetFiles(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2", @"*.tif").Select(f => new FileInfo(f)).ToList();
                //files.AddRange(Directory.GetFiles(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Split", @"*.tif").Select(f => new FileInfo(f)));

                //foreach (var file in files)
                //{
                //    await UploadDocumentObject(storage, importBucket, file.FullName, $"Mock/{file.Name}");
                //}

                //await CallDocumentApi_HttpClient(files.Select(f => ($"{TempBucketFolder}{f.Name}", $"{TempBucketFolder}{f.Name}")));

                //await DownloadDocumentObjects(storage, exportBucket, TempBucketFolder, dName);

                //---------------------------------------------------------------------------------------------------------------
                // Process file already in the storage bucket

                //await DeleteDocumentObjects(storage, exportBucket, "KeithVoelsTest"); // Delete the files that already exist


                //var documentObjects = await GetAllDocumentObjects(storage, importBucket, @"Google Pilot Test Docs Batch 2 Single Page/");
                //var documentObjects = await GetAllDocumentObjects(storage, importBucket, @"Google Pilot Test Docs Batch 2/");

                //var documentNames = documentObjects.Where(d => d.Name.EndsWith("tif")).Select(d => d.Name).ToList();

                //var directoryNames = Directory.GetDirectories($@"C:\2021_Local\OCR\Google Alpha\Beta3\Mixed\", "*", SearchOption.AllDirectories).Select(d => new DirectoryInfo(d)).Select(d => d.Name).ToList();

                //////// Split and Classify
                //foreach (var f in documentNames)
                //{
                //    var shortName = Path.GetFileNameWithoutExtension(f);

                //    if (!directoryNames.Contains(shortName))
                //    {
                //        var pdr = new BatchRequest($"gs://doc-processing-import-poc/{f}", $"gs://documentprocessing_export_poc/{$"{TempBucketFolder}/{shortName}"}");
                //        await CallDocumentApi_HttpClient_BatchProcess(pdr);
                //    }
                //}

                //await DownloadDocumentObjects(storage, exportBucket, "KeithVoelsTest", dName);

                //File.WriteAllText($@"{dName}\Summary.txt", string.Join('\n', Log.Select(l => l.ToString())));

                //---------------------------------------------------------------------------------------------------------------
                // Extract

                //var fileNames = new List<(string FileName, string classification)>()
                //{
                //    ("3459817085_21_2635078027_V_2", "w2_2019")
                //};

                var files = Directory.GetFiles(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\SinglePage", @"*.tif").Select(f => new FileInfo(f)).ToList();

                //var files = documentNames.Select(x => new FileInfo($@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Form1040\{x}"));

                //var files = Directory.GetFiles(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2", @"*.tif").Select(f => new FileInfo(f)).ToList();
                //files.AddRange(Directory.GetFiles(@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Split", @"*.tif").Select(f => new FileInfo(f)));


                foreach (var doc in files)
                {
                    //var doc = new FileInfo($@"C:\2021_Local\OCR\Documents\Google Pilot Test Docs Batch 2\Visually Split\{fileName.FileName}.tif");

                    if (!doc.Exists) { throw new FileNotFoundException(); }
                    var pdr = new Request(doc);
                    try
                    {
                        //var type = DocumentType.Form1040;

                        //if (fileName.classification.Contains("w2"))
                        //{
                        //    type = DocumentType.W2;
                        //}

                        var json = await CallDocumentApi_HttpClient_Process(pdr, DocumentType.ImageQuality);

                        var result = new FileInfo($@"{dName}\{doc.Name}");
                        if (!result.Directory.Exists) { result.Directory.Create(); }
                        await File.WriteAllTextAsync($@"{dName}\{ Path.GetFileNameWithoutExtension(result.Name) }.json", json);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exceptions: {ex.Message}");
                        await File.WriteAllTextAsync($@"{dName}\{Path.GetFileNameWithoutExtension(doc.Name)}.json", ex.Message);
                    }
                }


                //---------------------------------------------------------------------------------------------------------------
                // Process file already in the storage bucket
                // One Off - Download Documents

                //await DownloadDocumentObjects(storage, exportBucket, @"KeithVoelsTest", dName);

            }
        }

        public enum DocumentType { W2, Form1040, ImageQuality, Classify };

        private static async Task DeleteDocumentObject(StorageClient storage, string bucket, string remoteFileName)
        {
            using (new Timing($"Delete {bucket}:{remoteFileName}"))
            {
                var document = await storage.GetObjectAsync(bucket, remoteFileName);

                await storage.DeleteObjectAsync(document);
            }
        }

        private static async Task DeleteDocumentObjects(StorageClient storage, string bucket, string remotePrefix)
        {
            using (new Timing($"Delete All {bucket}:{remotePrefix}"))
            {
                var documents = await GetAllDocumentObjects(storage, bucket, remotePrefix);

                foreach (var doc in documents)
                {
                    await storage.DeleteObjectAsync(doc);
                }
            }
        }

        private static async Task UploadDocumentObject(StorageClient storage, string bucket, string localFileName, string remoteFileName)
        {

            try
            {
                await DeleteDocumentObject(storage, bucket, remoteFileName);
            }
            catch (GoogleApiException) { }
            {
                using (new Timing($"Upload {localFileName}"))
                {
                    using (var readStream = File.OpenRead(localFileName))
                    {
                        await storage.UploadObjectAsync(bucket, remoteFileName, "image/tiff", readStream);
                    }
                }
            }
        }

        private static async Task DownloadDocumentObjects(StorageClient storage, string bucket, string remotePrefix, string localDirectory)
        {
            using (new Timing($"Download {localDirectory}"))
            {
                var docs = await GetAllDocumentObjects(storage, bucket, remotePrefix);

                foreach (var d in docs.Select(d => d.Name).ToList())
                {
                    var fileInfo = new FileInfo($@"{localDirectory}\{d}");

                    if (!Directory.Exists(fileInfo.DirectoryName)) { Directory.CreateDirectory(fileInfo.DirectoryName); }

                    using (var writeStream = File.Create(fileInfo.FullName))
                    {
                        await storage.DownloadObjectAsync(bucket, d, writeStream);
                    }

                }
            }
        }

        private static async Task<List<GoogleObject>> GetAllDocumentObjects(StorageClient storage, string bucket, string prefix)
        {
            using (new Timing($"GetAllDocumentObjects in {bucket} with '{prefix}'"))
            {


                var gObjects = new List<GoogleObject>();

                var objects = storage.ListObjectsAsync(bucket, prefix);

                var e = objects.GetAsyncEnumerator();

                while (await e.MoveNextAsync())
                {
                    gObjects.Add(e.Current);
                }

                return gObjects;
            }
        }


        //private static async Task CallDocumentApi_GoogleClient()
        //{
        //    //          Grpc.Core.RpcException
        //    //HResult = 0x80131500
        //    //Message = Status(StatusCode = "PermissionDenied", Detail = "Cloud Document AI API has not been used in project 112362353145 before or it is disabled. Enable it by visiting https://console.developers.google.com/apis/api/documentai.googleapis.com/overview?project=112362353145 then retry. If you enabled this API recently, wait a few minutes for the action to propagate to our systems and retry.", DebugException = "Grpc.Core.Internal.CoreErrorDetailException: {"created":"@1607100712.682000000","description":"Error received from peer ipv4:172.217.1.42:443","file":"T:\src\github\grpc\workspace_csharp_ext_windows_x64\src\core\lib\surface\call.cc","file_line":1062,"grpc_message":"Cloud Document AI API has not been used in project 112362353145 before or it is disabled.Enable it by visiting https://console.developers.google.com/apis/api/documentai.googleapis.com/overview?project=112362353145 then retry. If you enabled this API recently, wait a few minutes for the action to propagate to our systems and retry.","grpc_status":7}")
        //    //      }

        //    var builder = new DocumentUnderstandingServiceClientBuilder() { CredentialsPath = serviceAccountJsonFile };

        //    var docAi = await builder.BuildAsync();


        //    var result = await docAi.ProcessDocumentAsync(new ProcessDocumentRequest()
        //    {
        //        DocumentType = "lending_doc_split_and_classify",
        //        InputConfig = new InputConfig()
        //        {
        //            MimeType = "image/tiff",
        //            GcsSource = new GcsSource() { Uri = "gs://doc-processing-import-poc/3457721808_35_2635352504.tif" }
        //        },
        //        OutputConfig = new OutputConfig()
        //        {
        //            GcsDestination = new GcsDestination() { Uri = "gs://documentprocessing_export_poc/3459267114_35_2635112976.tif" }
        //        }
        //    });
        //}


        private static async Task CallDocumentApi_HttpClient_BatchProcess(BatchRequest pdr)
        {

            ProcessDocumentResponse pdrResponse;


            using (new Timing($"DocumentAPI - All"))
            {
                var token = await GoogleServiceAccountToken();

                using (new Timing($"DocumentAPI - Post"))
                {

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                    IContractResolver contractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };

                    var body = JsonConvert.SerializeObject(pdr, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ContractResolver = contractResolver });

                    var bodyContent = new StringContent(body, Encoding.UTF8, "application/json");

                    using (var httpResponse = await httpClient.PostAsync(@"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/4a130aa1feaae77f:batchProcess", bodyContent))
                    {
                        var content = await httpResponse.Content.ReadAsStringAsync();
                        if (httpResponse.IsSuccessStatusCode)
                        {
                            pdrResponse = JsonConvert.DeserializeObject<ProcessDocumentResponse>(content, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ContractResolver = contractResolver })!;
                        }
                        else
                        {
                            throw new Exception(content);
                        }
                    }
                }



                await Task.Delay(30000);

                int calls = 0;

                using (new Timing($"DocumentAPI - Get"))
                {
                    bool cont = false;

                    do
                    {
                        cont = false;
                        await Task.Delay(15000);
                        using (var httpResponse = await httpClient.GetAsync($"https://us-documentai.googleapis.com/v1beta3/{pdrResponse.Name}"))
                        {
                            calls++;
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                var content = await httpResponse.Content.ReadAsStringAsync();
                                pdrResponse = JsonConvert.DeserializeObject<ProcessDocumentResponse>(content);
                                if (pdrResponse.Metadata.State == OperationMetadata.Types.State.Running)
                                {
                                    cont = true;
                                }
                            }
                            else
                            {
                                throw new Exception(httpResponse.StatusCode.ToString());
                            }
                        }
                    } while (cont);
                }

                Log.Add($"DocumentAPI - Get #calls {calls}");

            }

        }

        private static async Task<string> CallDocumentApi_HttpClient_Process(Request pdr, DocumentType type)
        {
            string content = string.Empty;

            Timing timing;
            using (timing = new Timing($"DocumentAPI - Process - All"))
            {
                var token = await GoogleServiceAccountToken();


                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                IContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };

                var body = JsonConvert.SerializeObject(pdr, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ContractResolver = contractResolver });

                var bodyContent = new StringContent(body, Encoding.UTF8, "application/json");

                string uri = string.Empty;

                if (type == DocumentType.W2)
                {
                    uri = @"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/ce888e90e98d9c44:process";
                }
                else if (type == DocumentType.Form1040)
                {
                    uri = @"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/7b132113fe97a250:process";
                }
                else if (type == DocumentType.ImageQuality)
                {
                    uri = @"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/58e4c4aed33d6875:process";
                }
                else if (type == DocumentType.Classify)
                {
                    uri = @"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/4a130aa1feaae77f:process";
                }

                // Most files too big
                //using (var httpResponse = await httpClient.PostAsync(@"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/4a130aa1feaae77f:process", bodyContent)) // Classify

                //using (var httpResponse = await httpClient.PostAsync(@"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/ce888e90e98d9c44:process", bodyContent)) // W2

                //using (var httpResponse = await httpClient.PostAsync(@"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/7b132113fe97a250:process", bodyContent)) // 1040

                //using (var httpResponse = await httpClient.PostAsync(@"https://us-documentai.googleapis.com/v1beta3/projects/112362353145/locations/us/processors/58e4c4aed33d6875:process", bodyContent)) // Image Quality

                using (var httpResponse = await httpClient.PostAsync(uri, bodyContent))
                {
                    content = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new Exception(content);
                    }
                }

            }

            content = content.Insert(content.IndexOf("{") + 1, $" \"timing\": {{ \"duration\" : {(timing.End!.Value - timing.Start).TotalSeconds} }}, ");

            return content;
        }


        static ServiceAccountCredential? credential;

        private static async Task<string> GoogleServiceAccountToken()
        {
            using (new Timing($"GoogleServiceAccountToken"))
            {
                if (credential is null)
                {
                    String serviceAccountEmail = "docprocessing@docsprocessing-poc.iam.gserviceaccount.com";
                    string privateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDLP2xIQ+G6/+Yj\nYj4ztHZWVzDbIZHxFgWIXn5CEj5ACw5yKegrktwKEXNAa0zwARsz897HC6PMXySy\n5/bQ8yLdmooz/uaYaeoLqpAbC7uaIC/jphPXNf+RYbMeN2q6n4lxDnqBdKiyjOKh\n8XuTDdICH3MBkuTJfSa32PiI865H0FKl1My5A+/eYV6dunSynHJpjF7/GfBTaTab\nehHF2gPrTYcNH7T4quMiavCY4+/k3KFofE2pL70uJzyu5QgzNTvgHFb/NLzBYGGp\nhrQ6gkf3Skz4MQPgW4lbSBcRQeHQo9RSZkYF2nsJVNJwlWuERzvCQtehEOlDlJr6\nFlLs/uFlAgMBAAECggEAPsz4DlQ7OWa8m45NnfhS4FbMl8kFqTKevwZDiPLOHUfN\nTOU6Acy9BLdjnzIIcdhFqYXe/i3QjOORGV6nWuTljwejUHGgmtOPJ8+p+1FixDrR\n0UuNsd/Tef5wNBP3fHazJyXMIJgFUDZMCLHu9v88Nr+073WOD9wFzKTUFq0E2tcs\nI8mrC5OgLG+0hXu7XEujORPG+9Qqdv50n9T1x0AKqbhK+pE0UECmcjr/SHESj5Ex\noi5HP23eDiJQT3yFKckX+BfZHeE73MDL/tD0lRE1orgsO8zKLv/XLD4oRR6cOE8/\nCdQ6u2xFFt/ihOYOV2GAYc9SxiXdcdQxHEiN+MFoeQKBgQD8UguPR49KNPqWAt6u\nWYHoYuZjSbr7gCG0uPWAeTnusa+8qFCv9H8eNEq771dWz4hkVDDoXtlOgJemDaE4\nj0Ae7PDqGiwZqe2tH7rQhFaomEN9Lp50b0TmX3D2BA203jjjb3MEbZmbegAJgSir\nL6ZWRuSlSxqZIsb2g+WFHCP4xwKBgQDONi5XCkNsI9PylicI0biE9cvvBVrU05Iz\nc0yLE6mxIQDEuCRq6t/1ikLgS/59UD5nQsR829UpeczLk7o7n9U7rh8Yx0UlE5i5\nCzAL/RgyEc/VEnQEe9vs1k1Zsl9rYv0XJyAAcUC9iduH3qd+2i4pLP3Oi/Xp4jro\nZMY0H5jgcwKBgQCwPlrGtPRYoBLcz5pdbDX6bYKBndGWtNRWWM9a8tJNcR3QiDz7\n+qsEHQCKr05xW7roYYpb9UySse27Vk/jouPl9hj9XFSrhG94+u3Rkm65ismxWevi\nZopY5BeSMBim2oYgDwvm7utZl2kJOod+s2TbZN92ubQVfR4+uLiMNrDFpwKBgHZz\nHuk2ZwYeCmgFIgTp2rDdM7hnfgZURV2ydBxLPiUVzQgyshCMO/sh4UpPvK4kwsOz\n0YPbDrWVVjL193q6U9TFLu2fwTML76UTLRUl25kfLB+7StMshmajrqjAUhkwMirz\neWlnpIV7Q5PnLJUsJGnYgy36rVYccjP9dvCH2dvLAoGAAfeWTL3gWPM8V/BEXTC0\nZPuyOys2VYR0X1BUVm4phn5ckKSK1VkeZVIKgl9x8mAiRIULvM4tl+CR7zw7Cyjn\nSO4tzlNAxXScfnXUhN/6MFwTZvN1y8wjtld4z2M5vG4jfqNZ+C+QW9gdF5AEi6+v\ngkii4CC8BygSpfjVXuqGlU4=\n-----END PRIVATE KEY-----\n";

                    credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = new[] { "https://www.googleapis.com/auth/cloud-platform" }
                    }.FromPrivateKey(privateKey));
                }

                return await credential.GetAccessTokenForRequestAsync();
            }
        }
    }



    //public class ProcessDocumentRequest
    //{
    //    public ProcessDocumentRequest(string sourceUri, string documentType)
    //    {
    //        Requests = new List<Request>();

    //        Requests.Add(new Request(sourceUri, documentType));
    //    }
    //    public ProcessDocumentRequest(string sourceUri, string destinationUri, string documentType)
    //    {
    //        Requests = new List<Request>();

    //        Requests.Add(new Request(sourceUri, destinationUri, documentType));
    //    }

    //    public ProcessDocumentRequest(IEnumerable<(string sourceUri, string destinationUri, string documentType)> requests)
    //    {
    //        Requests = new List<Request>();

    //        foreach (var r in requests)
    //        {
    //            Requests.Add(new Request(r.sourceUri, r.destinationUri, r.documentType));
    //        }
    //    }

    //    public List<Request> Requests { get; }
    //}

    public class BatchRequest
    {
        //public Request(string sourceUri)
        //{
        //    InputConfig = new List<InputConfig>() { new InputConfig(sourceUri) };
        //}

        public BatchRequest(string sourceUri, string destinationUri)
        {
            InputConfigs = new List<InputConfig>() { new InputConfig(sourceUri) };
            OutputConfig = new OutputConfig(destinationUri);
        }

        public List<InputConfig> InputConfigs { get; }
        public OutputConfig? OutputConfig { get; }

    }

    public class InputConfig
    {
        public InputConfig(string sourceUri)
        {
            GcsSource = sourceUri;
        }
        public string GcsSource { get; set; }
        public string MimeType { get; } = @"image/tiff";
    }

    //public class GcsSource
    //{
    //    public GcsSource(string uri)
    //    {
    //        Uri = uri;
    //    }

    //    public string Uri { get; }
    //}

    public class OutputConfig
    {
        public OutputConfig(string destinationUri)
        {
            GcsDestination = destinationUri;
        }
        public string GcsDestination { get; }
    }

    //public class GcsDestination
    //{
    //    public GcsDestination(string uri)
    //    {
    //        Uri = uri;
    //    }

    //    public string Uri { get; }
    //}

    public class ProcessDocumentResponse
    {
        public string Name { get; set; } = null!;
        public bool Done { get; set; }
        public Metadata_ Metadata { get; set; } = null!;
        public class Metadata_
        {
            public Google.Cloud.DocumentAI.V1Beta2.OperationMetadata.Types.State State { get; set; }
        }
    }

    public class Request
    {
        public Request(FileInfo fn)
        {
            var bytes = File.ReadAllBytes(fn.FullName);
            var content = Convert.ToBase64String(bytes);

            Document = new Document(content);
        }

        public Document Document { get; set; }

    }

    public class Document
    {
        public Document(string content)
        {
            Content = content;
        }

        public string MimeType { get; } = "image/tiff";
        public string Content { get; }
    }

    public class Timing : IDisposable
    {
        public Timing(string description)
        {
            Description = description;
            Program.Log.Add(this);
        }

        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? End { get; set; }
        public string Description { get; }

        public override string ToString()
        {
            return $"{Description} :: {((End ?? DateTime.Now) - Start).TotalSeconds}";
        }

        public void Dispose()
        {
            End = DateTime.Now;
            Console.WriteLine(this.ToString());
        }
    }
}