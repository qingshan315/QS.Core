﻿using System.IO;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// 根据条件展示数据
        /// </summary>
        /// <param name="val"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static string IF(this string val, bool condition)
        {
            if (condition)
            {
                return val;
            }
            return "";
        }

        #region 空值转换
        /// <summary>
        /// 判断字符串是否为Null、空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNull(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// 判断字符串是否不为Null、空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool NotNull(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
        #endregion


        #region 路径转换
        /// <summary>
        /// 将相对路径转换成程序所在的绝对路径
        /// </summary>
        /// <param name="path">要进行转换的路径，可以是绝对路径，相以路径和URL地址</param>
        /// <returns>转换后的全路径</returns>
        public static string ToLocalDirectory(this string path)
        {
            if (!path.Contains(":"))
            {
                var basePath = Directory.GetCurrentDirectory();

                if (!basePath.EndsWith("\\") && !path.StartsWith("\\"))
                {
                    return string.Concat(Directory.GetCurrentDirectory(), "\\", path);
                }
                else if (basePath.EndsWith("\\") && path.StartsWith("\\"))
                {
                    path = path.Remove(0, 1);

                }
                return string.Concat(Directory.GetCurrentDirectory(), path);

            }
            return path;
        }
        #endregion
    }
}