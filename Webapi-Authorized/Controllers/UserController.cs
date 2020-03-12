using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Webapi_Authorized.Models;
using Webapi_Authorized.MyFilter;

namespace Webapi_Authorized.Controllers
{
    [MyGoodsFilter]
    public class UserController : ApiController
    {
        //用户
        //1、查询用户信息
        //2、用户可以登录
        //3、用户可以注册
        //4、用户可以修改个人信息
        DataClasses1DataContext goods = new DataClasses1DataContext();
        [AllowAnonymous]
        [Route("api/Login")]
        public IHttpActionResult Post(UserInfo user)
        {
            if (user!=null)
            {
                //1、判断是否登录成功
                var uinfo = goods.UserInfo.FirstOrDefault(x => x.User_Name == user.User_Name && x.User_Pwd == user.User_Pwd);

                if (uinfo !=null)
                {
                    //表示对象登录成功
                    //1、删除过期令牌
                    DeleteToken(uinfo.User_id);
                    //2、生成新令牌  
                    //使用Guid 生成一个
                    Guid guid = Guid.NewGuid();
                    //当前guid内包含有- ,转换一下
                    string token = guid.ToString().Replace("-", "");
                    TokenInfo newtoken = new TokenInfo
                    {
                        Uid = uinfo.User_id,
                        Token = token,
                        ExpireData = DateTime.Now.AddHours(2)  //设置过期时间
                    };
                    goods.TokenInfo.InsertOnSubmit(newtoken);
                    goods.SubmitChanges();
                    return Ok(token);
                }
                else
                {
                    //登录失败
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }
        public void DeleteToken(int uid)
        {
            //在令牌表中，根据用户id 来查询所有的对应的token
            var list = goods.TokenInfo.Where(x => x.Uid == uid);
            goods.TokenInfo.DeleteAllOnSubmit(list);
            goods.SubmitChanges();

        }
    }
}
