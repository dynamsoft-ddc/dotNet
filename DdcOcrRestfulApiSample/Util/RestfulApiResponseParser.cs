using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace DdcOcrRestfulApiSample.Util
{
    #region basic response
    public class RestfulApiBasicResponse
    {
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public string request_id { get; set; }
    }
    #endregion

    #region upload api response
    public class RestfulApiUploadResponse : RestfulApiBasicResponse
    {
        public string name { get; set; }
        public int size { get; set; }
        public string ctime { get; set; }
        public string mtime { get; set; }
        public string md5 { get; set; }
        public int page_number { get; set; }
    }
    #endregion

    #region recognizatio api response
    public class RestfulApiRecognizationResponse : RestfulApiBasicResponse
    {
        public List<RestfulApiRecognizationSingleItemResponse> outputs { get; set; }
    }

    public class RestfulApiRecognizationSingleItemResponse
    {
        public string output { get; set; }
        public int size { get; set; }
        public string ctime { get; set; }
        public string mtime { get; set; }
        public string md5 { get; set; }
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public List<string> input { get; set; }
    }
    #endregion

    #region download api response
    public class RestfulApiDownloadResponse : RestfulApiBasicResponse
    {
        public byte[] buffer { get; set; }
    } 
    #endregion

    public class RestfulApiResponseParser
    {
        public static RestfulApiBasicResponse Parse(HttpWebResponse httpWebResponse, EnumOcrFileMethod enumOcrFileMethod)
        {
            if (httpWebResponse == null) throw new Exception("HttpWebResponse is null.");

            if (httpWebResponse.StatusCode != HttpStatusCode.OK) 
                throw new Exception(string.Format("Request failed, status code is: {0}", Convert.ToInt32(httpWebResponse.StatusCode)));

            var strResponse = string.Empty;

            if (httpWebResponse.ContentType.ToLower().Trim().Contains("application/json"))
            {
                using (var stream = httpWebResponse.GetResponseStream())
                {
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    strResponse = reader.ReadToEnd();
                }
            }

            switch (enumOcrFileMethod)
            {
                case EnumOcrFileMethod.Upload:
                    return JsonConvert.DeserializeObject<RestfulApiUploadResponse>(strResponse);

                case EnumOcrFileMethod.Recognize:
                    return JsonConvert.DeserializeObject<RestfulApiRecognizationResponse>(strResponse);

                case EnumOcrFileMethod.Download:
                    if (string.IsNullOrEmpty(strResponse))
                    {
                        using (var stream = httpWebResponse.GetResponseStream())
                        {
                            return new RestfulApiDownloadResponse { buffer = Comm.ReadStreamToBytes(stream) };
                        }
                    }

                    return JsonConvert.DeserializeObject<RestfulApiDownloadResponse>(strResponse);
                    
                default:
                    throw new Exception("Unsupported ocr method.");
            }
        }
    }
}
