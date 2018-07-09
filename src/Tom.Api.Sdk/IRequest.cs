using System.Collections.Generic;
using Tom.Api.Response;

namespace Tom.Api.Request
{
    public interface IRequest<T> where T: IResponse
    {
        string Biz_Content { get; set; }
        bool Debug { get; set; }
        string Session { get; set; }


        /// <summary>
        /// 获取的API名称。
        /// </summary>
        /// <returns>API名称</returns>
        string GetApiName();

        /// <summary>
        /// 接口版本
        /// </summary>
        /// <returns>接口版本</returns>
        string Version { get; set; }

        /// <summary>
        /// 获取所有的Key-Value形式的文本请求参数字典。其中：
        /// Key: 请求参数名
        /// Value: 请求参数文本值
        /// </summary>
        /// <returns>文本请求参数字典</returns>
        IDictionary<string, string> GetParameters();

        /// <summary>
        /// 设置请求是否需要加密
        /// </summary>
        void SetNeedEncrypt(bool needEncrypt);

        /// <summary>
        /// 获取请求是否需要加密
        /// </summary>
        /// <returns>结果是否加密</returns>
        bool GetNeedEncrypt();

        /// <summary>
        /// 获取BizModel
        /// </summary>
        /// <returns></returns>
        IObject GetBizModel();

        /// <summary>
        /// 设置BizModel
        /// </summary>
        /// <param name="bizModel"></param>
        void SetBizModel(IObject bizModel);


    }
}
