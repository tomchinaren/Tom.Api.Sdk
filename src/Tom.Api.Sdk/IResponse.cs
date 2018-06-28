using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tom.Api.Response
{
    [Serializable]
    public abstract class IResponse
    {
        private string code;
        private string msg;
        private string subCode;
        private string subMsg;
        private string body;
        private string requestBody;
        private string sign;

        /// <summary>
        /// 错误码
        /// 对应 ErrCode
        /// </summary>
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// 错误信息
        /// 对应 ErrMsg
        /// </summary>
        public string Msg
        {
            get { return msg; }
            set { msg = value; }
        }

        /// <summary>
        /// 子错误码
        /// 对应 SubErrCode
        /// </summary>
        public string SubCode
        {
            get { return subCode; }
            set { subCode = value; }
        }

        /// <summary>
        /// 子错误信息
        /// 对应 SubErrMsg
        /// </summary>
        public string SubMsg
        {
            get { return subMsg; }
            set { subMsg = value; }
        }

        /// <summary>
        /// 响应原始内容
        /// </summary>
        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        /// <summary>
        /// 响应结果是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return !string.IsNullOrEmpty(this.SubCode);
            }
        }

        /// <summary>
        /// 请求原始内容
        /// </summary>
        public string RequestBody
        {
            get { return requestBody; }
            set { requestBody = value; }
        }

        public string Sign
        {
            get { return sign; }
            set { sign = value; }
        }
    }
}
