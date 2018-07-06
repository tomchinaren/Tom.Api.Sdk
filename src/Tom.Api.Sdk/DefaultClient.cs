using System;
using System.Collections.Generic;
using Tom.Api.Parser;
using Tom.Api.Response;
using Tom.Api.Util;

namespace Tom.Api.Request
{
    public class DefaultClient : IClient
    {
        public const string APP_ID = "app_id";
        public const string FORMAT = "format";
        public const string METHOD = "method";
        public const string TIMESTAMP = "timestamp";
        public const string VERSION = "version";
        public const string SIGN_TYPE = "sign_type";
        public const string ACCESS_TOKEN = "auth_token";
        public const string SIGN = "sign";
        public const string TERMINAL_TYPE = "terminal_type";
        public const string TERMINAL_INFO = "terminal_info";
        public const string PROD_CODE = "prod_code";
        public const string NOTIFY_URL = "notify_url";
        public const string CHARSET = "charset";
        public const string ENCRYPT_TYPE = "encrypt_type";
        public const string BIZ_CONTENT = "biz_content";
        public const string APP_AUTH_TOKEN = "app_auth_token";
        public const string RETURN_URL = "return_url";

        private string serverUrl;
        private string version;
        private string httpmethod;
        private string format;
        private string charset = "utf-8";
        private string signType = "RSA";
        private string encyptKey;
        private string encyptType = "AES";
        private string privateKeyPem;
        private string publicKey;
        private bool keyFromFile = false;

        private string appId;

        public string Version
        {
            get { return version != null ? version : "1.0"; }
            set { version = value; }
        }

        public string Format
        {
            get { return format != null ? format : "json"; }
            set { format = value; }
        }

        public string AppId
        {
            get { return appId; }
            set { appId = value; }
        }

        private WebUtils webUtils;


        #region Constructors
        public DefaultClient(string serverUrl, string signType, string encyptKey)
        {
            this.serverUrl = serverUrl;
            this.signType = signType;
            this.encyptKey = encyptKey;
            this.webUtils = new WebUtils();
        }
        public DefaultClient(string serverUrl, string version, string httpmethod, string format, string charset, string signType, string encyptKey)
        {
            this.serverUrl = serverUrl;
            this.version = version;
            this.httpmethod = httpmethod;
            this.format = format;
            this.charset = charset;

            this.signType = signType;
            this.encyptKey = encyptKey;
            this.webUtils = new WebUtils();
        }
        #endregion


        #region IClient Members
        public T Execute<T>(IRequest<T> request) where T : IResponse
        {
            return Execute<T>(request, null);
        }

        public T Execute<T>(IRequest<T> request, string session) where T : IResponse
        {
            return Execute<T>(request, session, null);
        }

        public T Execute<T>(IRequest<T> request, string session, string appAuthToken) where T : IResponse
        {
            // 构造请求参数
            ParamDictionary requestParams = buildRequestParams(request, session, appAuthToken);

            // 字典排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(requestParams);
            ParamDictionary sortedDic = new ParamDictionary(sortedParams);

            // 参数签名
            string charset = string.IsNullOrEmpty(this.charset) ? "utf-8" : this.charset;
            string signResult = Signature.RSASign(sortedDic, privateKeyPem, charset, this.keyFromFile, this.signType);
            // 添加签名结果参数
            sortedDic.Add(SIGN, signResult);

            // 参数拼接
            string signedResult = WebUtils.BuildQuery(sortedDic, charset);
            var txtParams = sortedDic;


            // 是否需要上传文件
            string body;
            string requestBody = null;
            string url = "";// this.serverUrl + "?" + CHARSET + "=" + this.charset;
            url = GetFullUrl(this.serverUrl, request.GetApiName());// + "?" + CHARSET + "=" + this.charset;
            if (request is IUploadRequest<T>)
            {
                IUploadRequest<T> uRequest = (IUploadRequest<T>)request;
                IDictionary<string, FileItem> fileParams = SdkUtils.CleanupDictionary(uRequest.GetFileParameters());
                body = webUtils.DoPost(url, txtParams, fileParams, this.charset, out requestBody);
            }
            else
            {
                body = webUtils.DoPost(url, txtParams, this.charset, out requestBody);
            }

            T rsp = null;
            IParser<T> parser = null;
            if ("xml".Equals(format))
            {
                parser = new XmlParser<T>();
                rsp = parser.Parse(body, charset);
            }
            else
            {
                parser = new JsonParser<T>();
                rsp = parser.Parse(body, charset);
            }

            ResponseParseItem item = parseRespItem(request, body, parser, this.encyptKey, this.encyptType, charset);
            rsp = parser.Parse(item.realContent, charset);
            rsp.RequestBody = requestBody;

            CheckResponseSign(request, item.respContent, rsp.IsError, parser, this.publicKey, this.charset, signType, this.keyFromFile);

            return rsp;
        }

