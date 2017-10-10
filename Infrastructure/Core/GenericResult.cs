using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiThreeTierPhotoGallery.Infrastructure.Core
{
    /// <summary>
    /// 用来返回登录注册结果
    /// </summary>
    public class GenericResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
    }
}
