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
    [MyGoodsFilter]  //对整个控制器进行过滤授权
    public class CommodityController : ApiController
    {

        //1、实例化上下文对象
        DataClasses1DataContext goods = new DataClasses1DataContext();
        //功能1 指定商品推荐同类商品
        /// <summary>
        /// 指定商品推荐同类商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/GetCommGoodsByid/{id}")]
        public IHttpActionResult GetCommGoodsByid(int id)
        {
            //1、先更具商品编号查询对应的类别编号
            var typeid = goods.GoodsInfo.FirstOrDefault(x => x.Goods_id == id).GoodsType;
            //2、根据类别id，来查询同一个类别下的所有商品,但是应该除去当前的产品
            var result = goods.GoodsInfo.
                Where(x => x.GoodsType == typeid && x.Goods_id != id)
                .Select(x=>
                new {
                    x.Goods_id,
                    x.Goods_Name,
                    x.Goods_Price,
                    x.Goods_Img,
                    x.Goods_Time
                });
            return Json(result);

        }
        /// <summary>
        /// 查询商品名称以及分页
        /// </summary>
        /// <param name="page">请求页码</param>
        /// <returns>指定页数</returns>
        [Route("api/GetGoodsName/{page?}")]
        public IHttpActionResult GetGoodsName(int page=1)
        {
            //根据实际情况，有可能还会显示总条数 以及总页数
            int pagesize = 2;
            //总条数
            var count = goods.GoodsInfo.Count();
           //分页
            int totalpage = (int)Math.Ceiling(count * 1.0 / pagesize);
            var result = goods.GoodsInfo.Select(x => new
            {
                x.Goods_id,
                x.Goods_Name
            }).Skip((page - 1) * pagesize).Take(pagesize);
            //需要把总数据条数以及页码，还有每页显示数据，都传递出去？
            //使用匿名对象
            var data = new { datacount = count, pages = totalpage, rs = result };
            return Json(data);
        }
        /// <summary>
        /// 获取商品评论以及分页
        /// </summary>
        /// <param name="id">商品编号</param>
        /// <param name="page">页码</param>
        /// <returns>指定商品的对应也得评论内容</returns>
        public IHttpActionResult GetCommentByGoodsId(int id,int page)
        {
            //评论中：评论的内容 评论的事件，uid (用户名（用户的昵称）)
            //如果还需要显示商品的相关信息，比如名称， 商品图片
            //评论的内容，时间，用户表，商品的名称，商品的图片
            //分析：评论表中 用户表中 商品表中
            int pagesize = 2;
            //三表连接查询
            var result = from x in goods.CommentInfo
                         join y in goods.UserInfo
                         on x.Comment_Uid equals y.User_id
                         join z in goods.GoodsInfo
                         on x.Conmment_Goodsid equals z.Goods_id
                         where x.Conmment_Goodsid == id
                         select new
                         {
                             x.Comment_id,
                             x.Comment_Content,
                             x.Comment_Time,
                             y.User_Name,
                             z.Goods_Name,
                             z.Goods_Img,
                             z.Goods_id
                         };
            //对查询出来的数据分页
            result = result.Skip((page - 1) * pagesize).Take(pagesize);
            var result2 = goods.CommentInfo.Select(
                x => new
                {
                    x.Comment_id,
                    x.Comment_Content,
                    x.Comment_Time,
                    x.UserInfo.User_Name,
                    x.GoodsInfo.Goods_Name,
                    x.GoodsInfo.Goods_Img,
                    x.Comment_Uid
                }
                );
            return Json(result);
        }

    }
}
