using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using DdcOcrRestfulApiSample.Util;

namespace DdcOcrRestfulApiSample
{
    class Program
    {
        // sample entry
        static void Main()
        {
            #region setup ocr url and api key
            var strOcrBaseUri = ConfigurationManager.AppSettings["ocrBaseUri"];
            var dicHeader = new Dictionary<string, string>
            {
                {"x-api-key", ConfigurationManager.AppSettings["x-api-key"]}
            };
            #endregion

            #region 1. upload file
            Console.WriteLine("-----------------------------------------------------------------------");
            Console.WriteLine("1. Upload file...");
            
            var formData = new FormData();
            formData.Append("method", EnumOcrFileMethod.Upload.ToString());
            formData.Append("file", Comm.GetFileData("example.jpg"), "example.jpg");

            HttpWebResponse httpWebResponse;
            RestfulApiBasicResponse restfulApiResponse;
            try
            {
                httpWebResponse = HttpMultiPartRequest.Post(strOcrBaseUri, dicHeader, formData);
                restfulApiResponse = RestfulApiResponseParser.Parse(httpWebResponse, EnumOcrFileMethod.Upload);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }

            string strFileName;
            if (!HandleRestfulApiResponse(restfulApiResponse, EnumOcrFileMethod.Upload, out strFileName)) return;
            #endregion

            #region 2. recognize the uploaded file
            Console.WriteLine("{0}-----------------------------------------------------------------------", Environment.NewLine);
            Console.WriteLine("2. Recognize the uploaded file...");

            formData.Clear();
            formData.Append("method", EnumOcrFileMethod.Recognize);
            formData.Append("file_name", strFileName);
            formData.Append("language", "eng");
            formData.Append("output_format", "UFormattedTxt");
            formData.Append("page_range", "1-10");

            try
            {
                httpWebResponse = HttpMultiPartRequest.Post(strOcrBaseUri, dicHeader, formData);
                restfulApiResponse = RestfulApiResponseParser.Parse(httpWebResponse, EnumOcrFileMethod.Recognize) as RestfulApiRecognizationResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }

            if (!HandleRestfulApiResponse(restfulApiResponse, EnumOcrFileMethod.Recognize, out strFileName)) return;
            #endregion

            #region 3. download the recognized file
            Console.WriteLine("{0}-----------------------------------------------------------------------", Environment.NewLine);
            Console.WriteLine("3. Download the recognized file...");

            formData.Clear();
            formData.Append("method", EnumOcrFileMethod.Download);
            formData.Append("file_name", strFileName);

            try
            {
                httpWebResponse = HttpMultiPartRequest.Post(strOcrBaseUri, dicHeader, formData);
                restfulApiResponse = RestfulApiResponseParser.Parse(httpWebResponse, EnumOcrFileMethod.Download) as RestfulApiDownloadResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }

            if (!HandleRestfulApiResponse(restfulApiResponse, EnumOcrFileMethod.Download, out strFileName)) return;
            #endregion

            Console.ReadKey();
        }

        // handle restful api response to control ocr step and print message
        static bool HandleRestfulApiResponse(RestfulApiBasicResponse restfulApiResponse, EnumOcrFileMethod enumOcrFileMethod,
            out string strFileName)
        {
            strFileName = string.Empty;

            switch (enumOcrFileMethod)
            {
                case EnumOcrFileMethod.Upload:
                    var uploadResponse = restfulApiResponse as RestfulApiUploadResponse;
                    if (uploadResponse == null || uploadResponse.error_code != 0)
                    {
                        if (uploadResponse == null) Console.WriteLine("Upload Failed.");
                        else Console.WriteLine("Upload Failed: {0}", uploadResponse.error_msg);

                        Console.ReadKey();
                        return false;
                    }

                    strFileName = uploadResponse.name;
                    Console.WriteLine("Upload success: {0}", strFileName);
                    break;

                case EnumOcrFileMethod.Recognize:
                    var recognizationResponse = restfulApiResponse as RestfulApiRecognizationResponse;
                    if (!(recognizationResponse != null && recognizationResponse.outputs != null &&
                          recognizationResponse.outputs.Count > 0 && recognizationResponse.outputs[0].error_code == 0))
                    {
                        if (recognizationResponse != null && recognizationResponse.outputs != null &&
                            recognizationResponse.outputs.Count > 0 && recognizationResponse.outputs[0].error_code != 0)
                        {
                            Console.WriteLine("Recognization failed: {0}", recognizationResponse.outputs[0].error_msg);
                        }
                        else if (recognizationResponse != null && recognizationResponse.error_code != 0)
                        {
                            Console.WriteLine("Recognization failed: {0}", recognizationResponse.error_msg);
                        }
                        else
                        {
                            Console.WriteLine("Recognization failed.");
                        }

                        Console.ReadKey();
                        return false;
                    }

                    strFileName = recognizationResponse.outputs[0].output;
                    Console.WriteLine("Recognization success: {0}", strFileName);
                    break;

                case EnumOcrFileMethod.Download:
                    var downloadResponse = restfulApiResponse as RestfulApiDownloadResponse;
                    if (downloadResponse == null || downloadResponse.error_code != 0)
                    {
                        if (downloadResponse == null) Console.WriteLine("Download failed.");
                        else Console.WriteLine("Download failed: {0}", downloadResponse.error_msg);

                        Console.ReadKey();
                        return false;
                    }

                    // use Substring to hide BOM
                    Console.WriteLine("Result: {0}", System.Text.Encoding.Unicode.GetString(downloadResponse.buffer).Substring(1));
                    break;

                default:
                    Console.WriteLine("Unsupported ocr method.");
                    Console.ReadKey();
                    return false;
            }

            return true;
        }
    }
}
