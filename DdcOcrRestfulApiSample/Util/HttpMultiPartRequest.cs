using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DdcOcrRestfulApiSample.Util
{
    public class HttpMultiPartRequest
    {
        public static readonly Encoding Encoding = Encoding.UTF8;
        public static readonly string StrBoundary = string.Format("DdcOrcRestfulApiSample{0}", Guid.NewGuid().ToString().Replace("-", ""));

        // post multi-part form data
        public static HttpWebResponse Post(string strUrl, Dictionary<string, string> dicHeader, FormData formData)
        {
            if(string.IsNullOrEmpty(strUrl)) throw new Exception("Url is invalid.");

            var bodyData = ConstructRequestBodyData(formData);

            var request = WebRequest.Create(strUrl) as HttpWebRequest;

            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + StrBoundary;
            request.ContentLength = bodyData.Length;
            if (dicHeader != null)
            {
                foreach (var head in dicHeader)
                {
                    request.Headers[head.Key] = head.Value;
                }
            }

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bodyData, 0, bodyData.Length);
            }

            return request.GetResponse() as HttpWebResponse;
        }

        // construct request body data
        private static byte[] ConstructRequestBodyData(FormData formData)
        {
            if (formData == null || !formData.IsValid()) return new byte[0];

            using (var smRequestBodyData = new System.IO.MemoryStream())
            {
                var bHasItemAdded = false;
                const string strNewLine = "\r\n";
                const string strBoundarySeparator = "--";

                foreach (var formDataItem in formData.GetAll())
                {
                    if (bHasItemAdded) smRequestBodyData.Write(Encoding.GetBytes(strNewLine), 0, Encoding.GetByteCount(strNewLine));

                    var strKey = formDataItem.Key ?? string.Empty;
                    var value = formDataItem.Value.Key ?? string.Empty;
                    var strFileName = formDataItem.Value.Value ?? string.Empty;

                    // write key value pair
                    if (string.IsNullOrEmpty(strFileName))
                    {
                        var strFormDataItem = string.Format(
                            "{0}{1}{2}Content-Disposition: form-data; name=\"{3}\"{2}{2}{4}",
                            strBoundarySeparator,
                            StrBoundary,
                            strNewLine,
                            strKey,
                            value);

                        smRequestBodyData.Write(Encoding.GetBytes(strFormDataItem), 0, Encoding.GetByteCount(strFormDataItem));
                    }
                    // write file data
                    else
                    {
                        // write base64 or binary data
                        var fileByte = value as byte[];

                        var strHeader =
                            string.Format(
                                "{0}{1}{2}Content-Disposition: form-data; name=\"{3}\"; filename=\"{4}\"{2}Content-Type: {5}{2}{2}",
                                strBoundarySeparator,
                                StrBoundary,
                                strNewLine,
                                strKey,
                                strFileName,
                                fileByte == null ? "text/plain" : "application/octet-stream");

                        smRequestBodyData.Write(Encoding.GetBytes(strHeader), 0, Encoding.GetByteCount(strHeader));

                        smRequestBodyData.Write(fileByte ?? Encoding.GetBytes(value.ToString()), 0, 
                            fileByte == null ? Encoding.GetByteCount(value.ToString()) : fileByte.Length);
                    }

                    bHasItemAdded = true;
                }

                if (bHasItemAdded)
                {
                    var strFooter = strNewLine + strBoundarySeparator + StrBoundary + strBoundarySeparator + strNewLine;
                    smRequestBodyData.Write(Encoding.GetBytes(strFooter), 0, Encoding.GetByteCount(strFooter));
                }

                // dump stream into a byte[]
                return Comm.ReadStreamToBytes(smRequestBodyData);
            }
        }
    }
}