        #endregion

        private string GetFullUrl(string serverUrl, string apiName)
        {
            return string.Format("{0}{1}", serverUrl, apiName.Replace("tom.chinesechess.", "").Replace(".", "/"));
        }

        public static void CheckResponseSign<T>(IRequest<T> request, string responseBody, bool isError, IParser<T> parser, string publicKey, string charset, string signType, bool keyFromFile) where T : IResponse
        {
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(charset))
            {
                return;
            }

            SignItem signItem = parser.GetSignItem(request, responseBody);
            if (signItem == null)
            {
                throw new ApiException("sign check fail: Body is Empty!");
            }

            if (!isError ||
                (isError && !string.IsNullOrEmpty(signItem.Sign)))
            {
                bool rsaCheckContent = Signature.RSACheckContent(signItem.SignSourceDate, signItem.Sign, publicKey, charset, signType, keyFromFile);
                if (!rsaCheckContent)
                {
                    if (!string.IsNullOrEmpty(signItem.SignSourceDate) && signItem.SignSourceDate.Contains("\\/"))
                    {
                        string srouceData = signItem.SignSourceDate.Replace("\\/", "/");
                        bool jsonCheck = Signature.RSACheckContent(srouceData, signItem.Sign, publicKey, charset, signType, keyFromFile);
                        if (!jsonCheck)
                        {
                            throw new ApiException("sign check fail: check Sign and Data Fail JSON also");
                        }
                    }
                    else
                    {
                        throw new ApiException("sign check fail: check Sign and Data Fail!");
                    }
                }
            }
        }

        private static ResponseParseItem parseRespItem<T>(IRequest<T> request, string respBody, IParser<T> parser, string encryptKey, string encryptType, string charset) where T : IResponse
        {
            string realContent = null;

            if (request.GetNeedEncrypt())
            {
                realContent = parser.EncryptSourceData(request, respBody, encryptType, encryptKey, charset);
            }
            else
            {
                realContent = respBody;
            }

            ResponseParseItem item = new ResponseParseItem();
            item.realContent = realContent;
            item.respContent = respBody;

            return item;

        }

        private ParamDictionary buildRequestParams<T>(IRequest<T> request, string accessToken, string appAuthToken) where T : IResponse
        {
            // 默认参数
            ParamDictionary oriParams = new ParamDictionary(request.GetParameters());
            // 序列化BizModel
            ParamDictionary result = SerializeBizModel(oriParams, request);

            // 获取参数
            string charset = string.IsNullOrEmpty(this.charset) ? "utf-8" : this.charset;
            string apiVersion = string.IsNullOrEmpty(request.GetApiVersion()) ? this.Version : request.GetApiVersion();

            // 添加协议级请求参数，为空的参数后面会自动过滤，这里不做处理。
            result.Add(METHOD, request.GetApiName());
            result.Add(VERSION, apiVersion);
            result.Add(APP_ID, appId);
            result.Add(FORMAT, format);
            result.Add(TIMESTAMP, DateTime.Now);
            result.Add(ACCESS_TOKEN, accessToken);
            result.Add(SIGN_TYPE, signType);
            result.Add(CHARSET, charset);
            result.Add(APP_AUTH_TOKEN, appAuthToken);

            if (request.GetNeedEncrypt())
            {
                if (string.IsNullOrEmpty(result[BIZ_CONTENT]))
                {
                    throw new ApiException("api request Fail ! The reason: encrypt request is not supported!");
                }

                if (string.IsNullOrEmpty(this.encyptKey) || string.IsNullOrEmpty(this.encyptType))
                {
                    throw new ApiException("encryptType or encryptKey must not null!");
                }

                if (!"AES".Equals(this.encyptType))
                {
                    throw new ApiException("api only support Aes!");
                }

                string encryptContent = EncryptUtils.AesEncrypt(this.encyptKey, result[BIZ_CONTENT], this.charset);
                result.Remove(BIZ_CONTENT);
                result.Add(BIZ_CONTENT, encryptContent);
                result.Add(ENCRYPT_TYPE, this.encyptType);
            }

            return result;
        }
        private ParamDictionary SerializeBizModel<T>(ParamDictionary requestParams, IRequest<T> request) where T : IResponse
        {
            ParamDictionary result = requestParams;
            Boolean isBizContentEmpty = !requestParams.ContainsKey(BIZ_CONTENT) || string.IsNullOrEmpty(requestParams[BIZ_CONTENT]);
            if (isBizContentEmpty && request.GetBizModel() != null)
            {
                IObject bizModel = request.GetBizModel();
                string content = Serialize(bizModel);
                result.Add(BIZ_CONTENT, content);
            }
            return result;
        }

        /// <summary>
        /// IObject序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string Serialize(IObject obj)
        {
            //导出string格式的Json
            string result = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return result;
        }

    }
}
