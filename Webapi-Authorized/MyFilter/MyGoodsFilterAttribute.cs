using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http; //引入命名空间
using System.Web.Http.Controllers;
using Webapi_Authorized.Models;
using System.Threading;
using System.Security.Principal;
namespace Webapi_Authorized.MyFilter
{
    public class MyGoodsFilterAttribute:AuthorizeAttribute  //1、要继承授权的类
     {
        /// <summary>
        /// 指示指定的控件是否已获得授权
        /// </summary>
        /// <param name="actionContext">上下文</param>
        /// <returns>如果获得授权，则true ，否则false</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            //1、获取授权对象（请求头部中包含得Authrization对象）
            var auth = actionContext.Request.Headers.Authorization;
            if (auth!=null)
            {
                //不为空
                if (auth.Scheme.ToLower()=="basic"&&auth.Parameter!="")
                {
                    //认证的方式位basic认证，并且参数值不为空
                    //可以来进行验证你的令牌或者票据是否是正确的，如果正确返回true
                    //否则false
                    string token = auth.Parameter;
                   return CheckToken(token);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //为空 授权失败
                return false;
            }
               
        }
        //令牌或票据是在有效期内，如果过了有效期 则失效 不能授权
        //需要重新进行登录，然后重新生成新的票据/令牌
        //我们需要一个表 来存储对应用户的生成token
        /// <summary>
        /// 验证令牌或票据是否正确，而且是否在有效期内
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool CheckToken(string token)
        {
            DataClasses1DataContext goods = new DataClasses1DataContext();
            //可以来进行验证token值是否正确，如果正确还可以获取到对应的uid
            var rstoken = goods.TokenInfo.FirstOrDefault(x => x.Token == token);
            if (rstoken!=null)
            {
                //不为空，意味着查询到，但是不知道是否过期
                if (rstoken.ExpireData>DateTime.Now)
                {
                    //验证是否过了有效期 在有效期内
                    //还可以来进一步设置一个传递一个身份的id
                    //利用线程中的身份表示来存储用户信息（id）
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(rstoken.Uid.ToString()), null);
                   
                        return true;
                }
                else
                {
                  
                    return false;
                }
            }
            else
            {
                //token错误
                return false;
            }
        
        }
    }
}