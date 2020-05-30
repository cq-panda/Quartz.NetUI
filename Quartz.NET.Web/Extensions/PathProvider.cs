using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Extensions
{
    public interface IPathProvider
    {
        string MapPath(string path);
        string MapPath(string path, bool rootPath);
        IWebHostEnvironment GetHostingEnvironment();
    }

    public class PathProvider : IPathProvider
    {
        private IWebHostEnvironment _hostingEnvironment;

        public PathProvider(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }
        public IWebHostEnvironment GetHostingEnvironment()
        {
            return _hostingEnvironment;
        }

        public string MapPath(string path)
        {
            return MapPath(path, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rootPath">获取wwwroot路径</param>
        /// <returns></returns>
        public string MapPath(string path, bool rootPath)
        {
            if (rootPath)
            {
                return Path.Combine(_hostingEnvironment.WebRootPath, path);
            }
            return Path.Combine(_hostingEnvironment.ContentRootPath, path);
        }
    }
}
