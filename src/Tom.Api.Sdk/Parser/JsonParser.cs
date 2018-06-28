using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tom.Api.Request;
using Tom.Api.Response;
using Tom.Api.Util;

namespace Tom.Api.Parser
{
    /// <summary>
    /// JSON响应通用解释器。
    /// </summary>
    public class JsonParser<T> : IParser<T> where T : IResponse
    {
        #region IParser<T> Members
        public T Parse(string body, string charset)
        {
            T rsp = null;

            rsp = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(body);
            if (rsp == null)
            {
                rsp = Activator.CreateInstance<T>();
            }

            if (rsp != null)
            {
                rsp.Body = body;
            }

            return rsp;
        }


        public SignItem GetSignItem(IRequest<T> request, string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody))
            {
                return null;
            }

            SignItem signItem = Newtonsoft.Json.JsonConvert.DeserializeObject<SignItem>(responseBody);

            string signSourceData = GetSignSourceData(request, responseBody);
            signItem.SignSourceDate = signSourceData;

            return signItem;
        }

        #endregion


        private static string GetSignSourceData(IRequest<T> request, string body)
        {
            string rootNode = request.GetApiName().Replace(".", "_") + Constants.RESPONSE_SUFFIX;
            string errorRootNode = Constants.ERROR_RESPONSE;

            int indexOfRootNode = body.IndexOf(rootNode);
            int indexOfErrorRoot = body.IndexOf(errorRootNode);

            string result = null;
            if (indexOfRootNode > 0)
            {
                result = ParseSignSourceData(body, rootNode, indexOfRootNode);
            }
            else if (indexOfErrorRoot > 0)
            {
                result = ParseSignSourceData(body, errorRootNode, indexOfErrorRoot);
            }

            return result;
        }

        private static string ParseSignSourceData(string body, string rootNode, int indexOfRootNode)
        {
            int signDataStartIndex = indexOfRootNode + rootNode.Length + 2;
            int indexOfSign = body.IndexOf("\"" + Constants.SIGN + "\"");
            if (indexOfSign < 0)
            {
                return null;
            }

            int signDataEndIndex = indexOfSign - 1;
            int length = signDataEndIndex - signDataStartIndex;

            return body.Substring(signDataStartIndex, length);
        }


        public string EncryptSourceData(IRequest<T> request, string body, string encryptType, string encryptKey, string charset)
        {

            if (!"AES".Equals(encryptType))
            {
                throw new ApiException("API only support AES!");
            }

            EncryptParseItem item = parseEncryptData(request, body);

            string bodyIndexContent = body.Substring(0, item.startIndex);
            string bodyEndexContent = body.Substring(item.endIndex);

            //TODO 解密逻辑
            string bizContent = EncryptUtils.AesDencrypt(encryptKey, item.encryptContent, charset);

            return bodyIndexContent + bizContent + bodyEndexContent;
        }



        /// <summary>
        /// 解析加密节点
        /// </summary>
        /// <param name="request"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private static EncryptParseItem parseEncryptData(IRequest<T> request, string body)
        {
            string rootNode = request.GetApiName().Replace(".", "_") + Constants.RESPONSE_SUFFIX;
            string errorRootNode = Constants.ERROR_RESPONSE;

            int indexOfRootNode = body.IndexOf(rootNode);
            int indexOfErrorRoot = body.IndexOf(errorRootNode);

            EncryptParseItem result = null;
            if (indexOfRootNode > 0)
            {
                result = ParseEncryptItem(body, rootNode, indexOfRootNode);
            }
            else if (indexOfErrorRoot > 0)
            {
                result = ParseEncryptItem(body, errorRootNode, indexOfErrorRoot);
            }

            return result;
        }

        private static EncryptParseItem ParseEncryptItem(string body, string rootNode, int indexOfRootNode)
        {
            int signDataStartIndex = indexOfRootNode + rootNode.Length + 2;
            int indexOfSign = body.IndexOf("\"" + Constants.SIGN + "\"");

            int signDataEndIndex = indexOfSign - 1;

            if (signDataEndIndex < 0)
            {
                signDataEndIndex = body.Length - 1;
            }

            int length = signDataEndIndex - signDataStartIndex;

            string encyptContent = body.Substring(signDataStartIndex + 1, length - 2);

            EncryptParseItem item = new EncryptParseItem();
            item.encryptContent = encyptContent;
            item.startIndex = signDataStartIndex;
            item.endIndex = signDataEndIndex;


            return item;
        }
    }
}
