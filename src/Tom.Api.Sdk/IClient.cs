using Tom.Api.Response;

namespace Tom.Api.Request
{
    public interface IClient
    {
        /// <summary>
        /// 执行公开API请求。
        /// </summary>
        /// <typeparam name="T">领域对象</typeparam>
        /// <param name="request">具体的 API请求</param>
        /// <returns>领域对象</returns>
        T Execute<T>(IRequest<T> request) where T : IResponse;

        /// <summary>
        /// 执行隐私API请求。
        /// </summary>
        /// <typeparam name="T">领域对象</typeparam>
        /// <param name="request">具体的 API请求</param>
        /// <param name="session">用户会话码</param>
        /// <returns>领域对象</returns>
        T Execute<T>(IRequest<T> request, string session) where T : IResponse;


        /// <summary>
        /// 执行隐私API请求。
        /// </summary>
        /// <typeparam name="T">领域对象</typeparam>
        /// <param name="request">具体的 API请求</param>
        /// <param name="session">用户会话码</param>
        /// <param name="appAuthToken">应用授权码</param>
        /// <returns>领域对象</returns>
        T Execute<T>(IRequest<T> request, string session, string appAuthToken) where T : IResponse;

    }

}
