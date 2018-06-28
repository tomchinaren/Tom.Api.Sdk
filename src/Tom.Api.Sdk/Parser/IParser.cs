using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tom.Api.Request;
using Tom.Api.Response;

namespace Tom.Api.Parser
{
    public interface IParser<T> where T : IResponse
    {
        /// <summary>
        /// 把响应字符串解释成相应的领域对象。
        /// </summary>
        /// <param name="body">响应字符串</param>
        /// <returns>领域对象</returns>
        T Parse(string body, string charset);

        /// <summary>
        /// 解析签名内容
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseBody"></param>
        /// <returns></returns>
        SignItem GetSignItem(IRequest<T> request, string responseBody);


        /// <summary>
        /// 将响应串解密
        /// </summary>
        /// <param name="request"></param>
        /// <param name="body"></param>
        /// <param name="encryptType"></param>
        /// <param name="encryptKey"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        string EncryptSourceData(IRequest<T> request, string body, string encryptType, string encryptKey, string charset);
    }
}
