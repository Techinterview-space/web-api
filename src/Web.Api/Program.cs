#pragma warning disable SA1005, SA1515
//
//            God Bless         No Bugs
//
//
//
//                      _oo0oo_
//                     o8888888o
//                     88" . "88
//                     (| -_- |)
//                     0\  =  /0
//                   ___/`---'\___
//                 .' \\|     |// '.
//                / \\|||  :  |||// \
//               / _||||| -:- |||||- \
//              |   | \\\  -  /// |   |
//              | \_|  ''\---/''  |_/ |
//              \  .-\__  '-'  ___/-. /
//            ___'. .'  /--.--\  `. .'___
//         ."" '<  `.___\_<|>_/___.' >' "".
//        | | :  `- \`.;`\ _ /`;.`/ - ` : | |
//        \  \ `_.   \_ __\ /__ _/   .-` /  /
//    =====`-.____`.___ \_____/___.-`___.-'=====
//                      `=---='
//
//  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
//
#pragma warning restore SA1005, SA1515
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Web.Api;

public class Program
{
#pragma warning disable UseAsyncSuffix
    public static async Task Main(string[] args)
#pragma warning restore UseAsyncSuffix
    {
        try
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Could not start the application due to error:\r\n{e.Message}", e);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}