using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="Logger{T}"/> for all generic types.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogger(this IServiceCollection services) 
        {   
            services.TryAddTransient(typeof(Logger<>));

            return services;
        }
    }
}
